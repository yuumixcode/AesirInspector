using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	public static class BackgroundTaskRunner
	{
		private static int updateCount;

		private static DateTime time;

		internal static float dt;

		internal static float TotalWorkLastTickMs;

		public static List<BackgroundTaskHandle> AllTasks;

		[HideInInspector]
		public static readonly object Relax;

		public static EditorPrefFloat MaxBackgroundTaskMSPerFrame;

		[OnInspectorGUI]
		private static void Refresh()
		{
			GUIHelper.RequestRepaint();
		}

		static BackgroundTaskRunner()
		{
			updateCount = 0;
			time = DateTime.Now;
			dt = 0.02f;
			TotalWorkLastTickMs = 0f;
			AllTasks = new List<BackgroundTaskHandle>();
			Relax = new object();
			MaxBackgroundTaskMSPerFrame = new EditorPrefFloat("ODIN_VALIDATOR_MaxBackgroundTaskMSPerFrame", 2f);
			IEnumerator mainRunner = TaskEnumerator();
			EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, (EditorApplication.CallbackFunction)delegate
			{
				dt = (float)(DateTime.Now - time).TotalSeconds;
				time = DateTime.Now;
				mainRunner.MoveNext();
			});
		}

		[HideInInspector]
		private static IEnumerator TaskEnumerator()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			List<BackgroundTaskHandle> taskBuffer = new List<BackgroundTaskHandle>();
			while (true)
			{
				if (Application.isPlaying)
				{
					yield return null;
				}
				taskBuffer.Clear();
				taskBuffer.AddRange(AllTasks);
				foreach (BackgroundTaskHandle item in taskBuffer)
				{
					item.FrameTimeMs = 0.0;
					item.FrameTickCount = 0;
					item.AverageFrameTickMs = 0.0;
				}
				float totalWorkMs = 0f;
				float maxMs = MaxBackgroundTaskMSPerFrame.Value;
				bool skipWhileSceneViewIsBeingInteractedWith = GlobalConfig<GlobalValidationConfig>.Instance.PauseValidationWhileWorkingInSceneView.Value;
				while (sw.Elapsed.TotalMilliseconds < (double)maxMs && taskBuffer.Count > 0)
				{
					for (int i = 0; i < taskBuffer.Count; i++)
					{
						BackgroundTaskHandle task = taskBuffer[i];
						float t1 = (float)sw.Elapsed.TotalMilliseconds;
						bool skip = false;
						if (skipWhileSceneViewIsBeingInteractedWith && SceneViewUtility.lastSceneViewInteraction + 0.2 > EditorApplication.timeSinceStartup)
						{
							skip = true;
						}
						else
						{
							task.MoveNext();
						}
						float t2 = (float)sw.Elapsed.TotalMilliseconds;
						float delta = t2 - t1;
						task.FrameTimeMs += delta;
						task.AverageFrameTickMs += delta;
						task.TotalMsWorked += delta;
						task.FrameTickCount++;
						totalWorkMs += delta;
						if (!task.IsAlive || task.IsPaused || task.Relaxed || skip)
						{
							task.AverageFrameTickMs /= task.FrameTickCount;
							taskBuffer.RemoveAt(i);
							i--;
							if (!task.IsAlive)
							{
								AllTasks.Remove(task);
							}
						}
					}
				}
				foreach (BackgroundTaskHandle item2 in taskBuffer)
				{
					item2.AverageFrameTickMs /= item2.FrameTickCount;
				}
				if (totalWorkMs > 0f)
				{
					foreach (BackgroundTaskHandle item3 in AllTasks)
					{
						double weight = item3.FrameTimeMs / (double)totalWorkMs;
						item3.WorkWeight = weight;
					}
				}
				TotalWorkLastTickMs = totalWorkMs;
				yield return null;
				sw.Reset();
				sw.Start();
			}
		}

		public static BackgroundTaskHandle StartTask(string taskName, IEnumerator task)
		{
			BackgroundTaskHandle currTask = AllTasks.FirstOrDefault((BackgroundTaskHandle x) => x.Task == task);
			if (currTask != null)
			{
				if (currTask.IsAlive)
				{
					throw new InvalidOperationException("Task is already running");
				}
				AllTasks.Remove(currTask);
			}
			BackgroundTaskHandle handle = new BackgroundTaskHandle(taskName, task);
			AllTasks.Add(handle);
			return handle;
		}
	}
}

using System;
using System.Collections;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	[HideReferenceObjectPicker]
	public class BackgroundTaskHandle
	{
		public enum State
		{
			Running,
			Paused,
			Relaxed,
			Killed
		}

		[DisplayAsString]
		public string Name;

		[DisplayAsString]
		public bool IsPaused;

		[DisplayAsString]
		public int TickCount;

		[DisplayAsString]
		public int FrameTickCount;

		[DisplayAsString]
		public double AverageFrameTickMs;

		[DisplayAsString]
		public double FrameTimeMs;

		[DisplayAsString]
		public double TotalMsWorked;

		[DisplayAsString]
		public double WorkWeight;

		[DisplayAsString]
		public bool IsAlive;

		[HideInInspector]
		public readonly IEnumerator Task;

		[DisplayAsString]
		public bool Relaxed;

		public State CurrentState
		{
			get
			{
				if (IsPaused)
				{
					return State.Paused;
				}
				if (Relaxed)
				{
					return State.Relaxed;
				}
				if (IsAlive)
				{
					return State.Running;
				}
				return State.Killed;
			}
		}

		internal BackgroundTaskHandle(string taskName, IEnumerator task)
		{
			Name = taskName;
			Task = task;
			IsAlive = true;
		}

		public void Kill()
		{
			if (IsAlive)
			{
				Relaxed = false;
				IsPaused = false;
				IsAlive = false;
			}
		}

		public void CompleteNow()
		{
			if (!IsAlive)
			{
				throw new InvalidOperationException("Cannot complete a task that is not alive.");
			}
			IsPaused = false;
			try
			{
				while (Task.MoveNext())
				{
					TickCount++;
				}
			}
			finally
			{
				IsAlive = false;
			}
		}

		public void Pause()
		{
			IsPaused = true;
		}

		public void Continue()
		{
			IsPaused = false;
		}

		internal void MoveNext()
		{
			try
			{
				if (!IsPaused && IsAlive)
				{
					if (Task.MoveNext())
					{
						Relaxed = Task.Current == BackgroundTaskRunner.Relax;
					}
					else
					{
						IsAlive = false;
					}
					TickCount++;
				}
			}
			catch (Exception ex)
			{
				IsAlive = false;
				Exception ex2 = ex.UnwrapException();
				Debug.LogException(ex2);
			}
		}
	}
}

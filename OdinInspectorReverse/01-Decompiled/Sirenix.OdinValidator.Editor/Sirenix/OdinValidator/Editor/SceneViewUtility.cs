using System;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	internal static class SceneViewUtility
	{
		public static double lastSceneViewInteraction;

		[InitializeOnLoadMethod]
		private static void Init()
		{
			UnityEditorEventUtility.DuringSceneGUI += delegate
			{
				if (Event.current.isKey || Event.current.isScrollWheel)
				{
					lastSceneViewInteraction = EditorApplication.timeSinceStartup;
				}
				else if (Event.current.isMouse && Event.current.rawType != EventType.MouseMove)
				{
					lastSceneViewInteraction = EditorApplication.timeSinceStartup;
				}
				else if (Event.current.rawType == EventType.Used)
				{
					lastSceneViewInteraction = EditorApplication.timeSinceStartup;
				}
			};
			ValidationSessionResultCollector.OnAnyResultsChanged += delegate(ValidationSession session)
			{
				foreach (ValidationSessionEditor current in ValidationSessionEditor.ActiveEditors)
				{
					if (current.ValidationSession == session)
					{
						current.Window.Repaint();
					}
				}
				if (!GlobalConfig<GlobalValidationConfig>.Instance.ShowWidget)
				{
					return;
				}
				foreach (object current2 in SceneView.sceneViews)
				{
					if (current2 is SceneView sceneView && sceneView != null)
					{
						sceneView.Repaint();
					}
				}
			};
			Vector3[] positions = new Vector3[0];
			Quaternion[] rotations = new Quaternion[0];
			EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, (EditorApplication.CallbackFunction)delegate
			{
				Camera[] allSceneCameras = SceneView.GetAllSceneCameras();
				if (allSceneCameras.Length != positions.Length)
				{
					positions = new Vector3[allSceneCameras.Length];
					rotations = new Quaternion[allSceneCameras.Length];
				}
				bool flag = false;
				for (int i = 0; i < allSceneCameras.Length; i++)
				{
					Transform transform = allSceneCameras[i].transform;
					ref Vector3 reference = ref positions[i];
					ref Quaternion reference2 = ref rotations[i];
					Vector3 position = transform.position;
					Quaternion rotation = transform.rotation;
					flag = flag || reference != position || reference2 != rotation;
					reference = position;
					reference2 = rotation;
				}
				if (flag)
				{
					lastSceneViewInteraction = EditorApplication.timeSinceStartup;
				}
			});
		}
	}
}

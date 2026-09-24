using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Sirenix.OdinValidator.Editor
{
	internal static class UnitySceneSetupChangeId
	{
		public static int SceneSetupChangeId { get; private set; }

		static UnitySceneSetupChangeId()
		{
			EditorSceneManager.sceneClosed += delegate
			{
				Increment();
			};
			SceneManager.activeSceneChanged += delegate
			{
				Increment();
			};
			SceneManager.sceneLoaded += delegate
			{
				Increment();
			};
			EditorSceneManager.activeSceneChangedInEditMode += delegate
			{
				Increment();
			};
			EditorSceneManager.sceneOpened += delegate
			{
				Increment();
			};
			EditorSceneManager.newSceneCreated += delegate
			{
				Increment();
			};
			EditorSceneManager.sceneOpening += delegate
			{
				Increment();
			};
			EditorSceneManager.sceneClosing += delegate
			{
				Increment();
			};
			EditorSceneManager.sceneSaving += delegate
			{
				Increment();
			};
			EditorSceneManager.sceneSaved += delegate
			{
				Increment();
			};
		}

		private static void Increment()
		{
			SceneSetupChangeId++;
			EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, (EditorApplication.CallbackFunction)delegate
			{
				SceneSetupChangeId++;
			});
		}
	}
}

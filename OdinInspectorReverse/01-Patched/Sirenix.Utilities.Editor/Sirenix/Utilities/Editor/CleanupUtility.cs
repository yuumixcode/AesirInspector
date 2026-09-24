using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	public static class CleanupUtility
	{
		private static List<UnityEngine.Object> toCleanUpUnityObjects;

		private static List<WeakReference> toCleanUpDisposables;

		static CleanupUtility()
		{
			toCleanUpUnityObjects = new List<UnityEngine.Object>();
			toCleanUpDisposables = new List<WeakReference>();
			AssemblyReloadEvents.beforeAssemblyReload += CleanUp;
		}

		public static void DestroyObjectOnAssemblyReload(UnityEngine.Object unityObj)
		{
			if (!(unityObj == null))
			{
				toCleanUpUnityObjects.Add(unityObj);
			}
		}

		public static void DisposeObjectOnAssemblyReload(IDisposable disposable)
		{
			if (disposable != null)
			{
				toCleanUpDisposables.Add(new WeakReference(disposable));
			}
		}

		private static void CleanUp()
		{
			foreach (UnityEngine.Object unityObj in toCleanUpUnityObjects)
			{
				try
				{
					if (unityObj != null)
					{
						UnityEngine.Object.DestroyImmediate(unityObj);
					}
				}
				catch
				{
				}
			}
			foreach (WeakReference reference in toCleanUpDisposables)
			{
				try
				{
					if (reference.Target is IDisposable disposable)
					{
						disposable.Dispose();
					}
				}
				catch
				{
				}
			}
			toCleanUpUnityObjects.Clear();
			toCleanUpDisposables.Clear();
		}
	}
}

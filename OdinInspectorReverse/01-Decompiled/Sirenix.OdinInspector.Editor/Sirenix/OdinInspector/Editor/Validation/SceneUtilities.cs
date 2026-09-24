using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public static class SceneUtilities
	{
		private static readonly MethodInfo Scene_GetRootGameObjects_Method;

		private static readonly ConstructorInfo HierarchyProperty_Constructor;

		private static readonly MethodInfo HierarchyProperty_Next_Method;

		private static readonly PropertyInfo HierarchyProperty_PptrValue_Property;

		private static readonly object HierarchyType_GameObjects;

		private static PropertyInfo loadedSceneCount;

		static SceneUtilities()
		{
			Scene_GetRootGameObjects_Method = typeof(Scene).GetMethod("GetRootGameObjects", BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
			loadedSceneCount = typeof(SceneManager).GetProperty("loadedSceneCount", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public) ?? typeof(EditorSceneManager).GetProperty("loadedSceneCount", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
			if (loadedSceneCount == null)
			{
				Debug.LogError("Unity's SceneManager.loadedSceneCount and EditorSceneManager.loadedSceneCount properties have both been renamed or removed in this version of Unity (" + Application.unityVersion + "). Odin will not be able to determine how many scenes are loaded, which may cause UI issues at times, such as the Validator being unable to warn you about new, unsaved scenes not being validated. Please update Odin, or inform the developers of this issue if you are on the latest version of Odin. You are currently on Odin version " + OdinInspectorVersion.Version + ".");
			}
			TryGetHierarchyPropertyMembers(out HierarchyProperty_Constructor, out HierarchyProperty_Next_Method, out HierarchyProperty_PptrValue_Property, out HierarchyType_GameObjects);
		}

		private static bool TryGetHierarchyPropertyMembers(out ConstructorInfo constructor, out MethodInfo nextMethod, out PropertyInfo pptrValueProperty, out object hierarchyTypeGameObjects)
		{
			constructor = null;
			nextMethod = null;
			pptrValueProperty = null;
			hierarchyTypeGameObjects = null;
			Assembly unityEditorAssembly = typeof(EditorUtility).Assembly;
			Type hierarchyPropertyType = unityEditorAssembly.GetType("UnityEditor.HierarchyProperty");
			Type hierarchyTypeType = unityEditorAssembly.GetType("UnityEditor.HierarchyType");
			if (hierarchyPropertyType == null || hierarchyTypeType == null)
			{
				return false;
			}
			try
			{
				hierarchyTypeGameObjects = Enum.Parse(hierarchyTypeType, "GameObjects");
			}
			catch
			{
				return false;
			}
			constructor = hierarchyPropertyType.GetConstructor(new Type[1] { hierarchyTypeType });
			nextMethod = hierarchyPropertyType.GetMethod("Next", BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(int[]) }, null);
			pptrValueProperty = hierarchyPropertyType.GetProperty("pptrValue", BindingFlags.Instance | BindingFlags.Public);
			if (constructor != null && nextMethod != null)
			{
				return pptrValueProperty != null;
			}
			return false;
		}

		public static int GetLoadedSceneCount()
		{
			if (loadedSceneCount == null)
			{
				return 0;
			}
			return (int)loadedSceneCount.GetValue(null);
		}

		public static IEnumerable<GameObject> GetSceneRoots(Scene scene)
		{
			if (Scene_GetRootGameObjects_Method != null && scene.IsValid())
			{
				GameObject[] roots = (GameObject[])Scene_GetRootGameObjects_Method.Invoke(scene, null);
				GameObject[] array = roots;
				for (int i = 0; i < array.Length; i++)
				{
					yield return array[i];
				}
				yield break;
			}
			object prop = ((HierarchyProperty_Constructor == null) ? null : HierarchyProperty_Constructor.Invoke(new object[1] { HierarchyType_GameObjects }));
			int[] expanded = new int[0];
			if (prop != null && !(HierarchyProperty_Next_Method == null) && !(HierarchyProperty_PptrValue_Property == null))
			{
				while ((bool)HierarchyProperty_Next_Method.Invoke(prop, new object[1] { expanded }))
				{
					yield return HierarchyProperty_PptrValue_Property.GetValue(prop, null) as GameObject;
				}
			}
		}
	}
}

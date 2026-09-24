using System;
using System.Reflection;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector
{
	public static class EditorOnlyModeConfigUtility
	{
		private static bool initialized;

		private static object instance;

		private static WeakValueGetter<bool> isInEditorOnlyModeGetter;

		public const string SERIALIZATION_DISABLED_ERROR_TEXT = "ERROR: EDITOR ONLY MODE ENABLED\n\nOdin is currently in editor only mode, meaning the serialization system is disabled in builds. This class is specially serialized by Odin - if you try to compile with this class in your project, you *will* get compiler errors. Either disable editor only mode in Tools -> Odin Inspector -> Preferences -> Editor Only Mode, or make sure that this type does not inherit from a type that is serialized by Odin.";

		public static bool IsSerializationEnabled
		{
			get
			{
				if (!initialized)
				{
					initialized = true;
					Type editorOnlyModeConfigType = null;
					try
					{
						Assembly editorAssembly = null;
						Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
						foreach (Assembly assembly in assemblies)
						{
							if (assembly.GetName().Name == "Sirenix.OdinInspector.Editor")
							{
								editorAssembly = assembly;
								break;
							}
						}
						if (editorAssembly != null)
						{
							editorOnlyModeConfigType = editorAssembly.GetType("Sirenix.OdinInspector.Editor.EditorOnlyModeConfig");
						}
					}
					catch
					{
					}
					PropertyInfo instanceProperty = editorOnlyModeConfigType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
					FieldInfo isInEditorOnlyModeField = editorOnlyModeConfigType.GetField("isInEditorOnlyMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (isInEditorOnlyModeField != null)
					{
						instance = instanceProperty.GetValue(null, null);
						isInEditorOnlyModeGetter = EmitUtilities.CreateWeakInstanceFieldGetter<bool>(editorOnlyModeConfigType, isInEditorOnlyModeField);
					}
					else
					{
						instance = instanceProperty.GetValue(null, null);
						isInEditorOnlyModeGetter = delegate
						{
							return true;
						};
					}
				}
				return !isInEditorOnlyModeGetter(ref instance);
			}
		}

		public static void InternalOnInspectorGUI(UnityEngine.Object obj)
		{
			if (!IsSerializationEnabled)
			{
				GUILayout.Space(10f);
				Color color = GUI.color;
				GUI.color = new Color(0.8f, 0.1f, 0.4f, 1f);
				EditorGUILayout.HelpBox("ERROR: EDITOR ONLY MODE ENABLED\n\nOdin is currently in editor only mode, meaning the serialization system is disabled in builds. This class is specially serialized by Odin - if you try to compile with this class in your project, you *will* get compiler errors. Either disable editor only mode in Tools -> Odin Inspector -> Preferences -> Editor Only Mode, or make sure that this type does not inherit from a type that is serialized by Odin.", MessageType.Error);
				GUI.color = color;
			}
			if (!GlobalConfig<GlobalSerializationConfig>.Instance.HideSerializationCautionaryMessage && !obj.GetType().FullName.StartsWith("Sirenix.OdinInspector."))
			{
				GUILayout.Space(10f);
				EditorGUILayout.HelpBox("Odin's custom serialization protocol is stable and fast. It is built to be fast, reliable and resilient above all.\n\n*Words of caution* \nHowever, caveats apply - there is a reason Unity chose such a drastically limited serialization protocol. It keeps things simple and manageable, and limits how much complexity you can introduce into your data structures. It can be very easy to get carried away and shoot yourself in the foot when all limitations suddenly disappear, and hence we have included this cautionary warning.\n\nWarning words aside, there can of course be valid reasons to use a more powerful serialization protocol such as Odin's. However, we advise you to use it wisely and with restraint. After all, with great power comes great responsibility!\n\n\n\n", MessageType.Warning);
				Rect rect = GUILayoutUtility.GetLastRect();
				rect.xMin += 34f;
				rect.yMax -= 10f;
				rect.xMax -= 10f;
				rect.yMin = rect.yMax - 25f;
				if (GUI.Button(rect, "I know what I'm about, son. Hide message forever."))
				{
					GlobalConfig<GlobalSerializationConfig>.Instance.HideSerializationCautionaryMessage = true;
					EditorUtility.SetDirty(GlobalConfig<GlobalSerializationConfig>.Instance);
					HandleUtility.Repaint();
				}
				GUILayout.Space(10f);
			}
			if (!GlobalConfig<GlobalSerializationConfig>.Instance.HidePrefabCautionaryMessage && obj is Component && OdinPrefabSerializationEditorUtility.HasNewPrefabWorkflow && (OdinPrefabSerializationEditorUtility.ObjectIsPrefabInstance(obj) || AssetDatabase.Contains(obj)) && !obj.GetType().FullName.StartsWith("Sirenix.OdinInspector."))
			{
				GUILayout.Space(10f);
				EditorGUILayout.HelpBox("In 2018.3, Unity introduced a new prefab workflow, and in so doing, changed how all prefabs fundamentally work. Despite our best efforts, we have so far been unable to achieve a stable implementation of Odin-serialized prefab modifications on prefab instances and variants in the new prefab workflow.This has nothing to do with Odin serializer itself, which remains rock solid. Odin-serialized ScriptableObjects and non-prefab Components/Behaviours are still perfectly stable - you are only seeing this message because this is an Odin-serialized prefab asset or instance.\n\nUsing prefabs with Odin serialization in 2018.3 and above is considered a *deprecated feature* and is officially unsupported. In short, if you disregard this message and then experience issues, we will not be able to help or support you.\n\nPlease keep all this in mind, if you wish to continue using Odin-serialized prefabs.\n\n\n\n", MessageType.Error);
				Rect rect2 = GUILayoutUtility.GetLastRect();
				rect2.xMin += 34f;
				rect2.yMax -= 10f;
				rect2.xMax -= 10f;
				rect2.yMin = rect2.yMax - 25f;
				if (GUI.Button(rect2, "I understand that I'm on my own. Hide message forever."))
				{
					GlobalConfig<GlobalSerializationConfig>.Instance.HidePrefabCautionaryMessage = true;
					EditorUtility.SetDirty(GlobalConfig<GlobalSerializationConfig>.Instance);
					HandleUtility.Repaint();
				}
				GUILayout.Space(10f);
			}
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public static class CustomEditorUtility
	{
		private static class UniversalAPI
		{
			public static Type CustomEditorAttributesType;

			public static Type MonoEditorType;

			public static FieldInfo MonoEditorType_InspectorType;

			public static FieldInfo MonoEditorType_EditorForChildClasses;

			public static FieldInfo MonoEditorType_IsFallback;

			public static FieldInfo CustomEditor_EditorForChildClassesField;

			public static bool IsValid;

			static UniversalAPI()
			{
				try
				{
					CustomEditorAttributesType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.CustomEditorAttributes");
					MonoEditorType = CustomEditorAttributesType.GetNestedType("MonoEditorType", BindingFlags.Public | BindingFlags.NonPublic);
					MonoEditorType_InspectorType = MonoEditorType.GetField("m_InspectorType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? MonoEditorType.GetField("inspectorType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					MonoEditorType_EditorForChildClasses = MonoEditorType.GetField("m_EditorForChildClasses", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? MonoEditorType.GetField("editorForChildClasses", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					MonoEditorType_IsFallback = MonoEditorType.GetField("m_IsFallback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? MonoEditorType.GetField("isFallback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					CustomEditor_EditorForChildClassesField = typeof(CustomEditor).GetField("m_EditorForChildClasses", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					IsValid = true;
				}
				catch (NullReferenceException)
				{
					IsValid = false;
				}
				if (IsValid && (MonoEditorType_InspectorType == null || MonoEditorType_EditorForChildClasses == null || MonoEditorType_IsFallback == null || CustomEditor_EditorForChildClassesField == null))
				{
					IsValid = false;
				}
			}
		}

		private static class Unity_2023_1_API
		{
			public static readonly PropertyInfo CustomEditorAttributesType_Instance;

			public static readonly MethodInfo CustomEditorAttributesType_Rebuild;

			public static readonly FieldInfo CustomEditorAttributesType_Cache;

			public static readonly Type CustomEditorCache_Type;

			public static readonly FieldInfo CustomEditorCache_CustomEditorCacheDict;

			public static readonly Type MonoEditorTypeStorage_Type;

			public static readonly FieldInfo MonoEditorTypeStorage_CustomEditors;

			public static readonly FieldInfo MonoEditorTypeStorage_CustomEditorsMultiEdition;

			public static readonly Type Dictionary_Type_MonoEditorTypeStorage;

			public static readonly MethodInfo Dictionary_Type_MonoEditorTypeStorage_Add;

			public static readonly MethodInfo Dictionary_Type_MonoEditorTypeStorage_TryGetValue;

			public static bool IsValid;

			static Unity_2023_1_API()
			{
				if (!UniversalAPI.IsValid)
				{
					IsValid = false;
					return;
				}
				try
				{
					CustomEditorAttributesType_Instance = UniversalAPI.CustomEditorAttributesType.GetProperty("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					CustomEditorAttributesType_Rebuild = UniversalAPI.CustomEditorAttributesType.GetMethod("Rebuild", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
					CustomEditorAttributesType_Cache = UniversalAPI.CustomEditorAttributesType.GetField("m_Cache", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					MonoEditorTypeStorage_Type = UniversalAPI.CustomEditorAttributesType.GetNestedType("MonoEditorTypeStorage", BindingFlags.Public | BindingFlags.NonPublic);
					MonoEditorTypeStorage_CustomEditors = MonoEditorTypeStorage_Type.GetField("customEditors", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					MonoEditorTypeStorage_CustomEditorsMultiEdition = MonoEditorTypeStorage_Type.GetField("customEditorsMultiEdition", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					CustomEditorCache_Type = UniversalAPI.CustomEditorAttributesType.GetNestedType("CustomEditorCache", BindingFlags.Public | BindingFlags.NonPublic);
					CustomEditorCache_CustomEditorCacheDict = CustomEditorCache_Type.GetField("m_CustomEditorCache", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					Dictionary_Type_MonoEditorTypeStorage = typeof(Dictionary<, >).MakeGenericType(typeof(Type), MonoEditorTypeStorage_Type);
					Dictionary_Type_MonoEditorTypeStorage_Add = Dictionary_Type_MonoEditorTypeStorage.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public, null, new Type[2]
					{
						typeof(Type),
						MonoEditorTypeStorage_Type
					}, null);
					Dictionary_Type_MonoEditorTypeStorage_TryGetValue = Dictionary_Type_MonoEditorTypeStorage.GetMethod("TryGetValue", BindingFlags.Instance | BindingFlags.Public, null, new Type[2]
					{
						typeof(Type),
						MonoEditorTypeStorage_Type.MakeByRefType()
					}, null);
					if (!(CustomEditorCache_CustomEditorCacheDict.FieldType != Dictionary_Type_MonoEditorTypeStorage) && !(CustomEditorAttributesType_Rebuild == null) && !(CustomEditorAttributesType_Cache == null) && !(MonoEditorTypeStorage_CustomEditors == null) && !(MonoEditorTypeStorage_CustomEditorsMultiEdition == null) && !(CustomEditorCache_CustomEditorCacheDict == null) && !(Dictionary_Type_MonoEditorTypeStorage_Add == null) && !(Dictionary_Type_MonoEditorTypeStorage_TryGetValue == null))
					{
						IsValid = true;
					}
				}
				catch (NullReferenceException)
				{
					IsValid = false;
				}
			}

			public static void ResetCustomEditors()
			{
				if (IsValid)
				{
					if (CustomEditorAttributesType_Rebuild.IsStatic)
					{
						CustomEditorAttributesType_Rebuild.Invoke(null, null);
						return;
					}
					object instance = CustomEditorAttributesType_Instance.GetValue(null);
					CustomEditorAttributesType_Rebuild.Invoke(instance, null);
				}
			}

			public static void RegisterCustomMonoEditorEntry(object entry, Type inspectedType, Type editorType, bool isMultiEditor)
			{
				if (IsValid)
				{
					object instance = CustomEditorAttributesType_Instance.GetValue(null);
					object cache = CustomEditorAttributesType_Cache.GetValue(instance);
					object cacheDict = CustomEditorCache_CustomEditorCacheDict.GetValue(cache);
					object[] args = new object[2] { inspectedType, null };
					if (!(bool)Dictionary_Type_MonoEditorTypeStorage_TryGetValue.Invoke(cacheDict, args))
					{
						args[1] = Activator.CreateInstance(MonoEditorTypeStorage_Type);
						MonoEditorTypeStorage_CustomEditors.SetValue(args[1], Activator.CreateInstance(MonoEditorTypeStorage_CustomEditors.FieldType));
						MonoEditorTypeStorage_CustomEditorsMultiEdition.SetValue(args[1], Activator.CreateInstance(MonoEditorTypeStorage_CustomEditorsMultiEdition.FieldType));
						Dictionary_Type_MonoEditorTypeStorage_Add.Invoke(cacheDict, args);
					}
					object monoEditorTypeStorage = args[1];
					IList editorsList = MonoEditorTypeStorage_CustomEditors.GetValue(monoEditorTypeStorage) as IList;
					editorsList.Insert(0, entry);
					if (isMultiEditor)
					{
						IList multiEditorsList = MonoEditorTypeStorage_CustomEditorsMultiEdition.GetValue(monoEditorTypeStorage) as IList;
						multiEditorsList.Insert(0, entry);
					}
				}
			}
		}

		private static class Unity_Pre_2023_API
		{
			public static readonly FieldInfo CustomEditorAttributesType_CachedEditorForType;

			public static readonly FieldInfo CustomEditorAttributesType_CachedMultiEditorForType;

			public static readonly FieldInfo CustomEditorAttributesType_CustomEditors;

			public static readonly FieldInfo CustomEditorAttributesType_CustomMultiEditors;

			public static readonly FieldInfo CustomEditorAttributesType_Initialized;

			public static readonly MethodInfo CustomEditorAttributesType_Rebuild;

			public static FieldInfo MonoEditorType_InspectedType;

			public static readonly bool IsBackedByADictionary;

			public static bool IsValid;

			static Unity_Pre_2023_API()
			{
				if (!UniversalAPI.IsValid)
				{
					IsValid = false;
					return;
				}
				try
				{
					CustomEditorAttributesType_Initialized = UniversalAPI.CustomEditorAttributesType.GetField("s_Initialized", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					CustomEditorAttributesType_CachedEditorForType = UniversalAPI.CustomEditorAttributesType.GetField("kCachedEditorForType", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					CustomEditorAttributesType_CachedMultiEditorForType = UniversalAPI.CustomEditorAttributesType.GetField("kCachedMultiEditorForType", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					CustomEditorAttributesType_CustomEditors = UniversalAPI.CustomEditorAttributesType.GetField("kSCustomEditors", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					CustomEditorAttributesType_CustomMultiEditors = UniversalAPI.CustomEditorAttributesType.GetField("kSCustomMultiEditors", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					CustomEditorAttributesType_Rebuild = UniversalAPI.CustomEditorAttributesType.GetMethod("Rebuild", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					MonoEditorType_InspectedType = UniversalAPI.MonoEditorType.GetField("m_InspectedType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (CustomEditorAttributesType_Initialized == null || CustomEditorAttributesType_CustomEditors == null || CustomEditorAttributesType_CustomMultiEditors == null || MonoEditorType_InspectedType == null)
					{
						throw new NullReferenceException();
					}
					IsBackedByADictionary = typeof(IDictionary).IsAssignableFrom(CustomEditorAttributesType_CustomEditors.FieldType);
					IsValid = true;
				}
				catch (NullReferenceException)
				{
					IsValid = false;
				}
			}

			public static void ResetCustomEditors()
			{
				if (!IsValid)
				{
					return;
				}
				if (IsBackedByADictionary)
				{
					((IDictionary)CustomEditorAttributesType_CustomEditors.GetValue(null)).Clear();
					((IDictionary)CustomEditorAttributesType_CustomMultiEditors.GetValue(null)).Clear();
				}
				else
				{
					if (CustomEditorAttributesType_CachedEditorForType != null)
					{
						((Dictionary<Type, Type>)CustomEditorAttributesType_CachedEditorForType.GetValue(null)).Clear();
					}
					if (CustomEditorAttributesType_CachedMultiEditorForType != null)
					{
						((Dictionary<Type, Type>)CustomEditorAttributesType_CachedMultiEditorForType.GetValue(null)).Clear();
					}
					((IList)CustomEditorAttributesType_CustomEditors.GetValue(null)).Clear();
					((IList)CustomEditorAttributesType_CustomMultiEditors.GetValue(null)).Clear();
				}
				if (UnityVersion.IsVersionOrGreater(2019, 1))
				{
					CustomEditorAttributesType_Rebuild.Invoke(null, null);
					CustomEditorAttributesType_Initialized.SetValue(null, true);
				}
				else
				{
					CustomEditorAttributesType_Initialized.SetValue(null, false);
				}
			}

			public static void RegisterCustomMonoEditorEntry(object entry, Type inspectedType, Type editorType, bool isMultiEditor)
			{
				if (!IsValid)
				{
					return;
				}
				MonoEditorType_InspectedType.SetValue(entry, inspectedType);
				if (IsBackedByADictionary)
				{
					AddEntryToDictList((IDictionary)CustomEditorAttributesType_CustomEditors.GetValue(null), entry, inspectedType);
					if (isMultiEditor)
					{
						AddEntryToDictList((IDictionary)CustomEditorAttributesType_CustomMultiEditors.GetValue(null), entry, inspectedType);
					}
					return;
				}
				if (CustomEditorAttributesType_CachedEditorForType != null && CustomEditorAttributesType_CachedMultiEditorForType != null)
				{
					((IDictionary)CustomEditorAttributesType_CachedEditorForType.GetValue(null))[inspectedType] = editorType;
					if (isMultiEditor)
					{
						((IDictionary)CustomEditorAttributesType_CachedMultiEditorForType.GetValue(null))[inspectedType] = editorType;
					}
				}
				((IList)CustomEditorAttributesType_CustomEditors.GetValue(null)).Insert(0, entry);
				if (isMultiEditor)
				{
					((IList)CustomEditorAttributesType_CustomMultiEditors.GetValue(null)).Insert(0, entry);
				}
			}

			private static void AddEntryToDictList(IDictionary dict, object entry, Type inspectedType)
			{
				IList list = (IList)(dict.Contains(inspectedType) ? ((IList)dict[inspectedType]) : (dict[inspectedType] = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(UniversalAPI.MonoEditorType))));
				list.Insert(0, entry);
			}
		}

		public static readonly bool IsValid;

		static CustomEditorUtility()
		{
			IsValid = UniversalAPI.IsValid && (Unity_2023_1_API.IsValid || Unity_Pre_2023_API.IsValid);
			if (!IsValid)
			{
				Debug.LogError("Unity's internal custom editor management classes have changed in this version of Unity (" + Application.unityVersion + "). Odin will not be able to dynamically register any editors; only hardcoded Odin editors will work.");
			}
		}

		public static void ResetCustomEditors()
		{
			if (Unity_2023_1_API.IsValid)
			{
				Unity_2023_1_API.ResetCustomEditors();
			}
			else if (Unity_Pre_2023_API.IsValid)
			{
				Unity_Pre_2023_API.ResetCustomEditors();
			}
		}

		public static void SetCustomEditor(Type inspectedType, Type editorType)
		{
			if (IsValid)
			{
				CustomEditor attr = editorType.GetCustomAttribute<CustomEditor>();
				if (attr == null)
				{
					throw new ArgumentException("Editor type to set '" + editorType.GetNiceName() + "' has no CustomEditor attribute applied! Use a SetCustomEditor overload that takes isFallbackEditor and isEditorForChildClasses parameters.");
				}
				SetCustomEditor(inspectedType, editorType, attr.isFallback, (bool)UniversalAPI.CustomEditor_EditorForChildClassesField.GetValue(attr));
			}
		}

		public static void SetCustomEditor(Type inspectedType, Type editorType, bool isFallbackEditor, bool isEditorForChildClasses)
		{
			if (IsValid)
			{
				SetCustomEditor(inspectedType, editorType, isFallbackEditor, isEditorForChildClasses, editorType.IsDefined<CanEditMultipleObjects>());
			}
		}

		public static void SetCustomEditor(Type inspectedType, Type editorType, bool isFallbackEditor, bool isEditorForChildClasses, bool isMultiEditor)
		{
			if (IsValid)
			{
				object entry = Activator.CreateInstance(UniversalAPI.MonoEditorType);
				UniversalAPI.MonoEditorType_InspectorType.SetValue(entry, editorType);
				UniversalAPI.MonoEditorType_IsFallback.SetValue(entry, isFallbackEditor);
				UniversalAPI.MonoEditorType_EditorForChildClasses.SetValue(entry, isEditorForChildClasses);
				if (Unity_2023_1_API.IsValid)
				{
					Unity_2023_1_API.RegisterCustomMonoEditorEntry(entry, inspectedType, editorType, isMultiEditor);
				}
				else if (Unity_Pre_2023_API.IsValid)
				{
					Unity_Pre_2023_API.RegisterCustomMonoEditorEntry(entry, inspectedType, editorType, isMultiEditor);
				}
			}
		}
	}
}

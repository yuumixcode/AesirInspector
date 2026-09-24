using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Serialization
{
	public static class OdinPrefabSerializationEditorUtility
	{
		private static bool? hasNewPrefabWorkflow;

		private static MethodInfo PrefabUtility_GetPrefabAssetType_Method;

		private static MethodInfo PrefabUtility_GetPrefabParent_Method;

		private static MethodInfo PrefabUtility_GetCorrespondingObjectFromSource_Method;

		private static MethodInfo PrefabUtility_GetPrefabType_Method;

		private static MethodInfo PrefabUtility_ApplyPropertyOverride_Method;

		public static bool HasNewPrefabWorkflow
		{
			get
			{
				if (!hasNewPrefabWorkflow.HasValue)
				{
					hasNewPrefabWorkflow = DetectNewPrefabWorkflow();
				}
				return hasNewPrefabWorkflow.Value;
			}
		}

		public static bool HasApplyPropertyOverride => PrefabUtility_ApplyPropertyOverride_Method != null;

		static OdinPrefabSerializationEditorUtility()
		{
			PrefabUtility_GetPrefabAssetType_Method = typeof(PrefabUtility).GetMethod("GetPrefabAssetType", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(UnityEngine.Object) }, null);
			PrefabUtility_GetPrefabParent_Method = typeof(PrefabUtility).GetMethod("GetPrefabParent", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(UnityEngine.Object) }, null);
			PrefabUtility_GetCorrespondingObjectFromSource_Method = typeof(PrefabUtility).GetMethod("GetCorrespondingObjectFromSource", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(UnityEngine.Object) }, null);
			PrefabUtility_GetPrefabType_Method = typeof(PrefabUtility).GetMethod("GetPrefabType", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(UnityEngine.Object) }, null);
			Type interactionModeEnum = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InteractionMode");
			if (interactionModeEnum != null)
			{
				PrefabUtility_ApplyPropertyOverride_Method = typeof(PrefabUtility).GetMethod("ApplyPropertyOverride", BindingFlags.Static | BindingFlags.Public, null, new Type[3]
				{
					typeof(SerializedProperty),
					typeof(string),
					interactionModeEnum
				}, null);
			}
		}

		private static bool DetectNewPrefabWorkflow()
		{
			try
			{
				MethodInfo method = typeof(PrefabUtility).GetMethod("GetPrefabType", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(UnityEngine.Object) }, null);
				if (method == null)
				{
					return true;
				}
				if (method.IsDefined(typeof(ObsoleteAttribute), inherit: false))
				{
					return true;
				}
				return false;
			}
			catch
			{
				return false;
			}
		}

		public static void ApplyPropertyOverride(SerializedProperty instanceProperty, string assetPath)
		{
			if (!HasApplyPropertyOverride)
			{
				throw new NotSupportedException("PrefabUtility.ApplyPropertyOverride doesn't exist in this version of Unity");
			}
			PrefabUtility_ApplyPropertyOverride_Method.Invoke(null, new object[3] { instanceProperty, assetPath, 0 });
		}

		public static bool ObjectIsPrefabInstance(UnityEngine.Object unityObject)
		{
			if (PrefabUtility_GetPrefabType_Method != null)
			{
				try
				{
					int prefabType = Convert.ToInt32((Enum)PrefabUtility_GetPrefabType_Method.Invoke(null, new object[1] { unityObject }));
					if (prefabType == 3)
					{
						return true;
					}
				}
				catch (Exception)
				{
				}
			}
			if (PrefabUtility_GetPrefabAssetType_Method != null)
			{
				int prefabAssetType = Convert.ToInt32((Enum)PrefabUtility_GetPrefabAssetType_Method.Invoke(null, new object[1] { unityObject }));
				if (prefabAssetType != 1)
				{
					return prefabAssetType == 3;
				}
				return true;
			}
			if (PrefabUtility_GetPrefabType_Method == null && PrefabUtility_GetPrefabAssetType_Method == null)
			{
				Debug.LogError("Neither PrefabUtility.GetPrefabType or PrefabUtility.GetPrefabAssetType methods could be located. Prefab functionality will likely be broken in this build of Odin.");
			}
			return GetCorrespondingObjectFromSource(unityObject) != null;
		}

		public static bool ObjectHasNestedOdinPrefabData(UnityEngine.Object unityObject)
		{
			if (!HasNewPrefabWorkflow)
			{
				return false;
			}
			if (!(unityObject is ISupportsPrefabSerialization))
			{
				return false;
			}
			UnityEngine.Object prefab = GetCorrespondingObjectFromSource(unityObject);
			return IsOdinSerializedPrefabInstance(prefab);
		}

		private static bool IsOdinSerializedPrefabInstance(UnityEngine.Object unityObject)
		{
			if (!(unityObject is ISupportsPrefabSerialization))
			{
				return false;
			}
			return GetCorrespondingObjectFromSource(unityObject) != null;
		}

		public static UnityEngine.Object GetCorrespondingObjectFromSource(UnityEngine.Object unityObject)
		{
			if (PrefabUtility_GetCorrespondingObjectFromSource_Method != null)
			{
				return (UnityEngine.Object)PrefabUtility_GetCorrespondingObjectFromSource_Method.Invoke(null, new object[1] { unityObject });
			}
			if (PrefabUtility_GetPrefabParent_Method != null)
			{
				return (UnityEngine.Object)PrefabUtility_GetPrefabParent_Method.Invoke(null, new object[1] { unityObject });
			}
			Debug.LogError("Neither PrefabUtility.GetCorrespondingObjectFromSource or PrefabUtility.GetPrefabParent methods could be located. Prefab functionality will be broken in this build of Odin.");
			return null;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	public struct ValidationWorkItem : IEquatable<ValidationWorkItem>
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		public struct Comparer : IEqualityComparer<ValidationWorkItem>
		{
			public bool Equals(ValidationWorkItem x, ValidationWorkItem y)
			{
				return x == y;
			}

			public int GetHashCode(ValidationWorkItem key)
			{
				return key.GetHashCode();
			}
		}

		public object NonUnityObjectValue;

		public string AssetGuid;

		public SceneReference? SceneContent;

		public SceneReference? SceneValidators;

		[Obsolete("Use EntityId instead.")]
		public int InstanceID;

		public OdinEntityId EntityId;

		public ProjectEventSource Source;

		public uint ResultCountEstimate;

		public GlobalValidator GlobalValidator;

		public SceneReference? GetSceneReference()
		{
			return SceneContent ?? SceneValidators;
		}

		public static ValidationWorkItem CreateForNonUnityObject(object obj, ProjectEventSource projectEventSource)
		{
			return new ValidationWorkItem
			{
				Source = projectEventSource,
				NonUnityObjectValue = obj
			};
		}

		public static ValidationWorkItem CreateForAssetGuid(string assetGuid, OdinEntityId entityId, ProjectEventSource projectEventSource)
		{
			ValidationWorkItem result = new ValidationWorkItem
			{
				Source = projectEventSource,
				AssetGuid = assetGuid,
				InstanceID = OdinEntityId.Internal.ToInt32(entityId),
				EntityId = entityId
			};
			if (AssetDatabase.GUIDToAssetPath(assetGuid).FastEndsWith(".unity"))
			{
				result.ResultCountEstimate = 1u;
			}
			else if (!WorkItemResultCountCache.TryGetLastWorkItemResultCount(assetGuid, out result.ResultCountEstimate))
			{
				result.ResultCountEstimate = 1u;
			}
			return result;
		}

		[Obsolete("Use CreateForAssetGuid(string, OdinEntityId, ProjectEventSource) instead.", false)]
		public static ValidationWorkItem CreateForAssetGuid(string assetGuid, int instanceId, ProjectEventSource projectEventSource)
		{
			return CreateForAssetGuid(assetGuid, OdinEntityId.Internal.FromInstanceId(instanceId), projectEventSource);
		}

		public static ValidationWorkItem CreateForAssetGuid(string assetGuid, ProjectEventSource projectEventSource)
		{
			ValidationWorkItem result = new ValidationWorkItem
			{
				Source = projectEventSource,
				AssetGuid = assetGuid
			};
			if (AssetDatabase.GUIDToAssetPath(assetGuid).FastEndsWith(".unity"))
			{
				result.ResultCountEstimate = 1u;
			}
			else if (!WorkItemResultCountCache.TryGetLastWorkItemResultCount(assetGuid, out result.ResultCountEstimate))
			{
				result.ResultCountEstimate = 1u;
			}
			return result;
		}

		public static ValidationWorkItem CreateForSceneContent(SceneReference scene, ProjectEventSource projectEventSource)
		{
			ValidationWorkItem result = new ValidationWorkItem
			{
				Source = projectEventSource,
				SceneContent = scene
			};
			if (!WorkItemResultCountCache.TryGetLastWorkItemResultCount(scene.GUID, out result.ResultCountEstimate))
			{
				result.ResultCountEstimate = 1u;
			}
			return result;
		}

		public static ValidationWorkItem CreateForSceneValidators(SceneReference scene, ProjectEventSource projectEventSource)
		{
			return new ValidationWorkItem
			{
				Source = projectEventSource,
				SceneValidators = scene,
				ResultCountEstimate = 1u
			};
		}

		public static ValidationWorkItem CreateForGlobalValidator(GlobalValidator globalValidator, ProjectEventSource projectEventSource)
		{
			return new ValidationWorkItem
			{
				Source = projectEventSource,
				GlobalValidator = globalValidator,
				ResultCountEstimate = 1u
			};
		}

		public static ValidationWorkItem CreateForEntityId(OdinEntityId entityId, ProjectEventSource projectEventSource)
		{
			return new ValidationWorkItem
			{
				Source = projectEventSource,
				InstanceID = OdinEntityId.Internal.ToInt32(entityId),
				EntityId = entityId,
				ResultCountEstimate = 1u
			};
		}

		[Obsolete("Use CreateForEntityId instead.", false)]
		public static ValidationWorkItem CreateForInstanceId(int instanceId, ProjectEventSource projectEventSource)
		{
			return CreateForEntityId(OdinEntityId.Internal.FromInstanceId(instanceId), projectEventSource);
		}

		public bool IsValid()
		{
			if (NonUnityObjectValue == null && string.IsNullOrEmpty(AssetGuid) && !EntityId.IsValid && !SceneContent.HasValue)
			{
				return SceneValidators.HasValue;
			}
			return true;
		}

		public static bool operator ==(ValidationWorkItem x, ValidationWorkItem y)
		{
			if (x.NonUnityObjectValue != null || y.NonUnityObjectValue != null)
			{
				if (x.NonUnityObjectValue == null || y.NonUnityObjectValue == null)
				{
					return false;
				}
				if (x.NonUnityObjectValue != y.NonUnityObjectValue)
				{
					return x.NonUnityObjectValue.Equals(y.NonUnityObjectValue);
				}
				return true;
			}
			if (x.AssetGuid != null || y.AssetGuid != null)
			{
				return x.AssetGuid == y.AssetGuid;
			}
			if (x.SceneValidators.HasValue || y.SceneValidators.HasValue)
			{
				SceneReference? sceneValidators = x.SceneValidators;
				SceneReference? sceneValidators2 = y.SceneValidators;
				if (sceneValidators.HasValue != sceneValidators2.HasValue)
				{
					return false;
				}
				if (!sceneValidators.HasValue)
				{
					return true;
				}
				return sceneValidators.GetValueOrDefault() == sceneValidators2.GetValueOrDefault();
			}
			if (x.SceneContent.HasValue || y.SceneContent.HasValue)
			{
				SceneReference? sceneValidators2 = x.SceneContent;
				SceneReference? sceneValidators = y.SceneContent;
				if (sceneValidators2.HasValue != sceneValidators.HasValue)
				{
					return false;
				}
				if (!sceneValidators2.HasValue)
				{
					return true;
				}
				return sceneValidators2.GetValueOrDefault() == sceneValidators.GetValueOrDefault();
			}
			if (x.GlobalValidator != null || y.GlobalValidator != null)
			{
				return x.GlobalValidator == y.GlobalValidator;
			}
			return x.EntityId == y.EntityId;
		}

		public static bool operator !=(ValidationWorkItem x, ValidationWorkItem y)
		{
			return !(x == y);
		}

		public override int GetHashCode()
		{
			if (NonUnityObjectValue != null)
			{
				return NonUnityObjectValue.GetHashCode();
			}
			if (AssetGuid != null)
			{
				return AssetGuid.GetHashCode();
			}
			if (EntityId.IsValid)
			{
				return EntityId.GetHashCode();
			}
			if (SceneValidators.HasValue)
			{
				return (SceneValidators.Value.GUID ?? "").GetHashCode();
			}
			if (SceneContent.HasValue)
			{
				return (SceneContent.Value.GUID ?? "").GetHashCode();
			}
			if (GlobalValidator != null)
			{
				return GlobalValidator.GetHashCode();
			}
			return 0;
		}

		public override bool Equals(object obj)
		{
			if (obj is ValidationWorkItem)
			{
				return this == (ValidationWorkItem)obj;
			}
			return false;
		}

		bool IEquatable<ValidationWorkItem>.Equals(ValidationWorkItem other)
		{
			return this == other;
		}

		public string ToNiceString()
		{
			StringBuilder sb = new StringBuilder();
			UnityEngine.Object uObj = null;
			if (EntityId.IsValid)
			{
				uObj = EntityId.ToObject();
				if ((bool)uObj)
				{
					sb.Append("Type name: " + uObj.name + ", ");
					sb.Append("Object name: " + GetNiceBaseUnityObjectTypeName(uObj) + ", ");
				}
				sb.Append($"Entity Id: {EntityId}, ");
			}
			if (!string.IsNullOrEmpty(AssetGuid))
			{
				sb.Append("Asset guid: " + AssetGuid + ", ");
				string assetPath = AssetDatabase.GUIDToAssetPath(AssetGuid);
				sb.Append("Asset path: " + assetPath + ", ");
				if (!uObj)
				{
					uObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
					if ((bool)uObj)
					{
						sb.Append("Type name: " + uObj.name + ", ");
						sb.Append("Object name: " + GetNiceBaseUnityObjectTypeName(uObj) + ", ");
					}
				}
			}
			if (SceneValidators.HasValue)
			{
				sb.Append("Scene Validators Name: " + SceneValidators.Value.Name + ", ");
			}
			if (SceneContent.HasValue)
			{
				sb.Append("Scene Validators Name: " + SceneContent.Value.Name + ", ");
			}
			return sb.ToString();
		}

		private static string GetNiceBaseUnityObjectTypeName(UnityEngine.Object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			bool isAsset = AssetDatabase.Contains(obj);
			Type unityType = obj.GetType();
			if (isAsset)
			{
				if (unityType == typeof(GameObject))
				{
					return "Prefab";
				}
				if (typeof(Component).IsAssignableFrom(unityType))
				{
					return "Prefab Component";
				}
				if (typeof(ScriptableObject).IsAssignableFrom(unityType))
				{
					return "Scriptable Object";
				}
			}
			else
			{
				if (unityType == typeof(GameObject))
				{
					return "GameObject";
				}
				if (typeof(Component).IsAssignableFrom(unityType))
				{
					return "Component";
				}
			}
			return unityType.GetNiceName();
		}
	}
}

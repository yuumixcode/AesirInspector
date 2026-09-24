using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sirenix.OdinValidator.Editor
{
	[Serializable]
	public class ObjectAddress : IEquatable<ObjectAddress>
	{
		[Serializable]
		public struct SubAssetAddress : IEquatable<SubAssetAddress>
		{
			public UnitySerializableType AssetType;

			[Obsolete("Use EntityId instead.")]
			public int InstanceID;

			public OdinEntityId EntityId;

			public int Index;

			public SubAssetAddress(Type assetType, OdinEntityId entityId, int index)
			{
				AssetType = assetType;
				InstanceID = OdinEntityId.Internal.ToInt32(entityId);
				EntityId = entityId;
				Index = index;
			}

			[Obsolete("Use SubAssetAddress(Type, OdinEntityId, int) instead.", false)]
			public SubAssetAddress(Type assetType, int instanceID, int index)
				: this(assetType, OdinEntityId.Internal.FromInstanceId(instanceID), index)
			{
			}

			public override bool Equals(object obj)
			{
				if (obj is SubAssetAddress)
				{
					return this == (SubAssetAddress)obj;
				}
				return false;
			}

			public bool Equals(SubAssetAddress other)
			{
				return this == other;
			}

			public override int GetHashCode()
			{
				return AssetType.GetHashCode() ^ EntityId.GetHashCode();
			}

			public static bool operator ==(SubAssetAddress a, SubAssetAddress b)
			{
				if ((object)a == (object)b)
				{
					return true;
				}
				if ((object)a == null != ((object)b == null))
				{
					return false;
				}
				if (a.EntityId == b.EntityId)
				{
					return FastTypeComparer.Instance.Equals(a.AssetType, b.AssetType);
				}
				return false;
			}

			public static bool operator !=(SubAssetAddress a, SubAssetAddress b)
			{
				return !(a == b);
			}

			public bool HasData()
			{
				if (EntityId.IsValid)
				{
					return (object)AssetType.Type != null;
				}
				return false;
			}
		}

		[Serializable]
		public struct HierarchyAddress : IEquatable<HierarchyAddress>
		{
			public string[] SubNames;

			public int[] SubIndices;

			public HierarchyAddress(string[] subNames, int[] subIndices)
			{
				if (subNames.Length != subIndices.Length)
				{
					throw new Exception("Length of subNames and subIndices must be the same!");
				}
				SubNames = subNames;
				SubIndices = subIndices;
			}

			public override int GetHashCode()
			{
				int hash = 2998421;
				if (SubNames != null)
				{
					for (int i = 0; i < SubNames.Length; i++)
					{
						hash ^= SubNames[i].GetHashCode();
						hash ^= SubIndices[i].GetHashCode();
					}
				}
				return hash;
			}

			public override bool Equals(object obj)
			{
				if (obj is HierarchyAddress)
				{
					return this == (HierarchyAddress)obj;
				}
				return false;
			}

			public bool Equals(HierarchyAddress other)
			{
				return this == other;
			}

			public static bool operator ==(HierarchyAddress a, HierarchyAddress b)
			{
				if (a.SubNames == b.SubNames)
				{
					return true;
				}
				if (a.SubNames == null != (b.SubNames == null))
				{
					return false;
				}
				if (a.SubNames.Length != b.SubNames.Length || a.SubIndices.Length != b.SubIndices.Length)
				{
					return false;
				}
				for (int i = 0; i < a.SubNames.Length; i++)
				{
					if (a.SubNames[i] != b.SubNames[i])
					{
						return false;
					}
				}
				for (int j = 0; j < a.SubIndices.Length; j++)
				{
					if (a.SubIndices[j] != b.SubIndices[j])
					{
						return false;
					}
				}
				return true;
			}

			public static bool operator !=(HierarchyAddress a, HierarchyAddress b)
			{
				return !(a == b);
			}

			public override string ToString()
			{
				if (SubNames == null || SubNames.Length == 0)
				{
					return string.Empty;
				}
				return string.Join("/", SubNames);
			}

			public bool HasData()
			{
				if (SubNames != null && SubNames.Length != 0 && SubIndices != null)
				{
					return SubIndices.Length != 0;
				}
				return false;
			}

			internal bool TryFindGameObject(Transform[] currentTransforms, out GameObject go, out Transform closestObject, out string errorMessage)
			{
				closestObject = null;
				if (SubNames == null || SubIndices == null)
				{
					errorMessage = "Invalid HierarchyAddress";
					go = null;
					return false;
				}
				int length = SubNames.Length;
				for (int i = 0; i < length; i++)
				{
					int index = SubIndices[i];
					string name = SubNames[i];
					Transform candidate = null;
					Transform next = null;
					if (index >= 0 && index < currentTransforms.Length && currentTransforms[index].name == name)
					{
						next = currentTransforms[index];
						if (i == length - 1)
						{
							closestObject = next;
							go = next.gameObject;
							errorMessage = null;
							return true;
						}
					}
					else
					{
						foreach (Transform transform in currentTransforms)
						{
							if (transform.name == name)
							{
								candidate = transform;
								break;
							}
						}
						if (!candidate)
						{
							errorMessage = "Could not find GameObject at path '" + string.Join("/", SubNames) + "'";
							go = null;
							return false;
						}
						next = candidate;
						if (i == length - 1)
						{
							go = next.gameObject;
							closestObject = next;
							errorMessage = null;
							return true;
						}
					}
					closestObject = next;
					int childCount = next.childCount;
					currentTransforms = new Transform[childCount];
					for (int n = 0; n < childCount; n++)
					{
						currentTransforms[n] = next.GetChild(n);
					}
				}
				go = null;
				errorMessage = "Could not find GameObject at path '" + string.Join("/", SubNames) + "'";
				return false;
			}

			public static HierarchyAddress GetHierarchyAddress(GameObject go)
			{
				Stack<int> indices = new Stack<int>();
				Stack<string> names = new Stack<string>();
				Transform transform = go.transform;
				if (go.scene.IsValid())
				{
					do
					{
						indices.Push(transform.GetSiblingIndex());
						names.Push(transform.name);
						transform = transform.parent;
					}
					while (transform != null);
				}
				else
				{
					while (transform.parent != null)
					{
						indices.Push(transform.GetSiblingIndex());
						names.Push(transform.name);
						transform = transform.parent;
					}
				}
				return new HierarchyAddress(names.ToArray(), indices.ToArray());
			}
		}

		[Serializable]
		public struct ComponentAddress : IEquatable<ComponentAddress>
		{
			public int ComponentIndex;

			public UnitySerializableType ComponentType;

			public ComponentAddress(int componentIndex, Type componentType)
			{
				ComponentIndex = componentIndex;
				ComponentType = componentType;
			}

			public override bool Equals(object obj)
			{
				if (obj is ComponentAddress)
				{
					return this == (ComponentAddress)obj;
				}
				return false;
			}

			public bool Equals(ComponentAddress other)
			{
				return this == other;
			}

			public override int GetHashCode()
			{
				return ComponentType.GetHashCode() ^ ComponentIndex;
			}

			public static bool operator ==(ComponentAddress a, ComponentAddress b)
			{
				if ((object)a == (object)b)
				{
					return true;
				}
				if ((object)a == null != ((object)b == null))
				{
					return false;
				}
				if (a.ComponentIndex == b.ComponentIndex)
				{
					return FastTypeComparer.Instance.Equals(a.ComponentType, b.ComponentType);
				}
				return false;
			}

			public static bool operator !=(ComponentAddress a, ComponentAddress b)
			{
				return !(a == b);
			}

			public bool HasData()
			{
				if (ComponentIndex != 0)
				{
					return (object)ComponentType.Type != null;
				}
				return false;
			}
		}

		public enum AddressType
		{
			Unknown,
			Asset,
			PrefabGameObject,
			PrefabComponent,
			SceneGameObject,
			SceneComponent
		}

		public static readonly ObjectAddress Unknown = new ObjectAddress
		{
			Type = AddressType.Unknown
		};

		[SerializeField]
		private string guid;

		public bool IsBroken;

		public AddressType Type;

		public string AssetPath;

		public SubAssetAddress SubAsset;

		public HierarchyAddress Hierarchy;

		public ComponentAddress Component;

		public string Name;

		public UnitySerializableType ObjectType;

		public string AssetGUID
		{
			get
			{
				return guid;
			}
			set
			{
				if (value == null || value.Length == 32)
				{
					guid = value;
					return;
				}
				throw new Exception("Invalid asset guid: '" + value + "' for ObjectAddress with path: '" + AssetPath + "'");
			}
		}

		public bool IsSceneAsset()
		{
			if (Type == AddressType.Asset && AssetPath.FastEndsWith(".unity"))
			{
				return true;
			}
			return false;
		}

		private ObjectAddress()
		{
		}

		public static bool TryCreateObjectAddress(UnityEngine.Object obj, out ObjectAddress finalResult, out string errorMessage)
		{
			if (obj == null)
			{
				if ((object)obj != null)
				{
					OdinEntityId entityId = OdinEntityId.FromObject(obj);
					if (entityId.IsInAssetDatabase())
					{
						string path = entityId.GetAssetPath();
						if (!string.IsNullOrEmpty(path))
						{
							finalResult = new ObjectAddress
							{
								ObjectType = obj.GetType(),
								IsBroken = true,
								AssetPath = path,
								AssetGUID = AssetDatabase.AssetPathToGUID(path),
								Name = path.GetObjectNameFromAssetPath(),
								Type = AddressType.Asset
							};
							errorMessage = null;
							return true;
						}
						finalResult = null;
						OdinEntityId odinEntityId = entityId;
						errorMessage = "No asset path was found for the asset with Entity Id: " + odinEntityId.ToString();
						return false;
					}
					if (obj is Component || obj is GameObject)
					{
						finalResult = null;
						errorMessage = "Cannot create ObjectAddress for unsaved gameobjects.";
						return false;
					}
					finalResult = null;
					errorMessage = "Cannot create address for non-asset " + obj.GetType().GetNiceName() + " instance.";
					return false;
				}
				finalResult = null;
				errorMessage = "Cannot create ObjectAddress for null object.";
				return false;
			}
			GameObject go = null;
			Component cmp = null;
			if (obj is GameObject)
			{
				go = obj as GameObject;
			}
			if (obj is Component)
			{
				cmp = obj as Component;
				go = cmp.gameObject;
			}
			if (!go)
			{
				if (!AssetDatabase.Contains(obj))
				{
					finalResult = null;
					errorMessage = "Cannot create address for non-asset " + obj.GetType().GetNiceName() + " instance.";
					return false;
				}
				string path2 = AssetDatabase.GetAssetPath(obj);
				string guid = AssetDatabase.AssetPathToGUID(path2);
				Type type = obj.GetType();
				bool isMainAsset = AssetDatabase.IsMainAsset(obj);
				finalResult = new ObjectAddress
				{
					ObjectType = obj.GetType(),
					Name = obj.name,
					AssetPath = path2,
					AssetGUID = guid,
					Type = AddressType.Asset,
					SubAsset = (isMainAsset ? default(SubAssetAddress) : new SubAssetAddress(type, OdinEntityId.FromObject(obj), -1))
				};
				errorMessage = null;
				return true;
			}
			bool isPrefab = AssetDatabase.Contains(go);
			if (!isPrefab && (!go.scene.IsValid() || string.IsNullOrEmpty(go.scene.path)))
			{
				errorMessage = "Cannot create address for GameObject in unsaved scene or prefab stage.";
				finalResult = null;
				return false;
			}
			string path3 = (isPrefab ? AssetDatabase.GetAssetPath(obj) : go.scene.path);
			string guid2 = AssetDatabase.AssetPathToGUID(path3);
			HierarchyAddress hierarchy = HierarchyAddress.GetHierarchyAddress(go);
			if (cmp != null)
			{
				Component[] components = go.GetComponents(cmp.GetType());
				int componentIndex = -1;
				for (int i = 0; i < components.Length; i++)
				{
					if (components[i] == cmp)
					{
						componentIndex = i;
						break;
					}
				}
				if (componentIndex < 0)
				{
					errorMessage = "Component was not in its own GameObject?!";
					finalResult = null;
					return false;
				}
				finalResult = new ObjectAddress
				{
					ObjectType = cmp.GetType(),
					AssetGUID = guid2,
					Name = cmp.gameObject.name,
					AssetPath = path3,
					Type = (isPrefab ? AddressType.PrefabComponent : AddressType.SceneComponent),
					Hierarchy = hierarchy,
					Component = new ComponentAddress(componentIndex, cmp.GetType())
				};
				errorMessage = null;
				return true;
			}
			finalResult = new ObjectAddress
			{
				ObjectType = obj.GetType(),
				Name = go.name,
				AssetGUID = guid2,
				AssetPath = path3,
				Type = (isPrefab ? AddressType.PrefabGameObject : AddressType.SceneGameObject),
				Hierarchy = hierarchy
			};
			errorMessage = null;
			return true;
		}

		public ObjectAddress(SceneReference validatedScene)
		{
			Type = AddressType.Asset;
			AssetGUID = validatedScene.GUID;
			AssetPath = validatedScene.Path;
			ObjectType = typeof(SceneAsset);
			Name = validatedScene.Name;
		}

		public ObjectAddress(AddressType type, string assetGUID, string assetPath, SubAssetAddress subAsset, HierarchyAddress hierarchyAddress, ComponentAddress component, string niceObjectName, Type objectType, bool isBroken)
		{
			Type = type;
			AssetGUID = assetGUID;
			AssetPath = assetPath;
			SubAsset = subAsset;
			Hierarchy = hierarchyAddress;
			Component = component;
			IsBroken = isBroken;
			ObjectType = objectType;
			Name = niceObjectName;
		}

		public static ObjectAddress Parse(string json)
		{
			StringSlice slice = json.Slice().Trim();
			if (slice == "Unknown")
			{
				return Unknown;
			}
			if (slice == "Broken" || slice == "Invalid")
			{
				return new ObjectAddress
				{
					IsBroken = true
				};
			}
			return JsonUtility.FromJson<ObjectAddress>(json);
		}

		public string ToString(bool prettyPrint = false)
		{
			if (Type == AddressType.Unknown)
			{
				return "Unknown";
			}
			if (string.IsNullOrWhiteSpace(AssetPath) && string.IsNullOrWhiteSpace(AssetGUID))
			{
				return "Invalid";
			}
			return JsonUtility.ToJson(this, prettyPrint);
		}

		public override string ToString()
		{
			return ToString();
		}

		public bool TryGetObjectReference(bool openSceneIfNeeded, bool autoSaveIfOpenScene, out UnityEngine.Object result, out string errorMessage)
		{
			UnityEngine.Object closestObject;
			return TryGetObjectReference(openSceneIfNeeded, autoSaveIfOpenScene, out result, out errorMessage, out closestObject);
		}

		public bool TryGetObjectReference(bool openSceneIfNeeded, bool autoSaveIfOpenScene, out UnityEngine.Object result, out string errorMessage, out UnityEngine.Object closestObject)
		{
			bool isAsset = AssetPath != null || AssetGUID != null;
			closestObject = null;
			if (!isAsset)
			{
				result = null;
				errorMessage = "Invalid asset address. Guid: " + AssetGUID + ", Path: " + AssetPath;
				closestObject = null;
				return false;
			}
			result = null;
			string assetPath = AssetPath;
			string assetGuid = AssetGUID;
			if (assetGuid == null)
			{
				assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
			}
			if (assetPath == null)
			{
				assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
			}
			UnityEngine.Object asset;
			if (SubAsset.HasData())
			{
				UnityEngine.Object[] assets;
				using (AssetLoadTimings.Time(assetPath))
				{
					assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
				}
				if (assets.Length != 0)
				{
					closestObject = assets[0];
				}
				if (assetGuid == "0000000000000000e000000000000000" || assetGuid == "0000000000000000f000000000000000")
				{
					UnityEngine.Object[] array = assets;
					int num = 0;
					while (num < array.Length)
					{
						asset = array[num];
						string name = asset.name;
						Type type = asset.GetType();
						if (!(name == Name) || (object)type != SubAsset.AssetType.Type)
						{
							num++;
							continue;
						}
						goto IL_00ff;
					}
				}
				UnityEngine.Object[] array2 = assets;
				int num2 = 0;
				while (true)
				{
					if (num2 < array2.Length)
					{
						UnityEngine.Object asset2 = array2[num2];
						OdinEntityId entityId = OdinEntityId.FromObject(asset2);
						if (entityId == SubAsset.EntityId)
						{
							result = asset2;
							break;
						}
						num2++;
						continue;
					}
					UnityEngine.Object[] array3 = assets;
					int num3 = 0;
					while (true)
					{
						if (num3 < array3.Length)
						{
							UnityEngine.Object asset3 = array3[num3];
							string name2 = asset3.name;
							Type type2 = asset3.GetType();
							if (name2 == Name && (object)type2 == SubAsset.AssetType.Type)
							{
								result = asset3;
								break;
							}
							num3++;
							continue;
						}
						result = SubAsset.EntityId.ToObject();
						break;
					}
					break;
				}
				goto IL_01c0;
			}
			bool isScene;
			GameObject foundGameobject;
			if (Hierarchy.HasData())
			{
				foundGameobject = null;
				isScene = false;
				Transform[] rootTransforms;
				if (assetPath.FastEndsWith(".unity"))
				{
					isScene = true;
					if (Hierarchy.SubNames.Length == 0)
					{
						errorMessage = "No scene hierarchy root was specified in address.";
						return false;
					}
					SceneReference sceneRef = new SceneReference(assetGuid);
					Scene scene;
					if (!sceneRef.IsLoaded)
					{
						if (!openSceneIfNeeded)
						{
							errorMessage = "Needed to open scene to find object, but the argument 'openSceneIfNeeded' is false.";
							return false;
						}
						if (autoSaveIfOpenScene)
						{
							EditorSceneManager.SaveOpenScenes();
						}
						else if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
						{
							errorMessage = "User cancelled scene load.";
							return false;
						}
						bool sceneLoadResult;
						using (AssetLoadTimings.Time(assetPath))
						{
							sceneLoadResult = sceneRef.TryOpenScene(OpenSceneMode.Single, out scene);
						}
						if (!sceneLoadResult)
						{
							errorMessage = "Could not load scene at path '" + assetPath + "'.";
							return false;
						}
					}
					else if (!sceneRef.TryGetScene(out scene))
					{
						errorMessage = "Could not get scene at path '" + assetPath + "'.";
						return false;
					}
					rootTransforms = (from n in SceneUtilities.GetSceneRoots(scene)
						select n.transform).ToArray();
				}
				else
				{
					if (!assetPath.FastEndsWith(".prefab"))
					{
						errorMessage = "Address contained hierarchy info for asset '" + assetPath + "' which is neither a scene or a prefab.";
						return false;
					}
					UnityEngine.Object mainAsset;
					using (AssetLoadTimings.Time(assetPath))
					{
						mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
					}
					if (mainAsset == null)
					{
						errorMessage = "Asset at path '" + assetPath + "' was null when loaded.";
						return false;
					}
					closestObject = mainAsset;
					if (!(mainAsset is GameObject))
					{
						errorMessage = "Asset at path '" + assetPath + "' was not a valid prefab, despite being in a .prefab file.";
						return false;
					}
					GameObject go = mainAsset as GameObject;
					if (Hierarchy.SubNames.Length == 0)
					{
						foundGameobject = go;
						goto IL_0463;
					}
					Transform transform = go.transform;
					rootTransforms = new Transform[transform.childCount];
					for (int i = 0; i < transform.childCount; i++)
					{
						rootTransforms[i] = transform.GetChild(i);
					}
				}
				Transform[] currentTransforms = rootTransforms;
				if (Hierarchy.TryFindGameObject(currentTransforms, out var targetGo, out var closestTransform, out errorMessage))
				{
					closestObject = closestTransform;
					result = targetGo;
					foundGameobject = targetGo;
					goto IL_0463;
				}
				return false;
			}
			UnityEngine.Object asset4;
			using (AssetLoadTimings.Time(assetPath))
			{
				asset4 = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
			}
			if ((bool)asset4)
			{
				result = asset4;
				closestObject = result;
				errorMessage = null;
				return true;
			}
			result = asset4;
			errorMessage = "Broken asset reference";
			return false;
			IL_00ff:
			result = asset;
			goto IL_01c0;
			IL_01c0:
			if (result != null)
			{
				if (result.GetType() != SubAsset.AssetType)
				{
					errorMessage = $"Expected asset with Entity Id '{SubAsset.EntityId}' to be of type '{SubAsset.AssetType.GetNiceName()}', but it was of type '{result.GetType().GetNiceName()}'.";
					return false;
				}
				errorMessage = null;
				closestObject = result;
				return true;
			}
			errorMessage = $"Could not find asset with Entity Id '{SubAsset.EntityId}' at path '{assetPath}'";
			return false;
			IL_0463:
			closestObject = foundGameobject;
			_ = Component;
			if (Component.ComponentIndex >= 0 && !string.IsNullOrEmpty(Component.ComponentType.TypeName))
			{
				result = foundGameobject;
				Component[] components = foundGameobject.GetComponents(Component.ComponentType);
				if (Component.ComponentIndex < components.Length)
				{
					errorMessage = null;
					result = components[Component.ComponentIndex];
					return true;
				}
				errorMessage = $"Expected at least {Component.ComponentIndex + 1} components " + "of type '" + Component.ComponentType.GetNiceName() + "' on GameObject '" + string.Join("/", Hierarchy.SubNames) + "' in " + string.Format("{0} '{1}', but there were {2} components of that type.", isScene ? "scene" : "prefab", assetPath, components.Length);
				return false;
			}
			errorMessage = null;
			result = foundGameobject;
			return true;
		}

		public override bool Equals(object obj)
		{
			if (obj is ObjectAddress)
			{
				return this == (ObjectAddress)obj;
			}
			return false;
		}

		public bool Equals(ObjectAddress other)
		{
			return this == other;
		}

		public static bool operator ==(ObjectAddress a, ObjectAddress b)
		{
			if ((object)a == b)
			{
				return true;
			}
			if ((object)a == null != ((object)b == null))
			{
				return false;
			}
			if (a.Type != b.Type)
			{
				return false;
			}
			if (a.Type == AddressType.Unknown)
			{
				return true;
			}
			if (a.IsBroken != b.IsBroken)
			{
				return false;
			}
			string aGuid = a.AssetGUID ?? AssetDatabase.AssetPathToGUID(a.AssetPath);
			string bGuid = b.AssetGUID ?? AssetDatabase.AssetPathToGUID(b.AssetPath);
			if (aGuid != bGuid)
			{
				return false;
			}
			switch (a.Type)
			{
			case AddressType.Asset:
				if (a.SubAsset != b.SubAsset)
				{
					return false;
				}
				break;
			case AddressType.PrefabGameObject:
			case AddressType.SceneGameObject:
				if (a.Hierarchy != b.Hierarchy)
				{
					return false;
				}
				break;
			case AddressType.PrefabComponent:
			case AddressType.SceneComponent:
				if (a.Hierarchy != b.Hierarchy || a.Component != b.Component)
				{
					return false;
				}
				break;
			default:
				throw new NotImplementedException(a.Type.ToString());
			}
			return true;
		}

		public static bool operator !=(ObjectAddress a, ObjectAddress b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			if (Type == AddressType.Unknown)
			{
				return 0;
			}
			int hash = 2998421;
			string guid = AssetGUID ?? AssetDatabase.AssetPathToGUID(AssetPath);
			hash = guid.GetHashCode() * 17;
			if (IsBroken)
			{
				hash *= 49339;
			}
			switch (Type)
			{
			case AddressType.Asset:
				_ = SubAsset;
				return hash ^ (SubAsset.GetHashCode() * 19);
			case AddressType.PrefabGameObject:
			case AddressType.SceneGameObject:
				return hash ^ (Hierarchy.GetHashCode() * 19);
			case AddressType.PrefabComponent:
			case AddressType.SceneComponent:
				hash ^= Hierarchy.GetHashCode() * 19;
				return hash ^ (Component.GetHashCode() * 23);
			default:
				throw new NotImplementedException(Type.ToString());
			}
		}
	}
}

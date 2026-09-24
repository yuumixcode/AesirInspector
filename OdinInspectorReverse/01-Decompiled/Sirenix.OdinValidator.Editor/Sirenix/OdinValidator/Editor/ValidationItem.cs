using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	[Serializable]
	public struct ValidationItem : IEquatable<ValidationItem>
	{
		[Serializable]
		public struct AssetToValidate : IEquatable<AssetToValidate>
		{
			public string Path;

			public string Filter;

			public string FileOrFolderName
			{
				get
				{
					if (string.IsNullOrEmpty(Path))
					{
						return "Invalid path";
					}
					string subPath = Path.TrimEnd(new char[1] { '/' });
					return subPath.Substring(subPath.LastIndexOf('/') + 1);
				}
			}

			public bool IsFile
			{
				get
				{
					if (!string.IsNullOrEmpty(Path))
					{
						return Path.LastIndexOf('.') > Path.LastIndexOf('/');
					}
					return false;
				}
			}

			public bool IsDirectory
			{
				get
				{
					if (!string.IsNullOrEmpty(Path))
					{
						return Path.LastIndexOf('/') > Path.LastIndexOf('.');
					}
					return false;
				}
			}

			public bool IsValid => !string.IsNullOrEmpty(Path);

			public override int GetHashCode()
			{
				return (Path ?? "").GetHashCode();
			}

			public static bool operator ==(AssetToValidate left, AssetToValidate right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(AssetToValidate left, AssetToValidate right)
			{
				return !(left == right);
			}

			public override bool Equals(object obj)
			{
				if (obj is AssetToValidate validate)
				{
					return Equals(validate);
				}
				return false;
			}

			public bool Equals(AssetToValidate other)
			{
				return (Path ?? "") == (other.Path ?? "");
			}
		}

		[Serializable]
		public struct SceneToValidate : IEquatable<SceneToValidate>
		{
			public SceneIncludeType Type;

			public string Value;

			public bool IncludeAssetDependencies;

			public override int GetHashCode()
			{
				return Type.GetHashCode() + (Value ?? "").GetHashCode();
			}

			public static bool operator ==(SceneToValidate left, SceneToValidate right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(SceneToValidate left, SceneToValidate right)
			{
				return !(left == right);
			}

			public override bool Equals(object obj)
			{
				if (obj is SceneToValidate validate)
				{
					return Equals(validate);
				}
				return false;
			}

			public bool Equals(SceneToValidate other)
			{
				if (Type == other.Type)
				{
					return (Value ?? "") == (other.Value ?? "");
				}
				return false;
			}
		}

		public enum ValidationItemType
		{
			Asset,
			Object,
			Scene,
			AssetBundle,
			AddressableGroup
		}

		public enum SceneIncludeType
		{
			ScenesInFolder,
			ScenesInBuildOptions,
			OpenScenes,
			SceneGuid
		}

		public bool Enabled;

		public ValidationItemType Type;

		public AssetToValidate Asset;

		public UnityEngine.Object Object;

		public SceneToValidate Scene;

		public string AssetBundle;

		public string AddressableGroup;

		public static ValidationItem FromSceneGuid(string guid, bool includeAssetDeps = false)
		{
			return new ValidationItem
			{
				Type = ValidationItemType.Scene,
				Enabled = true,
				Scene = new SceneToValidate
				{
					Type = SceneIncludeType.SceneGuid,
					Value = guid,
					IncludeAssetDependencies = includeAssetDeps
				}
			};
		}

		public static ValidationItem FromSceneFolderPath(string path, bool includeAssetDeps = false)
		{
			return new ValidationItem
			{
				Type = ValidationItemType.Scene,
				Enabled = true,
				Scene = new SceneToValidate
				{
					Type = SceneIncludeType.ScenesInFolder,
					Value = path,
					IncludeAssetDependencies = includeAssetDeps
				}
			};
		}

		public static ValidationItem FromScenePath(string path, bool includeAssetDeps = false)
		{
			if (path.EndsWith(".unity"))
			{
				return new ValidationItem
				{
					Type = ValidationItemType.Scene,
					Enabled = true,
					Scene = new SceneToValidate
					{
						Type = SceneIncludeType.SceneGuid,
						IncludeAssetDependencies = includeAssetDeps,
						Value = AssetDatabase.AssetPathToGUID(path)
					}
				};
			}
			return new ValidationItem
			{
				Type = ValidationItemType.Scene,
				Enabled = true,
				Scene = new SceneToValidate
				{
					Type = SceneIncludeType.ScenesInFolder,
					Value = path,
					IncludeAssetDependencies = includeAssetDeps
				}
			};
		}

		public static ValidationItem FromOpenScenes(bool includeAssetDeps = false)
		{
			return new ValidationItem
			{
				Type = ValidationItemType.Scene,
				Enabled = true,
				Scene = new SceneToValidate
				{
					Type = SceneIncludeType.OpenScenes,
					IncludeAssetDependencies = includeAssetDeps
				}
			};
		}

		public static ValidationItem FromScenesInBuildOptions(bool includeAssetDeps = false)
		{
			return new ValidationItem
			{
				Type = ValidationItemType.Scene,
				Enabled = true,
				Scene = new SceneToValidate
				{
					Type = SceneIncludeType.ScenesInBuildOptions,
					IncludeAssetDependencies = includeAssetDeps
				}
			};
		}

		public static bool TryMakeAssetItemFromUnityObjectReference(UnityEngine.Object obj, out ValidationItem item)
		{
			if (AssetDatabase.Contains(obj))
			{
				item = FromAssetPath(AssetDatabase.GetAssetPath(obj));
				return true;
			}
			item = default(ValidationItem);
			return false;
		}

		public static ValidationItem FromAssetPath(string path)
		{
			return new ValidationItem
			{
				Type = ValidationItemType.Asset,
				Enabled = true,
				Asset = new AssetToValidate
				{
					Path = path
				}
			};
		}

		public static ValidationItem FromAssetPath(string path, string filter = null)
		{
			return new ValidationItem
			{
				Type = ValidationItemType.Asset,
				Enabled = true,
				Asset = new AssetToValidate
				{
					Path = path,
					Filter = filter
				}
			};
		}

		public static ValidationItem FromAssetBundle(string assetBundle)
		{
			return new ValidationItem
			{
				Type = ValidationItemType.AssetBundle,
				Enabled = true,
				AssetBundle = assetBundle
			};
		}

		public static ValidationItem FromAddressableGroup(string addressableGroup)
		{
			return new ValidationItem
			{
				Type = ValidationItemType.AddressableGroup,
				Enabled = true,
				AddressableGroup = addressableGroup
			};
		}

		public static ValidationItem[] FromSelection(IEnumerable<UnityEngine.Object> objects, bool includeSceneDependencies)
		{
			List<ValidationItem> include = new List<ValidationItem>();
			HashSet<string> paths = new HashSet<string>();
			foreach (UnityEngine.Object obj in objects)
			{
				if (!obj || !AssetDatabase.Contains(obj))
				{
					continue;
				}
				string path = AssetDatabase.GetAssetPath(obj);
				if (!paths.Add(path))
				{
					continue;
				}
				if (obj is SceneAsset && File.Exists(path))
				{
					include.Add(FromScenePath(path, includeSceneDependencies));
				}
				else if (obj is DefaultAsset && Directory.Exists(path))
				{
					string[] scenes = AssetDatabase.FindAssets("t:scene", new string[1] { path });
					string[] assets = AssetDatabase.FindAssets("", new string[1] { path });
					string[] subFolders = AssetDatabase.GetSubFolders(path);
					if (scenes.Length != 0)
					{
						include.Add(FromSceneFolderPath(path, includeSceneDependencies));
					}
					if (assets.Length - subFolders.Length > scenes.Length)
					{
						include.Add(FromAssetPath(path));
					}
				}
				else
				{
					include.Add(FromAssetPath(path));
				}
			}
			return include.ToArray();
		}

		public override int GetHashCode()
		{
			switch (Type)
			{
			case ValidationItemType.Asset:
				return Asset.GetHashCode();
			case ValidationItemType.Object:
				if (!(Object == null))
				{
					return Object.GetHashCode();
				}
				return 14;
			case ValidationItemType.Scene:
				return Scene.GetHashCode();
			case ValidationItemType.AssetBundle:
				return (AssetBundle ?? "").GetHashCode();
			case ValidationItemType.AddressableGroup:
				return (AddressableGroup ?? "").GetHashCode();
			default:
				return 13;
			}
		}

		public static bool operator ==(ValidationItem left, ValidationItem right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ValidationItem left, ValidationItem right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (obj is ValidationItem item)
			{
				return Equals(item);
			}
			return false;
		}

		public bool Equals(ValidationItem other)
		{
			if (Type != other.Type)
			{
				return false;
			}
			return Type switch
			{
				ValidationItemType.Asset => Asset == other.Asset, 
				ValidationItemType.Object => Object == other.Object, 
				ValidationItemType.Scene => Scene == other.Scene, 
				ValidationItemType.AssetBundle => (AssetBundle ?? "") == (AssetBundle ?? ""), 
				ValidationItemType.AddressableGroup => (AddressableGroup ?? "") == (AddressableGroup ?? ""), 
				_ => false, 
			};
		}

		public string GetShortItemName()
		{
			switch (Type)
			{
			case ValidationItemType.Asset:
				return Asset.FileOrFolderName;
			case ValidationItemType.Object:
				if (!Object)
				{
					return "null";
				}
				return Object.name;
			case ValidationItemType.Scene:
				if (Scene.Type == SceneIncludeType.OpenScenes)
				{
					return "Open Scenes";
				}
				if (Scene.Type == SceneIncludeType.SceneGuid)
				{
					return new SceneReference(Scene.Value).Name + ".unity";
				}
				if (Scene.Type == SceneIncludeType.ScenesInBuildOptions)
				{
					return "Scenes in build options";
				}
				if (Scene.Type == SceneIncludeType.ScenesInFolder)
				{
					return Scene.Value + "/*.unity";
				}
				throw new NotImplementedException(Scene.Type.ToString());
			case ValidationItemType.AssetBundle:
				return AssetBundle;
			case ValidationItemType.AddressableGroup:
				return AddressableGroup;
			default:
				return "Unknown";
			}
		}

		public override string ToString()
		{
			return Type switch
			{
				ValidationItemType.Asset => Asset.Path + " (" + Asset.Filter + ")", 
				ValidationItemType.Object => $"{Object}", 
				ValidationItemType.Scene => string.Format("{0} : {1} : {2}", Scene.Type, Scene.Value, Scene.IncludeAssetDependencies ? "With dependencies" : "Without dependencies"), 
				ValidationItemType.AssetBundle => AssetBundle ?? "", 
				ValidationItemType.AddressableGroup => AddressableGroup ?? "", 
				_ => "Unknown", 
			};
		}

		internal bool TryGetAssetGuid(out string guid)
		{
			guid = null;
			if (Type == ValidationItemType.Asset)
			{
				if (!string.IsNullOrEmpty(Asset.Path))
				{
					guid = AssetDatabase.AssetPathToGUID(Asset.Path);
					return true;
				}
			}
			else if (Type == ValidationItemType.Scene && !string.IsNullOrEmpty(Scene.Value))
			{
				guid = AssetDatabase.AssetPathToGUID(Scene.Value);
				return true;
			}
			return false;
		}
	}
}

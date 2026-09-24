using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public struct SceneReference : IEquatable<SceneReference>
	{
		public bool IsValid;

		public string GUID;

		public static readonly SceneReference Invalid;

		public string Name
		{
			get
			{
				if (!IsValid)
				{
					return "";
				}
				string path = Path;
				if (string.IsNullOrEmpty(path))
				{
					return "";
				}
				return System.IO.Path.GetFileNameWithoutExtension(path);
			}
		}

		public string Path
		{
			get
			{
				if (!IsValid)
				{
					return "";
				}
				return AssetDatabase.GUIDToAssetPath(GUID);
			}
		}

		public bool IsLoaded
		{
			get
			{
				if (!IsValid)
				{
					return false;
				}
				return SceneManager.GetSceneByPath(Path).isLoaded;
			}
		}

		public bool IsActive
		{
			get
			{
				if (!IsValid)
				{
					return false;
				}
				SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
				for (int i = 0; i < setup.Length; i++)
				{
					if (setup[i].isActive && setup[i].path == Path)
					{
						return true;
					}
				}
				return false;
			}
		}

		public SceneReference(Scene scene)
		{
			if (!scene.IsValid())
			{
				IsValid = false;
				GUID = null;
				return;
			}
			string guid = AssetDatabase.AssetPathToGUID(scene.path);
			if (string.IsNullOrEmpty(guid))
			{
				IsValid = false;
				GUID = null;
			}
			else
			{
				IsValid = true;
				GUID = guid;
			}
		}

		public SceneReference(string guid)
		{
			GUID = guid;
			IsValid = true;
		}

		public bool TryOpenScene(OpenSceneMode mode, out Scene scene)
		{
			if (!File.Exists(Path))
			{
				scene = default(Scene);
				return false;
			}
			try
			{
				scene = EditorSceneManager.OpenScene(Path, mode);
				return true;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				scene = default(Scene);
				return false;
			}
		}

		public bool IsInBuildSettings(bool mustBeEnabled = false)
		{
			EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
			foreach (EditorBuildSettingsScene scene in buildScenes)
			{
				if (scene.path == Path && (!mustBeEnabled || scene.enabled))
				{
					return true;
				}
			}
			return false;
		}

		public SceneAsset GetSceneAsset()
		{
			if (!IsValid)
			{
				return null;
			}
			return AssetDatabase.LoadMainAssetAtPath(Path) as SceneAsset;
		}

		public override bool Equals(object obj)
		{
			if (obj is SceneReference)
			{
				return Equals((SceneReference)obj);
			}
			return false;
		}

		public bool Equals(SceneReference other)
		{
			if (IsValid == other.IsValid)
			{
				return GUID == other.GUID;
			}
			return false;
		}

		public override int GetHashCode()
		{
			int hashCode = -552061963;
			hashCode = hashCode * -1521134295 + IsValid.GetHashCode();
			return hashCode * -1521134295 + (GUID ?? "").GetHashCode();
		}

		public static bool operator ==(SceneReference a, SceneReference b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(SceneReference a, SceneReference b)
		{
			return !(a == b);
		}

		public bool TryGetScene(out Scene scene)
		{
			if (!IsValid)
			{
				scene = default(Scene);
				return false;
			}
			scene = SceneManager.GetSceneByPath(Path);
			if (scene.IsValid())
			{
				return true;
			}
			if (GUID != null)
			{
				string path = AssetDatabase.GUIDToAssetPath(GUID);
				if (path != null)
				{
					scene = SceneManager.GetSceneByPath(path);
					if (scene.IsValid())
					{
						return true;
					}
				}
			}
			scene = default(Scene);
			return false;
		}

		public static SceneReference FromPath(string path)
		{
			string guid = AssetDatabase.AssetPathToGUID(path);
			if (string.IsNullOrEmpty(guid))
			{
				return new SceneReference
				{
					IsValid = false
				};
			}
			return new SceneReference(guid);
		}

		public static SceneReference FromAsset(SceneAsset asset)
		{
			if (asset == null)
			{
				return Invalid;
			}
			string path = AssetDatabase.GetAssetPath(asset);
			string guid = AssetDatabase.AssetPathToGUID(path);
			if (string.IsNullOrEmpty(guid))
			{
				return new SceneReference
				{
					IsValid = false
				};
			}
			return new SceneReference(guid);
		}
	}
}

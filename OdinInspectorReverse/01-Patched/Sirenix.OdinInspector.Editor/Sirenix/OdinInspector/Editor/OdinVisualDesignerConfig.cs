using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	[SirenixEditorConfig]
	public class OdinVisualDesignerConfig : GlobalConfig<OdinVisualDesignerConfig>, ISerializationCallbackReceiver
	{
		[Serializable]
		internal struct SerializedAttributeType
		{
			public string TypeBinding;

			public SerializedAttributeType(Type attributeType)
			{
				TypeBinding = TwoWaySerializationBinder.Default.BindToName(attributeType);
			}

			public static implicit operator Type(SerializedAttributeType serialized)
			{
				return TwoWaySerializationBinder.Default.BindToType(serialized.TypeBinding);
			}
		}

		public const string DEFAULT_SAVE_PATH = "Assets/Plugins/Sirenix/Odin Inspector/Visual Designer/Saved";

		private const int DefaultPopupWidth = 350;

		private const int DefaultPopupHeight = 500;

		[SerializeField]
		[HideInInspector]
		internal List<SerializedAttributeType> SerializedFavoriteAttributes;

		[HideInInspector]
		[SerializeField]
		private string savePath = "Assets/Plugins/Sirenix/Odin Inspector/Visual Designer/Saved";

		[SerializeField]
		[HideInInspector]
		private List<string> searchPathRoots = new List<string>();

		public const string EDITOR_PREF_AUTO_SAVE = "OdinVisualDesignerConfig_AutoSave";

		public const string EDITOR_PREF_AUTO_SAVE_INTERVAL = "OdinVisualDesignerConfig_AutoSaveInterval";

		public const string EDITOR_PREF_LOCK_NON_DECL_KEY = "OdinVisualDesignerConfig_LockNonDeclTypesByDefault";

		public const string EDITOR_PREF_POPUP_WIDTH = "OdinVisualDesignerConfig_PopupWidth";

		public const string EDITOR_PREF_POPUP_HEIGHT = "OdinVisualDesignerConfig_PopupHeight";

		[BoxGroup("Attribute Popup", true, false, 0f)]
		[ShowInInspector]
		[OnValueChanged("OnValueChanged", false)]
		public HashSet<Type> FavoriteAttributes = new HashSet<Type>();

		public const string EDITOR_PREF_LOG_LEVEL_TYPE_MISMATCHES = "OdinVisualDesignerConfig_LogLevelTypeMismatches";

		[BoxGroup("Saving", true, false, 0f)]
		[ShowInInspector]
		[FolderPath(AbsolutePath = false, RequireExistingPath = true)]
		[LabelText("Default Save Path")]
		[PropertyTooltip("New OVDF files and read-only overrides are saved here. This path is always searched last, so files here have the highest precedence.")]
		public string SavePath
		{
			get
			{
				if (!(savePath == "Plugins/Sirenix/Odin Inspector/Visual Designer/Saved"))
				{
					return savePath;
				}
				return "Assets/Plugins/Sirenix/Odin Inspector/Visual Designer/Saved";
			}
			set
			{
				savePath = value;
				OVDFFileWatcher.UpdateRootFolders();
			}
		}

		[PropertyTooltip("Additional folders to search for OVDF files. Later roots have higher precedence.")]
		[OnValueChanged("OnSearchPathRootsChanged", true)]
		[ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
		[InfoBox("OVDF files are resolved in this order: Search Path Roots from top to bottom, then Default Save Path last. Later entries override earlier entries for the same type.", InfoMessageType.Info, null)]
		[BoxGroup("Saving", true, false, 0f)]
		[FolderPath(AbsolutePath = true, RequireExistingPath = true)]
		[ShowInInspector]
		public List<string> SearchPathRoots
		{
			get
			{
				if (searchPathRoots == null)
				{
					searchPathRoots = new List<string>();
				}
				return searchPathRoots;
			}
			set
			{
				searchPathRoots = value ?? new List<string>();
				OnSearchPathRootsChanged();
			}
		}

		[ShowInInspector]
		[BoxGroup("Saving", true, false, 0f)]
		public bool AutoSave
		{
			get
			{
				return EditorPrefs.GetBool("OdinVisualDesignerConfig_AutoSave", defaultValue: false);
			}
			set
			{
				EditorPrefs.SetBool("OdinVisualDesignerConfig_AutoSave", value);
			}
		}

		[EnableIf("AutoSave")]
		[BoxGroup("Saving", true, false, 0f)]
		[MinValue(5.0)]
		[Unit(Units.Second)]
		[ShowInInspector]
		public int AutoSaveInterval
		{
			get
			{
				return EditorPrefs.GetInt("OdinVisualDesignerConfig_AutoSaveInterval", 30);
			}
			set
			{
				EditorPrefs.SetInt("OdinVisualDesignerConfig_AutoSaveInterval", value);
			}
		}

		[LabelText("Lock Inherited Members By Default")]
		[ShowInInspector]
		[PropertyOrder(-1f)]
		[BoxGroup("Editor", true, false, 0f)]
		[PropertyTooltip("Automatically locks members inherited from parent types when enabled.")]
		public bool LockNonDeclTypesByDefault
		{
			get
			{
				return EditorPrefs.GetBool("OdinVisualDesignerConfig_LockNonDeclTypesByDefault", defaultValue: true);
			}
			set
			{
				EditorPrefs.SetBool("OdinVisualDesignerConfig_LockNonDeclTypesByDefault", value);
			}
		}

		[BoxGroup("Attribute Popup", true, false, 0f)]
		[ShowInInspector]
		[LabelText("Width")]
		public int PopupWidth
		{
			get
			{
				return EditorPrefs.GetInt("OdinVisualDesignerConfig_PopupWidth", 350);
			}
			set
			{
				EditorPrefs.SetInt("OdinVisualDesignerConfig_PopupWidth", value);
			}
		}

		[LabelText("Height")]
		[BoxGroup("Attribute Popup", true, false, 0f)]
		[ShowInInspector]
		public int PopupHeight
		{
			get
			{
				return EditorPrefs.GetInt("OdinVisualDesignerConfig_PopupHeight", 500);
			}
			set
			{
				EditorPrefs.SetInt("OdinVisualDesignerConfig_PopupHeight", value);
			}
		}

		[LabelText("Type Mismatches")]
		[ShowInInspector]
		[PropertyTooltip("Determines whether the Visual Designer should log a message when it encounters a file with a type that does not exist.")]
		[BoxGroup("Log Levels", true, false, 0f)]
		[EnumToggleButtons]
		public OdinVisualDesignerLogLevel LogLevelTypeMismatches
		{
			get
			{
				return (OdinVisualDesignerLogLevel)EditorPrefs.GetInt("OdinVisualDesignerConfig_LogLevelTypeMismatches", 0);
			}
			set
			{
				EditorPrefs.SetInt("OdinVisualDesignerConfig_LogLevelTypeMismatches", (int)value);
			}
		}

		internal List<string> GetEffectiveSearchRoots()
		{
			List<string> result = new List<string>();
			AddSearchRoot(result, SearchPathRoots);
			AddSearchRoot(result, SavePath);
			return result;
		}

		private void AddSearchRoot(List<string> output, IEnumerable<string> roots)
		{
			if (roots == null)
			{
				return;
			}
			foreach (string root in roots)
			{
				AddSearchRoot(output, root);
			}
		}

		private void AddSearchRoot(List<string> output, string root)
		{
			if (string.IsNullOrWhiteSpace(root))
			{
				return;
			}
			string fullPath = PathUtils.GetCrossPlatformPath(Path.GetFullPath(root));
			for (int i = output.Count - 1; i >= 0; i--)
			{
				if (string.Equals(output[i], fullPath, StringComparison.OrdinalIgnoreCase))
				{
					output.RemoveAt(i);
				}
			}
			output.Add(fullPath);
		}

		private void OnSearchPathRootsChanged()
		{
			EditorUtility.SetDirty(this);
			OVDFFileWatcher.UpdateRootFolders();
		}

		public void OnBeforeSerialize()
		{
			if (SerializedFavoriteAttributes == null)
			{
				SerializedFavoriteAttributes = new List<SerializedAttributeType>(FavoriteAttributes.Count);
			}
			else if (SerializedFavoriteAttributes.Capacity < FavoriteAttributes.Count)
			{
				SerializedFavoriteAttributes.Capacity = DesignerUtils.Round8(FavoriteAttributes.Count);
			}
			SerializedFavoriteAttributes.Clear();
			foreach (Type attr in FavoriteAttributes)
			{
				SerializedFavoriteAttributes.Add(new SerializedAttributeType(attr));
			}
		}

		public void OnAfterDeserialize()
		{
			FavoriteAttributes.Clear();
			for (int i = 0; i < SerializedFavoriteAttributes.Count; i++)
			{
				FavoriteAttributes.Add(SerializedFavoriteAttributes[i]);
			}
		}

		public void OnValueChanged()
		{
			EditorUtility.SetDirty(this);
		}

		[OnInspectorGUI]
		[PropertyOrder(float.MaxValue)]
		public void FlexSpace()
		{
			GUILayout.FlexibleSpace();
		}

		[Button("       Reset Config       ", ButtonSizes.Large, Stretch = false)]
		[PropertyOrder(float.MaxValue)]
		public void ResetConfig()
		{
			if (EditorUtility.DisplayDialog("Odin Visual Designer Config", "Are you sure you want to reset the configuration?", "Yes", "No"))
			{
				Undo.RecordObject(this, "Configuration Reset");
				SavePath = "Assets/Plugins/Sirenix/Odin Inspector/Visual Designer/Saved";
				SearchPathRoots.Clear();
				AutoSave = false;
				AutoSaveInterval = 30;
				LockNonDeclTypesByDefault = true;
				PopupWidth = 350;
				PopupHeight = 500;
				FavoriteAttributes.Clear();
				LogLevelTypeMismatches = OdinVisualDesignerLogLevel.None;
				EditorUtility.SetDirty(this);
			}
		}

		/// <returns>The favorite state *after* toggling.</returns>
		public bool ToggleFavorite(Type attributeType)
		{
			bool isFavorite = FavoriteAttributes.Contains(attributeType);
			Undo.RecordObject(this, isFavorite ? ("'" + attributeType.Name + "' removed from Favorites.") : ("'" + attributeType.Name + "' added to Favorites."));
			if (isFavorite)
			{
				FavoriteAttributes.Remove(attributeType);
			}
			else
			{
				FavoriteAttributes.Add(attributeType);
			}
			EditorUtility.SetDirty(this);
			return !isFavorite;
		}
	}
}

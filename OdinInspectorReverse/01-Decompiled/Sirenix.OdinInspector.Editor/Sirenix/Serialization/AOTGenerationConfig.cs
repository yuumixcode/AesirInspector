using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization.Editor;
using Sirenix.Serialization.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities.Editor.Expressions;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Contains configuration for generating an assembly that provides increased AOT support in Odin.
	/// </summary>
	[SirenixEditorConfig]
	public class AOTGenerationConfig : GlobalConfig<AOTGenerationConfig>
	{
		[Serializable]
		private class TypeEntry : ISearchFilterable
		{
			[NonSerialized]
			public bool IsInitialized;

			[NonSerialized]
			public bool IsNew;

			[NonSerialized]
			public string NiceTypeName;

			public string TypeName;

			public bool IsCustom;

			public bool Emit;

			public Type Type;

			public bool IsMatch(string searchString)
			{
				return FuzzySearch.Contains(searchString, (Type == null) ? "null invalid" : Type.GetNiceFullName());
			}
		}

		private class TypeEntryDrawer : OdinValueDrawer<TypeEntry>
		{
			private static readonly GUIStyle MissingLabelStyle = new GUIStyle("sv_label_6")
			{
				margin = new RectOffset(3, 3, 2, 0),
				alignment = TextAnchor.MiddleCenter
			};

			private static readonly GUIStyle NewLabelStyle = new GUIStyle("sv_label_3")
			{
				margin = new RectOffset(3, 3, 2, 0),
				alignment = TextAnchor.MiddleCenter
			};

			private static readonly GUIStyle ChangedLabelStyle = new GUIStyle("sv_label_4")
			{
				margin = new RectOffset(3, 3, 2, 0),
				alignment = TextAnchor.MiddleCenter
			};

			private bool isEditing;

			protected override void DrawPropertyLayout(GUIContent label)
			{
				IPropertyValueEntry<TypeEntry> entry = base.ValueEntry;
				TypeEntry value = entry.SmartValue;
				bool valueChanged = false;
				Rect rect = EditorGUILayout.GetControlRect();
				Rect toggleRect = rect.SetWidth(20f);
				rect.xMin += 20f;
				if (value.Type == null)
				{
					isEditing = true;
				}
				bool wasEditing = isEditing;
				if (string.IsNullOrEmpty(value.NiceTypeName) && value.Type != null)
				{
					value.NiceTypeName = value.Type.GetNiceName();
				}
				GUIHelper.PushGUIEnabled(value.Type != null);
				valueChanged = value.Emit != (value.Emit = EditorGUI.Toggle(toggleRect, value.Emit));
				GUIHelper.PopGUIEnabled();
				rect.y += 2f;
				rect.width -= 30f;
				Rect textBoxRect = rect;
				if (value.IsNew || value.IsCustom || value.Type == null)
				{
					textBoxRect.xMax -= 78f;
				}
				Rect lblRect = rect;
				lblRect.xMin = lblRect.xMax - 75f;
				lblRect.width = 75f;
				if (value.Type == null)
				{
					EditorGUI.LabelField(lblRect, GUIHelper.TempContent("INVALID"), MissingLabelStyle);
				}
				else if (value.IsCustom)
				{
					EditorGUI.LabelField(lblRect, GUIHelper.TempContent("MODIFIED"), ChangedLabelStyle);
				}
				else if (value.IsNew)
				{
					EditorGUI.LabelField(lblRect, GUIHelper.TempContent("NEW"), NewLabelStyle);
				}
				else
				{
					EditorGUI.LabelField(lblRect, GUIHelper.TempContent(""));
				}
				string newName = value.TypeName;
				if (isEditing)
				{
					GUI.SetNextControlName(entry.Property.Path);
					newName = EditorGUI.TextField(textBoxRect, value.TypeName, EditorStyles.textField);
					if (GUI.GetNameOfFocusedControl() == entry.Property.Path && (Event.current.Equals(Event.KeyboardEvent("return")) || Event.current.OnKeyUp(KeyCode.Return)))
					{
						isEditing = false;
					}
				}
				else
				{
					if (GUI.Button(textBoxRect, value.NiceTypeName, EditorStyles.label))
					{
						isEditing = true;
					}
					if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
					{
						EditorIcons.Pen.Draw(rect.AlignRight(30f).AddX(30f), 16f);
					}
				}
				if (isEditing && SirenixEditorGUI.IconButton(rect.AlignRight(30f).AddX(30f), EditorIcons.Checkmark))
				{
					isEditing = false;
				}
				if ((newName ?? "") != (value.TypeName ?? ""))
				{
					value.TypeName = newName;
					value.IsCustom = true;
					value.Type = GetTypeFromName(value.TypeName);
					value.NiceTypeName = ((value.Type == null) ? value.TypeName : value.Type.GetNiceName());
					valueChanged = true;
				}
				if (wasEditing && !isEditing)
				{
					if (value.Type != null)
					{
						value.TypeName = TypeBinder.BindToName(value.Type);
					}
					entry.Values.ForceMarkDirty();
				}
				if (valueChanged)
				{
					value.IsCustom = true;
					entry.Values.ForceMarkDirty();
				}
			}
		}

		private static readonly bool EditorOnlyBuild = false;

		private static readonly TwoWaySerializationBinder TypeBinder = new DefaultSerializationBinder();

		[EnableIf("EnableAutomateBeforeBuilds")]
		[ToggleLeft]
		[SerializeField]
		[SuffixLabel("$AutomateBeforeBuildsSuffix", false)]
		[DisableIf("EditorOnlyBuild")]
		private bool automateBeforeBuilds;

		[SerializeField]
		[ToggleLeft]
		[ShowIf("ShowAutomateConfig", true)]
		[Indent(1)]
		[DisableIf("EditorOnlyBuild")]
		private bool deleteDllAfterBuilds = true;

		[ToggleLeft]
		[Indent(1)]
		[ShowIf("ShowAutomateConfig", true)]
		[SerializeField]
		public bool AutomateForAllAOTPlatforms = true;

		[Indent(1)]
		[HideIf("AutomateForAllAOTPlatforms", true)]
		[SerializeField]
		[ShowIf("ShowAutomateConfig", true)]
		private List<BuildTarget> automateForPlatforms = new List<BuildTarget>
		{
			BuildTarget.iOS,
			BuildTarget.WebGL
		};

		private static bool hasInitializedLastScanTime;

		private static DateTime lastScanTime;

		[Searchable(FilterOptions = SearchFilterOptions.ISearchFilterableInterface)]
		[DisableIf("EditorOnlyBuild")]
		[PropertyOrder(4f)]
		[ListDrawerSettings(DraggableItems = false, OnTitleBarGUI = "GenericVariantsTitleGUI", HideAddButton = true)]
		[SerializeField]
		private List<TypeEntry> supportSerializedTypes;

		private string AutomateBeforeBuildsSuffix
		{
			get
			{
				if (!EnableAutomateBeforeBuilds)
				{
					return "The automation feature is only available in Unity 5.6 and up";
				}
				return "";
			}
		}

		private bool EnableAutomateBeforeBuilds => UnityVersion.IsVersionOrGreater(5, 6);

		private bool ShowAutomateConfig
		{
			get
			{
				if (EnableAutomateBeforeBuilds)
				{
					return automateBeforeBuilds;
				}
				return false;
			}
		}

		private static DateTime LastScanTime
		{
			get
			{
				if (!hasInitializedLastScanTime)
				{
					hasInitializedLastScanTime = true;
					try
					{
						string dateLongStr = EditorPrefs.GetString("OdinSerializer_AOTGenerationConfig_LastScan", "0");
						if (long.TryParse(dateLongStr, out var dateLong))
						{
							lastScanTime = DateTime.FromBinary(dateLong);
						}
						else
						{
							lastScanTime = default(DateTime);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
				return lastScanTime;
			}
			set
			{
				if (value != lastScanTime)
				{
					hasInitializedLastScanTime = true;
					lastScanTime = value;
					EditorPrefs.SetString("OdinSerializer_AOTGenerationConfig_LastScan", value.ToBinary().ToString("D"));
				}
			}
		}

		/// <summary>
		/// <para>
		/// Whether to automatically scan the project and generate an AOT dll, right before builds. This will only affect platforms that are in the <see cref="P:Sirenix.Serialization.AOTGenerationConfig.AutomateForPlatforms" /> list.
		/// </para>
		/// <para>
		/// **This will only work on Unity 5.6 and higher!**
		/// </para>
		/// </summary>
		public bool AutomateBeforeBuilds
		{
			get
			{
				return automateBeforeBuilds;
			}
			set
			{
				automateBeforeBuilds = value;
			}
		}

		/// <summary>
		/// Whether to automatically delete the generated AOT dll after a build has completed.
		/// </summary>
		public bool DeleteDllAfterBuilds
		{
			get
			{
				return deleteDllAfterBuilds;
			}
			set
			{
				deleteDllAfterBuilds = value;
			}
		}

		/// <summary>
		/// A list of platforms to automatically scan the project and generate an AOT dll for, right before builds. This will do nothing unless <see cref="P:Sirenix.Serialization.AOTGenerationConfig.AutomateBeforeBuilds" /> is true.
		/// </summary>
		public List<BuildTarget> AutomateForPlatforms => automateForPlatforms;

		/// <summary>
		/// The path to the AOT folder that the AOT .dll and linker file is created in, relative to the current project folder.
		/// </summary>
		public string AOTFolderPath => SirenixAssetPaths.SirenixAssembliesPath + "AOT/";

		public bool ShouldAutomationGeneration(BuildTarget target)
		{
			if (!AutomateBeforeBuilds)
			{
				return false;
			}
			if (AutomateForAllAOTPlatforms)
			{
				BuildTarget platform = EditorUserBuildSettings.activeBuildTarget;
				ScriptingImplementation backend = AssemblyImportSettingsUtilities.GetCurrentScriptingBackend();
				ApiCompatibilityLevel api = AssemblyImportSettingsUtilities.GetCurrentApiCompatibilityLevel();
				if (AssemblyImportSettingsUtilities.IsJITSupported(platform, backend, api))
				{
					return false;
				}
				return true;
			}
			if (AutomateForPlatforms != null && AutomateForPlatforms.Contains(target))
			{
				return true;
			}
			return false;
		}

		private void GenericVariantsTitleGUI()
		{
			SirenixEditorGUI.VerticalLineSeparator();
			GUILayout.Label("Last scan: " + LastScanTime, SirenixGUIStyles.CenteredGreyMiniLabel);
			if (SirenixEditorGUI.ToolbarButton(new GUIContent("  Sort  ")))
			{
				SortTypes();
			}
			if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus))
			{
				supportSerializedTypes.Insert(0, new TypeEntry
				{
					IsCustom = true,
					Emit = true
				});
			}
		}

		private void SortTypes()
		{
			Comparison<TypeEntry> sorter = delegate(TypeEntry a, TypeEntry b)
			{
				bool flag = a.Type != null;
				bool flag2 = b.Type != null;
				if (flag != flag2)
				{
					if (!flag)
					{
						return -1;
					}
					return 1;
				}
				if (!flag)
				{
					return (a.TypeName ?? "").CompareTo(b.TypeName ?? "");
				}
				if (a.IsCustom != b.IsCustom)
				{
					if (!a.IsCustom)
					{
						return 1;
					}
					return -1;
				}
				return (a.IsNew != b.IsNew) ? ((!a.IsNew) ? 1 : (-1)) : (a.NiceTypeName ?? "").CompareTo(b.NiceTypeName ?? "");
			};
			supportSerializedTypes.Sort(sorter);
		}

		[Button("Scan Project", 30)]
		[HorizontalGroup("ButtonMargin", 0.2f, 0, 0, 0f, PaddingRight = -4f)]
		[PropertyOrder(2f)]
		[DisableIf("EditorOnlyBuild")]
		private void ScanProjectButton()
		{
			UnityEditorEventUtility.EditorApplication_delayCall += ScanProject;
		}

		/// <summary>
		/// Scans the entire project for types to support AOT serialization for.
		/// </summary>
		public void ScanProject()
		{
			if (AOTSupportUtilities.ScanProjectForSerializedTypes(out var serializedTypes))
			{
				RegisterTypes(supportSerializedTypes, serializedTypes);
				SortTypes();
				LastScanTime = DateTime.Now;
				EditorUtility.SetDirty(this);
			}
		}

		private void RegisterTypes(List<TypeEntry> typeEntries, List<Type> types)
		{
			HashSet<Type> preExistingNonCustomTypes = new HashSet<Type>(from n in typeEntries
				where !n.IsCustom && n.Type != null
				select n.Type);
			typeEntries.RemoveAll((TypeEntry n) => !n.IsCustom);
			HashSet<Type> preExistingCustomTypes = new HashSet<Type>(from n in typeEntries
				where n.Type != null
				select n.Type);
			typeEntries.AddRange(from type in types
				where !preExistingCustomTypes.Contains(type)
				select new TypeEntry
				{
					Type = type,
					TypeName = TypeBinder.BindToName(type),
					NiceTypeName = type.GetNiceName(),
					IsCustom = false,
					Emit = true,
					IsNew = !preExistingNonCustomTypes.Contains(type),
					IsInitialized = false
				});
			InitializeTypeEntries();
		}

		[DisableIf("EditorOnlyBuild")]
		[PropertyOrder(-1f)]
		[OnInspectorGUI]
		private void DrawTopInfoBox()
		{
			SirenixEditorGUI.MessageBox("On AOT-compiled platforms, Unity's code stripping can remove classes that the serialization system needs, or fail to generate code for needed variants of generic types. Therefore, Odin can create an assembly that directly references all functionality that is needed at runtime, to ensure it is available.", MessageType.Info, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
		}

		[DisableIf("EditorOnlyBuild")]
		[OnInspectorGUI]
		[HorizontalGroup("ButtonMargin", 0f, 0, 0, 0f)]
		[PropertyOrder(1f)]
		private void DrawWarning()
		{
			SirenixEditorGUI.MessageBox("Scanning the entire project might take a while. It will scan the entire project for relevant types including ScriptableObjects, prefabs and scenes. Modified type entries will not be touched.", MessageType.Warning, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
		}

		[DisableIf("EditorOnlyBuild")]
		[TitleGroup("Generate AOT DLL", "Sirenix/Assemblies/AOT/Sirenix.Serialization.AOTGenerated.dll", TitleAlignments.Left, true, true, false, 0f)]
		[PropertyOrder(9f)]
		[Button("Generate DLL", ButtonSizes.Large)]
		private void GenerateDLLButton()
		{
			GenerateDLL();
			GUIHelper.ExitGUI(removeFocusControl: true);
		}

		/// <summary>
		/// Generates an AOT DLL, using the current configuration of the AOTGenerationConfig instance.
		/// </summary>
		public void GenerateDLL()
		{
			List<Type> generateForTypes = (from n in supportSerializedTypes
				where n.Emit && n.Type != null
				select n.Type).ToList();
			FixUnityAboutWindowBeforeEmit.Fix();
			AOTSupportUtilities.GenerateDLL(AOTFolderPath, "Sirenix.Serialization.AOTGenerated", generateForTypes);
		}

		public void GenerateDLL(string folderPath, bool generateLinkXML = true)
		{
			List<Type> generateForTypes = (from n in supportSerializedTypes
				where n.Emit && n.Type != null
				select n.Type).ToList();
			FixUnityAboutWindowBeforeEmit.Fix();
			AOTSupportUtilities.GenerateDLL(folderPath, "Sirenix.Serialization.AOTGenerated", generateForTypes, generateLinkXML);
		}

		[OnInspectorGUI]
		[DisableIf("EditorOnlyBuild")]
		[PropertyOrder(-1000f)]
		private void OnGUIInitializeTypeEntries()
		{
			if (Event.current.type == EventType.Layout)
			{
				InitializeTypeEntries();
			}
		}

		public List<Type> GetAOTSupportedTypes()
		{
			InitializeTypeEntries();
			return (from n in supportSerializedTypes
				where n.Emit && n.Type != null
				select n.Type).ToList();
		}

		private void InitializeTypeEntries()
		{
			supportSerializedTypes = supportSerializedTypes ?? new List<TypeEntry>();
			foreach (TypeEntry item in supportSerializedTypes)
			{
				if (item.IsInitialized)
				{
					continue;
				}
				if (item.Type == null)
				{
					if (item.TypeName != null)
					{
						item.Type = GetTypeFromName(item.TypeName);
					}
					if (item.Type != null)
					{
						item.NiceTypeName = item.Type.GetNiceName();
					}
				}
				item.IsInitialized = true;
			}
		}

		private static Type GetTypeFromName(string name)
		{
			Type result = TypeBinder.BindToType(name);
			if (result != null)
			{
				return result;
			}
			if (ExpressionUtility.TryParseTypeNameAsCSharpIdentifier(name, out result))
			{
				return result;
			}
			return null;
		}
	}
}

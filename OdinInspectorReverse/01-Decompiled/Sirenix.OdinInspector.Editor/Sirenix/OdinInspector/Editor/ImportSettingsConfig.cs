using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.Serialization.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Configurations for Odin DLLs import settings.
	/// </summary>
	[SirenixEditorConfig]
	public class ImportSettingsConfig : GlobalConfig<ImportSettingsConfig>
	{
		public enum OdinAssemblyOptions
		{
			Recommended,
			AOT,
			JIT
		}

		private const string AOTAssemblyFolder = "NoEmitAndNoEditor";

		private const string JITAssemblyFolder = "NoEditor";

		private const int LabelWidth = 270;

		private static bool editorOnlyMode;

		private static bool isHeaderInfoBoxFolded = true;

		[SerializeField]
		[HideInInspector]
		private bool automateBeforeBuild = true;

		private OdinAssemblyOptions currentOption;

		/// <summary>
		/// Gets or sets a value indicating whether or not Odin should automatically configure the import settings of its DLLs in a preprocess build step.
		/// Keep in mind that this feature is only supported by Unity version 5.6 and up.
		/// </summary>
		[SuffixLabel("$AutomateSuffix", false)]
		[DisableIf("editorOnlyMode")]
		[BoxGroup("AutomateBox", true, false, 0f, ShowLabel = false)]
		[LabelWidth(270f)]
		[ShowInInspector]
		[EnableIf("IsAutomationSupported")]
		public bool AutomateBeforeBuild
		{
			get
			{
				if (IsAutomationSupported)
				{
					return automateBeforeBuild;
				}
				return false;
			}
			set
			{
				automateBeforeBuild = value;
				if (automateBeforeBuild && !IsAutomationSupported)
				{
					Debug.LogWarning("Automatic configuration of Odin DLL import settings is only supported by Unity versions 5.6 and up.");
				}
			}
		}

		[EnableGUI]
		[LabelWidth(270f)]
		[BoxGroup("SettingsBox", true, false, 0f, ShowLabel = false)]
		[ShowInInspector]
		[DisplayAsString]
		[SuffixLabel("$BuildTargetSuffix", false)]
		[DisableIf("editorOnlyMode")]
		private BuildTarget CurrentBuildTarget => EditorUserBuildSettings.activeBuildTarget;

		[DisableIf("editorOnlyMode")]
		[SuffixLabel("$ScriptingBackendSuffix", false)]
		[DisplayAsString]
		[ShowInInspector]
		[LabelWidth(270f)]
		[BoxGroup("SettingsBox", true, false, 0f, ShowLabel = false)]
		[EnableGUI]
		private ScriptingImplementation CurrentScriptingBackend => AssemblyImportSettingsUtilities.GetCurrentScriptingBackend();

		[DisableIf("editorOnlyMode")]
		[EnableGUI]
		[LabelWidth(270f)]
		[BoxGroup("SettingsBox", true, false, 0f, ShowLabel = false)]
		[ShowInInspector]
		[DisplayAsString]
		[SuffixLabel("$ApiLevelSuffix", false)]
		private ApiCompatibilityLevel CurrentApiCompatibilityLevel => AssemblyImportSettingsUtilities.GetCurrentApiCompatibilityLevel();

		[EnableGUI]
		[DisableIf("editorOnlyMode")]
		[LabelWidth(270f)]
		[SuffixLabel("$RecommendedSuffix", false)]
		[BoxGroup("SelectApplyBox", true, false, 0f, ShowLabel = false)]
		[ShowInInspector]
		[DisplayAsString]
		private string CurrentRecommendedBuildConfiguration
		{
			get
			{
				if (editorOnlyMode)
				{
					return "Editor Only Mode enabled.";
				}
				return GetRecommendedOption().ToString();
			}
		}

		[HorizontalGroup("SelectApplyBox/Select", 590f, 0, 0, 0f)]
		[LabelWidth(270f)]
		[EnumToggleButtons]
		[EnableIf("EnableApplyButton")]
		[BoxGroup("SelectApplyBox", true, false, 0f, ShowLabel = false)]
		[ShowInInspector]
		public OdinAssemblyOptions AssemblyBuildConfiguration
		{
			get
			{
				return currentOption;
			}
			set
			{
				currentOption = value;
			}
		}

		private string AutomateSuffix
		{
			get
			{
				if (!IsAutomationSupported)
				{
					return "The automation feature is only available in Unity 5.6 and up";
				}
				return "Recommended";
			}
		}

		private string BuildTargetSuffix
		{
			get
			{
				if (!AssemblyImportSettingsUtilities.PlatformSupportsJIT(EditorUserBuildSettings.activeBuildTarget))
				{
					return "Only AOT";
				}
				return "Supports JIT";
			}
		}

		private string ScriptingBackendSuffix
		{
			get
			{
				if (!AssemblyImportSettingsUtilities.ScriptingBackendSupportsJIT(AssemblyImportSettingsUtilities.GetCurrentScriptingBackend()))
				{
					return "Only AOT";
				}
				return "Supports JIT";
			}
		}

		private string ApiLevelSuffix
		{
			get
			{
				if (!AssemblyImportSettingsUtilities.ApiCompatibilityLevelSupportsJIT(AssemblyImportSettingsUtilities.GetCurrentApiCompatibilityLevel()))
				{
					return "Only AOT";
				}
				return "Supports JIT";
			}
		}

		private string RecommendedSuffix
		{
			get
			{
				if (GetRecommendedOption() != OdinAssemblyOptions.JIT)
				{
					return "Some settings are only AOT";
				}
				return "All settings support JIT";
			}
		}

		private bool EnableApplyButton
		{
			get
			{
				if (!editorOnlyMode)
				{
					if (IsAutomationSupported)
					{
						if (IsAutomationSupported)
						{
							return !automateBeforeBuild;
						}
						return false;
					}
					return true;
				}
				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether or not automatic configuration of Odin's DLL import settings is supported by the current Unity version.
		/// </summary>
		public static bool IsAutomationSupported => UnityVersion.IsVersionOrGreater(5, 6);

		[BoxGroup("SelectApplyBox", true, false, 0f, ShowLabel = false)]
		[Button(ButtonSizes.Large)]
		[EnableIf("EnableApplyButton")]
		private void Apply()
		{
			UnityEditorEventUtility.EditorApplication_delayCall += ApplyDelayed;
		}

		private void ApplyDelayed()
		{
			ApplyImportSettings();
		}

		public void ApplyImportSettings()
		{
			if (EditorOnlyModeConfig.Instance.IsEditorOnlyModeEnabled())
			{
				throw new InvalidOperationException("Editor Only Mode enabled.");
			}
			string assemblyDir = new DirectoryInfo(SirenixAssetPaths.SirenixAssembliesPath).FullName;
			string projectAssetsPath = Directory.GetCurrentDirectory().TrimEnd('\\', '/');
			bool isPackage = !new DirectoryInfo(projectAssetsPath).HasSubDirectory(new DirectoryInfo(assemblyDir));
			string aotDirPath = assemblyDir + "NoEmitAndNoEditor/";
			string jitDirPath = assemblyDir + "NoEditor/";
			DirectoryInfo aotDir = new DirectoryInfo(aotDirPath);
			DirectoryInfo jitDir = new DirectoryInfo(jitDirPath);
			List<string> aotAssemblies = new List<string>();
			List<string> jitAssemblies = new List<string>();
			FileInfo[] files = aotDir.GetFiles("*.dll");
			foreach (FileInfo file in files)
			{
				string path = file.FullName;
				path = ((!isPackage) ? path.Substring(projectAssetsPath.Length + 1) : (SirenixAssetPaths.SirenixAssembliesPath.TrimEnd('\\', '/') + "/" + path.Substring(assemblyDir.Length)));
				aotAssemblies.Add(path);
			}
			FileInfo[] files2 = jitDir.GetFiles("*.dll");
			foreach (FileInfo file2 in files2)
			{
				string path2 = file2.FullName;
				path2 = ((!isPackage) ? path2.Substring(projectAssetsPath.Length + 1) : (SirenixAssetPaths.SirenixAssembliesPath.TrimEnd('\\', '/') + "/" + path2.Substring(assemblyDir.Length)));
				jitAssemblies.Add(path2);
			}
			OdinAssemblyOptions option = currentOption;
			if (option == OdinAssemblyOptions.Recommended)
			{
				option = GetRecommendedOption();
			}
			AssetDatabase.StartAssetEditing();
			try
			{
				switch (option)
				{
				case OdinAssemblyOptions.AOT:
					SetImportSettings(EditorUserBuildSettings.activeBuildTarget, aotAssemblies, OdinAssemblyImportSettings.IncludeInBuildOnly);
					SetImportSettings(EditorUserBuildSettings.activeBuildTarget, jitAssemblies, OdinAssemblyImportSettings.ExcludeFromAll);
					break;
				case OdinAssemblyOptions.JIT:
					SetImportSettings(EditorUserBuildSettings.activeBuildTarget, aotAssemblies, OdinAssemblyImportSettings.ExcludeFromAll);
					SetImportSettings(EditorUserBuildSettings.activeBuildTarget, jitAssemblies, OdinAssemblyImportSettings.IncludeInBuildOnly);
					break;
				default:
					throw new ArgumentException("Unknown Odin assembly option: " + currentOption.ToString() + ". Please select either AOT or JIT");
				}
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
		}

		private static void SetImportSettings(BuildTarget platform, List<string> assemblyPaths, OdinAssemblyImportSettings importSettings)
		{
			foreach (string path in assemblyPaths)
			{
				string p = path.Replace('\\', '/');
				AssemblyImportSettingsUtilities.SetAssemblyImportSettings(platform, p, importSettings);
			}
		}

		[PropertyOrder(-1000000f)]
		[OnInspectorGUI]
		private void DrawEditorOnlyMode()
		{
			if (Event.current.type == EventType.Layout)
			{
				editorOnlyMode = EditorOnlyModeConfig.Instance.IsEditorOnlyModeEnabled();
			}
			if (editorOnlyMode)
			{
				SirenixEditorGUI.MessageBox("Editor Only Mode is currently enabled. These configurations are currently irrelevant.", MessageType.Info, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			}
		}

		[PropertyOrder(-10000f)]
		[OnInspectorGUI]
		private void DrawHeaderInfoBox()
		{
			isHeaderInfoBoxFolded = SirenixEditorGUI.DetailedMessageBox("Odin will automatically detect your current build target and make sure it uses the assemblies best suited for your target platform.\n\nClick here to learn more.", "The Odin Serializer has two sets of assemblies: one set for AOT platforms and one for those platforms where JIT'ing is supported. JIT is usually the most performant, but is not supported on all platforms. Finding out whether your setup supports it, goes beyond what Unity's Import Settings has to offer, which is why this tool becomes necessary.\n\nOdin has a predefined set of known setups where JIT is supported. If your setup doesn't match any of those, then it'll choose to use the AOT assemblies.\n\nEnabling the \"Automate Before Build\" option will enable a preprocess build step, that will configure the import settings automatically, based on your current build settings.\n\nIf you've stumbled on a setup where we're using AOT when we could be using JIT, or the other way around, then you can always disable the preprocess build set and manually configure your assemblies.", MessageType.Info, isHeaderInfoBoxFolded, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
		}

		public OdinAssemblyOptions GetRecommendedOption()
		{
			if (!AssemblyImportSettingsUtilities.IsJITSupported(EditorUserBuildSettings.activeBuildTarget, AssemblyImportSettingsUtilities.GetCurrentScriptingBackend(), AssemblyImportSettingsUtilities.GetCurrentApiCompatibilityLevel()))
			{
				return OdinAssemblyOptions.AOT;
			}
			return OdinAssemblyOptions.JIT;
		}
	}
}

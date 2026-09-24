using System;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor.GettingStarted;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Windows
{
	internal class OdinUnityContextMenuItems
	{
		private const int Group0 = -1000;

		private const int Group1 = 10000;

		private const int Group2 = 100000;

		private const int Group3 = 1000000;

		[MenuItem("Tools/Odin/Getting Started", priority = -1000)]
		private static void OpenGettingStarted()
		{
			GettingStartedWindow.ShowWindow();
		}

		[MenuItem("Tools/Odin/Inspector/Attribute Overview", priority = 10000)]
		public static void OpenAttributesOverview()
		{
			AttributesExampleWindow.OpenWindow(null);
		}

		[MenuItem("Tools/Odin/Serializer/Serialization Debugger", priority = 10000)]
		public static void ShowSerializationDebugger()
		{
			SerializationDebuggerWindow.ShowWindow();
		}

		[MenuItem("Tools/Odin/Serializer/Import Settings", priority = 10000)]
		public static void ShowSerializationImportSettings()
		{
			SirenixPreferencesWindow.OpenWindow(GlobalConfig<ImportSettingsConfig>.Instance);
		}

		[MenuItem("Tools/Odin/Serializer/AOT Generation", priority = 10000)]
		public static void ShowSerializationAOTGeneration()
		{
			SirenixPreferencesWindow.OpenWindow(GlobalConfig<AOTGenerationConfig>.Instance);
		}

		[MenuItem("Tools/Odin/Serializer/Preferences", priority = 10000)]
		public static void ShowSerializationPreferences()
		{
			SirenixPreferencesWindow.OpenWindow(GlobalConfig<GlobalSerializationConfig>.Instance);
		}

		[MenuItem("Tools/Odin/Validator", priority = 10000)]
		public static void ShowValidator()
		{
			Type t = TwoWaySerializationBinder.Default.BindToType("Sirenix.OdinValidator.Editor.OdinValidatorWindow");
			if (t != null)
			{
				MethodInfo method = t.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault((MethodInfo x) => x.Name == "OpenWindow" && x.GetParameters().Length == 0);
				method.Invoke(null, null);
			}
		}

		[MenuItem("Tools/Odin/Validator", true, priority = 10000)]
		public static bool ShowValidatorValidate()
		{
			return TwoWaySerializationBinder.Default.BindToType("Sirenix.OdinValidator.Editor.OdinValidatorWindow") != null;
		}

		[MenuItem("Tools/Odin/Inspector/Sdf Icon Overview", priority = 10002)]
		public static void OpenSdfIconOverview()
		{
			SdfIconOverviewWindow.ShowWindow();
		}

		[MenuItem("Tools/Odin/Inspector/Unit Overview", priority = 10003)]
		public static UnitOverviewWindow OpenUnitOverview()
		{
			return UnitOverviewWindow.ShowWindow();
		}

		[MenuItem("Tools/Odin/Inspector/Designer Files Overview", priority = 10004)]
		public static void OpenDesignerFilesOverview()
		{
			DesignerFilesOverviewWindow.ShowWindow();
		}

		[MenuItem("Tools/Odin/Inspector/Static Inspector", priority = 10005)]
		private static void OpenStaticInspector()
		{
			StaticInspectorWindow.ShowWindow();
		}

		[MenuItem("Tools/Odin/Inspector/Preferences", priority = 10006)]
		public static void OpenSirenixPreferences()
		{
			SirenixPreferencesWindow.OpenSirenixPreferences();
		}

		[MenuItem("Tools/Odin/Help/Discord", priority = 1000001)]
		private static void Discord()
		{
			Application.OpenURL("https://discord.gg/WTYJEra");
		}

		[MenuItem("Tools/Odin/Help/Report An Issue", priority = 1000002)]
		private static void ReportAnIssue()
		{
			Application.OpenURL("https://bitbucket.org/sirenix/odin-inspector/issues");
		}

		[MenuItem("Tools/Odin/Help/Contact", priority = 1000003)]
		private static void Contact()
		{
			Application.OpenURL("https://odininspector.com/support");
		}

		[MenuItem("Tools/Odin/Help/Release Notes", priority = 1000004)]
		private static void OpenReleaseNotes()
		{
			Application.OpenURL("https://odininspector.com/patch-notes");
		}

		[MenuItem("Tools/Odin/Help/Check for updates", priority = 1000005)]
		private static void CheckForUpdates()
		{
			CheckForUpdatesWindow.OpenWindow();
		}

		[MenuItem("Tools/Odin/Help/About", priority = 1000006)]
		private static void ShowAboutOdinInspector()
		{
			Rect rect = GUIHelper.GetEditorWindowRect().AlignCenter(465f).AlignMiddle(OdinInspectorVersion.HasLicensee ? 150f : 135f);
			OdinInspectorAboutWindow w = EditorWindow.GetWindowWithRect<OdinInspectorAboutWindow>(rect, utility: true, "Odin Inspector & Serializer");
			w.ShowUtility();
		}

		[MenuItem("CONTEXT/MonoBehaviour/Odin/Debug Serialization")]
		private static void ComponentContextMenuItem(MenuCommand menuCommand)
		{
			SerializationDebuggerWindow.ShowWindow(menuCommand.context.GetType());
		}
	}
}

using Sirenix.OdinInspector.Editor.Modules;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Sirenix preferences window.
	/// </summary>
	public class SirenixPreferencesWindow : OdinMenuEditorWindow
	{
		protected override OdinMenuTree BuildMenuTree()
		{
			OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: true)
			{
				{
					"General",
					GlobalConfig<GeneralDrawerConfig>.Instance
				},
				{
					"Editor Types",
					GlobalConfig<InspectorConfig>.Instance
				},
				{
					"Persistent Context Cache",
					PersistentContextCache.Instance
				},
				{
					"Color Palettes",
					GlobalConfig<ColorPaletteManager>.Instance
				},
				{
					"Serialization",
					GlobalConfig<GlobalSerializationConfig>.Instance
				},
				{
					"Import Settings",
					GlobalConfig<ImportSettingsConfig>.Instance
				},
				{
					"AOT Generation",
					GlobalConfig<AOTGenerationConfig>.Instance
				},
				{
					"Editor Only Mode",
					EditorOnlyModeConfig.Instance
				},
				{
					"Visual Designer",
					GlobalConfig<OdinVisualDesignerConfig>.Instance
				},
				{
					"Modules",
					GlobalConfig<OdinModuleConfig>.Instance
				}
			};
			tree.Config.SelectMenuItemsOnMouseDown = true;
			return tree;
		}

		protected override void DrawMenu()
		{
			base.DrawMenu();
			Rect rect = GUIHelper.GetCurrentLayoutRect().Padding(4f).AlignBottom(20f);
			GUI.Label(rect, "Odin Inspector Version " + OdinInspectorVersion.Version, SirenixGUIStyles.CenteredGreyMiniLabel);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			DefaultLabelWidth = 278f;
			ResizableMenuWidth = false;
		}

		/// <summary>
		/// Opens the Odin inspector preferences window.
		/// </summary>
		public static void OpenSirenixPreferences()
		{
			SirenixPreferencesWindow window = EditorWindow.GetWindow<SirenixPreferencesWindow>();
			window.position = GUIHelper.GetEditorWindowRect().AlignCenter(900f, 600f);
			window.titleContent = new GUIContent("Preferences", EditorIcons.OdinInspectorLogo);
		}

		/// <summary>
		/// Opens the Odin inspector preferences window.
		/// </summary>
		public static void OpenWindow(object selectedItem)
		{
			SirenixPreferencesWindow window = EditorWindow.GetWindow<SirenixPreferencesWindow>();
			window.TrySelectMenuItemWithObject(selectedItem);
			window.titleContent = new GUIContent("Preferences", EditorIcons.OdinInspectorLogo);
		}

		internal void GotoPreferencesTab(string tabName)
		{
			UnityEditorEventUtility.DelayAction(delegate
			{
				OdinMenuItem odinMenuItem = null;
				for (int i = 0; i < base.MenuTree.MenuItems.Count; i++)
				{
					OdinMenuItem odinMenuItem2 = base.MenuTree.MenuItems[i];
					if (odinMenuItem2.Name == "General")
					{
						odinMenuItem = odinMenuItem2;
						break;
					}
				}
				odinMenuItem.Select();
				GeneralDrawerConfig generalDrawerConfig = odinMenuItem.Value as GeneralDrawerConfig;
				generalDrawerConfig.TargetTabName = tabName;
			});
		}
	}
}

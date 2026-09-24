using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Modules
{
	[InitializeOnLoad]
	[SirenixEditorConfig]
	public class OdinModuleConfig : GlobalConfig<OdinModuleConfig>
	{
		public enum ModuleAutomationSettings
		{
			Ask,
			Automatic,
			Manual
		}

		private class ModuleSettings
		{
			[EnumToggleButtons]
			public ModuleAutomationSettings ModuleToggling;

			[EnumToggleButtons]
			public ModuleAutomationSettings ModuleUpdating;

			[OnInspectorGUI]
			[PropertyOrder(-1f)]
			private void OnInspectorGUI()
			{
				GUILayout.Label("Module Settings", ModuleDefinition.TitleStyle);
				SirenixEditorGUI.HorizontalLineSeparator(SirenixGUIStyles.BorderColor);
			}

			[OnInspectorGUI]
			private void DrawApplyButton()
			{
				GUILayout.FlexibleSpace();
				bool changed = GlobalConfig<OdinModuleConfig>.Instance.ModuleTogglingSettings != ModuleToggling || GlobalConfig<OdinModuleConfig>.Instance.ModuleUpdateSettings != ModuleUpdating;
				GUIHelper.PushGUIEnabled(changed);
				if (GUILayout.Button("Apply changes"))
				{
					GlobalConfig<OdinModuleConfig>.Instance.ModuleTogglingSettings = ModuleToggling;
					GlobalConfig<OdinModuleConfig>.Instance.ModuleUpdateSettings = ModuleUpdating;
					EditorUtility.SetDirty(GlobalConfig<OdinModuleConfig>.Instance);
					AssetDatabase.SaveAssets();
					RefreshModuleSetup();
					GUIHelper.ExitGUI(removeFocusControl: true);
				}
				GUIHelper.PopGUIEnabled();
			}
		}

		private class ModuleMenuItem : OdinMenuItem
		{
			private static GUIStyle backing_StatusStyle;

			private bool unstable;

			private string status;

			private float nextStatusUpdate;

			private static readonly OdinMenuStyle MenuItemStyle = new OdinMenuStyle
			{
				Height = 40,
				LabelVerticalOffset = -6f
			};

			private static readonly OdinMenuStyle UnstableMenuItemStyle = new OdinMenuStyle
			{
				Height = 52,
				LabelVerticalOffset = -12f
			};

			private static GUIStyle StatusStyle
			{
				get
				{
					if (backing_StatusStyle == null)
					{
						backing_StatusStyle = new GUIStyle(SirenixGUIStyles.LeftAlignedGreyMiniLabel)
						{
							richText = true
						};
					}
					return backing_StatusStyle;
				}
			}

			public ModuleMenuItem(OdinMenuTree tree, string name, object value)
				: base(tree, name, value)
			{
				UpdateStatus();
			}

			private void UpdateStatus()
			{
				ModuleDefinition module = (ModuleDefinition)base.Value;
				ModuleManifest manifest = module.LoadManifest();
				bool supportsCurrentEnvironment = module.CheckSupportsCurrentEnvironment();
				base.Style = (module.UnstableExperimental ? UnstableMenuItemStyle : MenuItemStyle);
				unstable = module.UnstableExperimental;
				if (manifest != null)
				{
					bool canUpgrade = manifest.Version < module.LatestVersion;
					if (canUpgrade)
					{
						base.Icon = EditorIcons.ArrowUp.Active;
					}
					else
					{
						base.Icon = EditorIcons.Checkmark.Active;
					}
					if (supportsCurrentEnvironment)
					{
						status = "Installed ( <color=#" + ColorUtility.ToHtmlStringRGBA(canUpgrade ? new Color(0.9f, 0.45f, 0.01f, 1f) : new Color(0.1f, 0.9f, 0.1f, 1f)) + ">" + manifest.Version?.ToString() + "</color> )";
					}
					else
					{
						status = "<color=#c0392b>Installed ( dependencies missing )</color>";
						base.Icon = EditorIcons.UnityErrorIcon;
					}
				}
				else if (supportsCurrentEnvironment)
				{
					base.Icon = EditorIcons.X.Active;
					status = "Inactive ( available: <color=#" + ColorUtility.ToHtmlStringRGBA(new Color(0.1f, 0.9f, 0.1f, 1f)) + ">" + (module.LatestVersion ?? new Version(0, 0, 0, 0)).ToString() + "</color> )";
				}
				else
				{
					base.Icon = EditorIcons.AlertCircle.Active;
					status = "Inactive ( <color=#c0392b>dependencies missing</color> )";
				}
				nextStatusUpdate = Time.realtimeSinceStartup + 5f;
			}

			protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
			{
				if (Time.realtimeSinceStartup > nextStatusUpdate)
				{
					UpdateStatus();
				}
				if (!EditorGUIUtility.isProSkin)
				{
					StatusStyle.normal.textColor = (base.IsSelected ? Color.white : Color.black);
				}
				GUI.Label(labelRect.AlignBottom(16f).AddY(12f), status, StatusStyle);
				if (unstable)
				{
					GUI.Label(labelRect.AlignBottom(16f).AddY(26f), "<color=#c0392b>EXPERIMENTAL & UNSTABLE</color>", StatusStyle);
				}
			}
		}

		[SerializeField]
		[HideInInspector]
		private List<ModuleConfiguration> configurations = new List<ModuleConfiguration>();

		[NonSerialized]
		private ModuleManager backing_moduleManager;

		private OdinMenuTree moduleTree;

		private PropertyTree selectedModuleTree;

		private object nextSelection;

		private bool hasNextSelection;

		private float MenuWidth = 220f;

		private bool ResizableMenuWidth = true;

		[HideInInspector]
		public ModuleAutomationSettings ModuleTogglingSettings = ModuleAutomationSettings.Automatic;

		[HideInInspector]
		public ModuleAutomationSettings ModuleUpdateSettings = ModuleAutomationSettings.Automatic;

		private static bool initialized;

		private static bool editorWasCompilingLastUpdate;

		public ModuleManager ModuleManager
		{
			get
			{
				if (backing_moduleManager == null)
				{
					backing_moduleManager = ModuleManager.CreateDefault();
				}
				return backing_moduleManager;
			}
		}

		public ModuleConfiguration GetConfig(ModuleDefinition module)
		{
			if (!ModuleManager.Modules.Contains(module))
			{
				return null;
			}
			if (configurations == null)
			{
				configurations = new List<ModuleConfiguration>();
			}
			ModuleConfiguration result = null;
			for (int i = 0; i < configurations.Count; i++)
			{
				ModuleConfiguration config = configurations[i];
				if (config.ID == module.ID)
				{
					result = config;
					continue;
				}
				bool existsInModuleManager = false;
				for (int j = 0; j < ModuleManager.Modules.Count; j++)
				{
					if (ModuleManager.Modules[j].ID == config.ID)
					{
						existsInModuleManager = true;
						break;
					}
				}
				if (!existsInModuleManager)
				{
					configurations.RemoveAt(i);
					i--;
				}
			}
			if (result != null)
			{
				return result;
			}
			result = new ModuleConfiguration
			{
				ID = module.ID
			};
			configurations.Add(result);
			SaveAssetChanges();
			return result;
		}

		public ModuleConfiguration GetConfig(string moduleID)
		{
			foreach (ModuleDefinition module in ModuleManager.Modules)
			{
				if (module.ID == moduleID)
				{
					return GetConfig(module);
				}
			}
			return null;
		}

		public void SaveAssetChanges()
		{
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
		}

		static OdinModuleConfig()
		{
			UnityEditorEventUtility.DelayAction(delegate
			{
				EnsureInitialized();
				if (!EditorApplication.isPlayingOrWillChangePlaymode)
				{
					RefreshModuleSetup();
				}
			});
		}

		private static void EnsureInitialized()
		{
			if (initialized)
			{
				return;
			}
			Type compilationPipelineType = TwoWaySerializationBinder.Default.BindToType("UnityEditor.Compilation.CompilationPipeline");
			bool subscriptedToCompilationEvent = false;
			if (compilationPipelineType != null)
			{
				EventInfo compilationFinishedEvent = compilationPipelineType.GetEvent("compilationFinished", BindingFlags.Static | BindingFlags.Public);
				if (compilationFinishedEvent != null)
				{
					compilationFinishedEvent.GetAddMethod(nonPublic: true).Invoke(null, new object[1] { (Action<object>)delegate
					{
						EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, (EditorApplication.CallbackFunction)delegate
						{
							TriggerModuleRefresh();
						});
					} });
					subscriptedToCompilationEvent = true;
				}
				else if (UnityVersion.IsVersionOrGreater(2019, 1))
				{
					Debug.LogWarning("Failed to find UnityEditor.Compilation.CompilationPipeline.compilationStarted event - Odin module automation may be broken in this version of Unity...");
				}
			}
			if (!subscriptedToCompilationEvent)
			{
				EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, new EditorApplication.CallbackFunction(EditorUpdate));
			}
			initialized = true;
		}

		private void OnEnable()
		{
			EnsureInitialized();
			SelectLastSelectedItem();
		}

		private void OnDisable()
		{
			if (selectedModuleTree != null)
			{
				selectedModuleTree.Dispose();
				selectedModuleTree = null;
			}
		}

		private void SelectLastSelectedItem()
		{
			EnsureInitialized();
			if (!EditorPrefs.HasKey("ODIN_LastSelectedModule"))
			{
				return;
			}
			string lastSelection = EditorPrefs.GetString("ODIN_LastSelectedModule");
			if (lastSelection == "ModuleConfig")
			{
				nextSelection = "ModuleConfig";
				hasNextSelection = true;
				return;
			}
			nextSelection = ModuleManager.Modules.FirstOrDefault((ModuleDefinition n) => n.GetType().Name == lastSelection);
			hasNextSelection = true;
		}

		private static void EditorUpdate()
		{
			EnsureInitialized();
			bool isCompiling = EditorApplication.isCompiling;
			if (isCompiling && !editorWasCompilingLastUpdate)
			{
				TriggerModuleRefresh();
			}
			editorWasCompilingLastUpdate = isCompiling;
		}

		private static void TriggerModuleRefresh()
		{
			try
			{
				if (!EditorApplication.isPlayingOrWillChangePlaymode)
				{
					RefreshModuleSetup();
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		public static void RefreshModuleSetup()
		{
			EnsureInitialized();
			OdinModuleConfig instance = GlobalConfig<OdinModuleConfig>.Instance;
			if (instance == null)
			{
				Debug.LogWarning("Couldn't load Odin Module Config asset; Odin module automation will not work...");
			}
			else if (instance.ModuleManager.Refresh())
			{
				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
				if (instance != null && instance.moduleTree != null)
				{
					instance.moduleTree = null;
					instance.SelectLastSelectedItem();
				}
			}
		}

		[OnInspectorGUI]
		private void OnInspectorGUI()
		{
			EnsureInitialized();
			if (moduleTree == null)
			{
				moduleTree = CreateModuleTree();
			}
			if (Event.current.type == EventType.Layout)
			{
				if (moduleTree == null)
				{
					moduleTree = CreateModuleTree();
				}
				if (hasNextSelection)
				{
					if (selectedModuleTree != null)
					{
						selectedModuleTree.Dispose();
						selectedModuleTree = null;
					}
					if (nextSelection != null)
					{
						if (nextSelection is string && (string)nextSelection == "ModuleConfig")
						{
							nextSelection = moduleTree.MenuItems[0].Value;
						}
						selectedModuleTree = PropertyTree.Create(nextSelection);
					}
					if (nextSelection is ModuleDefinition)
					{
						(nextSelection as ModuleDefinition).OnSelectedInInspector();
						EditorPrefs.SetString("ODIN_LastSelectedModule", nextSelection.GetType().Name);
					}
					else
					{
						EditorPrefs.SetString("ODIN_LastSelectedModule", "ModuleConfig");
					}
					if (moduleTree.Selection.Count != 1)
					{
						moduleTree.MenuItems.FirstOrDefault((OdinMenuItem n) => n.Value == nextSelection)?.Select();
					}
					nextSelection = null;
					hasNextSelection = false;
				}
			}
			GUILayout.Space(-4f);
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(-4f);
			Rect menuRect = EditorGUILayout.BeginVertical(GUILayoutOptions.Width(MenuWidth).ExpandHeight());
			EditorGUI.DrawRect(menuRect.AddXMin(-3f).AddYMax(3f), SirenixGUIStyles.MenuBackgroundColor);
			Rect menuBorderRect = menuRect;
			menuBorderRect.xMin = menuRect.xMax - 4f;
			menuBorderRect.xMax += 4f;
			if (ResizableMenuWidth)
			{
				EditorGUIUtility.AddCursorRect(menuBorderRect, MouseCursor.ResizeHorizontal);
				MenuWidth += SirenixEditorGUI.SlideRect(menuBorderRect).x;
			}
			moduleTree.DrawMenuTree();
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(GUILayoutOptions.ExpandWidth().ExpandHeight());
			if (selectedModuleTree != null)
			{
				selectedModuleTree.Draw(applyUndo: false);
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			EditorGUI.DrawRect(menuBorderRect.AlignCenter(1f).AddYMax(4f), SirenixGUIStyles.BorderColor);
			if (moduleTree != null)
			{
				moduleTree.HandleKeyboardMenuNavigation();
			}
		}

		private OdinMenuTree CreateModuleTree()
		{
			OdinMenuTree tree = new OdinMenuTree();
			tree.Config.DrawSearchToolbar = ModuleManager.Modules.Count > 10;
			tree.Selection.SupportsMultiSelect = false;
			tree.MenuItems.Add(new OdinMenuItem(tree, "Module Settings", new ModuleSettings
			{
				ModuleToggling = ModuleTogglingSettings,
				ModuleUpdating = ModuleUpdateSettings
			})
			{
				Icon = EditorIcons.SettingsCog.Active
			});
			foreach (ModuleDefinition module in ModuleManager.Modules)
			{
				tree.MenuItems.Add(new ModuleMenuItem(tree, module.NiceName, module));
			}
			tree.Selection.SelectionChanged += delegate
			{
				OdinMenuItem odinMenuItem = tree.Selection.LastOrDefault();
				if (odinMenuItem != null)
				{
					nextSelection = odinMenuItem.Value;
				}
				else
				{
					nextSelection = null;
				}
				hasNextSelection = true;
			};
			return tree;
		}
	}
}

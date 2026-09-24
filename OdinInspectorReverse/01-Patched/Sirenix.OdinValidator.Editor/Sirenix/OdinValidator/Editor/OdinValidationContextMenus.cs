using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sirenix.OdinValidator.Editor
{
	internal class OdinValidationContextMenus
	{
		private const int offset = 10000;

		private static EditorPrefBool drawValidatorIconsInProjectGUI = new EditorPrefBool("ODIN_VALIDATOR_PROJECT_WINDOW_ICONS", defaultValue: false);

		private static float _lastMenuCallTimestamp;

		[MenuItem("GameObject/Odin/Validate this", false, 11)]
		private static void ValidateThis1()
		{
			if (Time.unscaledTime.Equals(_lastMenuCallTimestamp))
			{
				return;
			}
			_lastMenuCallTimestamp = Time.unscaledTime;
			List<ValidationItem> include = new List<ValidationItem>();
			foreach (Scene item in Selection.gameObjects.Select((GameObject x) => x.scene).Distinct())
			{
				if (item.isLoaded)
				{
					include.Add(ValidationItem.FromScenePath(item.path));
				}
			}
			ValidationSessionEditor window = OdinValidatorWindow.OpenWindow("Custom session", include, null, startValidating: false);
			GameObject[] gameObjects = Selection.gameObjects;
			foreach (GameObject root in gameObjects)
			{
				if (!root)
				{
					continue;
				}
				Component[] components = root.GetComponents<Component>();
				foreach (Component c in components)
				{
					if ((bool)c)
					{
						if (c is Transform trs)
						{
							OdinEntityId entityId = OdinEntityId.FromObject(trs.gameObject);
							window.ValidationSession.Enqueue(new ProjectEvent
							{
								InstanceID = OdinEntityId.Internal.ToInt32(entityId),
								EntityId = entityId,
								Type = ProjectEventType.Revalidation
							}, insert: true);
						}
						OdinEntityId cEntityId = OdinEntityId.FromObject(c);
						window.ValidationSession.Enqueue(new ProjectEvent
						{
							InstanceID = OdinEntityId.Internal.ToInt32(cEntityId),
							EntityId = cEntityId,
							Type = ProjectEventType.Revalidation
						}, insert: true);
					}
				}
			}
			window.ValidationSession.ValidateQueuedUpWorkNow();
		}

		[MenuItem("GameObject/Odin/Validate this and children", false, 10)]
		private static void ValidateThis2()
		{
			if (Time.unscaledTime.Equals(_lastMenuCallTimestamp))
			{
				return;
			}
			_lastMenuCallTimestamp = Time.unscaledTime;
			List<ValidationItem> include = new List<ValidationItem>();
			foreach (Scene item in Selection.gameObjects.Select((GameObject x) => x.scene).Distinct())
			{
				if (item.isLoaded)
				{
					include.Add(ValidationItem.FromScenePath(item.path));
				}
			}
			ValidationSessionEditor window = OdinValidatorWindow.OpenWindow("Custom session", include, null, startValidating: false);
			GameObject[] gameObjects = Selection.gameObjects;
			foreach (GameObject root in gameObjects)
			{
				if (!root)
				{
					continue;
				}
				Component[] componentsInChildren = root.GetComponentsInChildren<Component>();
				foreach (Component c in componentsInChildren)
				{
					if ((bool)c)
					{
						if (c is Transform trs)
						{
							OdinEntityId entityId = OdinEntityId.FromObject(trs.gameObject);
							window.ValidationSession.Enqueue(new ProjectEvent
							{
								InstanceID = OdinEntityId.Internal.ToInt32(entityId),
								EntityId = entityId,
								Type = ProjectEventType.Revalidation
							}, insert: true);
						}
						OdinEntityId cEntityId = OdinEntityId.FromObject(c);
						window.ValidationSession.Enqueue(new ProjectEvent
						{
							InstanceID = OdinEntityId.Internal.ToInt32(cEntityId),
							EntityId = cEntityId,
							Type = ProjectEventType.Revalidation
						}, insert: true);
					}
				}
			}
			window.ValidationSession.ValidateQueuedUpWorkNow();
		}

		[MenuItem("Assets/Odin Validator/Validate this", priority = 12000)]
		private static void ValidateThis()
		{
			OdinValidatorWindow.OpenWindow("Custom session", ValidationItem.FromSelection(Selection.objects, includeSceneDependencies: true), null);
		}

		[MenuItem("Assets/Odin Validator/Filter/Include in active validation profile", priority = 9000)]
		public static void IncludeInActiveValidationProfile()
		{
			List<ValidationItem> currExclude = ValidationProfile.MainValidationProfile.Exclude;
			List<ValidationItem> currInclude = ValidationProfile.MainValidationProfile.Include;
			IEnumerable<ValidationItem> newItems = ValidationItem.FromSelection(Selection.objects, includeSceneDependencies: true).Except(currInclude);
			bool markDirty = false;
			foreach (ValidationItem item in newItems)
			{
				if (currExclude.Contains(item))
				{
					markDirty = true;
					currExclude.Remove(item);
				}
				else if (!currInclude.Contains(item))
				{
					markDirty = true;
					currInclude.Add(item);
				}
			}
			if (markDirty)
			{
				EditorUtility.SetDirty(ValidationProfile.MainValidationProfile);
			}
		}

		[MenuItem("Assets/Odin Validator/Filter/Exclude from active validation profile", priority = 9000)]
		public static void ExcludeFromActiveValidationProfile()
		{
			List<ValidationItem> currExclude = ValidationProfile.MainValidationProfile.Exclude;
			List<ValidationItem> currInclude = ValidationProfile.MainValidationProfile.Include;
			IEnumerable<ValidationItem> newItems = ValidationItem.FromSelection(Selection.objects, includeSceneDependencies: true).Except(currExclude);
			bool markDirty = false;
			foreach (ValidationItem item in newItems)
			{
				if (currInclude.Contains(item))
				{
					markDirty = true;
					currInclude.Remove(item);
				}
				else if (!currExclude.Contains(item))
				{
					markDirty = true;
					currExclude.Add(item);
				}
			}
			if (markDirty)
			{
				ValidationProfile.MainValidationProfile.SaveChanges();
			}
		}

		[MenuItem("Assets/Odin Validator/Filter/Exclude from active validation profile", validate = true)]
		public static bool ValidateExcludeFromActiveValidationProfile()
		{
			IEnumerable<ValidationItem> newItems = ValidationItem.FromSelection(Selection.objects, includeSceneDependencies: true).Except(ValidationProfile.MainValidationProfile.Exclude);
			return newItems.Any();
		}

		[MenuItem("Assets/Odin Validator/Filter/Include in active validation profile", validate = true)]
		public static bool ValidateIncludeInActiveValidationProfile()
		{
			IEnumerable<ValidationItem> newItems = ValidationItem.FromSelection(Selection.objects, includeSceneDependencies: true).Except(ValidationProfile.MainValidationProfile.Include);
			return newItems.Any();
		}

		[MenuItem("Assets/Odin Validator/Filter/Open filter settings", priority = 12500)]
		private static void ShowFil()
		{
			ValidationSessionEditor e = OdinValidatorWindow.OpenWindow(ValidationProfile.MainValidationProfile);
			e.MenuVisibility = true;
			e.SelectedMenu = ValidationSessionEditor.MenuOptions.FilterAssets;
		}

		[MenuItem("Assets/Odin Validator/Filter/Toggle filter icons in project window", priority = 13000)]
		private static void ToggleFilterIconsInProjectWindow()
		{
			drawValidatorIconsInProjectGUI.Value = !drawValidatorIconsInProjectGUI;
		}

		[MenuItem("Assets/Odin Validator/Create Rule/Root Object Validator", priority = 10000)]
		private static void CreateRuleRootObjectValidator_MenuItem()
		{
			if (AssetDatabase.Contains(Selection.activeObject))
			{
				string selectedFolder = GetSelectedFolderPath();
				ValidatorScriptTemplates.CreateRuleRootObjectValidator(selectedFolder);
			}
		}

		[MenuItem("Assets/Odin Validator/Create Rule/Attribute Validator", priority = 10000)]
		private static void CreateRuleAttributeValidator_MenuItem()
		{
			if (AssetDatabase.Contains(Selection.activeObject))
			{
				string selectedFolder = GetSelectedFolderPath();
				ValidatorScriptTemplates.CreateRuleAttributeValidator(selectedFolder);
			}
		}

		[MenuItem("Assets/Odin Validator/Create Rule/Value Validator", priority = 10000)]
		private static void CreateRuleValueValidator_MenuItem()
		{
			if (AssetDatabase.Contains(Selection.activeObject))
			{
				string selectedFolder = GetSelectedFolderPath();
				ValidatorScriptTemplates.CreateRuleValueValidator(selectedFolder);
			}
		}

		[MenuItem("Assets/Odin Validator/Create Rule/Scene Validator", priority = 10000)]
		private static void CreateRuleSceneValidator_MenuItem()
		{
			if (AssetDatabase.Contains(Selection.activeObject))
			{
				string selectedFolder = GetSelectedFolderPath();
				ValidatorScriptTemplates.CreateRuleSceneValidator(selectedFolder);
			}
		}

		[MenuItem("Assets/Odin Validator/Create Validator/Root Object Validator", priority = 10000)]
		private static void CreateStandardRootObjectValidator_MenuItem()
		{
			if (AssetDatabase.Contains(Selection.activeObject))
			{
				string selectedFolder = GetSelectedFolderPath();
				ValidatorScriptTemplates.CreateStandardRootObjectValidator(selectedFolder);
			}
		}

		[MenuItem("Assets/Odin Validator/Create Validator/Global Validator", priority = 10000)]
		private static void CreateStandardGlobalObjectValidator_MenuItem()
		{
			if (AssetDatabase.Contains(Selection.activeObject))
			{
				string selectedFolder = GetSelectedFolderPath();
				ValidatorScriptTemplates.CreateStandardGlobalValidator(selectedFolder);
			}
		}

		[MenuItem("Assets/Odin Validator/Create Validator/Attribute Validator", priority = 10000)]
		private static void CreateStandardAttributeValidator_MenuItem()
		{
			if (AssetDatabase.Contains(Selection.activeObject))
			{
				string selectedFolder = GetSelectedFolderPath();
				ValidatorScriptTemplates.CreateStandardAttributeValidator(selectedFolder);
			}
		}

		[MenuItem("Assets/Odin Validator/Create Validator/Value Validator", priority = 10000)]
		private static void CreateStandardValueValidator_MenuItem()
		{
			if (AssetDatabase.Contains(Selection.activeObject))
			{
				string selectedFolder = GetSelectedFolderPath();
				ValidatorScriptTemplates.CreateStandardValueValidator(selectedFolder);
			}
		}

		[MenuItem("Assets/Odin Validator/Create Validator/Scene Validator", priority = 10000)]
		private static void CreateStandardSceneValidator_MenuItem()
		{
			if (AssetDatabase.Contains(Selection.activeObject))
			{
				string selectedFolder = GetSelectedFolderPath();
				ValidatorScriptTemplates.CreateStandardSceneValidator(selectedFolder);
			}
		}

		private static string GetSelectedFolderPath()
		{
			string path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if (Directory.Exists(path))
			{
				return path;
			}
			return Path.GetDirectoryName(path);
		}

		[InitializeOnLoadMethod]
		private static void DrawFilterIconsInProjectWindow()
		{
			EditorApplication.projectWindowItemOnGUI = (EditorApplication.ProjectWindowItemCallback)Delegate.Combine(EditorApplication.projectWindowItemOnGUI, (EditorApplication.ProjectWindowItemCallback)delegate(string guid, Rect aa)
			{
				ValidationSession validationSession = ValidationSession.ActiveValidationSessions.LastOrDefault();
				if ((bool)drawValidatorIconsInProjectGUI && validationSession != null)
				{
					SessionConfig config = validationSession.Config;
					bool flag = config.GetAssetsToValidate().Contains(guid) || config.GetSceneGuidsToValidate().Contains(guid);
					bool flag2 = config.ContainsIncludeItemWithAssetGuid(guid);
					bool flag3 = config.ContainsExcludeItemWithAssetGuid(guid);
					aa.y += 2f;
					aa.height = 12f;
					aa.x += aa.width - 16f - 2f;
					aa.width = 16f;
					if (flag)
					{
						SdfIcons.DrawIcon(aa, SdfIconType.CircleFill, SirenixGUIStyles.ValidatorGreen);
					}
					else
					{
						Color textColor = EditorStyles.label.normal.textColor;
						textColor.a *= 0.5f;
						SdfIcons.DrawIcon(aa, SdfIconType.CircleFill, textColor);
					}
					if (flag2 || flag3)
					{
						Color color;
						SdfIconType icon;
						if (flag3 && flag2)
						{
							color = SirenixGUIStyles.YellowWarningColor;
							icon = SdfIconType.ExclamationTriangleFill;
						}
						else if (flag2 && !flag && File.Exists(AssetDatabase.GUIDToAssetPath(guid)))
						{
							color = SirenixGUIStyles.YellowWarningColor;
							icon = SdfIconType.ExclamationTriangleFill;
						}
						else if (flag3)
						{
							icon = SdfIconType.X;
							color = SirenixGUIStyles.RedErrorColor;
						}
						else
						{
							icon = SdfIconType.Plus;
							color = SirenixGUIStyles.ValidatorGreen;
						}
						aa.x -= aa.width;
						SdfIcons.DrawIcon(aa, icon, color);
					}
				}
			});
		}
	}
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor.Validators
{
	public class SceneNotInBuildSettingsValidator : RootObjectValidator<SceneAsset>, IDefinesGenericMenuItems
	{
		public struct WatchedFolder
		{
			[FolderPath(UseBackslashes = true)]
			public string Path;

			public bool IncludeSubdirectories;
		}

		[Tooltip("The severity of the validation result.")]
		public ValidatorSeverity ValidatorSeverity = ValidatorSeverity.Warning;

		[Tooltip("Scenes to watch for. If a scene is in this list, it will be validated.")]
		[InfoBox("Please add Scenes/Folders which include Scenes that should be inside the build settings.", InfoMessageType.Info, null)]
		public List<SceneAsset> WatchedScenes = new List<SceneAsset>();

		[Tooltip("Folders to watch for scenes. If a scene is in one of these folders, it will be validated.")]
		public List<WatchedFolder> WatchedFolders = new List<WatchedFolder>();

		[Tooltip("If true, the scene must be enabled in the build settings.")]
		public bool MustBeEnabled = true;

		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			genericMenu.AddItem(new GUIContent("See Rule Settings/For me only"), on: false, delegate
			{
				ValidationSessionEditor.OpenRuleSettingsWindow(typeof(SceneNotInBuildSettingsValidator), ConfigSourceType.Local);
			});
			genericMenu.AddItem(new GUIContent("See Rule Settings/For everyone"), on: false, delegate
			{
				ValidationSessionEditor.OpenRuleSettingsWindow(typeof(SceneNotInBuildSettingsValidator), ConfigSourceType.Project);
			});
		}

		protected override void Validate(ValidationResult result)
		{
			string scenePath = AssetDatabase.GetAssetPath(base.Object);
			string folderPath = Path.GetDirectoryName(scenePath);
			if (!WatchedFolders.Any((WatchedFolder folder) => folder.Path == folderPath || (folder.IncludeSubdirectories && folderPath.StartsWith(folder.Path))) && !WatchedScenes.Contains(base.Object))
			{
				return;
			}
			EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
			int i;
			for (i = 0; i < buildScenes.Length; i++)
			{
				if (!(buildScenes[i].path == scenePath))
				{
					continue;
				}
				if (MustBeEnabled && !buildScenes[i].enabled)
				{
					result.Add(ValidatorSeverity, "Scene is not enabled in the build settings.").WithFix(Fix.Create("Enable in build settings", delegate
					{
						EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
						scenes[i].enabled = true;
						EditorBuildSettings.scenes = scenes;
					}));
				}
				return;
			}
			result.Add(ValidatorSeverity, "Scene is not included in the build settings.").WithFix(Fix.Create("Include in build settings", delegate
			{
				EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
				EditorBuildSettingsScene[] array = new EditorBuildSettingsScene[scenes.Length + 1];
				scenes.CopyTo(array, 0);
				array[scenes.Length] = new EditorBuildSettingsScene(scenePath, enabled: true);
				EditorBuildSettings.scenes = array;
			}));
		}
	}
}

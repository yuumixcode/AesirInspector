using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities.Editor.Expressions.Internal;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	public static class MigrationWizard
	{
		public class ProfileMigrationInfo
		{
			public bool CanMigrate;

			public bool HasBeenMigrated;

			public string Name;

			public Expressionator Profile;

			public ValidationProfile NewProfile;
		}

		public static List<ProfileMigrationInfo> ProfileInfos;

		public static void UpdateProfiles()
		{
			Expressionator[] profiles = OldValidator.API.ForEachExpressionate("Sirenix.OdinValidator.Editor.OdinValidationConfig.Instance.MainValidationProfiles").ToArray();
			List<ValidationProfile> newProfiles = ValidationProfile.FindAll().ToList();
			ProfileInfos = profiles.Select(delegate(Expressionator profile)
			{
				string name = profile.String("this.Name");
				return new ProfileMigrationInfo
				{
					Name = name,
					Profile = profile,
					NewProfile = newProfiles.FirstOrDefault((ValidationProfile newProfile) => newProfile.name == name),
					CanMigrate = OldValidator.IsMigrateableProfileType(profile)
				};
			}).ToList();
		}

		public static void DrawMigrateHooks()
		{
			Expressionator[] hooks = OldValidator.API.ForEachExpressionate("Sirenix.OdinValidator.Editor.OdinValidationConfig.Instance.Hooks").ToArray();
			Expressionator[] array = hooks;
			foreach (Expressionator hook in array)
			{
				GUILayout.Label("Hook to migrate: " + hook.String("this.Name"));
				if (OldValidator.CanMigrateOldHook(hook))
				{
					if (GUILayout.Button("Migrate"))
					{
						OldValidator.MigrateOldHookToNewEvents(hook);
					}
				}
				else
				{
					GUIHelper.PushGUIEnabled(enabled: false);
					GUILayout.Button("Migration not available for custom hook event");
					GUIHelper.PopGUIEnabled();
				}
			}
		}

		public static void DrawMigrateProfiles()
		{
			if (ProfileInfos == null)
			{
				UpdateProfiles();
			}
			for (int i = 0; i < ProfileInfos.Count; i++)
			{
				ProfileMigrationInfo info = ProfileInfos[i];
				Rect rect = EditorGUILayout.BeginVertical(SirenixGUIStyles.ContentPadding);
				if (Event.current.type == EventType.Repaint)
				{
					EditorGUI.DrawRect(rect.Expand(3f), SirenixGUIStyles.ListItemColorEven);
					SirenixEditorGUI.DrawBorders(rect.Expand(3f), 1, SirenixGUIStyles.BorderColor * 0.3f);
				}
				string name = info.Profile.String("this.Name");
				string desc = info.Profile.String("this.Description");
				GUILayout.Label(name, SirenixGUIStyles.BoldTitle);
				GUILayout.Label(desc, SirenixGUIStyles.MultiLineLabel);
				GUILayout.Space(5f);
				if (info.HasBeenMigrated)
				{
					GUIHelper.PushColor(Color.green);
					if (GUILayout.Button("Migration complete!"))
					{
						GUIHelper.PingObject(info.NewProfile);
						Selection.activeObject = info.NewProfile;
					}
					GUIHelper.PopColor();
				}
				else if (!info.CanMigrate)
				{
					GUIHelper.PushGUIEnabled(enabled: false);
					GUILayout.Button("Migration unavailable for custom profiles");
					GUIHelper.PopGUIEnabled();
				}
				else if (info.NewProfile != null)
				{
					if (GUILayout.Button("Update existing profile '" + name + "'"))
					{
						OldValidator.MigrateOldProfileToNewProfile(info.Profile, info.NewProfile);
						EditorUtility.SetDirty(info.NewProfile);
						AssetDatabase.SaveAssets();
						info.HasBeenMigrated = true;
					}
				}
				else if (GUILayout.Button("Create new profile '" + name + "'"))
				{
					string path = ValidationProfile.DefaultConfigFolderPath + name + ".asset";
					ValidationProfile newProfile = ScriptableObject.CreateInstance<ValidationProfile>();
					path = AssetDatabase.GenerateUniqueAssetPath(path);
					OldValidator.MigrateOldProfileToNewProfile(info.Profile, newProfile);
					AssetDatabase.CreateAsset(newProfile, path);
					AssetDatabase.SaveAssets();
					info.NewProfile = newProfile;
					info.HasBeenMigrated = true;
				}
				EditorGUILayout.EndVertical();
				if (ProfileInfos.Count - 1 != i)
				{
					GUILayout.Space(10f);
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor.Expressions.Internal;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	public static class OldValidator
	{
		[Flags]
		public enum OldActionEnumAlias
		{
			OpenValidatorIfError = 1,
			OpenValidatorIfWarning = 2,
			StopHookEventOnError = 4,
			StopHookEventOnWarning = 8,
			LogError = 0x10,
			LogWarning = 0x20
		}

		private static readonly Type Old_OdinValidationConfig_Type = TwoWaySerializationBinder.Default.BindToType("Sirenix.OdinValidator.Editor.OdinValidationConfig");

		public static bool IsInstalled = Old_OdinValidationConfig_Type != null;

		public static readonly Expressionator API = new Expressionator(null);

		public static bool IsMigrateableProfileType(Expressionator oldProfile)
		{
			if (oldProfile.Bool("this is Sirenix.OdinValidator.Editor.ValidationProfileAsset"))
			{
				return oldProfile.Bool("this.Profile is Sirenix.OdinValidator.Editor.ValidationCollectionProfile || this.Profile is Sirenix.OdinValidator.Editor.AssetValidationProfile || this.Profile is Sirenix.OdinValidator.Editor.SceneValidationProfile");
			}
			return oldProfile.Bool("this is Sirenix.OdinValidator.Editor.ValidationCollectionProfile || this is Sirenix.OdinValidator.Editor.AssetValidationProfile || this is Sirenix.OdinValidator.Editor.SceneValidationProfile");
		}

		public static void MigrateOldProfileToNewProfile(Expressionator oldProfile, ValidationProfile newProfile, bool clearFirst = true)
		{
			if (clearFirst)
			{
				newProfile.Include?.Clear();
				newProfile.Exclude?.Clear();
			}
			if (oldProfile.Bool("this is Sirenix.OdinValidator.Editor.ValidationProfileAsset"))
			{
				oldProfile = oldProfile.Expressionate("this.Profile");
			}
			if (oldProfile.Bool("this is Sirenix.OdinValidator.Editor.ValidationCollectionProfile"))
			{
				foreach (Expressionator subProfile in oldProfile.ForEachExpressionate("this.Profiles"))
				{
					MigrateOldProfileToNewProfile(subProfile, newProfile, clearFirst: false);
				}
				return;
			}
			if (oldProfile.Bool("this is Sirenix.OdinValidator.Editor.AssetValidationProfile"))
			{
				string[] filters = (from n in oldProfile.Expr<string[]>("this.SearchFilters")?.Where((string n) => !string.IsNullOrWhiteSpace(n))
					select n.Trim()).ToArray();
				string filter = null;
				if (filters != null && filters.Length != 0)
				{
					filter = string.Join(" ", filters);
				}
				string[] array = oldProfile.Expr<string[]>("this.AssetPaths") ?? Array.Empty<string>();
				foreach (string assetPath in array)
				{
					if (assetPath != null)
					{
						newProfile.Include.Add(ValidationItem.FromAssetPath(assetPath, filter));
					}
				}
				UnityEngine.Object[] array2 = oldProfile.Expr<UnityEngine.Object[]>("this.AssetReferences") ?? Array.Empty<UnityEngine.Object>();
				foreach (UnityEngine.Object objReference in array2)
				{
					if (!(objReference == null))
					{
						if (ValidationItem.TryMakeAssetItemFromUnityObjectReference(objReference, out var item))
						{
							newProfile.Include.Add(item);
							continue;
						}
						Debug.LogError("Failed to migrate included object reference entry in profile " + oldProfile.String("this.Name") + " because it was not an asset; the object reference was named '" + objReference.name + "'.", objReference);
					}
				}
				string[] array3 = oldProfile.Expr<string[]>("this.ExcludeAssetPaths") ?? Array.Empty<string>();
				foreach (string assetPath2 in array3)
				{
					if (assetPath2 != null)
					{
						newProfile.Exclude.Add(ValidationItem.FromAssetPath(assetPath2, filter));
					}
				}
				UnityEngine.Object[] array4 = oldProfile.Expr<UnityEngine.Object[]>("this.ExcludeAssetReferences") ?? Array.Empty<UnityEngine.Object>();
				foreach (UnityEngine.Object objReference2 in array4)
				{
					if (!(objReference2 == null))
					{
						if (ValidationItem.TryMakeAssetItemFromUnityObjectReference(objReference2, out var item2))
						{
							newProfile.Exclude.Add(item2);
							continue;
						}
						Debug.LogError("Failed to migrate excluded object reference entry in profile " + oldProfile.String("this.Name") + " because it was not an asset; the object reference was named '" + objReference2.name + "'.", objReference2);
					}
				}
				return;
			}
			if (oldProfile.Bool("this is Sirenix.OdinValidator.Editor.SceneValidationProfile"))
			{
				bool includeAssetDependencies = oldProfile.Bool("this.IncludeAssetDependencies");
				if (oldProfile.Bool("this.IncludeScenesFromBuildOptions"))
				{
					newProfile.Include.Add(ValidationItem.FromScenesInBuildOptions(includeAssetDependencies));
				}
				if (oldProfile.Bool("this.IncludeOpenScenes"))
				{
					newProfile.Include.Add(ValidationItem.FromOpenScenes(includeAssetDependencies));
				}
				string[] array5 = oldProfile.Expr<string[]>("this.ScenePaths") ?? Array.Empty<string>();
				foreach (string scenePath in array5)
				{
					if (scenePath != null)
					{
						newProfile.Include.Add(ValidationItem.FromScenePath(scenePath, includeAssetDependencies));
					}
				}
				string[] array6 = oldProfile.Expr<string[]>("this.ExcludeScenePaths") ?? Array.Empty<string>();
				foreach (string scenePath2 in array6)
				{
					if (scenePath2 != null)
					{
						newProfile.Exclude.Add(ValidationItem.FromScenePath(scenePath2, includeAssetDependencies));
					}
				}
				return;
			}
			throw new NotSupportedException("Cannot migrate profiles of type '" + oldProfile.ValueType.GetNiceName() + "'!");
		}

		public static bool CanMigrateOldHook(Expressionator oldHook)
		{
			return oldHook.Bool("this.Hook is Sirenix.OdinValidator.Editor.OnBuildValidationHook || this.Hook is Sirenix.OdinValidator.Editor.OnPlayValidationHook || this.Hook is Sirenix.OdinValidator.Editor.OnProjectStartupValidationHook");
		}

		public static void MigrateOldHookToNewEvents(Expressionator oldHook)
		{
			string oldHookName = oldHook.String("this.Name");
			if (oldHook.Bool("this.Hook is Sirenix.OdinValidator.Editor.OnBuildValidationHook"))
			{
				GlobalConfig<AutomationConfig>.Instance.OnBuild = oldHook.Bool("this.Enabled");
				GlobalConfig<AutomationConfig>.Instance.OnBuildAlwaysCompleteValidationFully = oldHook.Bool("this.FinishValidationOnFailures");
				if (oldHook.Bool("this.Validations != null && this.Validations.Count > 1"))
				{
					Debug.LogError("Migration of validation hook events with multiple validations to run is not supported. Only the first validation to run on the hook '" + oldHook.String("this.Name") + "' has been migrated.");
				}
				Expressionator oldValidationToRun = oldHook.Expressionate("this.Validation");
				OldActionEnumAlias actions = oldValidationToRun.Expr<OldActionEnumAlias>("(Sirenix.OdinValidator.Editor.OldValidator.OldActionEnumAlias)(int)this.Actions");
				GlobalConfig<AutomationConfig>.Instance.OnBuildIfErrors = AutomationConfig.OnBuildActions.None;
				if ((actions & OldActionEnumAlias.OpenValidatorIfError) != 0)
				{
					GlobalConfig<AutomationConfig>.Instance.OnBuildIfErrors |= AutomationConfig.OnBuildActions.OpenValidator;
				}
				if ((actions & OldActionEnumAlias.StopHookEventOnError) != 0)
				{
					GlobalConfig<AutomationConfig>.Instance.OnBuildIfErrors |= AutomationConfig.OnBuildActions.StopBuild;
				}
				if ((actions & OldActionEnumAlias.LogError) != 0)
				{
					GlobalConfig<AutomationConfig>.Instance.OnBuildIfErrors |= AutomationConfig.OnBuildActions.LogToConsole;
				}
				GlobalConfig<AutomationConfig>.Instance.OnBuildIfWarnings = AutomationConfig.OnBuildActions.None;
				if ((actions & OldActionEnumAlias.OpenValidatorIfWarning) != 0)
				{
					GlobalConfig<AutomationConfig>.Instance.OnBuildIfWarnings |= AutomationConfig.OnBuildActions.OpenValidator;
				}
				if ((actions & OldActionEnumAlias.StopHookEventOnWarning) != 0)
				{
					GlobalConfig<AutomationConfig>.Instance.OnBuildIfWarnings |= AutomationConfig.OnBuildActions.StopBuild;
				}
				if ((actions & OldActionEnumAlias.LogWarning) != 0)
				{
					GlobalConfig<AutomationConfig>.Instance.OnBuildIfWarnings |= AutomationConfig.OnBuildActions.LogToConsole;
				}
				List<ValidationProfile> allNewProfiles = ValidationProfile.FindAll().ToList();
				List<Expressionator> oldProfilesToRun = oldValidationToRun.ForEachExpressionate("this.ProfilesToRun").ToList();
				if (oldProfilesToRun.Count <= 0)
				{
					return;
				}
				if (oldProfilesToRun.Count > 1)
				{
					Debug.LogError("Migration of validation hook events with multiple validation profiles to run is not supported. Only the first profile to run on the hook '" + oldHookName + "' has been migrated.");
				}
				Expressionator oldProfile = oldProfilesToRun[0];
				string name = oldProfile.String("this.Name");
				ValidationProfile newProfile = allNewProfiles.FirstOrDefault((ValidationProfile n) => n.name == name);
				if (newProfile == null)
				{
					if (IsMigrateableProfileType(oldProfile))
					{
						if (EditorUtility.DisplayDialog("Corresponding validation profile missing!", "The old validation hook " + oldHookName + " was running the old validation profile " + oldProfile.String("this.Name") + ", but no corresponding new profile with that name could be found. Would you like to create it now? If you press no, no profile to run will be set to the " + oldHookName + " event.", "Yes, create and assign new profile", "No, don't assign any profile"))
						{
							newProfile = ScriptableObject.CreateInstance<ValidationProfile>();
							MigrateOldProfileToNewProfile(oldProfile, newProfile);
							string path = ValidationProfile.DefaultConfigFolderPath + name + ".asset";
							path = AssetDatabase.GenerateUniqueAssetPath(path);
							AssetDatabase.CreateAsset(newProfile, path);
							AssetDatabase.SaveAssets();
						}
					}
					else
					{
						Debug.LogError("Could not automatically migrate old profile " + oldProfile.String("this.Name") + " of custom profile type " + oldProfile.Expr<Type>("this.GetType()").GetNiceName() + ". No profile to run has been migrated for hook event " + oldHookName + ".");
					}
				}
				if (newProfile != null)
				{
					GlobalConfig<AutomationConfig>.Instance.OnBuildSetup.Profile = AutomationConfig.ValidationSetup.RunKind.Custom;
					GlobalConfig<AutomationConfig>.Instance.OnBuildSetup.ProfileAsset = newProfile;
					EditorUtility.SetDirty(GlobalConfig<AutomationConfig>.Instance);
					AssetDatabase.SaveAssets();
				}
			}
			else if (oldHook.Bool("this.Hook is Sirenix.OdinValidator.Editor.OnPlayValidationHook"))
			{
				GlobalConfig<AutomationConfig>.Instance.OnPlayMode = oldHook.Bool("this.Enabled");
				GlobalConfig<AutomationConfig>.Instance.OnPlayModeAlwaysCompleteValidationFully = oldHook.Bool("this.FinishValidationOnFailures");
				if (oldHook.Bool("this.Validations != null && this.Validations.Count > 1"))
				{
					Debug.LogError("Migration of validation hook events with multiple validations to run is not supported. Only the first validation to run on the hook '" + oldHook.String("this.Name") + "' has been migrated.");
				}
				Expressionator oldValidationToRun2 = oldHook.Expressionate("this.Validation");
				OldActionEnumAlias actions2 = oldValidationToRun2.Expr<OldActionEnumAlias>("(Sirenix.OdinValidator.Editor.OldValidator.OldActionEnumAlias)(int)this.Actions");
				GlobalConfig<AutomationConfig>.Instance.OnPlayModeIfErrors = AutomationConfig.OnPlayModeActions.None;
				if ((actions2 & OldActionEnumAlias.OpenValidatorIfError) != 0)
				{
					GlobalConfig<AutomationConfig>.Instance.OnPlayModeIfErrors |= AutomationConfig.OnPlayModeActions.OpenValidator;
				}
				if ((actions2 & OldActionEnumAlias.StopHookEventOnError) != 0)
				{
					GlobalConfig<AutomationConfig>.Instance.OnPlayModeIfErrors |= AutomationConfig.OnPlayModeActions.StopPlayMode;
				}
				if ((actions2 & OldActionEnumAlias.LogError) != 0)
				{
					GlobalConfig<AutomationConfig>.Instance.OnPlayModeIfErrors |= AutomationConfig.OnPlayModeActions.LogToConsole;
				}
				GlobalConfig<AutomationConfig>.Instance.OnPlayModeIfWarnings = AutomationConfig.OnPlayModeActions.None;
				if ((actions2 & OldActionEnumAlias.OpenValidatorIfWarning) != 0)
				{
					GlobalConfig<AutomationConfig>.Instance.OnPlayModeIfWarnings |= AutomationConfig.OnPlayModeActions.OpenValidator;
				}
				if ((actions2 & OldActionEnumAlias.StopHookEventOnWarning) != 0)
				{
					GlobalConfig<AutomationConfig>.Instance.OnPlayModeIfWarnings |= AutomationConfig.OnPlayModeActions.StopPlayMode;
				}
				if ((actions2 & OldActionEnumAlias.LogWarning) != 0)
				{
					GlobalConfig<AutomationConfig>.Instance.OnPlayModeIfWarnings |= AutomationConfig.OnPlayModeActions.LogToConsole;
				}
				List<ValidationProfile> allNewProfiles2 = ValidationProfile.FindAll().ToList();
				List<Expressionator> oldProfilesToRun2 = oldValidationToRun2.ForEachExpressionate("this.ProfilesToRun").ToList();
				if (oldProfilesToRun2.Count <= 0)
				{
					return;
				}
				if (oldProfilesToRun2.Count > 1)
				{
					Debug.LogError("Migration of validation hook events with multiple validation profiles to run is not supported. Only the first profile to run on the hook '" + oldHookName + "' has been migrated.");
				}
				Expressionator oldProfile2 = oldProfilesToRun2[0];
				string name2 = oldProfile2.String("this.Name");
				ValidationProfile newProfile2 = allNewProfiles2.FirstOrDefault((ValidationProfile n) => n.name == name2);
				if (newProfile2 == null)
				{
					if (IsMigrateableProfileType(oldProfile2))
					{
						if (EditorUtility.DisplayDialog("Corresponding validation profile missing!", "The old validation hook " + oldHookName + " was running the old validation profile " + oldProfile2.String("this.Name") + ", but no corresponding new profile with that name could be found. Would you like to create it now? If you press no, no profile to run will be set to the " + oldHookName + " event.", "Yes, create and assign new profile", "No, don't assign any profile"))
						{
							newProfile2 = ScriptableObject.CreateInstance<ValidationProfile>();
							MigrateOldProfileToNewProfile(oldProfile2, newProfile2);
							string path2 = ValidationProfile.DefaultConfigFolderPath + name2 + ".asset";
							path2 = AssetDatabase.GenerateUniqueAssetPath(path2);
							AssetDatabase.CreateAsset(newProfile2, path2);
							AssetDatabase.SaveAssets();
						}
					}
					else
					{
						Debug.LogError("Could not automatically migrate old profile " + oldProfile2.String("this.Name") + " of custom profile type " + oldProfile2.Expr<Type>("this.GetType()").GetNiceName() + ". No profile to run has been migrated for hook event " + oldHookName + ".");
					}
				}
				if (newProfile2 != null)
				{
					GlobalConfig<AutomationConfig>.Instance.OnPlayModeSetup.Profile = AutomationConfig.ValidationSetup.RunKind.Custom;
					GlobalConfig<AutomationConfig>.Instance.OnPlayModeSetup.ProfileAsset = newProfile2;
					EditorUtility.SetDirty(GlobalConfig<AutomationConfig>.Instance);
					AssetDatabase.SaveAssets();
				}
			}
			else
			{
				if (!oldHook.Bool("this.Hook is Sirenix.OdinValidator.Editor.OnProjectStartupValidationHook"))
				{
					return;
				}
				GlobalConfig<AutomationConfig>.Instance.OnProjectStartup = oldHook.Bool("this.Enabled");
				GlobalConfig<AutomationConfig>.Instance.OnProjectStartupAlwaysCompleteValidationFully = oldHook.Bool("this.FinishValidationOnFailures");
				if (oldHook.Bool("this.Validations != null && this.Validations.Count > 1"))
				{
					Debug.LogError("Migration of validation hook events with multiple validations to run is not supported. Only the first validation to run on the hook '" + oldHook.String("this.Name") + "' has been migrated.");
				}
				Expressionator oldValidationToRun3 = oldHook.Expressionate("this.Validation");
				OldActionEnumAlias actions3 = oldValidationToRun3.Expr<OldActionEnumAlias>("(Sirenix.OdinValidator.Editor.OldValidator.OldActionEnumAlias)(int)this.Actions");
				GlobalConfig<AutomationConfig>.Instance.OnProjectStartupIfErrors = AutomationConfig.OnProjectStartupActions.None;
				if ((actions3 & OldActionEnumAlias.OpenValidatorIfError) != 0)
				{
					GlobalConfig<AutomationConfig>.Instance.OnProjectStartupIfErrors |= AutomationConfig.OnProjectStartupActions.OpenValidator;
				}
				if ((actions3 & OldActionEnumAlias.LogError) != 0)
				{
					GlobalConfig<AutomationConfig>.Instance.OnProjectStartupIfErrors |= AutomationConfig.OnProjectStartupActions.LogToConsole;
				}
				GlobalConfig<AutomationConfig>.Instance.OnProjectStartupIfWarnings = AutomationConfig.OnProjectStartupActions.None;
				if ((actions3 & OldActionEnumAlias.OpenValidatorIfWarning) != 0)
				{
					GlobalConfig<AutomationConfig>.Instance.OnProjectStartupIfWarnings |= AutomationConfig.OnProjectStartupActions.OpenValidator;
				}
				if ((actions3 & OldActionEnumAlias.LogWarning) != 0)
				{
					GlobalConfig<AutomationConfig>.Instance.OnProjectStartupIfWarnings |= AutomationConfig.OnProjectStartupActions.LogToConsole;
				}
				List<ValidationProfile> allNewProfiles3 = ValidationProfile.FindAll().ToList();
				List<Expressionator> oldProfilesToRun3 = oldValidationToRun3.ForEachExpressionate("this.ProfilesToRun").ToList();
				if (oldProfilesToRun3.Count <= 0)
				{
					return;
				}
				if (oldProfilesToRun3.Count > 1)
				{
					Debug.LogError("Migration of validation hook events with multiple validation profiles to run is not supported. Only the first profile to run on the hook '" + oldHookName + "' has been migrated.");
				}
				Expressionator oldProfile3 = oldProfilesToRun3[0];
				string name3 = oldProfile3.String("this.Name");
				ValidationProfile newProfile3 = allNewProfiles3.FirstOrDefault((ValidationProfile n) => n.name == name3);
				if (newProfile3 == null)
				{
					if (IsMigrateableProfileType(oldProfile3))
					{
						if (EditorUtility.DisplayDialog("Corresponding validation profile missing!", "The old validation hook " + oldHookName + " was running the old validation profile " + oldProfile3.String("this.Name") + ", but no corresponding new profile with that name could be found. Would you like to create it now? If you press no, no profile to run will be set to the " + oldHookName + " event.", "Yes, create and assign new profile", "No, don't assign any profile"))
						{
							newProfile3 = ScriptableObject.CreateInstance<ValidationProfile>();
							MigrateOldProfileToNewProfile(oldProfile3, newProfile3);
							string path3 = ValidationProfile.DefaultConfigFolderPath + name3 + ".asset";
							path3 = AssetDatabase.GenerateUniqueAssetPath(path3);
							AssetDatabase.CreateAsset(newProfile3, path3);
							AssetDatabase.SaveAssets();
						}
					}
					else
					{
						Debug.LogError("Could not automatically migrate old profile " + oldProfile3.String("this.Name") + " of custom profile type " + oldProfile3.Expr<Type>("this.GetType()").GetNiceName() + ". No profile to run has been migrated for hook event " + oldHookName + ".");
					}
				}
				if (newProfile3 != null)
				{
					GlobalConfig<AutomationConfig>.Instance.OnProjectStartupSetup.Profile = AutomationConfig.ValidationSetup.RunKind.Custom;
					GlobalConfig<AutomationConfig>.Instance.OnProjectStartupSetup.ProfileAsset = newProfile3;
					EditorUtility.SetDirty(GlobalConfig<AutomationConfig>.Instance);
					AssetDatabase.SaveAssets();
				}
			}
		}

		public static void DeleteOldValidator()
		{
			if (IsInstalled)
			{
				string validatorPath = SirenixAssetPaths.SirenixPluginPath + "Odin Validator";
				DeleteAsset(validatorPath + "/Editor/Config/OdinValidationConfig.asset");
				string[] array = AssetDatabase.FindAssets("t:Sirenix.OdinValidator.Editor.ValidationProfileAsset");
				foreach (string guid in array)
				{
					DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
				}
				DeleteAsset(validatorPath + "/Editor/Scripts");
				AssetDatabase.Refresh();
			}
		}

		private static void DeleteAsset(string path)
		{
			bool deleted = false;
			for (int i = 0; i < 10; i++)
			{
				try
				{
					if (File.Exists(path))
					{
						File.Delete(path);
					}
					else if (Directory.Exists(path))
					{
						Directory.Delete(path, recursive: true);
					}
					deleted = true;
				}
				catch
				{
					continue;
				}
				break;
			}
			if (!deleted)
			{
				return;
			}
			for (int j = 0; j < 10; j++)
			{
				try
				{
					if (File.Exists(path + ".meta"))
					{
						File.Delete(path + ".meta");
					}
					break;
				}
				catch
				{
				}
			}
		}
	}
}

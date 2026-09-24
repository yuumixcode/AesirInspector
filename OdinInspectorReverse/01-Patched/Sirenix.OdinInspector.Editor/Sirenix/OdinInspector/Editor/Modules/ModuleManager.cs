using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Modules
{
	public class ModuleManager
	{
		public class ModuleNotFoundException : Exception
		{
			public ModuleNotFoundException(string moduleId)
				: base("Module '" + moduleId + "' could not be found.")
			{
			}
		}

		public ModuleDataManager DataManager;

		public List<ModuleDefinition> Modules;

		public static ModuleManager CreateDefault()
		{
			ModuleManager result = new ModuleManager
			{
				DataManager = new ModuleDataManager
				{
					DataPath = SirenixAssetPaths.OdinPath + "Modules",
					InstallPath = SirenixAssetPaths.OdinPath + "Modules"
				},
				Modules = new List<ModuleDefinition>()
			};
			result.Modules.Add(new UnityMathematicsModuleDefinition
			{
				ModuleManager = result
			});
			result.Modules.Add(new UnityLocalizationModuleDefinition
			{
				ModuleManager = result
			});
			result.Modules.Add(new UnityAddressablesModuleDefinition
			{
				ModuleManager = result
			});
			result.Modules.Add(new ECSModuleDefinition
			{
				ModuleManager = result
			});
			return result;
		}

		public bool Refresh()
		{
			Version currentOdinVersion = OdinInspectorVersion.GetOnDiskVersion();
			bool changed = false;
			foreach (ModuleDefinition module in Modules)
			{
				bool activated = module.CheckIsActivated();
				bool supported = module.CheckSupportsCurrentEnvironment();
				if (activated && !supported)
				{
					bool automate = false;
					if (GlobalConfig<OdinModuleConfig>.HasInstanceLoaded)
					{
						ModuleConfiguration config = GlobalConfig<OdinModuleConfig>.Instance.GetConfig(module);
						if (config != null && config.ActivationSettings == ActivationSettings.GlobalSettings)
						{
							automate = ((GlobalConfig<OdinModuleConfig>.Instance.ModuleTogglingSettings != OdinModuleConfig.ModuleAutomationSettings.Ask) ? (GlobalConfig<OdinModuleConfig>.Instance.ModuleTogglingSettings == OdinModuleConfig.ModuleAutomationSettings.Automatic) : AskActivationAutomationQuestion(module, activate: false, GlobalConfig<OdinModuleConfig>.Instance));
						}
					}
					if (automate)
					{
						Debug.Log("Automatically deactivating Odin Module '" + module.NiceName + "' because its dependencies have gone missing...");
						module.Deactivate();
						changed = true;
					}
				}
				else if (!activated && supported)
				{
					bool automate2 = false;
					if (!module.UnstableExperimental && GlobalConfig<OdinModuleConfig>.HasInstanceLoaded)
					{
						ModuleConfiguration config2 = GlobalConfig<OdinModuleConfig>.Instance.GetConfig(module);
						if (config2 != null && config2.ActivationSettings == ActivationSettings.GlobalSettings)
						{
							automate2 = GlobalConfig<OdinModuleConfig>.Instance.ModuleTogglingSettings switch
							{
								OdinModuleConfig.ModuleAutomationSettings.Ask => AskActivationAutomationQuestion(module, activate: true, GlobalConfig<OdinModuleConfig>.Instance), 
								OdinModuleConfig.ModuleAutomationSettings.Automatic => true, 
								_ => false, 
							};
						}
					}
					if (automate2)
					{
						Debug.Log("Automatically activating Odin Module '" + module.NiceName + "' version '" + module.LatestVersion?.ToString() + "', because its dependencies were detected...");
						module.Activate();
						changed = true;
					}
				}
				else
				{
					if (!activated)
					{
						continue;
					}
					ModuleManifest manifest = module.LoadManifest();
					ModuleDefinitionInfo moduleInfo = module.LoadInfo();
					if (currentOdinVersion != null && moduleInfo.RequiresOdinVersion != null && currentOdinVersion >= moduleInfo.RequiresOdinVersion)
					{
						bool isIncompatibleWithCurrentOdin = true;
						if (manifest != null && manifest.RequiresOdinVersion != null && manifest.RequiresOdinVersion >= moduleInfo.RequiresOdinVersion)
						{
							isIncompatibleWithCurrentOdin = false;
						}
						if (isIncompatibleWithCurrentOdin)
						{
							switch (GlobalConfig<OdinModuleConfig>.Instance.ModuleUpdateSettings)
							{
							case OdinModuleConfig.ModuleAutomationSettings.Ask:
							case OdinModuleConfig.ModuleAutomationSettings.Manual:
								switch (EditorUtility.DisplayDialogComplex("Odin Module compatibility", $"The installed '{module.NiceName}' module isn't compatible with this version of the Odin Inspector ({currentOdinVersion}) and may cause compile errors.\n\n" + $"Update it to the latest version ({moduleInfo.CurrentVersion}), uninstall it, or cancel and handle it manually?", "Update", "Cancel", "Uninstall"))
								{
								case 0:
									module.Deactivate();
									module.Activate();
									changed = true;
									break;
								case 2:
									module.Deactivate();
									changed = true;
									break;
								}
								break;
							case OdinModuleConfig.ModuleAutomationSettings.Automatic:
								Debug.Log(string.Format("Automatically updating Odin Module '{0}' {1} '{2}', due to the current version of the Odin Inspector ({3}) not being compatible with the current installed version anymore.", module.NiceName, (manifest != null) ? $"from version '{manifest.Version}' to" : "to version", moduleInfo.CurrentVersion, currentOdinVersion));
								module.Deactivate();
								module.Activate();
								changed = true;
								break;
							}
							continue;
						}
					}
					if (manifest == null)
					{
						continue;
					}
					if (moduleInfo.CurrentVersion != null && manifest.Version > moduleInfo.CurrentVersion && (manifest.RequiresOdinVersion == null || (currentOdinVersion != null && currentOdinVersion < manifest.RequiresOdinVersion)))
					{
						switch (GlobalConfig<OdinModuleConfig>.Instance.ModuleUpdateSettings)
						{
						case OdinModuleConfig.ModuleAutomationSettings.Ask:
						case OdinModuleConfig.ModuleAutomationSettings.Automatic:
							switch (EditorUtility.DisplayDialogComplex("Odin Module compatibility", $"The installed '{module.NiceName}' module ({manifest.Version}) is newer than the version bundled with your Odin Inspector; the bundled module version is {moduleInfo.CurrentVersion}. This may cause compile errors.\n\n" + "Downgrade it to the bundled version, uninstall it, or cancel and handle it manually?\n\nYou can set the module update setting to Manual to stop this dialog from appearing.", "Downgrade", "Cancel", "Uninstall"))
							{
							case 0:
								module.Deactivate();
								module.Activate();
								changed = true;
								break;
							case 2:
								module.Deactivate();
								changed = true;
								break;
							}
							break;
						}
					}
					else if (manifest.Version < module.LatestVersion)
					{
						bool automate3 = false;
						if (GlobalConfig<OdinModuleConfig>.Instance.ModuleUpdateSettings switch
						{
							OdinModuleConfig.ModuleAutomationSettings.Ask => AskUpdateAutomationQuestion(module, manifest.Version, GlobalConfig<OdinModuleConfig>.Instance), 
							OdinModuleConfig.ModuleAutomationSettings.Automatic => true, 
							_ => false, 
						})
						{
							Debug.Log("Automatically updating Odin Module '" + module.NiceName + "' from version '" + manifest.Version?.ToString() + "' to '" + module.LatestVersion?.ToString() + "'");
							module.Deactivate();
							module.Activate();
							changed = true;
						}
					}
				}
			}
			return changed;
		}

		private bool AskActivationAutomationQuestion(ModuleDefinition module, bool activate, OdinModuleConfig instance)
		{
			string dependencies = "";
			if (!string.IsNullOrEmpty(module.DependenciesDescription))
			{
				dependencies = " (" + module.DependenciesDescription + ")";
			}
			string message = (activate ? ("The Odin Module '" + module.NiceName + "' is not activated, but the conditions for its activation have been detected" + dependencies + ". Do you want to automatically activate the module?") : ("The Odin Module '" + module.NiceName + "' is activated, but some of its dependencies" + dependencies + " are missing and you will likely get compiler errors. Do you want to automatically deactivate the module?"));
			switch (EditorUtility.DisplayDialogComplex("Odin Module automation", message, "Always automate modules", "Just this once", "Never do this"))
			{
			case 0:
				instance.ModuleTogglingSettings = OdinModuleConfig.ModuleAutomationSettings.Automatic;
				EditorUtility.SetDirty(instance);
				AssetDatabase.SaveAssets();
				return true;
			case 1:
				return true;
			default:
				instance.ModuleTogglingSettings = OdinModuleConfig.ModuleAutomationSettings.Manual;
				EditorUtility.SetDirty(instance);
				AssetDatabase.SaveAssets();
				return false;
			}
		}

		private bool AskUpdateAutomationQuestion(ModuleDefinition module, Version old, OdinModuleConfig instance)
		{
			string message = "The installed Odin Module '" + module.NiceName + "' is out of date (" + old?.ToString() + "), and a new version is available (" + module.LatestVersion?.ToString() + "). Do you want to automatically update the module?";
			switch (EditorUtility.DisplayDialogComplex("Odin Module automation", message, "Always update modules", "Just this once", "Never do this"))
			{
			case 0:
				instance.ModuleUpdateSettings = OdinModuleConfig.ModuleAutomationSettings.Automatic;
				EditorUtility.SetDirty(instance);
				AssetDatabase.SaveAssets();
				return true;
			case 1:
				return true;
			default:
				instance.ModuleUpdateSettings = OdinModuleConfig.ModuleAutomationSettings.Manual;
				EditorUtility.SetDirty(instance);
				AssetDatabase.SaveAssets();
				return false;
			}
		}

		public void PackageAllModules()
		{
			foreach (ModuleDefinition module in Modules)
			{
				module.SaveInfo();
				ModuleData data = module.GetModuleDataForPackaging();
				byte[] bytes = ModuleData.Serialize(data);
				DataManager.SaveData(module.ID, bytes);
			}
			AssetDatabase.Refresh();
		}

		public ModuleDefinition GetModule(string moduleId)
		{
			for (int i = 0; i < Modules.Count; i++)
			{
				if (Modules[i].ID == moduleId)
				{
					return Modules[i];
				}
			}
			throw new ModuleNotFoundException(moduleId);
		}

		public bool ContainsModule(string moduleId)
		{
			for (int i = 0; i < Modules.Count; i++)
			{
				if (Modules[i].ID == moduleId)
				{
					return true;
				}
			}
			return false;
		}
	}
}

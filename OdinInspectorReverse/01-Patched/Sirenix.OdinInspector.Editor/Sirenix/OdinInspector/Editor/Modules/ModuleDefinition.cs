using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Modules
{
	public abstract class ModuleDefinition
	{
		private static Rect lastEnumButtonRectWhyUnity;

		private bool isActivatedCached;

		private bool supportsCurrentEnvironmentCached;

		private ModuleManifest installedManifestCached;

		private string statusStringCached;

		private static GUIStyle private_titleStyle;

		private static GUIStyle private_statusStyle;

		public ModuleManager ModuleManager;

		public static GUIStyle TitleStyle
		{
			get
			{
				if (private_titleStyle == null)
				{
					private_titleStyle = new GUIStyle(EditorStyles.largeLabel)
					{
						fontSize = 14,
						font = EditorStyles.boldFont
					};
				}
				return private_titleStyle;
			}
		}

		public static GUIStyle StatusStyle
		{
			get
			{
				if (private_statusStyle == null)
				{
					private_statusStyle = new GUIStyle(SirenixGUIStyles.SubtitleRight);
					private_statusStyle.margin.top += 7;
				}
				return private_statusStyle;
			}
		}

		public abstract string ID { get; }

		public abstract string NiceName { get; }

		public abstract Version LatestVersion { get; }

		public virtual Version RequiresOdinVersion => null;

		public abstract string Description { get; }

		public abstract string BuildFromPath { get; }

		public virtual string DocumentationLink => null;

		public virtual string DependenciesDescription => null;

		public virtual bool UnstableExperimental => false;

		public virtual void OnSelectedInInspector()
		{
			isActivatedCached = CheckIsActivated();
			supportsCurrentEnvironmentCached = CheckSupportsCurrentEnvironment();
			if (isActivatedCached)
			{
				installedManifestCached = LoadManifest();
			}
			statusStringCached = (isActivatedCached ? ("Installed (" + installedManifestCached.Version?.ToString() + ")") : ("Inactive (available: " + (LatestVersion ?? new Version(0, 0, 0, 0)).ToString() + ")"));
		}

		[OnInspectorGUI]
		[PropertyOrder(-10f)]
		protected virtual void DrawTitle()
		{
			GUILayout.Label(NiceName, TitleStyle);
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.Label(rect.AlignCenterY(16f).AddY(2f).SetXMin(rect.center.x), statusStringCached, StatusStyle);
			SirenixEditorGUI.HorizontalLineSeparator(SirenixGUIStyles.BorderColor);
		}

		[OnInspectorGUI]
		[PropertyOrder(-5f)]
		protected virtual void DrawDescription()
		{
			GUILayout.Label(Description, SirenixGUIStyles.MultiLineLabel);
			if (!string.IsNullOrEmpty(DependenciesDescription))
			{
				GUILayout.Space(6f);
				GUILayout.Label("Dependencies: " + DependenciesDescription, SirenixGUIStyles.MultiLineLabel);
			}
			if (!string.IsNullOrEmpty(DocumentationLink))
			{
				GUILayout.Space(6f);
				GUIHelper.PushColor(new Color(0.3f, 0.6f, 0.7f, 1f));
				if (GUILayout.Button("Go to documentation.", EditorStyles.label))
				{
					Application.OpenURL(DocumentationLink);
				}
				Rect btnRect = GUILayoutUtility.GetLastRect();
				SirenixEditorGUI.DrawSolidRect(btnRect.SetWidth(EditorStyles.label.CalcSize(GUIHelper.TempContent("Go to documentation.")).x).Expand(-2f).AlignBottom(1f)
					.AddY(0f), Color.white * 0.7f);
				GUIHelper.PopColor();
			}
		}

		[PropertyOrder(5f)]
		[OnInspectorGUI]
		protected virtual void DrawActivationButtons()
		{
			GUILayout.FlexibleSpace();
			if (!supportsCurrentEnvironmentCached)
			{
				GUIHelper.PushColor(new Color(64f / 85f, 0.2235294f, 0.1686275f, 1f) * 1.3f);
				GUILayout.Label("Dependencies are missing for this module" + ((DependenciesDescription != null) ? (": " + DependenciesDescription) : ""), SirenixGUIStyles.MultiLineLabel);
				GUIHelper.PopColor();
			}
			ModuleConfiguration config = GlobalConfig<OdinModuleConfig>.Instance.GetConfig(this);
			bool allowManual = false;
			if (config != null)
			{
				if (config.ActivationSettings == ActivationSettings.Manual)
				{
					allowManual = true;
				}
				else if (config.ActivationSettings == ActivationSettings.GlobalSettings && GlobalConfig<OdinModuleConfig>.Instance.ModuleTogglingSettings != OdinModuleConfig.ModuleAutomationSettings.Automatic)
				{
					allowManual = true;
				}
			}
			else
			{
				allowManual = true;
			}
			EditorGUILayout.BeginHorizontal();
			if (!isActivatedCached)
			{
				GUIHelper.PushGUIEnabled(allowManual);
				if (GUILayout.Button("Activate", SirenixGUIStyles.ButtonLeft))
				{
					Activate();
					isActivatedCached = true;
					AssetDatabase.Refresh();
					GUIHelper.ExitGUI(removeFocusControl: true);
				}
				GUIHelper.PopGUIEnabled();
			}
			else
			{
				GUIHelper.PushGUIEnabled(allowManual);
				if (GUILayout.Button("Deactivate", SirenixGUIStyles.ButtonLeft))
				{
					Deactivate();
					isActivatedCached = false;
					AssetDatabase.Refresh();
					GUIHelper.ExitGUI(removeFocusControl: true);
				}
				GUIHelper.PopGUIEnabled();
				if (installedManifestCached != null && installedManifestCached.Version < LatestVersion && GUILayout.Button("Update to " + LatestVersion, SirenixGUIStyles.ButtonMid))
				{
					Deactivate();
					Activate();
					AssetDatabase.Refresh();
					GUIHelper.ExitGUI(removeFocusControl: true);
				}
			}
			string labelText;
			if (config == null)
			{
				GUIHelper.PushGUIEnabled(enabled: false);
				labelText = "Activation Rule: Config Broken";
			}
			else
			{
				ActivationSettings activationSettings = config.ActivationSettings;
				labelText = ((activationSettings == ActivationSettings.GlobalSettings || activationSettings != ActivationSettings.Manual) ? "Activation Rule: Global Settings" : "Activation Rule: Manual");
			}
			bool buttonClick = GUILayout.Button(GUIHelper.TempContent(labelText), SirenixGUIStyles.ButtonRight, GUILayoutOptions.Width(250f));
			Rect rect = GUILayoutUtility.GetLastRect();
			if (rect.x != 0f && rect.y != 0f)
			{
				lastEnumButtonRectWhyUnity = rect;
			}
			if (buttonClick)
			{
				EnumSelector<ActivationSettings> selector = new EnumSelector<ActivationSettings>();
				selector.SelectionChanged += delegate(IEnumerable<ActivationSettings> values)
				{
					ActivationSettings activationSettings2 = values.FirstOrDefault();
					if (activationSettings2 != config.ActivationSettings)
					{
						config.ActivationSettings = activationSettings2;
						GlobalConfig<OdinModuleConfig>.Instance.SaveAssetChanges();
						OdinModuleConfig.RefreshModuleSetup();
					}
				};
				selector.ShowInPopup(new Vector2(lastEnumButtonRectWhyUnity.xMin, lastEnumButtonRectWhyUnity.yMax));
				SirenixEditorGUI.DrawSolidRect(rect, Color.green);
			}
			if (config == null)
			{
				GUIHelper.PopGUIEnabled();
			}
			EditorGUILayout.EndHorizontal();
		}

		public abstract bool CheckSupportsCurrentEnvironment();

		public virtual bool CheckIsActivated()
		{
			string path = ModuleManager.DataManager.InstallPath + "/" + ID + "/manifest.txt";
			return File.Exists(path);
		}

		protected virtual void OnBeforeActivate()
		{
		}

		protected virtual void OnAfterActivate()
		{
		}

		protected virtual void OnBeforeDeactivate()
		{
		}

		protected virtual void OnAfterDeactivate()
		{
		}

		public virtual void Activate()
		{
			if (CheckIsActivated())
			{
				return;
			}
			OnBeforeActivate();
			byte[] bytes = ModuleManager.DataManager.LoadData(ID);
			if (bytes == null)
			{
				throw new Exception("Could not load module data for module '" + ID + "': data could not be found.");
			}
			ModuleData data = ModuleData.Deserialize(bytes);
			if (bytes == null)
			{
				throw new Exception("Could not load module data for module '" + ID + "': data could not be parsed.");
			}
			string installFolder = ModuleManager.DataManager.InstallPath + "/" + ID;
			if (!Directory.Exists(installFolder))
			{
				Directory.CreateDirectory(installFolder);
			}
			foreach (ModuleData.ModuleFile file in data.Files)
			{
				string path = installFolder + "/" + file.Path;
				string dir = Path.GetDirectoryName(path);
				if (!Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
				}
				File.WriteAllBytes(path, file.Data);
			}
			string manifestPath = installFolder + "/manifest.txt";
			ModuleManifest manifest = data.ToManifest();
			ModuleManifest.Save(manifestPath, manifest);
			OnAfterActivate();
		}

		public virtual void Deactivate()
		{
			if (!CheckIsActivated())
			{
				return;
			}
			string installFolder = ModuleManager.DataManager.InstallPath + "/" + ID;
			string manifestPath = installFolder + "/manifest.txt";
			ModuleManifest manifest = ModuleManifest.Load(manifestPath);
			if (manifest == null)
			{
				throw new Exception("Could not load module manifest.");
			}
			OnBeforeDeactivate();
			foreach (string file in manifest.Files)
			{
				string fullPath = installFolder + "/" + file;
				if (File.Exists(fullPath))
				{
					File.Delete(fullPath);
				}
			}
			File.Delete(manifestPath);
			File.Delete(manifestPath + ".meta");
			DeleteIfEmpty(new DirectoryInfo(installFolder));
			OnAfterDeactivate();
		}

		private string GetInfoPath()
		{
			return Path.Combine(Path.Combine(SirenixAssetPaths.OdinPath, "Modules"), ID + ".info.txt");
		}

		public ModuleDefinitionInfo LoadInfo()
		{
			return ModuleDefinitionInfo.Load(GetInfoPath());
		}

		public void SaveInfo()
		{
			ModuleDefinitionInfo info = new ModuleDefinitionInfo
			{
				CurrentVersion = LatestVersion,
				RequiresOdinVersion = RequiresOdinVersion
			};
			ModuleDefinitionInfo.Save(info, GetInfoPath());
		}

		public virtual ModuleData GetModuleDataForPackaging()
		{
			string folderPath = BuildFromPath;
			DirectoryInfo dir = new DirectoryInfo(folderPath);
			if (!dir.Exists)
			{
				throw new InvalidOperationException("Directory '" + dir.FullName + "' does not exist to build module package from.");
			}
			return new ModuleData
			{
				ID = ID,
				Version = LatestVersion,
				RequiresOdinVersion = RequiresOdinVersion,
				Files = (from n in dir.GetFiles("*", SearchOption.AllDirectories).Select(delegate(FileInfo file)
					{
						switch (file.Name.ToLower())
						{
						case "thumbs.db":
						case "manifest.txt":
						case "manifest.txt.meta":
							return (ModuleData.ModuleFile)null;
						default:
						{
							string path = PathUtilities.MakeRelative(dir.FullName, file.FullName);
							byte[] data = File.ReadAllBytes(file.FullName);
							return new ModuleData.ModuleFile
							{
								Path = path,
								Data = data
							};
						}
						}
					})
					where n != null
					select n).ToList()
			};
		}

		public virtual ModuleManifest LoadManifest()
		{
			if (!CheckIsActivated())
			{
				return null;
			}
			string installFolder = ModuleManager.DataManager.InstallPath + "/" + ID;
			string manifestPath = installFolder + "/manifest.txt";
			return ModuleManifest.Load(manifestPath);
		}

		protected static void DeleteIfEmpty(DirectoryInfo dir)
		{
			if (!dir.Exists)
			{
				return;
			}
			if (dir.Name.ToLower() == "__macosx")
			{
				if (dir.Parent != null)
				{
					string metaFile = dir.Parent.FullName + "/" + dir.Name + ".meta";
					if (File.Exists(metaFile))
					{
						File.Delete(metaFile);
					}
				}
				dir.Delete(recursive: true);
				return;
			}
			DirectoryInfo[] directories = dir.GetDirectories();
			foreach (DirectoryInfo subDir in directories)
			{
				DeleteIfEmpty(subDir);
			}
			if (dir.GetDirectories().Length != 0)
			{
				return;
			}
			FileInfo[] files = dir.GetFiles();
			if (files.Length > 2)
			{
				return;
			}
			FileInfo[] array = files;
			foreach (FileInfo file in array)
			{
				string name = file.Name.ToLower();
				if (name != "thumbs.db" && name != ".ds_store")
				{
					return;
				}
			}
			if (dir.Parent != null)
			{
				string metaFile2 = dir.Parent.FullName + "/" + dir.Name + ".meta";
				if (File.Exists(metaFile2))
				{
					File.Delete(metaFile2);
				}
			}
			dir.Delete(recursive: true);
		}
	}
}

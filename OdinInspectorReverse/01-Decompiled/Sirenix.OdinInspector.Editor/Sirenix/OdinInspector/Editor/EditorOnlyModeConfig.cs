using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Editor Only Mode Utility.
	/// </summary>
	public sealed class EditorOnlyModeConfig
	{
		private static readonly object instance_LOCK = new object();

		private static EditorOnlyModeConfig instance;

		private const string BACKUP_FILE_SUFFIX = ".backup.txt";

		private const string SOURCE_CODE_NOT_SUPPORTED_MESSAGE = "Enabling and disabling Editor Only Mode is not supported when using Odin with source code.";

		private static string ExcludeFromEverything = "PluginImporter:\r\n  serializedVersion: 1\r\n  iconMap: {}\r\n  executionOrder: {}\r\n  isPreloaded: 0\r\n  isOverridable: 0\r\n  platformData:\r\n    Any:\r\n      enabled: 0\r\n      settings:\r\n        Exclude Android: 1\r\n        Exclude Editor: 1\r\n        Exclude Linux: 1\r\n        Exclude Linux64: 1\r\n        Exclude LinuxUniversal: 1\r\n        Exclude N3DS: 1\r\n        Exclude OSXIntel: 1\r\n        Exclude OSXIntel64: 1\r\n        Exclude OSXUniversal: 1\r\n        Exclude PS4: 1\r\n        Exclude PSM: 1\r\n        Exclude PSP2: 1\r\n        Exclude SamsungTV: 1\r\n        Exclude Tizen: 1\r\n        Exclude WebGL: 1\r\n        Exclude WiiU: 1\r\n        Exclude Win: 1\r\n        Exclude Win64: 1\r\n        Exclude WindowsStoreApps: 1\r\n        Exclude XboxOne: 1\r\n        Exclude iOS: 1\r\n        Exclude tvOS: 1\r\n    Editor:\r\n      enabled: 0\r\n      settings:\r\n        DefaultValueInitialized: true\r\n  userData:\r\n  assetBundleName:\r\n  assetBundleVariant: ";

		private static string ExcludeFromEverythingExceptEditor = "PluginImporter:\r\n  serializedVersion: 1\r\n  iconMap: {}\r\n  executionOrder: {}\r\n  isPreloaded: 0\r\n  isOverridable: 0\r\n  platformData:\r\n    Any:\r\n      enabled: 0\r\n      settings:\r\n        Exclude Android: 1\r\n        Exclude Editor: 0\r\n        Exclude Linux: 1\r\n        Exclude Linux64: 1\r\n        Exclude LinuxUniversal: 1\r\n        Exclude N3DS: 1\r\n        Exclude OSXIntel: 1\r\n        Exclude OSXIntel64: 1\r\n        Exclude OSXUniversal: 1\r\n        Exclude PS4: 1\r\n        Exclude PSM: 1\r\n        Exclude PSP2: 1\r\n        Exclude SamsungTV: 1\r\n        Exclude Tizen: 1\r\n        Exclude WebGL: 1\r\n        Exclude WiiU: 1\r\n        Exclude Win: 1\r\n        Exclude Win64: 1\r\n        Exclude WindowsStoreApps: 1\r\n        Exclude XboxOne: 1\r\n        Exclude iOS: 1\r\n        Exclude tvOS: 1\r\n    Editor:\r\n      enabled: 1\r\n      settings:\r\n        DefaultValueInitialized: true\r\n  userData:\r\n  assetBundleName:\r\n  assetBundleVariant: ";

		private readonly bool EditorOnlyBuild;

		private bool isUsingSourceCode;

		private string[] platformSpecificAssemblyFiles;

		private string[] globalAssemblyFiles;

		private bool isInEditorOnlyMode;

		public static EditorOnlyModeConfig Instance
		{
			get
			{
				if (instance == null)
				{
					lock (instance_LOCK)
					{
						if (instance == null)
						{
							instance = new EditorOnlyModeConfig();
						}
					}
				}
				return instance;
			}
		}

		private bool SerializationModeIsForceText => EditorSettings.serializationMode == SerializationMode.ForceText;

		private bool ShowEnableEditorOnlyMode
		{
			get
			{
				if (!isInEditorOnlyMode)
				{
					return !isUsingSourceCode;
				}
				return false;
			}
		}

		private bool ShowDisableEditorOnlyMode
		{
			get
			{
				if (isInEditorOnlyMode)
				{
					return !isUsingSourceCode;
				}
				return false;
			}
		}

		[OnInspectorGUI]
		[PropertyOrder(-2399f)]
		[InfoBox("If you're not interested in using Odin's serialization system - inheriting from classes such as SerializedMonoBehaviour and SerializedScriptableObject etc. - you can disable the serialization system completely, without losing the ability to leverage all of the attributes and editor functionality that Odin provides. This will also let you use Odin while targeting non-IL2CPP UWP platform targets.\n\nDisabling the serialization system will prevent almost all of Odin from being included in your builds. The only Odin code that will be included is the small Sirenix.OdinInspector.Attributes assembly, containing only the attribute definitions. If you are using IL2CPP, many of these attributes will likely be removed during Unity's code stripping step.\n\nNote that Odin still uses the serialization system in the editor itself to provide you with various editor functionality. You will still be able to inherit from classes like SerializedMonoBehaviour while in the editor, but you will get a warning in the inspector if you do so, and you will get compiler errors if you try to build.\n", InfoMessageType.Info, null)]
		private void TopMessage()
		{
		}

		private EditorOnlyModeConfig()
		{
			Update();
		}

		/// <summary>
		/// Gaither all necessary information about the editor only state.
		/// </summary>
		public void Update()
		{
			string assemblyPath = SirenixAssetPaths.SirenixAssembliesPath;
			if (!Directory.Exists(assemblyPath))
			{
				isInEditorOnlyMode = false;
				isUsingSourceCode = true;
				return;
			}
			string serializationConfigDll = SirenixAssetPaths.SirenixAssembliesPath + "Sirenix.Serialization.Config.dll";
			globalAssemblyFiles = ((!File.Exists(serializationConfigDll)) ? new string[0] : new string[1] { serializationConfigDll });
			platformSpecificAssemblyFiles = new string[4]
			{
				SirenixAssetPaths.SirenixAssembliesPath + "NoEditor/Sirenix.Serialization.dll",
				SirenixAssetPaths.SirenixAssembliesPath + "NoEditor/Sirenix.Utilities.dll",
				SirenixAssetPaths.SirenixAssembliesPath + "NoEmitAndNoEditor/Sirenix.Serialization.dll",
				SirenixAssetPaths.SirenixAssembliesPath + "NoEmitAndNoEditor/Sirenix.Utilities.dll"
			};
			int n = 0;
			for (int i = 0; i < platformSpecificAssemblyFiles.Length; i++)
			{
				string item = platformSpecificAssemblyFiles[i];
				if (File.Exists(item))
				{
					platformSpecificAssemblyFiles[n] = item;
					n++;
				}
			}
			Array.Resize(ref platformSpecificAssemblyFiles, n);
			if (platformSpecificAssemblyFiles.Length + globalAssemblyFiles.Length == 0)
			{
				isUsingSourceCode = true;
				return;
			}
			isUsingSourceCode = false;
			isInEditorOnlyMode = platformSpecificAssemblyFiles.Concat(globalAssemblyFiles).All((string x) => File.Exists(x + ".backup.txt"));
		}

		[PropertyOrder(-10f)]
		[InfoBox("In order to make make the proper modification to the assembly import settings, the serializationMode in the EditorSettings must be set to ForceText.", InfoMessageType.Info, null)]
		[DisableIf("EditorOnlyBuild")]
		[HideIf("SerializationModeIsForceText", true)]
		[Button(ButtonSizes.Gigantic)]
		private void SetForceText()
		{
			EditorSettings.serializationMode = SerializationMode.ForceText;
			AssetDatabase.Refresh();
		}

		/// <summary>
		/// Disables Editor Only Mode.
		/// </summary>
		[ShowIf("ShowDisableEditorOnlyMode", true)]
		[PropertyOrder(-8f)]
		[Button(ButtonSizes.Gigantic)]
		[GUIColor(1f, 0.8f, 0f, 1f)]
		[EnableIf("SerializationModeIsForceText")]
		[DisableIf("EditorOnlyBuild")]
		public void DisableEditorOnlyMode()
		{
			if (isUsingSourceCode)
			{
				Debug.LogError("Enabling and disabling Editor Only Mode is not supported when using Odin with source code.");
				return;
			}
			if (!isInEditorOnlyMode)
			{
				Debug.LogError("Editor mode is already disabled.");
				return;
			}
			UnityEditorEventUtility.EditorApplication_delayCall += delegate
			{
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				if (!SerializationModeIsForceText)
				{
					Debug.LogError("In order to make make the proper modification to the assembly import settings, the serializationMode in the EditorSettings must be set to ForceText.");
				}
				else
				{
					Update();
					foreach (string current in globalAssemblyFiles.Concat(platformSpecificAssemblyFiles))
					{
						if (!File.Exists(current + ".backup.txt"))
						{
							Debug.LogError("The old import settings was not found which was supposed to be located at: '" + current + ".backup.txt");
						}
						else
						{
							SetPluginImportSettings(current + ".meta", File.ReadAllText(current + ".backup.txt"));
							File.Delete(current + ".backup.txt");
						}
					}
					string[] array = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension("link.xml.backup.txt"), new string[1] { SirenixAssetPaths.SirenixAssembliesPath.TrimEnd(new char[1] { '/' }) });
					for (int i = 0; i < array.Length; i++)
					{
						string text = AssetDatabase.GUIDToAssetPath(array[i]);
						string newPath = PathUtilities.GetDirectoryName(text).Replace('\\', '/').TrimEnd(new char[1] { '/' }) + "/link.xml";
						AssetDatabase.MoveAsset(text, newPath);
					}
					GlobalConfigAttribute globalConfigAttribute = typeof(GlobalSerializationConfig).BaseType.GetProperty("ConfigAttribute", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).GetValue(null, null) as GlobalConfigAttribute;
					string assetPath = AssetDatabase.GetAssetPath(GlobalConfig<GlobalSerializationConfig>.Instance);
					string fileName = Path.GetFileName(assetPath);
					string text2 = globalConfigAttribute.AssetPath.TrimEnd(new char[1] { '/' });
					if (!Directory.Exists(text2))
					{
						Directory.CreateDirectory(text2);
						AssetDatabase.Refresh();
					}
					AssetDatabase.MoveAsset(assetPath, text2 + "/" + fileName);
					AssetDatabase.Refresh();
					Update();
					GlobalConfig<ImportSettingsConfig>.Instance.ApplyImportSettings();
				}
			};
		}

		private static void MoveAsset(string from, string to)
		{
			if (!File.Exists(from))
			{
				return;
			}
			if (File.Exists(to))
			{
				File.Delete(to);
			}
			File.Move(from, to);
			if (File.Exists(from + ".meta"))
			{
				if (File.Exists(to + ".meta"))
				{
					File.Delete(to + ".meta");
				}
				File.Move(from + ".meta", to + ".meta");
			}
		}

		/// <summary>
		/// Enables editor only mode.
		/// </summary>
		public void EnableEditorOnlyMode(bool force)
		{
			if (isUsingSourceCode)
			{
				Debug.LogError("Enabling and disabling Editor Only Mode is not supported when using Odin with source code.");
				return;
			}
			if (isInEditorOnlyMode && !force)
			{
				Debug.LogError("Editor mode is already enabled.");
				return;
			}
			AssetDatabase.SaveAssets();
			if (!SerializationModeIsForceText)
			{
				Debug.LogError("In order to make make the proper modification to the assembly import settings, the serializationMode in the EditorSettings must be set to ForceText.");
				return;
			}
			Update();
			foreach (string dllFilePath in globalAssemblyFiles.Concat(platformSpecificAssemblyFiles))
			{
				if (File.Exists(dllFilePath + ".backup.txt"))
				{
					File.Delete(dllFilePath + ".backup.txt");
				}
				File.Copy(dllFilePath + ".meta", dllFilePath + ".backup.txt");
			}
			string[] linkFiles = AssetDatabase.FindAssets("link", new string[1] { SirenixAssetPaths.SirenixAssembliesPath.TrimEnd(new char[1] { '/' }) });
			for (int i = 0; i < linkFiles.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(linkFiles[i]);
				if (path.ToLower().EndsWith(".xml"))
				{
					if (File.Exists(path + ".backup.txt"))
					{
						AssetDatabase.DeleteAsset(path + ".backup.txt");
					}
					AssetDatabase.MoveAsset(path, path + ".backup.txt");
				}
			}
			string serializationConfigPath = AssetDatabase.GetAssetPath(GlobalConfig<GlobalSerializationConfig>.Instance);
			string fileName = Path.GetFileName(serializationConfigPath);
			if (!File.Exists(SirenixAssetPaths.OdinEditorConfigsPath + fileName))
			{
				AssetDatabase.MoveAsset(serializationConfigPath, SirenixAssetPaths.OdinEditorConfigsPath + fileName);
			}
			string[] array = platformSpecificAssemblyFiles;
			foreach (string dllFilePath2 in array)
			{
				SetPluginImportSettings(dllFilePath2 + ".meta", ExcludeFromEverything);
			}
			string[] array2 = globalAssemblyFiles;
			foreach (string dllFilePath3 in array2)
			{
				SetPluginImportSettings(dllFilePath3 + ".meta", ExcludeFromEverythingExceptEditor);
			}
			AssetDatabase.Refresh();
			Update();
		}

		[PropertyOrder(-8f)]
		[ShowIf("ShowEnableEditorOnlyMode", true)]
		[Button(ButtonSizes.Gigantic)]
		[GUIColor(0f, 1f, 0f, 1f)]
		[DisableIf("EditorOnlyBuild")]
		[EnableIf("SerializationModeIsForceText")]
		private void EnableEditorOnlyMode()
		{
			UnityEditorEventUtility.EditorApplication_delayCall += delegate
			{
				EnableEditorOnlyMode(force: false);
			};
		}

		/// <summary>
		/// Checks to see whether Editor Only Mode is enabled.
		/// </summary>
		public bool IsEditorOnlyModeEnabled()
		{
			if (isInEditorOnlyMode)
			{
				return !isUsingSourceCode;
			}
			return false;
		}

		/// <summary>
		/// Checks to see whether Odin Inspector is installed in Source Code mode.
		/// </summary>
		public bool IsInSourceCode()
		{
			return isUsingSourceCode;
		}

		[OnInspectorGUI]
		private void OnInspectorGUI()
		{
			if (isUsingSourceCode)
			{
				SirenixEditorGUI.MessageBox("Enabling and disabling Editor Only Mode is not supported when using Odin with source code.", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			}
		}

		private bool TryThisNTimes(Action action, int numberOfTries = 20, int sleepBetweenTries = 10)
		{
			Exception exx = null;
			while (numberOfTries-- > 0)
			{
				try
				{
					action();
					return true;
				}
				catch (Exception ex)
				{
					exx = ex;
					Thread.Sleep(sleepBetweenTries);
				}
			}
			Debug.LogException(exx);
			return false;
		}

		private void SetPluginImportSettings(string metaFile, string pluginImportSettings)
		{
			List<string> pluginImportSettingsLines = new List<string>();
			string[] tmpLines = pluginImportSettings.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
			bool foundImportSettings = false;
			string[] array = tmpLines;
			foreach (string line in array)
			{
				if (foundImportSettings || line.StartsWith("PluginImporter:"))
				{
					pluginImportSettingsLines.Add(line);
					foundImportSettings = true;
				}
			}
			List<string> currLines = new List<string>();
			TryThisNTimes(delegate
			{
				currLines.Clear();
				using FileStream stream = new FileStream(metaFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
				while (streamReader.Peek() >= 0)
				{
					currLines.Add(streamReader.ReadLine());
				}
			});
			List<string> newLines = new List<string>();
			foreach (string line2 in currLines)
			{
				if (line2.StartsWith("PluginImporter:"))
				{
					break;
				}
				newLines.Add(line2);
			}
			newLines.AddRange(pluginImportSettingsLines);
			TryThisNTimes(delegate
			{
				File.Delete(metaFile);
				using FileStream stream = new FileStream(metaFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
				using StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8);
				for (int j = 0; j < newLines.Count; j++)
				{
					streamWriter.WriteLine(newLines[j]);
				}
			});
		}

		private bool HasDemos()
		{
			DirectoryInfo dir = new DirectoryInfo(SirenixAssetPaths.SirenixPluginPath + "Demos");
			if (!dir.Exists)
			{
				return false;
			}
			return dir.GetDirectories().Any();
		}

		[ShowIf("ShowDisableEditorOnlyMode", true)]
		[InfoBox("Since Editor Only Mode is enabled, remember to remove any Odin demos you might have imported before you build. Otherwise, you'll get compiler errors when you build, since most demos utilize Odin's serialization system which is not included in builds in Editor Only mode.", InfoMessageType.Warning, null)]
		[OnInspectorGUI]
		private void DeleteImportedDemosInfo()
		{
		}

		[Button("$GetDeleteImportedDemosBtnText", ButtonSizes.Large)]
		[ShowIf("ShowDisableEditorOnlyMode", true)]
		[EnableIf("HasDemos")]
		private void DeleteImportedDemos()
		{
			List<string> directoriesToDelete = new List<string>();
			if (Directory.Exists(SirenixAssetPaths.SirenixPluginPath + "Demos"))
			{
				directoriesToDelete.AddRange(from x in new DirectoryInfo(SirenixAssetPaths.SirenixPluginPath + "Demos").GetDirectories()
					select x.FullName);
			}
			DeleteDirsAndFiles(directoriesToDelete);
		}

		private string GetDeleteImportedDemosBtnText()
		{
			if (HasDemos())
			{
				return "Delete all imported demos located in \"" + SirenixAssetPaths.SirenixPluginPath + "Demos/\"";
			}
			return "No imported demos were found in \"" + SirenixAssetPaths.SirenixPluginPath + "Demos/\"";
		}

		private static void DeleteDirsAndFiles(List<string> directoriesToDelete)
		{
			foreach (string dir in directoriesToDelete.Select((string x) => x.Replace('\\', '/')))
			{
				string mdb = dir + ".mdb";
				if (Directory.Exists(dir))
				{
					string[] paths = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
					for (int i = 0; i < paths.Length; i++)
					{
						string p = paths[i].Replace('\\', '/');
						DeleteFile(p);
					}
					DeleteDirectory(dir);
				}
				DeleteFile(mdb);
			}
			AssetDatabase.Refresh();
		}

		private static void DeleteFile(string file)
		{
			if (File.Exists(file))
			{
				try
				{
					File.Delete(file);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}

		private static void DeleteDirectory(string dir)
		{
			if (Directory.Exists(dir))
			{
				try
				{
					Directory.Delete(dir, recursive: true);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
	}
}

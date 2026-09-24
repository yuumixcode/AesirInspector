using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities
{
	/// <summary>
	/// Paths to Sirenix assets.
	/// </summary>
	public static class SirenixAssetPaths
	{
		public const string DefaultSirenixPluginPath = "Assets/Plugins/Sirenix/";

		public const string SirenixAssetPathsSOGuid = "08379ccefc05200459f90a1c0711a340";

		public const string LookupAssetName = "OdinPathLookup.asset";

		/// <summary>
		/// Path to Odin Inspector folder.
		/// </summary>
		public static readonly string OdinPath;

		/// <summary>
		/// Path to Sirenix assets folder.
		/// </summary>
		public static readonly string SirenixAssetsPath;

		/// <summary>
		/// Path to Sirenix folder.
		/// </summary>
		public static readonly string SirenixPluginPath;

		/// <summary>
		/// Path to Sirenix assemblies.
		/// </summary>
		public static readonly string SirenixAssembliesPath;

		/// <summary>
		/// Path to Odin Inspector resources folder.
		/// </summary>
		public static readonly string OdinResourcesPath;

		/// <summary>
		/// Path to Odin Inspector configuration folder.
		/// </summary>
		public static readonly string OdinEditorConfigsPath;

		/// <summary>
		/// Path to Odin Inspector resources configuration folder.
		/// </summary>
		public static readonly string OdinResourcesConfigsPath;

		/// <summary>
		/// Path to Odin Inspector temporary folder.
		/// </summary>
		public static readonly string OdinTempPath;

		private static readonly HashSet<char> IllegalChars;

		static SirenixAssetPaths()
		{
			IllegalChars = new HashSet<char>(Path.GetInvalidFileNameChars()) { '.' };
			bool log = false;
			StringBuilder sb = new StringBuilder();
			if (File.Exists("Assets/Plugins/Sirenix/Odin Inspector/Assets/Editor/OdinPathLookup.asset"))
			{
				SirenixPluginPath = "Assets/Plugins/Sirenix/";
			}
			else
			{
				sb.AppendLine("There were some problems trying to locate where Odin was installed, please read the messages below.");
				sb.AppendLine("- Odin was not found in its default location: 'Assets/Plugins/Sirenix/'.");
				string pathLookupAssetPath = AssetDatabase.GUIDToAssetPath("08379ccefc05200459f90a1c0711a340");
				if (!File.Exists(pathLookupAssetPath))
				{
					pathLookupAssetPath = null;
				}
				if (string.IsNullOrEmpty(pathLookupAssetPath))
				{
					log = true;
					sb.AppendLine("- No SirenixPathLookupScriptableObject with the Guid: '08379ccefc05200459f90a1c0711a340' was found.");
					string[] results = AssetDatabase.FindAssets("t:SirenixPathLookupScriptableObject") ?? new string[0];
					string newPath = results.FirstOrDefault((string x) => x?.Contains("Sirenix/") ?? false);
					if (newPath == null)
					{
						string[] paths = AssetDatabase.GetAllAssetPaths();
						for (int i = 0; i < paths.Length; i++)
						{
							if (paths[i].FastEndsWith("OdinPathLookup.asset"))
							{
								pathLookupAssetPath = paths[i];
								break;
							}
						}
						if (string.IsNullOrEmpty(pathLookupAssetPath))
						{
							sb.AppendLine("- We were unable to find Odin elsewhere. Please re-import Odin and make sure the OdinPathLookup.asset and OdinPathLookup.asset.meta files are included.");
						}
						else
						{
							sb.AppendLine("- We were able to find Odin using a very slow fallback method. This will increase your project reload times. To fix it, please re-import Odin and make sure the OdinPathLookup.asset file is included.");
						}
					}
					else
					{
						sb.AppendLine("- The SirenixPathLookupScriptableObject was found, but must have had its Guid changed. Please report this issue with details about your project setup. You should be able to fix the issue by re-importing Odin.");
						pathLookupAssetPath = newPath;
					}
				}
				if (!string.IsNullOrEmpty(pathLookupAssetPath))
				{
					int i2 = pathLookupAssetPath.LastIndexOf("Sirenix/", StringComparison.CurrentCultureIgnoreCase);
					if (i2 < 0)
					{
						i2 = pathLookupAssetPath.LastIndexOf("/Odin Inspector/", StringComparison.CurrentCultureIgnoreCase);
						if (i2 < 0)
						{
							pathLookupAssetPath = "Assets/Plugins/Sirenix/";
							log = true;
							sb.AppendLine("- The path Odin found ('" + pathLookupAssetPath + "') was invalid and contained neither 'Sirenix/' nor '/Odin Inspector/' - Odin's path setup has defaulted to the default path 'Assets/Plugins/Sirenix/', which is likely wrong. You should be able to fix the issue by re-importing Odin.");
						}
						else
						{
							string path = pathLookupAssetPath.Substring(0, i2);
							DirectoryInfo dir = new DirectoryInfo(path);
							if (!dir.Exists)
							{
								SirenixPluginPath = "Assets/Plugins/Sirenix/";
								log = true;
								sb.AppendLine("- The path Odin found ('" + pathLookupAssetPath + "') was invalid somehow and parts of it don't even exist (O_o?!) - Odin's path setup has defaulted to the default path 'Assets/Plugins/Sirenix/', which is likely wrong. You should be able to fix the issue by re-importing Odin.");
							}
							else
							{
								pathLookupAssetPath = path.TrimEnd(new char[1] { '/' }) + "/";
							}
						}
					}
					else
					{
						pathLookupAssetPath = pathLookupAssetPath.Substring(0, i2 + "Sirenix/".Length);
					}
					SirenixPluginPath = pathLookupAssetPath;
				}
				if (string.IsNullOrEmpty(pathLookupAssetPath) || log)
				{
					Debug.LogError(sb.ToString());
				}
			}
			string companyName = ReplaceIllegalCharacters(PlayerSettings.companyName, '_');
			string productName = ReplaceIllegalCharacters(PlayerSettings.productName, '_');
			OdinTempPath = Path.Combine(Path.GetTempPath().Replace('\\', '/'), "Sirenix/Odin/" + companyName + "/" + productName);
			if (SirenixPluginPath == null)
			{
				SirenixPluginPath = "Assets/Plugins/Sirenix/";
			}
			OdinPath = SirenixPluginPath + "Odin Inspector/";
			SirenixAssetsPath = SirenixPluginPath + "Assets/";
			SirenixAssembliesPath = SirenixPluginPath + "Assemblies/";
			OdinResourcesPath = OdinPath + "Config/Resources/Sirenix/";
			OdinEditorConfigsPath = OdinPath + "Config/Editor/";
			OdinResourcesConfigsPath = OdinResourcesPath;
		}

		private static string ReplaceIllegalCharacters(string source, char replacement)
		{
			char[] chars = source.ToCharArray();
			for (int i = 0; i < chars.Length; i++)
			{
				if (IllegalChars.Contains(chars[i]))
				{
					chars[i] = replacement;
				}
			}
			return new string(chars);
		}
	}
}

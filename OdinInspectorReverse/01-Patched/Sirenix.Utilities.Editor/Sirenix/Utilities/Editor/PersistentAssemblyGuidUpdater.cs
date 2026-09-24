using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	internal static class PersistentAssemblyGuidUpdater
	{
		[InitializeOnLoadMethod]
		private static void Init()
		{
			UnityEditorEventUtility.EditorApplication_delayCall += delegate
			{
				UpdateGuids(log: false);
			};
		}

		private static void UpdateGuids()
		{
			UpdateGuids(log: true);
		}

		private static void UpdateGuids(bool log)
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			bool updateAssetDatabase = false;
			if (log)
			{
				Debug.Log("Scanning " + assemblies.Length + " assemblies.");
			}
			foreach (Assembly assembly in assemblies)
			{
				if (assembly.SafeGetCustomAttributes(typeof(PersistentAssemblyAttribute), inherit: true).Length == 0)
				{
					continue;
				}
				object[] assemblyGuids = assembly.SafeGetCustomAttributes(typeof(GuidAttribute), inherit: true);
				if (assemblyGuids.Length == 0)
				{
					LogError(assembly, "Assembly has is marked with PersistentAssemblyAttribute, but not a GuidAttribute.");
					continue;
				}
				string assemblyMetaFile = assembly.GetAssemblyFilePath() + ".meta";
				if (!File.Exists(assemblyMetaFile))
				{
					if (log)
					{
						Debug.Log("Skipping persistent assembly: " + assembly.FullName + ", No meta file was found at: " + assemblyMetaFile);
					}
					continue;
				}
				string[] lines = File.ReadAllLines(assemblyMetaFile);
				for (int j = 0; j < lines.Length; j++)
				{
					string line = lines[j];
					if (!line.StartsWith("guid: ", StringComparison.InvariantCultureIgnoreCase) || line.Length < 38)
					{
						continue;
					}
					string metaFileGuid = line.Substring(6, line.Length - 6).Trim();
					if (!IsValidGuid(metaFileGuid))
					{
						LogError(assembly, "Invalid or unsupported guid format was found in meta file at line nr " + line + ". Line content: " + line);
						break;
					}
					string assemblyGuid = (assemblyGuids[0] as GuidAttribute).Value.Replace("-", "").ToLower();
					if (!IsValidGuid(assemblyGuid))
					{
						LogError(assembly, "Invalid guid format was specified in assembly. GuidAttribute value: " + assemblyGuid);
					}
					else if (assemblyGuid != metaFileGuid)
					{
						if (log)
						{
							Debug.Log("Persistent assembly: " + assembly.FullName + ", already have the right guid (" + assemblyGuid + ")");
						}
						lines[j] = line.Substring(0, 6) + assemblyGuid;
						updateAssetDatabase = true;
						try
						{
							if (log)
							{
								Debug.Log("Persistent assembly '" + assembly.FullName + "' guid is being updated from: '" + metaFileGuid + "' to '" + assemblyGuid + "'");
							}
							using (FileStream fs = File.Open(assemblyMetaFile, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
							{
								using StreamWriter sw = new StreamWriter(fs);
								for (int k = 0; k < lines.Length; k++)
								{
									sw.WriteLine(lines[k]);
								}
							}
							if (log)
							{
								Debug.Log("Meta file for '" + assemblyMetaFile + "' has been updated");
							}
						}
						catch (Exception ex)
						{
							LogError(assembly, ex.Message);
							Debug.LogWarning("Could not write to meta file. Please start Unity in Administrative mode");
							return;
						}
					}
					else if (log)
					{
						Debug.Log("Persistent assembly: " + assembly.FullName + ", is already updated with the right guid: '" + assemblyGuid + "'");
					}
					break;
				}
			}
			if (log)
			{
				Debug.Log("Finished scanning " + assemblies.Length + " assemblies. Refreshing AssetDatabase...");
			}
			if (updateAssetDatabase)
			{
				AssetDatabase.Refresh();
			}
		}

		private static void LogError(Assembly assembly, string msg)
		{
			Debug.LogError("Could not update asset guid for assembly " + Path.GetFileName(assembly.GetAssemblyFilePath()) + ". " + msg);
		}

		private static bool IsValidGuid(string guid)
		{
			if (guid.Length != 32)
			{
				return false;
			}
			for (int i = 0; i < guid.Length; i++)
			{
				if (!char.IsLetterOrDigit(guid[i]))
				{
					return false;
				}
			}
			return true;
		}
	}
}

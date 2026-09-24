using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Modules
{
	public class ModuleManifest
	{
		private struct LineData
		{
			public string Key;

			public string Data;

			public static LineData Parse(string line)
			{
				if (line.Length > 2 && line[0] == ' ' && line[1] == ' ')
				{
					return new LineData
					{
						Key = null,
						Data = line.TrimEnd(Array.Empty<char>())
					};
				}
				int separator = line.IndexOf(':');
				if (separator < 0)
				{
					return new LineData
					{
						Key = null,
						Data = line.Trim()
					};
				}
				return new LineData
				{
					Key = line.Substring(0, separator).Trim(),
					Data = ((separator + 1 == line.Length) ? "" : line.Substring(separator + 1).Trim())
				};
			}

			public override string ToString()
			{
				if (Key == null)
				{
					return "{ " + Data + " }";
				}
				return "{ " + Key + ", " + Data + " }";
			}
		}

		public string ID;

		public Version Version;

		public Version RequiresOdinVersion;

		public List<string> Files;

		private const string MANIFEST_VERSION_KEY = "ManifestVersion";

		private const string MODULE_ID_KEY = "ModuleID";

		private const string MODULE_VERSION_KEY = "ModuleVersion";

		private const string MODULE_REQUIRES_ODIN_VERSION_KEY = "ModuleRequiresOdinVersion";

		private const string MODULE_FILES_KEY = "ModuleFiles";

		private const string MANIFEST_V1 = "ManifestVersion: 1";

		public static void Save(string path, ModuleManifest manifest)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("ManifestVersion: 1");
			sb.Append("ModuleID");
			sb.Append(": ");
			sb.Append(manifest.ID);
			sb.AppendLine();
			sb.Append("ModuleVersion");
			sb.Append(": ");
			sb.Append(manifest.Version);
			sb.AppendLine();
			if (manifest.RequiresOdinVersion != null)
			{
				sb.Append("ModuleRequiresOdinVersion");
				sb.Append(": ");
				sb.Append(manifest.RequiresOdinVersion);
				sb.AppendLine();
			}
			sb.Append("ModuleFiles");
			sb.Append(":");
			sb.AppendLine();
			foreach (string file in manifest.Files)
			{
				sb.Append("  ");
				sb.Append(file);
				sb.AppendLine();
			}
			File.WriteAllText(path, sb.ToString());
		}

		public static ModuleManifest Load(string path)
		{
			if (!File.Exists(path))
			{
				return null;
			}
			string[] lines = File.ReadAllLines(path);
			if (lines.Length == 0)
			{
				return null;
			}
			string header = lines[0].TrimEnd(Array.Empty<char>());
			if (header == "ManifestVersion: 1")
			{
				return Load_Version1(lines);
			}
			Debug.LogError("Cannot read Odin module manifest file with version '" + lines[0] + "'.");
			return null;
		}

		private static ModuleManifest Load_Version1(string[] lines)
		{
			LineData[] dataEntries = (from n in lines
				where !n.StartsWith("#") && !string.IsNullOrEmpty(n.Trim())
				select LineData.Parse(n)).ToArray();
			if (dataEntries.Length < 4)
			{
				return null;
			}
			ModuleManifest manifest = new ModuleManifest
			{
				Files = new List<string>()
			};
			for (int i = 1; i < dataEntries.Length; i++)
			{
				LineData data = dataEntries[i];
				switch (data.Key)
				{
				case "ModuleID":
					manifest.ID = data.Data;
					break;
				case "ModuleVersion":
					manifest.Version = new Version(data.Data);
					break;
				case "ModuleRequiresOdinVersion":
					manifest.RequiresOdinVersion = new Version(data.Data);
					break;
				case "ModuleFiles":
				{
					int index = i + 1;
					while (index < dataEntries.Length)
					{
						LineData fileData = dataEntries[index++];
						if (fileData.Key != null || fileData.Data.Length < 3 || fileData.Data[0] != ' ' || fileData.Data[1] != ' ')
						{
							break;
						}
						manifest.Files.Add(fileData.Data.Trim());
					}
					i = index - 1;
					break;
				}
				}
			}
			return manifest;
		}
	}
}

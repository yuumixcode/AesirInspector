using System;
using System.IO;
using System.Text;

namespace Sirenix.OdinInspector.Editor.Modules
{
	public struct ModuleDefinitionInfo
	{
		public Version CurrentVersion;

		public Version RequiresOdinVersion;

		private const char SEPARATOR = ':';

		private const string CURRENT_VERSION_KEY = "CurrentVersion";

		private const string REQUIRES_ODIN_VERSION_KEY = "RequiresOdinVersion";

		public static ModuleDefinitionInfo Load(string path)
		{
			ModuleDefinitionInfo result = new ModuleDefinitionInfo
			{
				RequiresOdinVersion = null
			};
			if (!File.Exists(path))
			{
				return result;
			}
			string[] lines = File.ReadAllLines(path);
			string[] array = lines;
			foreach (string line in array)
			{
				int separatorIndex = line.IndexOf(':');
				if (separatorIndex < 0)
				{
					continue;
				}
				string identifier = line.Substring(0, separatorIndex).Trim();
				string value = line.Substring(separatorIndex + 1).Trim();
				Version version2;
				if (!(identifier == "CurrentVersion"))
				{
					if (identifier == "RequiresOdinVersion" && Version.TryParse(value, out Version version))
					{
						result.RequiresOdinVersion = version;
					}
				}
				else if (Version.TryParse(value, out version2))
				{
					result.CurrentVersion = version2;
				}
			}
			return result;
		}

		public static void Save(ModuleDefinitionInfo info, string path)
		{
			StringBuilder sb = new StringBuilder();
			if (info.CurrentVersion != null)
			{
				sb.Append("CurrentVersion");
				sb.Append(':');
				sb.Append(' ');
				sb.Append(info.CurrentVersion);
				sb.AppendLine();
			}
			if (info.RequiresOdinVersion != null)
			{
				sb.Append("RequiresOdinVersion");
				sb.Append(':');
				sb.Append(' ');
				sb.Append(info.RequiresOdinVersion);
				sb.AppendLine();
			}
			File.WriteAllText(path, sb.ToString());
		}
	}
}

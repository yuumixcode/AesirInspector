namespace Sirenix.OdinValidator.Editor
{
	internal static class StringExtensions
	{
		internal static string IndentString(string str, bool skipFirstLine, string indent = "    ")
		{
			string[] lines = str.Replace("\r\n", "\n").Split('\r', '\n');
			for (int i = (skipFirstLine ? 1 : 0); i < lines.Length; i++)
			{
				lines[i] = indent + lines[i];
			}
			return string.Join("\n", lines);
		}

		internal static string GetObjectNameFromAssetPath(this string path)
		{
			if (path == null)
			{
				return null;
			}
			int dot = path.LastIndexOf('.');
			if (dot >= 0)
			{
				path = path.Substring(0, dot);
				int dash = path.LastIndexOf('/');
				if (dash >= 0)
				{
					path = path.Substring(dash + 1);
				}
			}
			else
			{
				int dash2 = path.LastIndexOf('/');
				if (dash2 >= 0)
				{
					path = path.Substring(dash2 + 1);
				}
			}
			return path;
		}
	}
}

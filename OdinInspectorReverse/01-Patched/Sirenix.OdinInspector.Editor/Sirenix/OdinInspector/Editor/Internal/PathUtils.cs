using System.IO;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class PathUtils
	{
		public static string GetCrossPlatformPath(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return path;
			}
			path = path.Replace(Path.DirectorySeparatorChar, '/');
			if (path[path.Length - 1] == '/')
			{
				path = path.Substring(0, path.Length - 1);
			}
			return path;
		}
	}
}

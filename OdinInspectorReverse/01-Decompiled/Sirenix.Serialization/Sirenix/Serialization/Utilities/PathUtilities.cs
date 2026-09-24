using System.IO;

namespace Sirenix.Serialization.Utilities
{
	/// <summary>
	/// DirectoryInfo method extensions.
	/// </summary>
	internal static class PathUtilities
	{
		/// <summary>
		/// Determines whether the directory has a given directory in its hierarchy of children.
		/// </summary>
		/// <param name="parentDir">The parent directory.</param>
		/// <param name="subDir">The sub directory.</param>
		public static bool HasSubDirectory(this DirectoryInfo parentDir, DirectoryInfo subDir)
		{
			string parentDirName = parentDir.FullName.TrimEnd('\\', '/');
			while (subDir != null)
			{
				if (subDir.FullName.TrimEnd('\\', '/') == parentDirName)
				{
					return true;
				}
				subDir = subDir.Parent;
			}
			return false;
		}
	}
}

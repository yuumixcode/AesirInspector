using System;
using System.IO;
using System.Text;

namespace Sirenix.Utilities
{
	/// <summary>
	/// DirectoryInfo method extensions.
	/// </summary>
	public static class PathUtilities
	{
		/// <summary>
		/// Gets the name of the directory. Always returns forward slash seperators as opposed to Path.GetDirectoryName().
		/// </summary>
		public static string GetDirectoryName(string x)
		{
			if (x == null)
			{
				return null;
			}
			return Path.GetDirectoryName(x).Replace("\\", "/");
		}

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

		/// <summary>
		/// Finds a parent directory with a given name, or null if no such parent directory exists.
		/// </summary>
		public static DirectoryInfo FindParentDirectoryWithName(this DirectoryInfo dir, string folderName)
		{
			if (dir.Parent == null)
			{
				return null;
			}
			if (string.Equals(dir.Name, folderName, StringComparison.InvariantCultureIgnoreCase))
			{
				return dir;
			}
			return dir.Parent.FindParentDirectoryWithName(folderName);
		}

		/// <summary>
		/// Returns a value indicating whether or not a path can be made relative to another.
		/// </summary>
		/// <param name="absoluteParentPath">The parent path.</param>
		/// <param name="absolutePath">The path to make relative to the parent path.</param>
		/// <returns>A value indicating if the path can be made relative to the parent path.</returns>
		public static bool CanMakeRelative(string absoluteParentPath, string absolutePath)
		{
			if (absoluteParentPath == null)
			{
				throw new ArgumentNullException("absoluteParentPath");
			}
			if (absolutePath == null)
			{
				throw new ArgumentNullException("absoluteParentPath");
			}
			absoluteParentPath = absoluteParentPath.Replace('\\', '/').Trim(new char[1] { '/' });
			absolutePath = absolutePath.Replace('\\', '/').Trim(new char[1] { '/' });
			return Path.GetPathRoot(absoluteParentPath).Equals(Path.GetPathRoot(absolutePath), StringComparison.CurrentCultureIgnoreCase);
		}

		/// <summary>
		/// Returns a path string to path that is relative to the parent path.
		/// </summary>
		/// <param name="absoluteParentPath">The parent path.</param>
		/// <param name="absolutePath">The path to make relative to the parent path.</param>
		/// <returns>A relative path from parent path to path.</returns>
		public static string MakeRelative(string absoluteParentPath, string absolutePath)
		{
			absoluteParentPath = absoluteParentPath.TrimEnd('\\', '/');
			absolutePath = absolutePath.TrimEnd('\\', '/');
			string[] parentDirectories = absoluteParentPath.Split('/', '\\');
			string[] pathDirectories = absolutePath.Split('/', '\\');
			int commonDirectory = -1;
			for (int i = 0; i < parentDirectories.Length && i < pathDirectories.Length && parentDirectories[i].Equals(pathDirectories[i], StringComparison.CurrentCultureIgnoreCase); i++)
			{
				commonDirectory = i;
			}
			if (commonDirectory == -1)
			{
				throw new InvalidOperationException("No common directory found.");
			}
			StringBuilder relativePath = new StringBuilder();
			if (commonDirectory + 1 < parentDirectories.Length)
			{
				for (int j = commonDirectory + 1; j < parentDirectories.Length; j++)
				{
					if (relativePath.Length > 0)
					{
						relativePath.Append('/');
					}
					relativePath.Append("..");
				}
			}
			for (int k = commonDirectory + 1; k < pathDirectories.Length; k++)
			{
				if (relativePath.Length > 0)
				{
					relativePath.Append('/');
				}
				relativePath.Append(pathDirectories[k]);
			}
			return relativePath.ToString();
		}

		/// <summary>
		/// Tries to make a path that is relative from parent path to path.
		/// </summary>
		/// <param name="absoluteParentPath">The parent path.</param>
		/// <param name="absolutePath">The path to make relative to the parent path.</param>
		/// <param name="relativePath">A relative path from parent path to path. <c>null</c> if no relative path could be made.</param>
		/// <returns>A value indicating if the method succeeded in making a relative path.</returns>
		public static bool TryMakeRelative(string absoluteParentPath, string absolutePath, out string relativePath)
		{
			if (CanMakeRelative(absoluteParentPath, absolutePath))
			{
				relativePath = MakeRelative(absoluteParentPath, absolutePath);
				return true;
			}
			relativePath = null;
			return false;
		}

		/// <summary>
		/// Combines two paths, and replaces all backslases with forward slash.
		/// </summary>
		public static string Combine(string a, string b)
		{
			a = a.Replace("\\", "/").TrimEnd(new char[1] { '/' });
			b = b.Replace("\\", "/").TrimStart(new char[1] { '/' });
			return a + "/" + b;
		}
	}
}

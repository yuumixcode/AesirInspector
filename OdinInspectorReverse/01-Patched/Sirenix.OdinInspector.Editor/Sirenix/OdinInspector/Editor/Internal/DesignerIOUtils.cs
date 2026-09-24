using System;
using System.Collections.Generic;
using System.IO;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerIOUtils
	{
		private const int BLOCK_SIZE = 4096;

		public static char[] Buffer;

		public static int Length;

		static DesignerIOUtils()
		{
		}

		public static DesignerTempFileBuffer ReadFile(string path)
		{
			if (Buffer == null)
			{
				Buffer = new char[4096];
			}
			Length = 0;
			using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using StreamReader reader = new StreamReader(file);
				long size = file.Length;
				if (Buffer.Length < size)
				{
					Array.Resize(ref Buffer, CeilBlockSize((int)size));
				}
				Length = reader.ReadBlock(Buffer, 0, Buffer.Length);
			}
			return new DesignerTempFileBuffer
			{
				Buffer = Buffer,
				Length = Length
			};
		}

		private static int CeilBlockSize(int num)
		{
			return (num & -4096) + 4096;
		}

		public static IEnumerable<string> EnumerateFilesNoLinks(string path, string pattern)
		{
			if (!Directory.Exists(path))
			{
				yield break;
			}
			foreach (string subDir in Directory.EnumerateDirectories(path))
			{
				if ((File.GetAttributes(subDir) & FileAttributes.ReparsePoint) != (FileAttributes)0)
				{
					continue;
				}
				foreach (string item in EnumerateFilesNoLinks(subDir, pattern))
				{
					yield return item;
				}
			}
			foreach (string item2 in Directory.EnumerateFiles(path, pattern))
			{
				yield return item2;
			}
		}
	}
}

using System;
using System.IO;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal struct DesignerFile : IEquatable<DesignerFile>
	{
		public string Path;

		public DesignerFile(string path)
		{
			Path = path;
		}

		public DateTime GetLastWriteTimeUtc()
		{
			return File.GetLastWriteTimeUtc(Path);
		}

		public bool Equals(DesignerFile other)
		{
			return Path == other.Path;
		}

		public override bool Equals(object obj)
		{
			if (obj is DesignerFile other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			if (Path == null)
			{
				return 0;
			}
			return Path.GetHashCode();
		}
	}
}

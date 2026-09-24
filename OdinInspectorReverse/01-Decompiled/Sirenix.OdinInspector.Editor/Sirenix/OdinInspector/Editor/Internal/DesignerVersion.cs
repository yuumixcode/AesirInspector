using System;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal struct DesignerVersion : IEquatable<DesignerVersion>
	{
		public int Major;

		public int Minor;

		public static DesignerVersion Invalid => new DesignerVersion(-1, -1);

		public bool IsValid
		{
			get
			{
				if (Major >= 0)
				{
					return Minor >= 0;
				}
				return false;
			}
		}

		public DesignerVersion(int major, int minor)
		{
			Major = major;
			Minor = minor;
		}

		public bool IsGreaterThanOrEqualTo(int major, int minor)
		{
			if (Major < major)
			{
				return false;
			}
			if (Major > major)
			{
				return true;
			}
			return Minor >= minor;
		}

		public bool Equals(DesignerVersion other)
		{
			if (Major == other.Major)
			{
				return Minor == other.Minor;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is DesignerVersion other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (Major * 397) ^ Minor;
		}
	}
}

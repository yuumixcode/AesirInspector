using System.Collections.Generic;

namespace Sirenix.Utilities.Editor
{
	public class StringSliceEqualityComparer : IEqualityComparer<StringSlice>, IEqualityComparer<StringSlice.PreHashed>
	{
		public static readonly StringSliceEqualityComparer Instance = new StringSliceEqualityComparer();

		public bool Equals(StringSlice.PreHashed x, StringSlice.PreHashed y)
		{
			return x.Slice == y.Slice;
		}

		public bool Equals(StringSlice x, StringSlice y)
		{
			return x == y;
		}

		public int GetHashCode(StringSlice.PreHashed obj)
		{
			return obj.GetHashCode();
		}

		public int GetHashCode(StringSlice obj)
		{
			return obj.GetHashCode();
		}
	}
}

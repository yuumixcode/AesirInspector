using System.Collections.Generic;
using System.Reflection;

namespace Sirenix.OdinInspector.Editor
{
	public class FastMemberComparer : IEqualityComparer<MemberInfo>
	{
		public static readonly FastMemberComparer Instance = new FastMemberComparer();

		public bool Equals(MemberInfo x, MemberInfo y)
		{
			if ((object)x == y)
			{
				return true;
			}
			return x == y;
		}

		public int GetHashCode(MemberInfo obj)
		{
			return obj.GetHashCode();
		}
	}
}

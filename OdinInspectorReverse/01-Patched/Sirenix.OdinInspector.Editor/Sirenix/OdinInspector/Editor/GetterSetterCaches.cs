using System;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	internal static class GetterSetterCaches<TOwner>
	{
		public static readonly DoubleLookupDictionary<MemberInfo, Type, Delegate> Getters = new DoubleLookupDictionary<MemberInfo, Type, Delegate>(FastMemberComparer.Instance, FastTypeComparer.Instance);

		public static readonly DoubleLookupDictionary<MemberInfo, Type, Delegate> Setters = new DoubleLookupDictionary<MemberInfo, Type, Delegate>(FastMemberComparer.Instance, FastTypeComparer.Instance);
	}
}

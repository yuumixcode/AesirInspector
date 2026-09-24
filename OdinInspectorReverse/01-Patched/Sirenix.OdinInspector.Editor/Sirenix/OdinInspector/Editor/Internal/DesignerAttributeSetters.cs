using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerAttributeSetters
	{
		private static readonly Dictionary<MemberInfo, WeakValueSetter> ILGeneratedSetters = new Dictionary<MemberInfo, WeakValueSetter>(FastMemberComparer.Instance);

		public static WeakValueSetter GetSetter(MemberInfo memberInfo)
		{
			if (ILGeneratedSetters.TryGetValue(memberInfo, out var setter))
			{
				return setter;
			}
			setter = memberInfo.MemberType switch
			{
				MemberTypes.Field => EmitUtilities.CreateWeakInstanceFieldSetter(memberInfo.DeclaringType, (FieldInfo)memberInfo), 
				MemberTypes.Property => EmitUtilities.CreateWeakInstancePropertySetter(memberInfo.DeclaringType, (PropertyInfo)memberInfo), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			ILGeneratedSetters[memberInfo] = setter;
			return setter;
		}
	}
}

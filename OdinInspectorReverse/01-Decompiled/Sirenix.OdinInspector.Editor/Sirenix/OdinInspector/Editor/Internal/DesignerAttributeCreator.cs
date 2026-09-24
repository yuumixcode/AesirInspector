using System;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerAttributeCreator
	{
		private static readonly Dictionary<Type, Func<Attribute>> ILGeneratedConstructorCalls = new Dictionary<Type, Func<Attribute>>(128, FastTypeComparer.Instance);

		public static Attribute Create(Type attributeType)
		{
			if (!ILGeneratedConstructorCalls.TryGetValue(attributeType, out var ctorCall))
			{
				ctorCall = EmitUtilsWIP.CreateConstructorCall<Attribute>(attributeType, null);
				ILGeneratedConstructorCalls[attributeType] = ctorCall;
			}
			return ctorCall();
		}
	}
}

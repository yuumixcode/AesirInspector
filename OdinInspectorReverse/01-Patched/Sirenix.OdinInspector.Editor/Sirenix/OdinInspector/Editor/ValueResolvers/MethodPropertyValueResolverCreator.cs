using System;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
	public class MethodPropertyValueResolverCreator : BaseMemberValueResolverCreator
	{
		public override string GetPossibleMatchesString(ref ValueResolverContext context)
		{
			return null;
		}

		public override ValueResolverFunc<TResult> TryCreateResolverFunc<TResult>(ref ValueResolverContext context)
		{
			InspectorProperty prop = context.Property;
			if (string.IsNullOrEmpty(context.ResolvedString) && prop.Info.PropertyType == PropertyType.Method)
			{
				MethodInfo method = (prop.Info.GetMemberInfo() as MethodInfo) ?? prop.Info.GetMethodDelegate().Method;
				if (method.IsGenericMethodDefinition)
				{
					context.ErrorMessage = "Cannot invoke a generic method definition such as '" + method.GetNiceName() + "'.";
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
				Type containedType = method.ReturnType;
				if (containedType == typeof(void) || !ConvertUtility.CanConvert(containedType, typeof(TResult)))
				{
					return null;
				}
				NamedValues argSetup = default(NamedValues);
				if (BaseMemberValueResolverCreator.IsCompatibleMethod(method, ref context.NamedValues, ref argSetup, context.SyncRefParametersWithNamedValues, out context.ErrorMessage))
				{
					if ((object)prop.Info.GetMethodDelegate() != null)
					{
						return BaseMemberValueResolverCreator.GetDelegateGetter<TResult>(prop.Info.GetMethodDelegate(), argSetup);
					}
					return BaseMemberValueResolverCreator.GetMethodGetter<TResult>(method, argSetup, prop.ParentType.IsValueType);
				}
				if (context.ErrorMessage != null)
				{
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
			}
			return null;
		}
	}
}

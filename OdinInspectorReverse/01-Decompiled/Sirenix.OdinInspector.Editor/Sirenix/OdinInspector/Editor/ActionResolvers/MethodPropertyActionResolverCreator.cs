using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.ActionResolvers
{
	public class MethodPropertyActionResolverCreator : ActionResolverCreator
	{
		public override string GetPossibleMatchesString(ref ActionResolverContext context)
		{
			return null;
		}

		public override ResolvedAction TryCreateAction(ref ActionResolverContext context)
		{
			InspectorProperty prop = context.Property;
			if (string.IsNullOrEmpty(context.ResolvedString) && prop.Info.PropertyType == PropertyType.Method)
			{
				MethodInfo method = (prop.Info.GetMemberInfo() as MethodInfo) ?? prop.Info.GetMethodDelegate().Method;
				if (method.IsGenericMethodDefinition)
				{
					context.ErrorMessage = "Cannot invoke a generic method definition such as '" + method.GetNiceName() + "'.";
					return ActionResolverCreator.FailedResolveAction;
				}
				NamedValues argSetup = default(NamedValues);
				if (ActionResolverCreator.IsCompatibleMethod(method, ref context.NamedValues, ref argSetup, context.SyncRefParametersWithNamedValues, out context.ErrorMessage))
				{
					if ((object)prop.Info.GetMethodDelegate() != null)
					{
						return ActionResolverCreator.GetDelegateInvoker(prop.Info.GetMethodDelegate(), argSetup);
					}
					return ActionResolverCreator.GetMethodInvoker(method, argSetup, prop.ParentType.IsValueType);
				}
				if (context.ErrorMessage != null)
				{
					return ActionResolverCreator.FailedResolveAction;
				}
			}
			return null;
		}
	}
}

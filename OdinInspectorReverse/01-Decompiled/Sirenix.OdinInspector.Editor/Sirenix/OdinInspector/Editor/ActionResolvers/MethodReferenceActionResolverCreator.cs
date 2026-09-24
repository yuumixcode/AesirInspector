using System;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.ActionResolvers
{
	public class MethodReferenceActionResolverCreator : ActionResolverCreator
	{
		public override string GetPossibleMatchesString(ref ActionResolverContext context)
		{
			return "Method References: \"MethodName\"";
		}

		public override ResolvedAction TryCreateAction(ref ActionResolverContext context)
		{
			if (string.IsNullOrEmpty(context.ResolvedString))
			{
				return null;
			}
			string memberName = context.ResolvedString;
			if (!Sirenix.Utilities.TypeExtensions.IsValidIdentifier(memberName))
			{
				return null;
			}
			BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
			bool isStatic = context.Property == context.Property.Tree.RootProperty && context.Property.Tree.IsStatic;
			if (!isStatic)
			{
				flags |= BindingFlags.Instance;
			}
			Type contextType = context.ParentType;
			NamedValues argSetup = default(NamedValues);
			string errorMessage;
			MethodInfo method = GetCompatibleMethod(contextType, memberName, flags, ref context.NamedValues, ref argSetup, context.SyncRefParametersWithNamedValues, out errorMessage);
			if (errorMessage != null)
			{
				context.ErrorMessage = errorMessage;
				return ActionResolverCreator.FailedResolveAction;
			}
			if (method == null && !isStatic)
			{
				Type current = contextType.BaseType;
				BindingFlags newFlags = flags;
				newFlags &= ~BindingFlags.FlattenHierarchy;
				newFlags |= BindingFlags.DeclaredOnly;
				do
				{
					method = GetCompatibleMethod(current, memberName, flags, ref context.NamedValues, ref argSetup, context.SyncRefParametersWithNamedValues, out errorMessage);
					if (errorMessage != null)
					{
						context.ErrorMessage = errorMessage;
						return ActionResolverCreator.FailedResolveAction;
					}
					if (!(method == null))
					{
						break;
					}
					current = current.BaseType;
				}
				while (current != null);
			}
			if (method != null)
			{
				return ActionResolverCreator.GetMethodInvoker(method, argSetup, context.ParentType.IsValueType);
			}
			return null;
		}

		private static MethodInfo GetCompatibleMethod(Type type, string methodName, BindingFlags flags, ref NamedValues namedValues, ref NamedValues argSetup, bool requiresBackcasting, out string errorMessage)
		{
			MethodInfo method;
			try
			{
				method = type.GetMethod(methodName, flags);
			}
			catch (AmbiguousMatchException)
			{
				errorMessage = "Could not find exact method named '" + methodName + "' because there are several methods with that name defined, and so it is an ambiguous match.";
				return null;
			}
			if (method == null)
			{
				errorMessage = null;
				return null;
			}
			if (!ActionResolverCreator.IsCompatibleMethod(method, ref namedValues, ref argSetup, requiresBackcasting, out errorMessage))
			{
				return null;
			}
			return method;
		}
	}
}

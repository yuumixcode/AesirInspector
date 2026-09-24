using System;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
	public class MemberReferenceValueResolverCreator : BaseMemberValueResolverCreator
	{
		public override string GetPossibleMatchesString(ref ValueResolverContext context)
		{
			if (context.ResultType == typeof(string))
			{
				return "Member References: \"$MemberName\"";
			}
			return "Member References: \"MemberName\"";
		}

		public override ValueResolverFunc<TResult> TryCreateResolverFunc<TResult>(ref ValueResolverContext context)
		{
			if (string.IsNullOrEmpty(context.ResolvedString))
			{
				return null;
			}
			bool mustBeMember;
			string memberName;
			if (context.ResolvedString[0] == '$')
			{
				if (context.ResolvedString.Length == 1)
				{
					return null;
				}
				mustBeMember = true;
				memberName = context.ResolvedString.Substring(1);
			}
			else
			{
				if (typeof(TResult) == typeof(string) && context.HasFallbackValue)
				{
					return null;
				}
				mustBeMember = false;
				memberName = context.ResolvedString;
			}
			if (!Sirenix.Utilities.TypeExtensions.IsValidIdentifier(memberName))
			{
				if (mustBeMember)
				{
					context.ErrorMessage = "'" + memberName + "' is not a valid C# member identifier.";
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
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
			MemberInfo member = contextType.GetField(memberName, flags);
			if (member == null)
			{
				member = contextType.GetProperty(memberName, flags);
			}
			string errorMessage;
			if (member == null)
			{
				member = BaseMemberValueResolverCreator.GetCompatibleMethod(contextType, memberName, flags, ref context.NamedValues, ref argSetup, context.SyncRefParametersWithNamedValues, out errorMessage);
				if (errorMessage != null)
				{
					context.ErrorMessage = errorMessage;
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
			}
			if (member == null && !isStatic)
			{
				Type current = contextType.BaseType;
				BindingFlags newFlags = flags;
				newFlags &= ~BindingFlags.FlattenHierarchy;
				newFlags |= BindingFlags.DeclaredOnly;
				do
				{
					member = current.GetField(memberName, newFlags);
					if (member == null)
					{
						member = current.GetProperty(memberName, newFlags);
					}
					if (member == null)
					{
						member = BaseMemberValueResolverCreator.GetCompatibleMethod(current, memberName, flags, ref context.NamedValues, ref argSetup, context.SyncRefParametersWithNamedValues, out errorMessage);
						if (errorMessage != null)
						{
							context.ErrorMessage = errorMessage;
							return ValueResolverCreator.GetFailedResolverFunc<TResult>();
						}
					}
					if (!(member == null))
					{
						break;
					}
					current = current.BaseType;
				}
				while (current != null);
			}
			if (member != null)
			{
				Type containedType = member.GetReturnType();
				if (containedType == typeof(void) || !ConvertUtility.CanConvert(containedType, typeof(TResult)))
				{
					if (member is MethodInfo)
					{
						if (containedType == typeof(void))
						{
							context.ErrorMessage = "Method " + member.Name + " cannot return void; it must return a value that can be assigned or converted to the type '" + typeof(TResult).GetNiceName() + "'";
						}
						else
						{
							context.ErrorMessage = "Cannot convert method " + member.Name + "'s return type '" + containedType.GetNiceName() + "' to required type '" + typeof(TResult).GetNiceName() + "'";
						}
					}
					else
					{
						context.ErrorMessage = "Cannot convert member " + member.Name + "'s contained type '" + containedType.GetNiceName() + "' to required type '" + typeof(TResult).GetNiceName() + "'";
					}
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
				if (member is FieldInfo)
				{
					return BaseMemberValueResolverCreator.GetFieldGetter<TResult>(member as FieldInfo);
				}
				if (member is PropertyInfo)
				{
					return BaseMemberValueResolverCreator.GetPropertyGetter<TResult>(member as PropertyInfo, context.Property.ParentType.IsValueType);
				}
				return BaseMemberValueResolverCreator.GetMethodGetter<TResult>(member as MethodInfo, argSetup, context.Property.ParentType.IsValueType);
			}
			if (mustBeMember)
			{
				context.ErrorMessage = "Could not find a field, property or method with the name '" + memberName + "' on the type '" + contextType.GetNiceName() + "'.";
				return ValueResolverCreator.GetFailedResolverFunc<TResult>();
			}
			return null;
		}
	}
}

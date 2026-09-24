using System;
using System.Linq;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
	public static class DefaultIndexingRules
	{
		public static readonly TypeMatchIndexingRule MustBeAbleToInstantiateType = new TypeMatchIndexingRule("Must be able to instantiate {name}", delegate(ref TypeSearchInfo info, ref string error)
		{
			if (info.MatchType.IsAbstract)
			{
				error = "Type is an abstract";
				return false;
			}
			if (info.MatchType.IsInterface)
			{
				error = "Type is an interface";
				return false;
			}
			if (info.MatchType.GetConstructor(Type.EmptyTypes) == null)
			{
				error = "Has no public parameterless constructor";
				return false;
			}
			return true;
		});

		public static readonly TypeMatchIndexingRule NoAbstractOrInterfaceTargets = new TypeMatchIndexingRule("No abstract or interface targets", delegate(ref TypeSearchInfo info, ref string error)
		{
			for (int i = 0; i < info.Targets.Length; i++)
			{
				if (!info.Targets[i].IsGenericParameter)
				{
					if (info.Targets[i].IsInterface)
					{
						error = "You cannot use an interface '" + info.Targets[i].GetNiceName() + "' as a {name} target. Use a generic {name} with a constraint for that interface instead.";
						return false;
					}
					if (info.Targets[i].IsAbstract)
					{
						error = "You cannot use an abstract type '" + info.Targets[i].GetNiceName() + "' as a {name} target. Use a generic {name} with a constraint for that abstract class instead.";
						return false;
					}
				}
			}
			return true;
		});

		public static readonly TypeMatchIndexingRule GenericMatchTypeValidation = new TypeMatchIndexingRule("Generic {name} validation", delegate(ref TypeSearchInfo info, ref string error)
		{
			if (!info.MatchType.IsGenericTypeDefinition)
			{
				return true;
			}
			if (info.Targets.Length != 1)
			{
				return true;
			}
			if (info.Targets[0].IsGenericParameter)
			{
				return true;
			}
			Type type = info.Targets[0];
			if (!info.MatchType.IsNested || !info.MatchType.DeclaringType.IsGenericType)
			{
				if (type.IsGenericType && type.GenericArgumentsContainsTypes((from n in info.MatchType.GetGenericArguments()
					where n.IsGenericParameter
					select n).ToArray()))
				{
					return true;
				}
				error = "You cannot declare a generic {name} without passing a generic parameter as the target, or a generic type definition containing all the {name}'s generic parameters as the target. You passed '" + type.GetNiceName() + "'.";
				return false;
			}
			Type[] genericArguments = info.MatchType.DeclaringType.GetGenericArguments();
			Type[] genericArguments2 = info.MatchType.GetGenericArguments();
			Type[] genericArguments3 = type.GetGenericArguments();
			bool flag = genericArguments.Length == genericArguments2.Length && genericArguments.Length == genericArguments3.Length;
			if (flag)
			{
				for (int num = 0; num < genericArguments.Length; num++)
				{
					if (genericArguments[num].Name != genericArguments2[num].Name || genericArguments[num].Name != genericArguments3[num].Name)
					{
						flag = false;
						break;
					}
				}
			}
			if (!flag)
			{
				error = "You cannot declare {name}s nested inside generic types unless the following conditions are true: 1) the nested {name} itself is not generic, 2) the nested {name} must target a type that is nested within the same type as the nested {name}, 3) the target type must not be generic.";
				return false;
			}
			if (!type.IsNested || !type.DeclaringType.IsGenericType)
			{
				if (info.MatchType.IsGenericType && type.GenericArgumentsContainsTypes((from n in type.GetGenericArguments()
					where n.IsGenericParameter
					select n).ToArray()))
				{
					return true;
				}
				error = "You cannot declare a generic {name} without passing either a generic parameter or a generic type definition containing all the {name}'s generic parameters as the target. You passed '" + type.GetNiceName() + "'.";
				return false;
			}
			Type[] genericArguments4 = type.DeclaringType.GetGenericArguments();
			Type[] genericArguments5 = type.GetGenericArguments();
			Type[] genericArguments6 = info.MatchType.GetGenericArguments();
			bool flag2 = genericArguments4.Length == genericArguments5.Length && genericArguments4.Length == genericArguments6.Length;
			if (flag2)
			{
				for (int num2 = 0; num2 < genericArguments4.Length; num2++)
				{
					if (genericArguments4[num2].Name != genericArguments5[num2].Name || genericArguments4[num2].Name != genericArguments6[num2].Name)
					{
						flag2 = false;
						break;
					}
				}
			}
			if (!flag2)
			{
				error = "You cannot declare {name}s nested inside generic types unless the following conditions are true: 1) the nested {name} itself is not generic, 2) the nested {name} must target a type that is nested within the same type as the nested {name}, 3) the target type must not be generic.";
				return false;
			}
			return true;
		});

		public static readonly TypeMatchIndexingRule GenericDefinitionSanityCheck = new TypeMatchIndexingRule("Generic {name} definition sanity check", delegate(ref TypeSearchInfo info, ref string error)
		{
			if (!info.MatchType.IsGenericTypeDefinition)
			{
				return true;
			}
			for (int i = 0; i < info.Targets.Length && !info.Targets[i].IsGenericParameter; i++)
			{
			}
			return true;
		});
	}
}

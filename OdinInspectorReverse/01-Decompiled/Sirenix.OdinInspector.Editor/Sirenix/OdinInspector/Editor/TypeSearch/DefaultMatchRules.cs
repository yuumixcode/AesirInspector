using System;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
	public static class DefaultMatchRules
	{
		public static readonly TypeMatchRule ExactMatch = new TypeMatchRule("Exact Match --> Type : Match[<Target>]", delegate(TypeSearchInfo info, Type[] targets)
		{
			if (info.MatchType.IsGenericTypeDefinition)
			{
				return (Type)null;
			}
			if (targets.Length != info.Targets.Length)
			{
				return (Type)null;
			}
			for (int i = 0; i < targets.Length; i++)
			{
				if (targets[i] != info.Targets[i])
				{
					return (Type)null;
				}
			}
			return info.MatchType;
		});

		public static readonly TypeMatchRule GenericSingleTargetMatch = new TypeMatchRule("Generic Single Target Match --> Type<T1 [, T2]> : Match<GenericType<T1 [, T2]>> [where T1 [, T2] : constraints]", delegate(TypeSearchInfo info, Type[] targets)
		{
			if (!info.MatchType.IsGenericTypeDefinition)
			{
				return (Type)null;
			}
			if (targets.Length != 1)
			{
				return (Type)null;
			}
			if (!info.Targets[0].IsGenericType || !targets[0].IsGenericType)
			{
				return (Type)null;
			}
			if (info.Targets[0].GetGenericTypeDefinition() != targets[0].GetGenericTypeDefinition())
			{
				return (Type)null;
			}
			Type[] genericArguments = info.MatchType.GetGenericArguments();
			Type[] genericArguments2 = info.Targets[0].GetGenericArguments();
			Type[] genericArguments3 = targets[0].GetGenericArguments();
			if (genericArguments.Length != genericArguments2.Length || genericArguments.Length != genericArguments3.Length)
			{
				return (Type)null;
			}
			return (!info.MatchType.AreGenericConstraintsSatisfiedBy(genericArguments3)) ? null : info.MatchType.MakeGenericType(genericArguments3);
		});

		public static readonly TypeMatchRule TargetsSatisfyGenericParameterConstraints = new TypeMatchRule("Targets Satisfy Generic Parameter Constraints --> Type<T1 [, T2]> : Match<T1 [, T2]> [where T1 [, T2] : constraints]", delegate(TypeSearchInfo info, Type[] targets)
		{
			for (int i = 0; i < info.Targets.Length; i++)
			{
				if (!info.Targets[i].IsGenericParameter)
				{
					return (Type)null;
				}
			}
			return (info.MatchType.IsGenericType && info.MatchType.AreGenericConstraintsSatisfiedBy(targets)) ? info.MatchType.MakeGenericType(targets) : null;
		});

		public static readonly TypeMatchRule GenericParameterInference = new TypeMatchRule("Generic Parameter Inference ---> Type<T1 [, T2] : Match<T1> [where T1 : constraints [, T2]]", delegate(TypeSearchInfo info, Type[] targets)
		{
			if (!info.MatchType.IsGenericType)
			{
				return (Type)null;
			}
			int num = 0;
			for (int i = 0; i < info.Targets.Length; i++)
			{
				if (info.Targets[i].IsGenericParameter)
				{
					num++;
				}
				else if (info.Targets[i] != targets[i])
				{
					return (Type)null;
				}
			}
			if (num == 0)
			{
				return (Type)null;
			}
			Type[] array;
			if (num != targets.Length)
			{
				array = new Type[num];
				int num2 = 0;
				for (int j = 0; j < info.Targets.Length; j++)
				{
					if (info.Targets[j].IsGenericParameter)
					{
						array[num2++] = targets[j];
					}
				}
			}
			else
			{
				array = targets;
			}
			Type[] inferredParams;
			try
			{
				if (info.MatchType.TryInferGenericParameters(out inferredParams, array))
				{
					return info.MatchType.GetGenericTypeDefinition().MakeGenericType(inferredParams);
				}
			}
			catch (ArgumentException ex)
			{
				Debug.Log("WHoops");
				if (!info.MatchType.TryInferGenericParameters(out inferredParams, array))
				{
					throw ex;
				}
				return info.MatchType.GetGenericTypeDefinition().MakeGenericType(inferredParams);
			}
			return (Type)null;
		});

		public static readonly TypeMatchRule NestedInSameGenericType = new TypeMatchRule("Nested In Same Generic Type ---> Type<T1, [, T2]>.NestedType : Type<T1, [, T2]>.Match<Target>", delegate(TypeSearchInfo info, Type[] targets)
		{
			if (targets.Length != 1)
			{
				return (Type)null;
			}
			Type type = targets[0];
			if (!info.MatchType.IsNested || !info.Targets[0].IsNested || !type.IsNested)
			{
				return (Type)null;
			}
			if (!info.MatchType.DeclaringType.IsGenericType || !info.Targets[0].DeclaringType.IsGenericType || !type.DeclaringType.IsGenericType)
			{
				return (Type)null;
			}
			if (info.MatchType.DeclaringType.GetGenericTypeDefinition() != info.Targets[0].DeclaringType.GetGenericTypeDefinition() || info.MatchType.DeclaringType.GetGenericTypeDefinition() != type.DeclaringType.GetGenericTypeDefinition())
			{
				return (Type)null;
			}
			Type[] genericArguments = type.GetGenericArguments();
			return info.MatchType.AreGenericConstraintsSatisfiedBy(genericArguments) ? info.MatchType.MakeGenericType(genericArguments) : null;
		});
	}
}

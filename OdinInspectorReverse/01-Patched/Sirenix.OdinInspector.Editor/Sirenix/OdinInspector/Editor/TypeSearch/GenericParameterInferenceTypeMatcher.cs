using System;
using System.Collections.Generic;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
	public class GenericParameterInferenceTypeMatcher : TypeMatcher
	{
		public class Creator : TypeMatcherCreator
		{
			public override bool TryCreateMatcher(TypeSearchInfo info, out TypeMatcher matcher)
			{
				matcher = null;
				int genericParameterTargetCount = 0;
				if (!info.MatchType.IsGenericType)
				{
					return false;
				}
				for (int i = 0; i < info.Targets.Length; i++)
				{
					if (info.Targets[i].IsGenericParameter)
					{
						genericParameterTargetCount++;
					}
				}
				if (genericParameterTargetCount == 0)
				{
					return false;
				}
				bool[] targetIsGenericParameterCached = new bool[info.Targets.Length];
				for (int j = 0; j < targetIsGenericParameterCached.Length; j++)
				{
					targetIsGenericParameterCached[j] = info.Targets[j].IsGenericParameter;
				}
				Type[] matchTypeGenericArgs = info.MatchType.GetGenericArguments();
				Type[] matchTypeGenericDefinitionArgs = info.MatchType.GetGenericTypeDefinition().GetGenericArguments();
				matcher = new GenericParameterInferenceTypeMatcher
				{
					info = info,
					genericParameterTargetCount = genericParameterTargetCount,
					targetIsGenericParameterCached = targetIsGenericParameterCached,
					typeArrayOfGenericParameterTargetCountSize = new Type[genericParameterTargetCount],
					matchTypeGenericArgs = new Type[matchTypeGenericArgs.Length],
					matchTypeGenericArgs_Backup = matchTypeGenericArgs,
					matchTypeGenericDefinitionArgs = new Type[matchTypeGenericDefinitionArgs.Length],
					matchTypeGenericDefinitionArgs_Backup = matchTypeGenericDefinitionArgs
				};
				return true;
			}
		}

		private static readonly Dictionary<Type, Type[]> GenericParameterConstraintsCache = new Dictionary<Type, Type[]>(FastTypeComparer.Instance);

		private static readonly Dictionary<Type, Type[]> GenericArgumentsCache = new Dictionary<Type, Type[]>(FastTypeComparer.Instance);

		private TypeSearchInfo info;

		private int genericParameterTargetCount;

		private bool[] targetIsGenericParameterCached;

		private Type[] typeArrayOfGenericParameterTargetCountSize;

		private Type[] matchTypeGenericArgs_Backup;

		private Type[] matchTypeGenericDefinitionArgs_Backup;

		private Type[] matchTypeGenericArgs;

		private Type[] matchTypeGenericDefinitionArgs;

		private static readonly object LOCK = new object();

		private static readonly Dictionary<Type, Type> GenericConstraintsSatisfactionInferredParameters = new Dictionary<Type, Type>();

		private static readonly HashSet<Type> GenericConstraintsSatisfactionTypesToCheck = new HashSet<Type>();

		private static readonly List<Type> GenericConstraintsSatisfactionTypesToCheck_ToAdd = new List<Type>();

		public override string Name => "Generic Parameter Inference ---> Type<T1 [, T2] : Match<T1> [where T1 : constraints [, T2]]";

		public override Type Match(Type[] targets, ref bool stopMatching)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				if (!targetIsGenericParameterCached[i] && info.Targets[i] != targets[i])
				{
					return null;
				}
			}
			lock (LOCK)
			{
				Type[] inferTargets;
				if (genericParameterTargetCount != targets.Length)
				{
					inferTargets = typeArrayOfGenericParameterTargetCountSize;
					int count = 0;
					for (int j = 0; j < info.Targets.Length; j++)
					{
						if (targetIsGenericParameterCached[j])
						{
							inferTargets[count++] = targets[j];
						}
					}
				}
				else
				{
					inferTargets = targets;
				}
				if (TryInferGenericParameters(info.MatchType, out var inferredArgs, inferTargets))
				{
					return info.MatchType.GetGenericTypeDefinition().MakeGenericType(inferredArgs);
				}
				return null;
			}
		}

		private bool TryInferGenericParameters(Type genericTypeDefinition, out Type[] inferredParams, params Type[] knownParameters)
		{
			if (genericTypeDefinition == null)
			{
				throw new ArgumentNullException("genericTypeDefinition");
			}
			if (knownParameters == null)
			{
				throw new ArgumentNullException("knownParameters");
			}
			if (!genericTypeDefinition.IsGenericType)
			{
				throw new ArgumentException("The genericTypeDefinition parameter must be a generic type.");
			}
			for (int i = 0; i < matchTypeGenericArgs.Length; i++)
			{
				matchTypeGenericArgs[i] = matchTypeGenericArgs_Backup[i];
			}
			for (int j = 0; j < matchTypeGenericDefinitionArgs.Length; j++)
			{
				matchTypeGenericDefinitionArgs[j] = matchTypeGenericDefinitionArgs_Backup[j];
			}
			Dictionary<Type, Type> matches = GenericConstraintsSatisfactionInferredParameters;
			matches.Clear();
			HashSet<Type> typesToCheck = GenericConstraintsSatisfactionTypesToCheck;
			typesToCheck.Clear();
			List<Type> typesToCheck_ToAdd = GenericConstraintsSatisfactionTypesToCheck_ToAdd;
			typesToCheck_ToAdd.Clear();
			for (int k = 0; k < knownParameters.Length; k++)
			{
				typesToCheck.Add(knownParameters[k]);
			}
			Type[] definitions = matchTypeGenericArgs;
			if (!genericTypeDefinition.IsGenericTypeDefinition)
			{
				Type[] constructedParameters = definitions;
				genericTypeDefinition = genericTypeDefinition.GetGenericTypeDefinition();
				definitions = matchTypeGenericDefinitionArgs;
				int unknownCount = 0;
				for (int l = 0; l < constructedParameters.Length; l++)
				{
					if (!constructedParameters[l].IsGenericParameter && (!constructedParameters[l].IsGenericType || constructedParameters[l].IsFullyConstructedGenericType()))
					{
						matches[definitions[l]] = constructedParameters[l];
					}
					else
					{
						unknownCount++;
					}
				}
				if (unknownCount == knownParameters.Length)
				{
					int count = 0;
					for (int m = 0; m < constructedParameters.Length; m++)
					{
						if (constructedParameters[m].IsGenericParameter)
						{
							constructedParameters[m] = knownParameters[count++];
						}
					}
					if (TypeExtensions.AreGenericConstraintsSatisfiedBy(matchTypeGenericDefinitionArgs_Backup, constructedParameters))
					{
						inferredParams = constructedParameters;
						return true;
					}
				}
			}
			if (definitions.Length == knownParameters.Length && TypeExtensions.AreGenericConstraintsSatisfiedBy(matchTypeGenericDefinitionArgs_Backup, knownParameters))
			{
				inferredParams = knownParameters;
				return true;
			}
			Type[] array = definitions;
			foreach (Type typeArg in array)
			{
				Type[] constraints = GetGenericParameterConstraintsCached(typeArg);
				Type[] array2 = constraints;
				foreach (Type constraint in array2)
				{
					if (!constraint.IsGenericType)
					{
						continue;
					}
					Type constraintDefinition = constraint.GetGenericTypeDefinition();
					Type[] constraintParams = GetGenericArgumentsCached(constraint);
					foreach (Type parameter in typesToCheck)
					{
						Type[] paramParams;
						if (parameter.IsGenericType && constraintDefinition == parameter.GetGenericTypeDefinition())
						{
							paramParams = GetGenericArgumentsCached(parameter);
						}
						else if (constraintDefinition.IsInterface && parameter.ImplementsOpenGenericInterface(constraintDefinition))
						{
							paramParams = parameter.GetArgumentsOfInheritedOpenGenericInterface(constraintDefinition);
						}
						else
						{
							if (!constraintDefinition.IsClass || !parameter.ImplementsOpenGenericClass(constraintDefinition))
							{
								continue;
							}
							paramParams = parameter.GetArgumentsOfInheritedOpenGenericClass(constraintDefinition);
						}
						matches[typeArg] = parameter;
						typesToCheck_ToAdd.Add(parameter);
						for (int num2 = 0; num2 < constraintParams.Length; num2++)
						{
							if (constraintParams[num2].IsGenericParameter)
							{
								matches[constraintParams[num2]] = paramParams[num2];
								typesToCheck_ToAdd.Add(paramParams[num2]);
							}
						}
					}
					foreach (Type type in typesToCheck_ToAdd)
					{
						typesToCheck.Add(type);
					}
					typesToCheck_ToAdd.Clear();
				}
			}
			if (matches.Count == definitions.Length)
			{
				inferredParams = new Type[matches.Count];
				for (int num3 = 0; num3 < definitions.Length; num3++)
				{
					inferredParams[num3] = matches[definitions[num3]];
				}
				if (TypeExtensions.AreGenericConstraintsSatisfiedBy(matchTypeGenericDefinitionArgs_Backup, inferredParams))
				{
					return true;
				}
			}
			inferredParams = null;
			return false;
		}

		private static Type[] GetGenericParameterConstraintsCached(Type type)
		{
			if (!GenericParameterConstraintsCache.TryGetValue(type, out var result))
			{
				result = type.GetGenericParameterConstraints();
				GenericParameterConstraintsCache.Add(type, result);
			}
			return result;
		}

		private static Type[] GetGenericArgumentsCached(Type type)
		{
			if (!GenericArgumentsCache.TryGetValue(type, out var result))
			{
				result = type.GetGenericArguments();
				GenericArgumentsCache.Add(type, result);
			}
			return result;
		}
	}
}

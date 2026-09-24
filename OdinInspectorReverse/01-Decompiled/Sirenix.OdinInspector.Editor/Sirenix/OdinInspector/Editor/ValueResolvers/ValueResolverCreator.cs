using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
	public abstract class ValueResolverCreator
	{
		private struct ResolverAndPriority
		{
			public ValueResolverCreator ResolverCreator;

			public double Priority;
		}

		private static StringBuilder SB;

		private static ResolverAndPriority[] ValueResolverCreators;

		private static readonly Dictionary<Type, Delegate> FailedResolveFuncs;

		private static readonly Dictionary<Type, Delegate> FallbackResolveFuncs;

		private static readonly Dictionary<Type, MethodInfo> WeaklyTypedGetResolverMethods;

		private static readonly Dictionary<Type, MethodInfo> WeaklyTypedGetResolverFromContextMethods;

		private static readonly MethodInfo GetResolverMethodInfo;

		private static readonly MethodInfo GetResolverFromContextMethodInfo;

		private static readonly object[] GetResolverMethodParameters;

		private static readonly object[] GetResolverFromContextMethodParameters;

		static ValueResolverCreator()
		{
			SB = new StringBuilder();
			ValueResolverCreators = new ResolverAndPriority[8];
			FailedResolveFuncs = new Dictionary<Type, Delegate>(FastTypeComparer.Instance);
			FallbackResolveFuncs = new Dictionary<Type, Delegate>(FastTypeComparer.Instance);
			WeaklyTypedGetResolverMethods = new Dictionary<Type, MethodInfo>(FastTypeComparer.Instance);
			WeaklyTypedGetResolverFromContextMethods = new Dictionary<Type, MethodInfo>(FastTypeComparer.Instance);
			GetResolverMethodInfo = typeof(ValueResolverCreator).GetMethod("GetResolver", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[5]
			{
				typeof(InspectorProperty),
				typeof(string),
				typeof(object),
				typeof(bool),
				typeof(NamedValue[])
			}, null);
			GetResolverFromContextMethodInfo = typeof(ValueResolverCreator).GetMethod("GetResolverFromContext", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[1] { typeof(ValueResolverContext).MakeByRefType() }, null);
			GetResolverMethodParameters = new object[5];
			GetResolverFromContextMethodParameters = new object[1];
			List<Assembly> assemblies = ResolverUtilities.GetResolverAssemblies();
			for (int i = 0; i < assemblies.Count; i++)
			{
				Assembly assembly = assemblies[i];
				object[] attrs = assembly.SafeGetCustomAttributes(typeof(RegisterDefaultValueResolverCreatorAttribute), inherit: false);
				for (int j = 0; j < attrs.Length; j++)
				{
					RegisterDefaultValueResolverCreatorAttribute attr = (RegisterDefaultValueResolverCreatorAttribute)attrs[j];
					try
					{
						object instance = Activator.CreateInstance(attr.ResolverCreatorType);
						Register((ValueResolverCreator)instance, attr.Order);
					}
					catch (Exception innerException)
					{
						while (innerException.InnerException != null && innerException is TargetInvocationException)
						{
							innerException = innerException.InnerException;
						}
						Debug.LogException(new Exception("Failed to create instance of registered default resolver of type '" + attr.ResolverCreatorType.GetNiceFullName() + "'", innerException));
					}
				}
			}
		}

		public abstract ValueResolverFunc<TResult> TryCreateResolverFunc<TResult>(ref ValueResolverContext context);

		public abstract string GetPossibleMatchesString(ref ValueResolverContext context);

		private static TResult FailedResolveResult<TResult>(ref ValueResolverContext context, int selectionIndex)
		{
			if (context.HasFallbackValue)
			{
				return (TResult)context.FallbackValue;
			}
			return default(TResult);
		}

		public static void Register(ValueResolverCreator valueResolverCreator, double order = 0.0)
		{
			ResolverAndPriority[] array = ValueResolverCreators;
			bool added = false;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].ResolverCreator == null)
				{
					array[i].ResolverCreator = valueResolverCreator;
					array[i].Priority = order;
					added = true;
					break;
				}
				if (order > array[i].Priority)
				{
					ShiftUp(ref array, i);
					array[i].ResolverCreator = valueResolverCreator;
					array[i].Priority = order;
					added = true;
					break;
				}
			}
			if (!added)
			{
				int index = array.Length;
				Expand(ref array);
				array[index].ResolverCreator = valueResolverCreator;
				array[index].Priority = order;
			}
			ValueResolverCreators = array;
		}

		private static void Expand(ref ResolverAndPriority[] array)
		{
			ResolverAndPriority[] newArray = new ResolverAndPriority[array.Length * 2];
			Array.Copy(array, newArray, array.Length);
			array = newArray;
		}

		private static void ShiftUp(ref ResolverAndPriority[] array, int index)
		{
			int arrayEnd = array.Length - 1;
			if (array[arrayEnd].ResolverCreator != null)
			{
				Expand(ref array);
			}
			for (int i = arrayEnd; i >= index; i--)
			{
				if (i + 1 < array.Length)
				{
					array[i + 1].ResolverCreator = array[i].ResolverCreator;
					array[i + 1].Priority = array[i].Priority;
				}
			}
		}

		public static ValueResolver GetResolver(Type resultType, InspectorProperty property, string resolvedString)
		{
			return GetResolver(resultType, property, resolvedString, null, hasFallback: false, null);
		}

		public static ValueResolver GetResolver(Type resultType, InspectorProperty property, string resolvedString, params NamedValue[] namedArgs)
		{
			return GetResolver(resultType, property, resolvedString, null, hasFallback: false, namedArgs);
		}

		public static ValueResolver GetResolver(Type resultType, InspectorProperty property, string resolvedString, object fallbackValue)
		{
			return GetResolver(resultType, property, resolvedString, fallbackValue, hasFallback: true, null);
		}

		public static ValueResolver GetResolver(Type resultType, InspectorProperty property, string resolvedString, object fallbackValue, params NamedValue[] namedArgs)
		{
			return GetResolver(resultType, property, resolvedString, fallbackValue, hasFallback: true, namedArgs);
		}

		private static ValueResolver GetResolver(Type resultType, InspectorProperty property, string resolvedString, object fallbackValue, bool hasFallback, params NamedValue[] namedArgs)
		{
			if (!WeaklyTypedGetResolverMethods.TryGetValue(resultType, out var method))
			{
				method = GetResolverMethodInfo.MakeGenericMethod(resultType);
				WeaklyTypedGetResolverMethods.Add(resultType, method);
			}
			GetResolverMethodParameters[0] = property;
			GetResolverMethodParameters[1] = resolvedString;
			GetResolverMethodParameters[2] = fallbackValue;
			GetResolverMethodParameters[3] = hasFallback;
			GetResolverMethodParameters[4] = namedArgs;
			return (ValueResolver)method.Invoke(null, GetResolverMethodParameters);
		}

		public static ValueResolver<TResult> GetResolver<TResult>(InspectorProperty property, string resolvedString)
		{
			return GetResolver<TResult>(property, resolvedString, null, hasFallback: false, null);
		}

		public static ValueResolver<TResult> GetResolver<TResult>(InspectorProperty property, string resolvedString, params NamedValue[] namedArgs)
		{
			return GetResolver<TResult>(property, resolvedString, null, hasFallback: false, namedArgs);
		}

		public static ValueResolver<TResult> GetResolver<TResult>(InspectorProperty property, string resolvedString, TResult fallbackValue)
		{
			return GetResolver<TResult>(property, resolvedString, fallbackValue, hasFallback: true, null);
		}

		public static ValueResolver<TResult> GetResolver<TResult>(InspectorProperty property, string resolvedString, TResult fallbackValue, params NamedValue[] namedArgs)
		{
			return GetResolver<TResult>(property, resolvedString, fallbackValue, hasFallback: true, namedArgs);
		}

		private static ValueResolver<TResult> GetResolver<TResult>(InspectorProperty property, string resolvedString, object fallbackValue, bool hasFallback, params NamedValue[] namedArgs)
		{
			ValueResolver<TResult> resolver = new ValueResolver<TResult>();
			resolver.Context.Property = property;
			resolver.Context.ResolvedString = resolvedString;
			resolver.Context.ResultType = typeof(TResult);
			resolver.Context.LogExceptions = true;
			if (hasFallback)
			{
				resolver.Context.FallbackValue = fallbackValue;
				resolver.Context.HasFallbackValue = true;
			}
			if (namedArgs != null)
			{
				for (int i = 0; i < namedArgs.Length; i++)
				{
					resolver.Context.NamedValues.Add(namedArgs[i]);
				}
			}
			resolver.Context.AddDefaultContextValues();
			InitResolver(resolver);
			return resolver;
		}

		public static ValueResolver GetResolverFromContextWeak(ref ValueResolverContext context)
		{
			if (!WeaklyTypedGetResolverFromContextMethods.TryGetValue(context.ResultType, out var method))
			{
				method = GetResolverFromContextMethodInfo.MakeGenericMethod(context.ResultType);
				WeaklyTypedGetResolverFromContextMethods.Add(context.ResultType, method);
			}
			GetResolverFromContextMethodParameters[0] = context;
			return (ValueResolver)method.Invoke(null, GetResolverFromContextMethodParameters);
		}

		public static ValueResolver<TResult> GetResolverFromContext<TResult>(ref ValueResolverContext context)
		{
			ValueResolver<TResult> resolver = new ValueResolver<TResult>();
			resolver.Context = context;
			resolver.Context.ResultType = typeof(TResult);
			InitResolver(resolver);
			return resolver;
		}

		private static string GetPossibleMatchesMessage(ref ValueResolverContext context)
		{
			SB.Length = 0;
			SB.AppendLine("Could not match the given string '" + context.ResolvedString + "' to any possible value resolution in the context of the type '" + context.ParentType.GetNiceName() + "'. The following kinds of value resolutions are possible:");
			SB.AppendLine();
			ResolverAndPriority[] array = ValueResolverCreators;
			for (int i = 0; i < array.Length; i++)
			{
				ValueResolverCreator resolver = array[i].ResolverCreator;
				if (resolver == null)
				{
					break;
				}
				string matchLine = resolver.GetPossibleMatchesString(ref context);
				if (matchLine != null)
				{
					SB.AppendLine(matchLine);
				}
			}
			SB.AppendLine();
			SB.AppendLine("And the following named values are available:");
			SB.AppendLine();
			SB.Append(context.NamedValues.GetValueOverviewString());
			return SB.ToString();
		}

		private static void InitResolver<TResult>(ValueResolver<TResult> resolver)
		{
			if (resolver.Context.IsResolved)
			{
				throw new InvalidOperationException("This resolver's context has already been marked resolved! You cannot resolve the same context twice!");
			}
			bool hasFallback = resolver.Context.HasFallbackValue;
			ResolverAndPriority[] array = ValueResolverCreators;
			ValueResolverFunc<TResult> func = null;
			for (int i = 0; i < array.Length; i++)
			{
				ValueResolverCreator resolverCreator = array[i].ResolverCreator;
				if (resolverCreator == null)
				{
					break;
				}
				try
				{
					func = resolverCreator.TryCreateResolverFunc<TResult>(ref resolver.Context);
					if (resolver.Context.ErrorMessage != null)
					{
						break;
					}
				}
				catch (Exception ex)
				{
					InitFailedResolve(resolver, withError: false);
					resolver.Context.ErrorMessage = "Resolver creator '" + resolverCreator.GetType().Name + "' failed with exception:\n\n" + ex.ToString();
					return;
				}
				if (func != null)
				{
					break;
				}
			}
			if (func == null)
			{
				if (string.IsNullOrEmpty(resolver.Context.ResolvedString))
				{
					InitFailedResolve(resolver, withError: false);
				}
				else if (resolver.Context.ErrorMessage == null && resolver.Context.HasFallbackValue && resolver.Context.ResultType == typeof(string))
				{
					InitFailedResolve(resolver, withError: false);
				}
				else
				{
					InitFailedResolve(resolver, withError: true);
				}
			}
			else
			{
				resolver.Func = func;
			}
			resolver.Context.MarkResolved();
		}

		private static void InitFailedResolve<TResult>(ValueResolver<TResult> resolver, bool withError)
		{
			if (withError && resolver.Context.ErrorMessage == null)
			{
				resolver.Context.ErrorMessage = GetPossibleMatchesMessage(ref resolver.Context);
			}
			if (!FailedResolveFuncs.TryGetValue(typeof(TResult), out var failedResolveFunc))
			{
				failedResolveFunc = new ValueResolverFunc<TResult>(FailedResolveResult<TResult>);
				FailedResolveFuncs.Add(typeof(TResult), failedResolveFunc);
			}
			resolver.Func = (ValueResolverFunc<TResult>)failedResolveFunc;
		}

		protected static ValueResolverFunc<TResult> GetFailedResolverFunc<TResult>()
		{
			if (!FailedResolveFuncs.TryGetValue(typeof(TResult), out var failedResolveFunc))
			{
				failedResolveFunc = new ValueResolverFunc<TResult>(FailedResolveResult<TResult>);
				FailedResolveFuncs.Add(typeof(TResult), failedResolveFunc);
			}
			return (ValueResolverFunc<TResult>)failedResolveFunc;
		}
	}
}

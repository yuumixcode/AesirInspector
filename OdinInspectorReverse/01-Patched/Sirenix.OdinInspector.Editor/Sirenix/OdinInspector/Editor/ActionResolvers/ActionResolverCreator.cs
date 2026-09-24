using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.ActionResolvers
{
	public abstract class ActionResolverCreator
	{
		private struct ResolverAndPriority
		{
			public ActionResolverCreator ResolverCreator;

			public double Priority;
		}

		private static StringBuilder SB;

		private static ResolverAndPriority[] ActionResolverCreators;

		protected static readonly ResolvedAction FailedResolveAction;

		static ActionResolverCreator()
		{
			SB = new StringBuilder();
			ActionResolverCreators = new ResolverAndPriority[8];
			FailedResolveAction = delegate
			{
			};
			List<Assembly> assemblies = ResolverUtilities.GetResolverAssemblies();
			for (int i = 0; i < assemblies.Count; i++)
			{
				Assembly assembly = assemblies[i];
				object[] attrs = assembly.SafeGetCustomAttributes(typeof(RegisterDefaultActionResolverAttribute), inherit: false);
				for (int j = 0; j < attrs.Length; j++)
				{
					RegisterDefaultActionResolverAttribute attr = (RegisterDefaultActionResolverAttribute)attrs[j];
					try
					{
						object instance = Activator.CreateInstance(attr.ResolverType);
						Register((ActionResolverCreator)instance, attr.Order);
					}
					catch (Exception innerException)
					{
						while (innerException.InnerException != null && innerException is TargetInvocationException)
						{
							innerException = innerException.InnerException;
						}
						Debug.LogException(new Exception("Failed to create instance of registered default resolver of type '" + attr.ResolverType.GetNiceFullName() + "'", innerException));
					}
				}
			}
		}

		public abstract ResolvedAction TryCreateAction(ref ActionResolverContext context);

		public abstract string GetPossibleMatchesString(ref ActionResolverContext context);

		public static void Register(ActionResolverCreator valueResolverCreator, double order = 0.0)
		{
			ResolverAndPriority[] array = ActionResolverCreators;
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
			ActionResolverCreators = array;
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

		public static ActionResolver GetResolver(InspectorProperty property, string resolvedString)
		{
			return GetResolver(property, resolvedString, null);
		}

		public static ActionResolver GetResolver(InspectorProperty property, string resolvedString, params NamedValue[] namedArgs)
		{
			ActionResolver resolver = new ActionResolver();
			resolver.Context.Property = property;
			resolver.Context.ResolvedString = resolvedString;
			resolver.Context.LogExceptions = true;
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

		public static ActionResolver GetResolverFromContext(ref ActionResolverContext context)
		{
			ActionResolver resolver = new ActionResolver();
			resolver.Context = context;
			InitResolver(resolver);
			return resolver;
		}

		private static string GetPossibleMatchesMessage(ref ActionResolverContext context)
		{
			SB.Length = 0;
			SB.AppendLine("Could not match the given string '" + context.ResolvedString + "' to any action that can be performed in the context of the type '" + context.Property.ParentType.GetNiceName() + "'. The following kinds of actions are possible:");
			SB.AppendLine();
			ResolverAndPriority[] array = ActionResolverCreators;
			for (int i = 0; i < array.Length; i++)
			{
				ActionResolverCreator resolver = array[i].ResolverCreator;
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

		private static void InitResolver(ActionResolver resolver)
		{
			if (resolver.Context.IsResolved)
			{
				throw new InvalidOperationException("This resolver's context has already been marked resolved! You cannot resolve the same context twice!");
			}
			ResolverAndPriority[] array = ActionResolverCreators;
			ResolvedAction action = null;
			for (int i = 0; i < array.Length; i++)
			{
				ActionResolverCreator resolverCreator = array[i].ResolverCreator;
				if (resolverCreator == null)
				{
					break;
				}
				try
				{
					action = resolverCreator.TryCreateAction(ref resolver.Context);
				}
				catch (Exception ex)
				{
					resolver.Context.ErrorMessage = "Resolver creator '" + resolverCreator.GetType().Name + "' failed with exception:\n\n" + ex.ToString();
					resolver.Action = FailedResolveAction;
					return;
				}
				if (action != null)
				{
					break;
				}
			}
			if (action == null)
			{
				resolver.Context.ErrorMessage = GetPossibleMatchesMessage(ref resolver.Context);
				resolver.Action = FailedResolveAction;
			}
			else
			{
				resolver.Action = action;
			}
			resolver.Context.MarkResolved();
		}

		protected static ResolvedAction GetDelegateInvoker(Delegate @delegate, NamedValues argSetup)
		{
			object[] parameterValues = new object[argSetup.Count];
			ParameterInfo[] parameters = @delegate.Method.GetParameters();
			bool[] byRefParameters = new bool[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				byRefParameters[i] = parameters[i].ParameterType.IsByRef;
			}
			return delegate(ref ActionResolverContext context, int selectionIndex)
			{
				for (int j = 0; j < parameterValues.Length; j++)
				{
					object value = context.NamedValues.GetValue(argSetup[j].Name);
					parameterValues[j] = ConvertUtility.WeakConvert(value, argSetup[j].Type);
				}
				@delegate.DynamicInvoke(parameterValues);
				if (context.SyncRefParametersWithNamedValues)
				{
					for (int k = 0; k < parameterValues.Length; k++)
					{
						if (byRefParameters[k])
						{
							NamedValue namedValue = argSetup[k];
							if (!context.NamedValues.TryGetValue(namedValue.Name, out var value2))
							{
								throw new Exception("Expected named value '" + namedValue.Name + "' was not present!");
							}
							context.NamedValues.Set(namedValue.Name, ConvertUtility.WeakConvert(parameterValues[k], value2.Type));
						}
					}
				}
			};
		}

		protected static ResolvedAction GetMethodInvoker(MethodInfo method, NamedValues argSetup, bool parentIsValueType)
		{
			object[] parameterValues = new object[argSetup.Count];
			bool isStatic = method.IsStatic;
			ParameterInfo[] parameters = method.GetParameters();
			bool[] byRefParameters = new bool[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				byRefParameters[i] = parameters[i].ParameterType.IsByRef;
			}
			return delegate(ref ActionResolverContext context, int selectionIndex)
			{
				for (int j = 0; j < parameterValues.Length; j++)
				{
					object value = context.NamedValues.GetValue(argSetup[j].Name);
					parameterValues[j] = ConvertUtility.WeakConvert(value, argSetup[j].Type);
				}
				object obj = (isStatic ? null : context.GetParentValue(selectionIndex));
				method.Invoke(obj, parameterValues);
				if (!isStatic && parentIsValueType)
				{
					context.SetParentValue(selectionIndex, obj);
				}
				if (context.SyncRefParametersWithNamedValues)
				{
					for (int k = 0; k < parameterValues.Length; k++)
					{
						if (byRefParameters[k])
						{
							NamedValue namedValue = argSetup[k];
							if (!context.NamedValues.TryGetValue(namedValue.Name, out var value2))
							{
								throw new Exception("Expected named value '" + namedValue.Name + "' was not present!");
							}
							context.NamedValues.Set(namedValue.Name, ConvertUtility.WeakConvert(parameterValues[k], value2.Type));
						}
					}
				}
			};
		}

		protected unsafe static bool IsCompatibleMethod(MethodInfo method, ref NamedValues namedValues, ref NamedValues argSetup, bool requiresBackcasting, out string errorMessage)
		{
			ParameterInfo[] parameters = method.GetParameters();
			int namedValueCount = namedValues.Count;
			if (parameters.Length > namedValueCount)
			{
				errorMessage = "Method '" + method.GetNiceName() + "' has too many parameters (" + parameters.Length + "). The following '" + namedValueCount + "' parameters are available: \n\n" + namedValues.GetValueOverviewString();
				return false;
			}
			bool* claimedNamedValues = stackalloc bool[(int)(uint)namedValueCount];
			foreach (ParameterInfo parameter in parameters)
			{
				string parameterName = parameter.Name;
				Type parameterType = parameter.ParameterType;
				bool isByRef = false;
				if (parameterType.IsByRef)
				{
					isByRef = true;
					parameterType = parameterType.GetElementType();
				}
				bool foundMatch = false;
				for (int j = 0; j < namedValueCount; j++)
				{
					if (claimedNamedValues[j])
					{
						continue;
					}
					NamedValue value = namedValues[j];
					if (value.Name == parameterName)
					{
						if (!ConvertUtility.CanConvert(value.Type, parameterType))
						{
							errorMessage = "Method '" + method.Name + "' has an invalid signature; the parameter '" + parameterName + "' of type '" + parameter.ParameterType.GetNiceName() + "' cannot be assigned from the available type '" + value.Type.GetNiceName() + "'. The following parameters are available: \n\n" + namedValues.GetValueOverviewString();
							return false;
						}
						if (requiresBackcasting && isByRef && !ConvertUtility.CanConvert(parameterType, value.Type))
						{
							errorMessage = "Method '" + method.Name + "' has an invalid signature; the ref/in/out parameter '" + parameterName + "' of type '" + parameter.ParameterType.GetNiceName() + "' can be assigned from the available type '" + value.Type.GetNiceName() + "', but type '" + value.Type.GetNiceName() + "' cannot be reassigned assigned back to type '" + parameter.ParameterType.GetNiceName() + "'. The following parameters are available: \n\n" + namedValues.GetValueOverviewString();
							return false;
						}
						argSetup.Add(value.Name, parameterType, null);
						claimedNamedValues[j] = true;
						foundMatch = true;
						break;
					}
				}
				if (!foundMatch)
				{
					for (int k = 0; k < namedValueCount; k++)
					{
						if (!claimedNamedValues[k])
						{
							NamedValue value2 = namedValues[k];
							if (value2.Type == parameterType)
							{
								foundMatch = true;
								argSetup.Add(value2.Name, parameterType, null);
								claimedNamedValues[k] = true;
								break;
							}
						}
					}
				}
				if (!foundMatch && parameterType != typeof(string))
				{
					for (int l = 0; l < namedValueCount; l++)
					{
						if (!claimedNamedValues[l])
						{
							NamedValue value3 = namedValues[l];
							if (ConvertUtility.CanConvert(value3.Type, parameterType) && (!requiresBackcasting || ConvertUtility.CanConvert(parameterType, value3.Type)))
							{
								foundMatch = true;
								argSetup.Add(value3.Name, parameterType, null);
								claimedNamedValues[l] = true;
								break;
							}
						}
					}
				}
				if (!foundMatch)
				{
					errorMessage = "Method '" + method.Name + "' has an invalid signature; no values could be assigned to the parameter '" + parameterName + "' of type '" + parameter.ParameterType.GetNiceName() + "'. The following parameter values are available: \n\n" + namedValues.GetValueOverviewString();
					return false;
				}
			}
			errorMessage = null;
			return true;
		}
	}
}

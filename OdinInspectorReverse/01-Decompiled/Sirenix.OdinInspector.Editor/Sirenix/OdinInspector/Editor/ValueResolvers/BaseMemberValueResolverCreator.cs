using System;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
	public abstract class BaseMemberValueResolverCreator : ValueResolverCreator
	{
		protected static ValueResolverFunc<TResult> GetFieldGetter<TResult>(FieldInfo field)
		{
			if (field.IsStatic)
			{
				return delegate
				{
					return ConvertUtility.Convert<TResult>(field.GetValue(null));
				};
			}
			return delegate(ref ValueResolverContext context, int selectionIndex)
			{
				return ConvertUtility.Convert<TResult>(field.GetValue(context.GetParentValue(selectionIndex)));
			};
		}

		protected static ValueResolverFunc<TResult> GetPropertyGetter<TResult>(PropertyInfo property, bool parentIsValueType)
		{
			if (property.IsStatic())
			{
				return delegate
				{
					return ConvertUtility.Convert<TResult>(property.GetValue(null, null));
				};
			}
			return delegate(ref ValueResolverContext context, int selectionIndex)
			{
				object parentValue = context.GetParentValue(selectionIndex);
				TResult result = ConvertUtility.Convert<TResult>(property.GetValue(parentValue, null));
				if (parentIsValueType)
				{
					context.SetParentValue(selectionIndex, parentValue);
				}
				return result;
			};
		}

		protected static ValueResolverFunc<TResult> GetMethodGetter<TResult>(MethodInfo method, NamedValues argSetup, bool parentIsValueType)
		{
			object[] parameterValues = new object[argSetup.Count];
			bool isStatic = method.IsStatic;
			ParameterInfo[] parameters = method.GetParameters();
			bool[] byRefParameters = new bool[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				byRefParameters[i] = parameters[i].ParameterType.IsByRef;
			}
			return delegate(ref ValueResolverContext context, int selectionIndex)
			{
				for (int j = 0; j < parameterValues.Length; j++)
				{
					object value = context.NamedValues.GetValue(argSetup[j].Name);
					parameterValues[j] = ConvertUtility.WeakConvert(value, argSetup[j].Type);
				}
				object obj = (isStatic ? null : context.GetParentValue(selectionIndex));
				TResult result = ConvertUtility.Convert<TResult>(method.Invoke(obj, parameterValues));
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
				return result;
			};
		}

		protected static ValueResolverFunc<TResult> GetDelegateGetter<TResult>(Delegate @delegate, NamedValues argSetup)
		{
			object[] parameterValues = new object[argSetup.Count];
			ParameterInfo[] parameters = @delegate.Method.GetParameters();
			bool[] byRefParameters = new bool[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				byRefParameters[i] = parameters[i].ParameterType.IsByRef;
			}
			return delegate(ref ValueResolverContext context, int selectionIndex)
			{
				for (int j = 0; j < parameterValues.Length; j++)
				{
					object value = context.NamedValues.GetValue(argSetup[j].Name);
					parameterValues[j] = ConvertUtility.WeakConvert(value, argSetup[j].Type);
				}
				TResult result = ConvertUtility.Convert<TResult>(@delegate.DynamicInvoke(parameterValues));
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
				return result;
			};
		}

		protected static MethodInfo GetCompatibleMethod(Type type, string methodName, BindingFlags flags, ref NamedValues namedValues, ref NamedValues argSetup, bool requiresBackcasting, out string errorMessage)
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
			if (IsCompatibleMethod(method, ref namedValues, ref argSetup, requiresBackcasting, out errorMessage))
			{
				return method;
			}
			return null;
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

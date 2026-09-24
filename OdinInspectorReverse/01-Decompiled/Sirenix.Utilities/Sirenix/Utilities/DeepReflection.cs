using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Sirenix.Utilities
{
	/// <summary>
	/// Not yet documented.
	/// </summary>
	public static class DeepReflection
	{
		private enum PathStepType
		{
			Member,
			WeakListElement,
			StrongListElement,
			ArrayElement
		}

		private struct PathStep
		{
			public readonly PathStepType StepType;

			public readonly MemberInfo Member;

			public readonly int ElementIndex;

			public readonly Type ElementType;

			public readonly MethodInfo StrongListGetItemMethod;

			public PathStep(MemberInfo member)
			{
				StepType = PathStepType.Member;
				Member = member;
				ElementIndex = -1;
				ElementType = null;
				StrongListGetItemMethod = null;
			}

			public PathStep(int elementIndex)
			{
				StepType = PathStepType.WeakListElement;
				Member = null;
				ElementIndex = elementIndex;
				ElementType = null;
				StrongListGetItemMethod = null;
			}

			public PathStep(int elementIndex, Type strongListElementType, bool isArray)
			{
				StepType = (isArray ? PathStepType.ArrayElement : PathStepType.StrongListElement);
				Member = null;
				ElementIndex = elementIndex;
				ElementType = strongListElementType;
				StrongListGetItemMethod = typeof(IList<>).MakeGenericType(strongListElementType).GetMethod("get_Item");
			}
		}

		private static MethodInfo WeakListGetItem = typeof(IList).GetMethod("get_Item");

		private static MethodInfo WeakListSetItem = typeof(IList).GetMethod("set_Item");

		private static MethodInfo CreateWeakAliasForInstanceGetDelegate1MethodInfo = typeof(DeepReflection).GetMethod("CreateWeakAliasForInstanceGetDelegate1", BindingFlags.Static | BindingFlags.NonPublic);

		private static MethodInfo CreateWeakAliasForInstanceGetDelegate2MethodInfo = typeof(DeepReflection).GetMethod("CreateWeakAliasForInstanceGetDelegate2", BindingFlags.Static | BindingFlags.NonPublic);

		private static MethodInfo CreateWeakAliasForStaticGetDelegateMethodInfo = typeof(DeepReflection).GetMethod("CreateWeakAliasForStaticGetDelegate", BindingFlags.Static | BindingFlags.NonPublic);

		private static MethodInfo CreateWeakAliasForInstanceSetDelegate1MethodInfo = typeof(DeepReflection).GetMethod("CreateWeakAliasForInstanceSetDelegate1", BindingFlags.Static | BindingFlags.NonPublic);

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static Func<object> CreateWeakStaticValueGetter(Type rootType, Type resultType, string path, bool allowEmit = true)
		{
			if (rootType == null)
			{
				throw new ArgumentNullException("rootType");
			}
			bool rootIsStatic;
			List<PathStep> memberPath = GetMemberPath(rootType, ref resultType, path, out rootIsStatic, isSet: false);
			if (!rootIsStatic)
			{
				throw new ArgumentException("Given path root is not static.");
			}
			if (!allowEmit)
			{
				return CreateSlowDeepStaticValueGetterDelegate(memberPath);
			}
			Delegate emittedDelegate = CreateEmittedDeepValueGetterDelegate(path, rootType, resultType, memberPath, rootIsStatic);
			MethodInfo weakAliasCreator = CreateWeakAliasForStaticGetDelegateMethodInfo.MakeGenericMethod(resultType);
			return (Func<object>)weakAliasCreator.Invoke(null, new object[1] { emittedDelegate });
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static Func<object, object> CreateWeakInstanceValueGetter(Type rootType, Type resultType, string path, bool allowEmit = true)
		{
			if (rootType == null)
			{
				throw new ArgumentNullException("rootType");
			}
			bool rootIsStatic;
			List<PathStep> memberPath = GetMemberPath(rootType, ref resultType, path, out rootIsStatic, isSet: false);
			if (rootIsStatic)
			{
				throw new ArgumentException("Given path root is static.");
			}
			if (!allowEmit)
			{
				return CreateSlowDeepInstanceValueGetterDelegate(memberPath);
			}
			Delegate emittedDelegate = CreateEmittedDeepValueGetterDelegate(path, rootType, resultType, memberPath, rootIsStatic);
			MethodInfo weakAliasCreator = CreateWeakAliasForInstanceGetDelegate1MethodInfo.MakeGenericMethod(rootType, resultType);
			return (Func<object, object>)weakAliasCreator.Invoke(null, new object[1] { emittedDelegate });
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static Action<object, object> CreateWeakInstanceValueSetter(Type rootType, Type argType, string path, bool allowEmit = true)
		{
			if (rootType == null)
			{
				throw new ArgumentNullException("rootType");
			}
			bool rootIsStatic;
			List<PathStep> memberPath = GetMemberPath(rootType, ref argType, path, out rootIsStatic, isSet: true);
			if (rootIsStatic)
			{
				throw new ArgumentException("Given path root is static.");
			}
			allowEmit = false;
			if (!allowEmit)
			{
				return CreateSlowDeepInstanceValueSetterDelegate(memberPath);
			}
			Delegate emittedDelegate = null;
			MethodInfo weakAliasCreator = CreateWeakAliasForInstanceSetDelegate1MethodInfo.MakeGenericMethod(rootType, argType);
			return (Action<object, object>)weakAliasCreator.Invoke(null, new object[1] { emittedDelegate });
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static Func<object, TResult> CreateWeakInstanceValueGetter<TResult>(Type rootType, string path, bool allowEmit = true)
		{
			if (rootType == null)
			{
				throw new ArgumentNullException("rootType");
			}
			Type resultType = typeof(TResult);
			bool rootIsStatic;
			List<PathStep> memberPath = GetMemberPath(rootType, ref resultType, path, out rootIsStatic, isSet: false);
			if (rootIsStatic)
			{
				throw new ArgumentException("Given path root is static.");
			}
			if (!allowEmit)
			{
				Func<object, object> del = CreateSlowDeepInstanceValueGetterDelegate(memberPath);
				return (object obj) => (TResult)del(obj);
			}
			Delegate emittedDelegate = CreateEmittedDeepValueGetterDelegate(path, rootType, resultType, memberPath, rootIsStatic);
			MethodInfo weakAliasCreator = CreateWeakAliasForInstanceGetDelegate2MethodInfo.MakeGenericMethod(rootType, resultType);
			return (Func<object, TResult>)weakAliasCreator.Invoke(null, new object[1] { emittedDelegate });
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static Func<TResult> CreateValueGetter<TResult>(Type rootType, string path, bool allowEmit = true)
		{
			if (rootType == null)
			{
				throw new ArgumentNullException("rootType");
			}
			Type resultType = typeof(TResult);
			bool rootIsStatic;
			List<PathStep> memberPath = GetMemberPath(rootType, ref resultType, path, out rootIsStatic, isSet: false);
			if (!rootIsStatic)
			{
				throw new ArgumentException("Given path root is not static; use the generic overload with a target type.");
			}
			if (!allowEmit)
			{
				Func<object> slowDelegate = CreateSlowDeepStaticValueGetterDelegate(memberPath);
				return () => (TResult)slowDelegate();
			}
			Delegate emittedDelegate = CreateEmittedDeepValueGetterDelegate(path, rootType, resultType, memberPath, rootIsStatic);
			return (Func<TResult>)emittedDelegate;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static Func<TTarget, TResult> CreateValueGetter<TTarget, TResult>(string path, bool allowEmit = true)
		{
			Type resultType = typeof(TResult);
			bool rootIsStatic;
			List<PathStep> memberPath = GetMemberPath(typeof(TTarget), ref resultType, path, out rootIsStatic, isSet: false);
			if (rootIsStatic)
			{
				throw new ArgumentException("Given path root is static; use the generic overload without a target type.");
			}
			if (!allowEmit)
			{
				Func<object, object> slowDelegate = CreateSlowDeepInstanceValueGetterDelegate(memberPath);
				return (TTarget target) => (TResult)slowDelegate(target);
			}
			Delegate emittedDelegate = CreateEmittedDeepValueGetterDelegate(path, typeof(TTarget), resultType, memberPath, rootIsStatic);
			return (Func<TTarget, TResult>)emittedDelegate;
		}

		private static Func<object, object> CreateWeakAliasForInstanceGetDelegate1<TTarget, TResult>(Func<TTarget, TResult> func)
		{
			return (object obj) => func((TTarget)obj);
		}

		private static Func<object, TResult> CreateWeakAliasForInstanceGetDelegate2<TTarget, TResult>(Func<TTarget, TResult> func)
		{
			return (object obj) => func((TTarget)obj);
		}

		private static Func<object> CreateWeakAliasForStaticGetDelegate<TResult>(Func<TResult> func)
		{
			return () => func();
		}

		private static Action<object, object> CreateWeakAliasForInstanceSetDelegate1<TTarget, TArg1>(Action<TTarget, TArg1> func)
		{
			return delegate(object obj, object arg)
			{
				func((TTarget)obj, (TArg1)arg);
			};
		}

		private static Action<object, TArg1> CreateWeakAliasForInstanceSetDelegate2<TTarget, TArg1>(Action<TTarget, TArg1> func)
		{
			return delegate(object obj, TArg1 arg)
			{
				func((TTarget)obj, arg);
			};
		}

		private static Action<object> CreateWeakAliasForStaticSetDelegate<TArg1>(Action<TArg1> func)
		{
			return delegate(object arg)
			{
				func((TArg1)arg);
			};
		}

		private static Delegate CreateEmittedDeepValueGetterDelegate(string path, Type rootType, Type resultType, List<PathStep> memberPath, bool rootIsStatic)
		{
			DynamicMethod getterMethod = ((!rootIsStatic) ? new DynamicMethod(rootType.FullName + "_getter<" + path + ">", resultType, new Type[1] { rootType }, restrictedSkipVisibility: true) : new DynamicMethod(rootType.FullName + "_getter<" + path + ">", resultType, new Type[0], restrictedSkipVisibility: true));
			ILGenerator il = getterMethod.GetILGenerator();
			if (!rootIsStatic)
			{
				il.Emit(OpCodes.Ldarg_0);
			}
			for (int i = 0; i < memberPath.Count; i++)
			{
				PathStep step = memberPath[i];
				switch (step.StepType)
				{
				case PathStepType.Member:
				{
					MemberInfo member = step.Member;
					FieldInfo field = member as FieldInfo;
					if (field != null)
					{
						if (field.IsLiteral)
						{
							EmitConstant(il, field.GetRawConstantValue());
						}
						else if (field.IsStatic)
						{
							il.Emit(OpCodes.Ldsfld, field);
						}
						else
						{
							il.Emit(OpCodes.Ldfld, field);
						}
					}
					PropertyInfo property = member as PropertyInfo;
					if (property != null)
					{
						MethodInfo getMethod = property.GetGetMethod(nonPublic: true);
						if (getMethod.IsStatic)
						{
							il.Emit(OpCodes.Call, getMethod);
						}
						else if (getMethod.DeclaringType.IsValueType)
						{
							LocalBuilder localAddr = il.DeclareLocal(getMethod.DeclaringType);
							il.Emit(OpCodes.Stloc, localAddr);
							il.Emit(OpCodes.Ldloca, localAddr);
							il.Emit(OpCodes.Call, getMethod);
						}
						else
						{
							il.Emit(OpCodes.Callvirt, getMethod);
						}
					}
					MethodInfo method = member as MethodInfo;
					if (method != null)
					{
						if (method.IsStatic)
						{
							il.Emit(OpCodes.Call, method);
						}
						else if (method.DeclaringType.IsValueType)
						{
							LocalBuilder localAddr2 = il.DeclareLocal(method.DeclaringType);
							il.Emit(OpCodes.Stloc, localAddr2);
							il.Emit(OpCodes.Ldloca, localAddr2);
							il.Emit(OpCodes.Call, method);
						}
						else
						{
							il.Emit(OpCodes.Callvirt, method);
						}
					}
					Type returnType = member.GetReturnType();
					if ((resultType == typeof(object) || returnType.IsInterface) && returnType.IsValueType)
					{
						il.Emit(OpCodes.Box, returnType);
					}
					break;
				}
				case PathStepType.ArrayElement:
					il.Emit(OpCodes.Ldc_I4, step.ElementIndex);
					il.Emit(OpCodes.Ldelem, step.ElementType);
					break;
				case PathStepType.WeakListElement:
					il.Emit(OpCodes.Ldc_I4, step.ElementIndex);
					il.Emit(OpCodes.Callvirt, WeakListGetItem);
					break;
				case PathStepType.StrongListElement:
				{
					Type strongListType = typeof(IList<>).MakeGenericType(step.ElementType);
					MethodInfo getItemMethod = strongListType.GetMethod("get_Item");
					il.Emit(OpCodes.Ldc_I4, step.ElementIndex);
					il.Emit(OpCodes.Callvirt, getItemMethod);
					break;
				}
				}
			}
			il.Emit(OpCodes.Ret);
			if (rootIsStatic)
			{
				return getterMethod.CreateDelegate(typeof(Func<>).MakeGenericType(resultType));
			}
			return getterMethod.CreateDelegate(typeof(Func<, >).MakeGenericType(rootType, resultType));
		}

		private static Func<object> CreateSlowDeepStaticValueGetterDelegate(List<PathStep> memberPath)
		{
			return delegate
			{
				object obj = null;
				for (int i = 0; i < memberPath.Count; i++)
				{
					obj = SlowGetMemberValue(memberPath[i], obj);
				}
				return obj;
			};
		}

		private static Func<object, object> CreateSlowDeepInstanceValueGetterDelegate(List<PathStep> memberPath)
		{
			return delegate(object instance)
			{
				object obj = instance;
				for (int i = 0; i < memberPath.Count; i++)
				{
					obj = SlowGetMemberValue(memberPath[i], obj);
				}
				return obj;
			};
		}

		private static Action<object, object> CreateSlowDeepInstanceValueSetterDelegate(List<PathStep> memberPath)
		{
			return delegate(object instance, object arg)
			{
				object instance2 = instance;
				int num = memberPath.Count - 1;
				for (int i = 0; i < num; i++)
				{
					instance2 = SlowGetMemberValue(memberPath[i], instance2);
				}
				SlowSetMemberValue(memberPath[memberPath.Count - 1], instance2, arg);
			};
		}

		private static object SlowGetMemberValue(PathStep step, object instance)
		{
			switch (step.StepType)
			{
			case PathStepType.Member:
			{
				FieldInfo field = step.Member as FieldInfo;
				if (field != null)
				{
					if (field.IsLiteral)
					{
						return field.GetRawConstantValue();
					}
					return field.GetValue(instance);
				}
				PropertyInfo prop = step.Member as PropertyInfo;
				if (prop != null)
				{
					return prop.GetValue(instance, null);
				}
				MethodInfo method = step.Member as MethodInfo;
				if (method != null)
				{
					return method.Invoke(instance, null);
				}
				throw new NotSupportedException(step.Member.GetType().GetNiceName());
			}
			case PathStepType.WeakListElement:
				return WeakListGetItem.Invoke(instance, new object[1] { step.ElementIndex });
			case PathStepType.ArrayElement:
				return (instance as Array).GetValue(step.ElementIndex);
			case PathStepType.StrongListElement:
				return step.StrongListGetItemMethod.Invoke(instance, new object[1] { step.ElementIndex });
			default:
				throw new NotImplementedException(step.StepType.ToString());
			}
		}

		private static void SlowSetMemberValue(PathStep step, object instance, object value)
		{
			switch (step.StepType)
			{
			case PathStepType.Member:
			{
				FieldInfo field = step.Member as FieldInfo;
				if (field != null)
				{
					field.SetValue(instance, value);
					break;
				}
				PropertyInfo prop = step.Member as PropertyInfo;
				if (prop != null)
				{
					prop.SetValue(instance, value, null);
					break;
				}
				throw new NotSupportedException(step.Member.GetType().GetNiceName());
			}
			case PathStepType.WeakListElement:
				WeakListSetItem.Invoke(instance, new object[2] { step.ElementIndex, value });
				break;
			case PathStepType.ArrayElement:
				(instance as Array).SetValue(value, step.ElementIndex);
				break;
			case PathStepType.StrongListElement:
			{
				MethodInfo setItemMethod = typeof(IList<>).MakeGenericType(step.ElementType).GetMethod("set_Item");
				setItemMethod.Invoke(instance, new object[2] { step.ElementIndex, value });
				break;
			}
			default:
				throw new NotImplementedException(step.StepType.ToString());
			}
		}

		private static List<PathStep> GetMemberPath(Type rootType, ref Type resultType, string path, out bool rootIsStatic, bool isSet)
		{
			if (path.IsNullOrWhitespace())
			{
				throw new ArgumentException("Invalid path; is null or whitespace.");
			}
			rootIsStatic = false;
			List<PathStep> result = new List<PathStep>();
			string[] steps = path.Split(new char[1] { '.' });
			Type currentType = rootType;
			for (int i = 0; i < steps.Length; i++)
			{
				string step = steps[i];
				bool expectMethod = false;
				if (step.StartsWith("[", StringComparison.InvariantCulture) && step.EndsWith("]", StringComparison.InvariantCulture))
				{
					string indexStr = step.Substring(1, step.Length - 2);
					if (!int.TryParse(indexStr, out var index))
					{
						throw new ArgumentException("Couldn't parse an index from the path step '" + step + "'.");
					}
					if (currentType.IsArray)
					{
						Type elementType = currentType.GetElementType();
						result.Add(new PathStep(index, elementType, isArray: true));
						currentType = elementType;
						continue;
					}
					if (currentType.ImplementsOpenGenericInterface(typeof(IList<>)))
					{
						Type elementType2 = currentType.GetArgumentsOfInheritedOpenGenericInterface(typeof(IList<>))[0];
						result.Add(new PathStep(index, elementType2, isArray: false));
						currentType = elementType2;
						continue;
					}
					if (typeof(IList).IsAssignableFrom(currentType))
					{
						result.Add(new PathStep(index));
						currentType = typeof(object);
						continue;
					}
					throw new ArgumentException("Cannot get elements by index from the type '" + currentType.Name + "'.");
				}
				if (step.EndsWith("()", StringComparison.InvariantCulture))
				{
					expectMethod = true;
					step = step.Substring(0, step.Length - 2);
				}
				MemberInfo member = GetStepMember(currentType, step, expectMethod);
				if (member.IsStatic())
				{
					if (!(currentType == rootType))
					{
						throw new ArgumentException("The non-root member '" + step + "' is static; use that member as the path root instead.");
					}
					rootIsStatic = true;
				}
				currentType = member.GetReturnType();
				if (expectMethod && (currentType == null || currentType == typeof(void)))
				{
					throw new ArgumentException("The method '" + member.Name + "' has no return type and cannot be part of a deep reflection path.");
				}
				result.Add(new PathStep(member));
			}
			if (resultType == null)
			{
				resultType = currentType;
			}
			else if (currentType != typeof(object) && !resultType.IsAssignableFrom(currentType))
			{
				throw new ArgumentException("Last member '" + result[result.Count - 1].Member.Name + "' of path '" + path + "' contains type '" + currentType.AssemblyQualifiedName + "', which is not assignable to expected type '" + resultType.AssemblyQualifiedName + "'.");
			}
			return result;
		}

		private static MemberInfo GetStepMember(Type owningType, string name, bool expectMethod)
		{
			MemberInfo result = null;
			MemberInfo[] possibleMembers = owningType.GetAllMembers(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).ToArray();
			int stepMethodParameterCount = int.MaxValue;
			foreach (MemberInfo member in possibleMembers)
			{
				if (expectMethod)
				{
					MethodInfo method = member as MethodInfo;
					if (method != null)
					{
						int parameterCount = method.GetParameters().Length;
						if (result == null || parameterCount < stepMethodParameterCount)
						{
							result = method;
							stepMethodParameterCount = parameterCount;
						}
					}
					continue;
				}
				if (member is MethodInfo)
				{
					throw new ArgumentException("Found method member for name '" + name + "', but expected a field or property.");
				}
				result = member;
				break;
			}
			if (result == null)
			{
				throw new ArgumentException("Could not find expected " + (expectMethod ? "method" : "field or property") + " '" + name + "' on type '" + owningType.GetNiceName() + "' while parsing reflection path.");
			}
			if (expectMethod && stepMethodParameterCount > 0)
			{
				throw new NotSupportedException("Method '" + result.GetNiceName() + "' has " + stepMethodParameterCount + " parameters, but method parameters are currently not supported.");
			}
			if (!(result is FieldInfo) && !(result is PropertyInfo) && !(result is MethodInfo))
			{
				throw new NotSupportedException("Members of type " + result.GetType().GetNiceName() + " are not support; only fields, properties and methods are supported.");
			}
			return result;
		}

		private static void EmitConstant(ILGenerator il, object constant, Type type = null)
		{
			if (constant == null)
			{
				il.Emit(OpCodes.Ldnull);
				return;
			}
			if (type == null)
			{
				type = constant.GetType();
			}
			if (type == typeof(int) || type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort))
			{
				il.Emit(OpCodes.Ldc_I4, Convert.ToInt32(constant));
			}
			else if (type == typeof(uint))
			{
				il.Emit(OpCodes.Ldc_I4, (int)(uint)constant);
			}
			else if (type == typeof(long))
			{
				il.Emit(OpCodes.Ldc_I8, (long)constant);
			}
			else if (type == typeof(ulong))
			{
				il.Emit(OpCodes.Ldc_I8, (long)(ulong)constant);
			}
			else if (type == typeof(float))
			{
				il.Emit(OpCodes.Ldc_R4, (float)constant);
			}
			else if (type == typeof(double))
			{
				il.Emit(OpCodes.Ldc_R8, (double)constant);
			}
			else if (type == typeof(string))
			{
				il.Emit(OpCodes.Ldstr, (string)constant);
			}
			else if (type == typeof(char))
			{
				il.Emit(OpCodes.Ldc_I4, (char)constant);
			}
			else if (type == typeof(decimal))
			{
				int[] bits = decimal.GetBits((decimal)constant);
				ConstructorInfo constructor = typeof(decimal).GetConstructor(new Type[1] { typeof(int[]) });
				LocalBuilder arrLocal = il.DeclareLocal(typeof(int[]));
				il.Emit(OpCodes.Ldc_I4, bits.Length);
				il.Emit(OpCodes.Newarr, typeof(int));
				il.Emit(OpCodes.Stloc, arrLocal);
				for (int i = 0; i < bits.Length; i++)
				{
					il.Emit(OpCodes.Ldloc, arrLocal);
					il.Emit(OpCodes.Ldc_I4, i);
					il.Emit(OpCodes.Ldc_I4, bits[i]);
					il.Emit(OpCodes.Stelem_I4);
				}
				il.Emit(OpCodes.Ldloc, arrLocal);
				il.Emit(OpCodes.Newobj, constructor);
			}
			else if (type == typeof(bool))
			{
				il.Emit(((bool)constant) ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
			}
			else
			{
				if (!type.IsEnum)
				{
					throw new NotSupportedException("Type " + type.GetNiceFullName() + " is not supported as a constant.");
				}
				EmitConstant(il, constant, Enum.GetUnderlyingType(type));
			}
		}
	}
}

using System;
using System.Reflection;
using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Serialization.Utilities
{
	/// <summary>
	/// Provides utilities for using the <see cref="N:System.Reflection.Emit" /> namespace.
	/// <para />
	/// This class is due for refactoring. Use at your own peril.
	/// </summary>
	internal static class EmitUtilities
	{
		public delegate void InstanceRefMethodCaller<InstanceType>(ref InstanceType instance);

		public delegate void InstanceRefMethodCaller<InstanceType, TArg1>(ref InstanceType instance, TArg1 arg1);

		private static Assembly EditorAssembly = typeof(UnityEditor.Editor).Assembly;

		private static Assembly EngineAssembly = typeof(UnityEngine.Object).Assembly;

		/// <summary>
		/// Gets a value indicating whether emitting is supported on the current platform.
		/// </summary>
		/// <value>
		///   <c>true</c> if the current platform can emit; otherwise, <c>false</c>.
		/// </value>
		public static bool CanEmit => true;

		private static bool EmitIsIllegalForMember(MemberInfo member)
		{
			if (member.DeclaringType != null)
			{
				if (!(member.DeclaringType.Assembly == EditorAssembly))
				{
					return member.DeclaringType.Assembly == EngineAssembly;
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Creates a delegate which gets the value of a field. If emitting is not supported on the current platform, the delegate will use reflection to get the value.
		/// </summary>
		/// <typeparam name="FieldType">The type of the field to get a value from.</typeparam>
		/// <param name="fieldInfo">The <see cref="T:System.Reflection.FieldInfo" /> instance describing the field to create a getter for.</param>
		/// <returns>A delegate which gets the value of the given field.</returns>
		/// <exception cref="T:System.ArgumentNullException">The fieldInfo parameter is null.</exception>
		public static Func<FieldType> CreateStaticFieldGetter<FieldType>(FieldInfo fieldInfo)
		{
			if (fieldInfo == null)
			{
				throw new ArgumentNullException("fieldInfo");
			}
			if (!fieldInfo.IsStatic)
			{
				throw new ArgumentException("Field must be static.");
			}
			fieldInfo = fieldInfo.DeAliasField();
			if (fieldInfo.IsLiteral)
			{
				FieldType value = (FieldType)fieldInfo.GetValue(null);
				return () => value;
			}
			if (EmitIsIllegalForMember(fieldInfo))
			{
				return () => (FieldType)fieldInfo.GetValue(null);
			}
			string methodName = fieldInfo.ReflectedType.FullName + ".get_" + fieldInfo.Name;
			DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(FieldType), new Type[0], restrictedSkipVisibility: true);
			ILGenerator gen = getterMethod.GetILGenerator();
			gen.Emit(OpCodes.Ldsfld, fieldInfo);
			gen.Emit(OpCodes.Ret);
			return (Func<FieldType>)getterMethod.CreateDelegate(typeof(Func<FieldType>));
		}

		/// <summary>
		/// Creates a delegate which gets the value of a field. If emitting is not supported on the current platform, the delegate will use reflection to get the value.
		/// </summary>
		/// <param name="fieldInfo">The <see cref="T:System.Reflection.FieldInfo" /> instance describing the field to create a getter for.</param>
		/// <returns>A delegate which gets the value of the given field.</returns>
		/// <exception cref="T:System.ArgumentNullException">The fieldInfo parameter is null.</exception>
		public static Func<object> CreateWeakStaticFieldGetter(FieldInfo fieldInfo)
		{
			if (fieldInfo == null)
			{
				throw new ArgumentNullException("fieldInfo");
			}
			if (!fieldInfo.IsStatic)
			{
				throw new ArgumentException("Field must be static.");
			}
			fieldInfo = fieldInfo.DeAliasField();
			if (EmitIsIllegalForMember(fieldInfo))
			{
				return () => fieldInfo.GetValue(null);
			}
			string methodName = fieldInfo.ReflectedType.FullName + ".get_" + fieldInfo.Name;
			DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(object), new Type[0], restrictedSkipVisibility: true);
			ILGenerator gen = getterMethod.GetILGenerator();
			gen.Emit(OpCodes.Ldsfld, fieldInfo);
			if (fieldInfo.FieldType.IsValueType)
			{
				gen.Emit(OpCodes.Box, fieldInfo.FieldType);
			}
			gen.Emit(OpCodes.Ret);
			return (Func<object>)getterMethod.CreateDelegate(typeof(Func<object>));
		}

		/// <summary>
		/// Creates a delegate which sets the value of a field. If emitting is not supported on the current platform, the delegate will use reflection to set the value.
		/// </summary>
		/// <typeparam name="FieldType">The type of the field to set a value to.</typeparam>
		/// <param name="fieldInfo">The <see cref="T:System.Reflection.FieldInfo" /> instance describing the field to create a setter for.</param>
		/// <returns>A delegate which sets the value of the given field.</returns>
		/// <exception cref="T:System.ArgumentNullException">The fieldInfo parameter is null.</exception>
		public static Action<FieldType> CreateStaticFieldSetter<FieldType>(FieldInfo fieldInfo)
		{
			if (fieldInfo == null)
			{
				throw new ArgumentNullException("fieldInfo");
			}
			if (!fieldInfo.IsStatic)
			{
				throw new ArgumentException("Field must be static.");
			}
			fieldInfo = fieldInfo.DeAliasField();
			if (fieldInfo.IsLiteral)
			{
				throw new ArgumentException("Field cannot be constant.");
			}
			if (EmitIsIllegalForMember(fieldInfo))
			{
				return delegate(FieldType value)
				{
					fieldInfo.SetValue(null, value);
				};
			}
			string methodName = fieldInfo.ReflectedType.FullName + ".set_" + fieldInfo.Name;
			DynamicMethod setterMethod = new DynamicMethod(methodName, null, new Type[1] { typeof(FieldType) }, restrictedSkipVisibility: true);
			ILGenerator gen = setterMethod.GetILGenerator();
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Stsfld, fieldInfo);
			gen.Emit(OpCodes.Ret);
			return (Action<FieldType>)setterMethod.CreateDelegate(typeof(Action<FieldType>));
		}

		/// <summary>
		/// Creates a delegate which sets the value of a field. If emitting is not supported on the current platform, the delegate will use reflection to set the value.
		/// </summary>
		/// <param name="fieldInfo">The <see cref="T:System.Reflection.FieldInfo" /> instance describing the field to create a setter for.</param>
		/// <returns>A delegate which sets the value of the given field.</returns>
		/// <exception cref="T:System.ArgumentNullException">The fieldInfo parameter is null.</exception>
		public static Action<object> CreateWeakStaticFieldSetter(FieldInfo fieldInfo)
		{
			if (fieldInfo == null)
			{
				throw new ArgumentNullException("fieldInfo");
			}
			if (!fieldInfo.IsStatic)
			{
				throw new ArgumentException("Field must be static.");
			}
			fieldInfo = fieldInfo.DeAliasField();
			if (EmitIsIllegalForMember(fieldInfo))
			{
				return delegate(object value)
				{
					fieldInfo.SetValue(null, value);
				};
			}
			string methodName = fieldInfo.ReflectedType.FullName + ".set_" + fieldInfo.Name;
			DynamicMethod setterMethod = new DynamicMethod(methodName, null, new Type[1] { typeof(object) }, restrictedSkipVisibility: true);
			ILGenerator gen = setterMethod.GetILGenerator();
			gen.Emit(OpCodes.Ldarg_0);
			if (fieldInfo.FieldType.IsValueType)
			{
				gen.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
			}
			else
			{
				gen.Emit(OpCodes.Castclass, fieldInfo.FieldType);
			}
			gen.Emit(OpCodes.Stsfld, fieldInfo);
			gen.Emit(OpCodes.Ret);
			return (Action<object>)setterMethod.CreateDelegate(typeof(Action<object>));
		}

		/// <summary>
		/// Creates a delegate which gets the value of a field. If emitting is not supported on the current platform, the delegate will use reflection to get the value.
		/// </summary>
		/// <typeparam name="InstanceType">The type of the instance to get a value from.</typeparam>
		/// <typeparam name="FieldType">The type of the field to get a value from.</typeparam>
		/// <param name="fieldInfo">The <see cref="T:System.Reflection.FieldInfo" /> instance describing the field to create a getter for.</param>
		/// <returns>A delegate which gets the value of the given field.</returns>
		/// <exception cref="T:System.ArgumentNullException">The fieldInfo parameter is null.</exception>
		public static ValueGetter<InstanceType, FieldType> CreateInstanceFieldGetter<InstanceType, FieldType>(FieldInfo fieldInfo)
		{
			if (fieldInfo == null)
			{
				throw new ArgumentNullException("fieldInfo");
			}
			if (fieldInfo.IsStatic)
			{
				throw new ArgumentException("Field cannot be static.");
			}
			fieldInfo = fieldInfo.DeAliasField();
			if (EmitIsIllegalForMember(fieldInfo))
			{
				return delegate(ref InstanceType classInstance)
				{
					return (FieldType)fieldInfo.GetValue(classInstance);
				};
			}
			string methodName = fieldInfo.ReflectedType.FullName + ".get_" + fieldInfo.Name;
			DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(FieldType), new Type[1] { typeof(InstanceType).MakeByRefType() }, restrictedSkipVisibility: true);
			ILGenerator gen = getterMethod.GetILGenerator();
			if (typeof(InstanceType).IsValueType)
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldfld, fieldInfo);
			}
			else
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldind_Ref);
				gen.Emit(OpCodes.Ldfld, fieldInfo);
			}
			gen.Emit(OpCodes.Ret);
			return (ValueGetter<InstanceType, FieldType>)getterMethod.CreateDelegate(typeof(ValueGetter<InstanceType, FieldType>));
		}

		/// <summary>
		/// Creates a delegate which gets the value of a field from a weakly typed instance of a given type. If emitting is not supported on the current platform, the delegate will use reflection to get the value.
		/// </summary>
		/// <typeparam name="FieldType">The type of the field to get a value from.</typeparam>
		/// <param name="instanceType">The <see cref="T:System.Type" /> of the instance to get a value from.</param>
		/// <param name="fieldInfo">The <see cref="T:System.Reflection.FieldInfo" /> instance describing the field to create a getter for.</param>
		/// <returns>A delegate which gets the value of the given field.</returns>
		/// <exception cref="T:System.ArgumentNullException">The fieldInfo parameter is null.</exception>
		public static WeakValueGetter<FieldType> CreateWeakInstanceFieldGetter<FieldType>(Type instanceType, FieldInfo fieldInfo)
		{
			if (fieldInfo == null)
			{
				throw new ArgumentNullException("fieldInfo");
			}
			if (instanceType == null)
			{
				throw new ArgumentNullException("instanceType");
			}
			if (fieldInfo.IsStatic)
			{
				throw new ArgumentException("Field cannot be static.");
			}
			fieldInfo = fieldInfo.DeAliasField();
			if (EmitIsIllegalForMember(fieldInfo))
			{
				return delegate(ref object classInstance)
				{
					return (FieldType)fieldInfo.GetValue(classInstance);
				};
			}
			string methodName = fieldInfo.ReflectedType.FullName + ".get_" + fieldInfo.Name;
			DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(FieldType), new Type[1] { typeof(object).MakeByRefType() }, restrictedSkipVisibility: true);
			ILGenerator gen = getterMethod.GetILGenerator();
			if (instanceType.IsValueType)
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldind_Ref);
				gen.Emit(OpCodes.Unbox_Any, instanceType);
				gen.Emit(OpCodes.Ldfld, fieldInfo);
			}
			else
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldind_Ref);
				gen.Emit(OpCodes.Castclass, instanceType);
				gen.Emit(OpCodes.Ldfld, fieldInfo);
			}
			gen.Emit(OpCodes.Ret);
			return (WeakValueGetter<FieldType>)getterMethod.CreateDelegate(typeof(WeakValueGetter<FieldType>));
		}

		/// <summary>
		/// Creates a delegate which gets the weakly typed value of a field from a weakly typed instance of a given type. If emitting is not supported on the current platform, the delegate will use reflection to get the value.
		/// </summary>
		/// <param name="instanceType">The <see cref="T:System.Type" /> of the instance to get a value from.</param>
		/// <param name="fieldInfo">The <see cref="T:System.Reflection.FieldInfo" /> instance describing the field to create a getter for.</param>
		/// <returns>A delegate which gets the value of the given field.</returns>
		/// <exception cref="T:System.ArgumentNullException">The fieldInfo parameter is null.</exception>
		public static WeakValueGetter CreateWeakInstanceFieldGetter(Type instanceType, FieldInfo fieldInfo)
		{
			if (fieldInfo == null)
			{
				throw new ArgumentNullException("fieldInfo");
			}
			if (instanceType == null)
			{
				throw new ArgumentNullException("instanceType");
			}
			if (fieldInfo.IsStatic)
			{
				throw new ArgumentException("Field cannot be static.");
			}
			fieldInfo = fieldInfo.DeAliasField();
			if (EmitIsIllegalForMember(fieldInfo))
			{
				return delegate(ref object classInstance)
				{
					return fieldInfo.GetValue(classInstance);
				};
			}
			string methodName = fieldInfo.ReflectedType.FullName + ".get_" + fieldInfo.Name;
			DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(object), new Type[1] { typeof(object).MakeByRefType() }, restrictedSkipVisibility: true);
			ILGenerator gen = getterMethod.GetILGenerator();
			if (instanceType.IsValueType)
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldind_Ref);
				gen.Emit(OpCodes.Unbox_Any, instanceType);
				gen.Emit(OpCodes.Ldfld, fieldInfo);
				if (fieldInfo.FieldType.IsValueType)
				{
					gen.Emit(OpCodes.Box, fieldInfo.FieldType);
				}
			}
			else
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldind_Ref);
				gen.Emit(OpCodes.Castclass, instanceType);
				gen.Emit(OpCodes.Ldfld, fieldInfo);
				if (fieldInfo.FieldType.IsValueType)
				{
					gen.Emit(OpCodes.Box, fieldInfo.FieldType);
				}
			}
			gen.Emit(OpCodes.Ret);
			return (WeakValueGetter)getterMethod.CreateDelegate(typeof(WeakValueGetter));
		}

		/// <summary>
		/// Creates a delegate which sets the value of a field. If emitting is not supported on the current platform, the delegate will use reflection to set the value.
		/// </summary>
		/// <typeparam name="InstanceType">The type of the instance to set a value on.</typeparam>
		/// <typeparam name="FieldType">The type of the field to set a value to.</typeparam>
		/// <param name="fieldInfo">The <see cref="T:System.Reflection.FieldInfo" /> instance describing the field to create a setter for.</param>
		/// <returns>A delegate which sets the value of the given field.</returns>
		/// <exception cref="T:System.ArgumentNullException">The fieldInfo parameter is null.</exception>
		public static ValueSetter<InstanceType, FieldType> CreateInstanceFieldSetter<InstanceType, FieldType>(FieldInfo fieldInfo)
		{
			if (fieldInfo == null)
			{
				throw new ArgumentNullException("fieldInfo");
			}
			if (fieldInfo.IsStatic)
			{
				throw new ArgumentException("Field cannot be static.");
			}
			fieldInfo = fieldInfo.DeAliasField();
			if (EmitIsIllegalForMember(fieldInfo))
			{
				return delegate(ref InstanceType classInstance, FieldType value)
				{
					if (typeof(InstanceType).IsValueType)
					{
						object obj = classInstance;
						fieldInfo.SetValue(obj, value);
						classInstance = (InstanceType)obj;
					}
					else
					{
						fieldInfo.SetValue(classInstance, value);
					}
				};
			}
			string methodName = fieldInfo.ReflectedType.FullName + ".set_" + fieldInfo.Name;
			DynamicMethod setterMethod = new DynamicMethod(methodName, null, new Type[2]
			{
				typeof(InstanceType).MakeByRefType(),
				typeof(FieldType)
			}, restrictedSkipVisibility: true);
			ILGenerator gen = setterMethod.GetILGenerator();
			if (typeof(InstanceType).IsValueType)
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Stfld, fieldInfo);
			}
			else
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldind_Ref);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Stfld, fieldInfo);
			}
			gen.Emit(OpCodes.Ret);
			return (ValueSetter<InstanceType, FieldType>)setterMethod.CreateDelegate(typeof(ValueSetter<InstanceType, FieldType>));
		}

		/// <summary>
		/// Creates a delegate which sets the value of a field on a weakly typed instance of a given type. If emitting is not supported on the current platform, the delegate will use reflection to set the value.
		/// </summary>
		/// <typeparam name="FieldType">The type of the field to set a value to.</typeparam>
		/// <param name="instanceType">Type of the instance.</param>
		/// <param name="fieldInfo">The <see cref="T:System.Reflection.FieldInfo" /> instance describing the field to create a setter for.</param>
		/// <returns>
		/// A delegate which sets the value of the given field.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">The fieldInfo parameter is null.</exception>
		/// <exception cref="T:System.ArgumentException">Field cannot be static.</exception>
		public static WeakValueSetter<FieldType> CreateWeakInstanceFieldSetter<FieldType>(Type instanceType, FieldInfo fieldInfo)
		{
			if (fieldInfo == null)
			{
				throw new ArgumentNullException("fieldInfo");
			}
			if (instanceType == null)
			{
				throw new ArgumentNullException("instanceType");
			}
			if (fieldInfo.IsStatic)
			{
				throw new ArgumentException("Field cannot be static.");
			}
			fieldInfo = fieldInfo.DeAliasField();
			if (EmitIsIllegalForMember(fieldInfo))
			{
				return delegate(ref object classInstance, FieldType value)
				{
					fieldInfo.SetValue(classInstance, value);
				};
			}
			string methodName = fieldInfo.ReflectedType.FullName + ".set_" + fieldInfo.Name;
			DynamicMethod setterMethod = new DynamicMethod(methodName, null, new Type[2]
			{
				typeof(object).MakeByRefType(),
				typeof(FieldType)
			}, restrictedSkipVisibility: true);
			ILGenerator gen = setterMethod.GetILGenerator();
			if (instanceType.IsValueType)
			{
				LocalBuilder local = gen.DeclareLocal(instanceType);
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldind_Ref);
				gen.Emit(OpCodes.Unbox_Any, instanceType);
				gen.Emit(OpCodes.Stloc, local);
				gen.Emit(OpCodes.Ldloca_S, local);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Stfld, fieldInfo);
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldloc, local);
				gen.Emit(OpCodes.Box, instanceType);
				gen.Emit(OpCodes.Stind_Ref);
			}
			else
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldind_Ref);
				gen.Emit(OpCodes.Castclass, instanceType);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Stfld, fieldInfo);
			}
			gen.Emit(OpCodes.Ret);
			return (WeakValueSetter<FieldType>)setterMethod.CreateDelegate(typeof(WeakValueSetter<FieldType>));
		}

		/// <summary>
		/// Creates a delegate which sets the weakly typed value of a field on a weakly typed instance of a given type. If emitting is not supported on the current platform, the delegate will use reflection to set the value.
		/// </summary>
		/// <param name="instanceType">Type of the instance.</param>
		/// <param name="fieldInfo">The <see cref="T:System.Reflection.FieldInfo" /> instance describing the field to create a setter for.</param>
		/// <returns>
		/// A delegate which sets the value of the given field.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">The fieldInfo parameter is null.</exception>
		/// <exception cref="T:System.ArgumentException">Field cannot be static.</exception>
		public static WeakValueSetter CreateWeakInstanceFieldSetter(Type instanceType, FieldInfo fieldInfo)
		{
			if (fieldInfo == null)
			{
				throw new ArgumentNullException("fieldInfo");
			}
			if (instanceType == null)
			{
				throw new ArgumentNullException("instanceType");
			}
			if (fieldInfo.IsStatic)
			{
				throw new ArgumentException("Field cannot be static.");
			}
			fieldInfo = fieldInfo.DeAliasField();
			if (EmitIsIllegalForMember(fieldInfo))
			{
				return delegate(ref object classInstance, object value)
				{
					fieldInfo.SetValue(classInstance, value);
				};
			}
			string methodName = fieldInfo.ReflectedType.FullName + ".set_" + fieldInfo.Name;
			DynamicMethod setterMethod = new DynamicMethod(methodName, null, new Type[2]
			{
				typeof(object).MakeByRefType(),
				typeof(object)
			}, restrictedSkipVisibility: true);
			ILGenerator gen = setterMethod.GetILGenerator();
			if (instanceType.IsValueType)
			{
				LocalBuilder local = gen.DeclareLocal(instanceType);
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldind_Ref);
				gen.Emit(OpCodes.Unbox_Any, instanceType);
				gen.Emit(OpCodes.Stloc, local);
				gen.Emit(OpCodes.Ldloca_S, local);
				gen.Emit(OpCodes.Ldarg_1);
				if (fieldInfo.FieldType.IsValueType)
				{
					gen.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
				}
				else
				{
					gen.Emit(OpCodes.Castclass, fieldInfo.FieldType);
				}
				gen.Emit(OpCodes.Stfld, fieldInfo);
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldloc, local);
				gen.Emit(OpCodes.Box, instanceType);
				gen.Emit(OpCodes.Stind_Ref);
			}
			else
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldind_Ref);
				gen.Emit(OpCodes.Castclass, instanceType);
				gen.Emit(OpCodes.Ldarg_1);
				if (fieldInfo.FieldType.IsValueType)
				{
					gen.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
				}
				else
				{
					gen.Emit(OpCodes.Castclass, fieldInfo.FieldType);
				}
				gen.Emit(OpCodes.Stfld, fieldInfo);
			}
			gen.Emit(OpCodes.Ret);
			return (WeakValueSetter)setterMethod.CreateDelegate(typeof(WeakValueSetter));
		}

		/// <summary>
		/// Creates a delegate which gets the weakly typed value of a field from a weakly typed instance of a given type. If emitting is not supported on the current platform, the delegate will use reflection to get the value.
		/// </summary>
		/// <param name="instanceType">The <see cref="T:System.Type" /> of the instance to get a value from.</param>
		/// <param name="propertyInfo">The <see cref="T:System.Reflection.FieldInfo" /> instance describing the field to create a getter for.</param>
		/// <returns>A delegate which gets the value of the given field.</returns>
		/// <exception cref="T:System.ArgumentNullException">The fieldInfo parameter is null.</exception>
		public static WeakValueGetter CreateWeakInstancePropertyGetter(Type instanceType, PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
			{
				throw new ArgumentNullException("propertyInfo");
			}
			if (instanceType == null)
			{
				throw new ArgumentNullException("instanceType");
			}
			propertyInfo = propertyInfo.DeAliasProperty();
			if (propertyInfo.GetIndexParameters().Length != 0)
			{
				throw new ArgumentException("Property must not have any index parameters");
			}
			MethodInfo getMethod = propertyInfo.GetGetMethod(nonPublic: true);
			if (getMethod == null)
			{
				throw new ArgumentException("Property must have a getter.");
			}
			if (getMethod.IsStatic)
			{
				throw new ArgumentException("Property cannot be static.");
			}
			if (EmitIsIllegalForMember(propertyInfo))
			{
				return delegate(ref object classInstance)
				{
					return propertyInfo.GetValue(classInstance, null);
				};
			}
			string methodName = propertyInfo.ReflectedType.FullName + ".get_" + propertyInfo.Name;
			DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(object), new Type[1] { typeof(object).MakeByRefType() }, restrictedSkipVisibility: true);
			ILGenerator gen = getterMethod.GetILGenerator();
			if (instanceType.IsValueType)
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldind_Ref);
				gen.Emit(OpCodes.Unbox_Any, instanceType);
				if (getMethod.IsVirtual || getMethod.IsAbstract)
				{
					gen.Emit(OpCodes.Callvirt, getMethod);
				}
				else
				{
					gen.Emit(OpCodes.Call, getMethod);
				}
				if (propertyInfo.PropertyType.IsValueType)
				{
					gen.Emit(OpCodes.Box, propertyInfo.PropertyType);
				}
			}
			else
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldind_Ref);
				gen.Emit(OpCodes.Castclass, instanceType);
				if (getMethod.IsVirtual || getMethod.IsAbstract)
				{
					gen.Emit(OpCodes.Callvirt, getMethod);
				}
				else
				{
					gen.Emit(OpCodes.Call, getMethod);
				}
				if (propertyInfo.PropertyType.IsValueType)
				{
					gen.Emit(OpCodes.Box, propertyInfo.PropertyType);
				}
			}
			gen.Emit(OpCodes.Ret);
			return (WeakValueGetter)getterMethod.CreateDelegate(typeof(WeakValueGetter));
		}

		/// <summary>
		/// Creates a delegate which sets the weakly typed value of a property on a weakly typed instance of a given type. If emitting is not supported on the current platform, the delegate will use reflection to set the value.
		/// </summary>
		/// <param name="instanceType">Type of the instance.</param>
		/// <param name="propertyInfo">The <see cref="T:System.Reflection.PropertyInfo" /> instance describing the property to create a setter for.</param>
		/// <returns>
		/// A delegate which sets the value of the given field.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">The fieldInfo parameter is null.</exception>
		/// <exception cref="T:System.ArgumentException">Property cannot be static.</exception>
		public static WeakValueSetter CreateWeakInstancePropertySetter(Type instanceType, PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
			{
				throw new ArgumentNullException("propertyInfo");
			}
			if (instanceType == null)
			{
				throw new ArgumentNullException("instanceType");
			}
			propertyInfo = propertyInfo.DeAliasProperty();
			if (propertyInfo.GetIndexParameters().Length != 0)
			{
				throw new ArgumentException("Property must not have any index parameters");
			}
			MethodInfo setMethod = propertyInfo.GetSetMethod(nonPublic: true);
			if (setMethod.IsStatic)
			{
				throw new ArgumentException("Property cannot be static.");
			}
			if (EmitIsIllegalForMember(propertyInfo))
			{
				return delegate(ref object classInstance, object value)
				{
					propertyInfo.SetValue(classInstance, value, null);
				};
			}
			string methodName = propertyInfo.ReflectedType.FullName + ".set_" + propertyInfo.Name;
			DynamicMethod setterMethod = new DynamicMethod(methodName, null, new Type[2]
			{
				typeof(object).MakeByRefType(),
				typeof(object)
			}, restrictedSkipVisibility: true);
			ILGenerator gen = setterMethod.GetILGenerator();
			if (instanceType.IsValueType)
			{
				LocalBuilder local = gen.DeclareLocal(instanceType);
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldind_Ref);
				gen.Emit(OpCodes.Unbox_Any, instanceType);
				gen.Emit(OpCodes.Stloc, local);
				gen.Emit(OpCodes.Ldloca_S, local);
				gen.Emit(OpCodes.Ldarg_1);
				if (propertyInfo.PropertyType.IsValueType)
				{
					gen.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
				}
				else
				{
					gen.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
				}
				if (setMethod.IsVirtual || setMethod.IsAbstract)
				{
					gen.Emit(OpCodes.Callvirt, setMethod);
				}
				else
				{
					gen.Emit(OpCodes.Call, setMethod);
				}
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldloc, local);
				gen.Emit(OpCodes.Box, instanceType);
				gen.Emit(OpCodes.Stind_Ref);
			}
			else
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldind_Ref);
				gen.Emit(OpCodes.Castclass, instanceType);
				gen.Emit(OpCodes.Ldarg_1);
				if (propertyInfo.PropertyType.IsValueType)
				{
					gen.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
				}
				else
				{
					gen.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
				}
				if (setMethod.IsVirtual || setMethod.IsAbstract)
				{
					gen.Emit(OpCodes.Callvirt, setMethod);
				}
				else
				{
					gen.Emit(OpCodes.Call, setMethod);
				}
			}
			gen.Emit(OpCodes.Ret);
			return (WeakValueSetter)setterMethod.CreateDelegate(typeof(WeakValueSetter));
		}

		/// <summary>
		/// Creates a delegate which sets the value of a property. If emitting is not supported on the current platform, the delegate will use reflection to set the value.
		/// </summary>
		/// <typeparam name="PropType">The type of the property to set a value to.</typeparam>
		/// <param name="propertyInfo">The <see cref="T:System.Reflection.PropertyInfo" /> instance describing the property to create a setter for.</param>
		/// <returns>A delegate which sets the value of the given property.</returns>
		/// <exception cref="T:System.ArgumentNullException">The propertyInfo parameter is null.</exception>
		public static Action<PropType> CreateStaticPropertySetter<PropType>(PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
			{
				throw new ArgumentNullException("fieldInfo");
			}
			propertyInfo = propertyInfo.DeAliasProperty();
			if (propertyInfo.GetIndexParameters().Length != 0)
			{
				throw new ArgumentException("Property must not have any index parameters");
			}
			MethodInfo setMethod = propertyInfo.GetSetMethod(nonPublic: true);
			if (setMethod == null)
			{
				throw new ArgumentException("Property must have a set method.");
			}
			if (!setMethod.IsStatic)
			{
				throw new ArgumentException("Property must be static.");
			}
			if (EmitIsIllegalForMember(propertyInfo))
			{
				return delegate(PropType value)
				{
					propertyInfo.SetValue(null, value, null);
				};
			}
			string methodName = propertyInfo.ReflectedType.FullName + ".set_" + propertyInfo.Name;
			DynamicMethod setterMethod = new DynamicMethod(methodName, null, new Type[1] { typeof(PropType) }, restrictedSkipVisibility: true);
			ILGenerator gen = setterMethod.GetILGenerator();
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Call, setMethod);
			gen.Emit(OpCodes.Ret);
			return (Action<PropType>)setterMethod.CreateDelegate(typeof(Action<PropType>));
		}

		/// <summary>
		/// Creates a delegate which gets the value of a property. If emitting is not supported on the current platform, the delegate will use reflection to get the value.
		/// </summary>
		/// <typeparam name="PropType">The type of the property to get a value from.</typeparam>
		/// <param name="propertyInfo">The <see cref="T:System.Reflection.PropertyInfo" /> instance describing the property to create a getter for.</param>
		/// <returns>A delegate which gets the value of the given property.</returns>
		/// <exception cref="T:System.ArgumentNullException">The propertyInfo parameter is null.</exception>
		public static Func<PropType> CreateStaticPropertyGetter<PropType>(PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
			{
				throw new ArgumentNullException("propertyInfo");
			}
			propertyInfo = propertyInfo.DeAliasProperty();
			if (propertyInfo.GetIndexParameters().Length != 0)
			{
				throw new ArgumentException("Property must not have any index parameters");
			}
			MethodInfo getMethod = propertyInfo.GetGetMethod(nonPublic: true);
			if (getMethod == null)
			{
				throw new ArgumentException("Property must have a get method.");
			}
			if (!getMethod.IsStatic)
			{
				throw new ArgumentException("Property must be static.");
			}
			if (EmitIsIllegalForMember(propertyInfo))
			{
				return () => (PropType)propertyInfo.GetValue(null, null);
			}
			string methodName = propertyInfo.ReflectedType.FullName + ".get_" + propertyInfo.Name;
			DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(PropType), new Type[0], restrictedSkipVisibility: true);
			ILGenerator gen = getterMethod.GetILGenerator();
			gen.Emit(OpCodes.Call, getMethod);
			Type returnType = propertyInfo.GetReturnType();
			if (returnType.IsValueType && !typeof(PropType).IsValueType)
			{
				gen.Emit(OpCodes.Box, returnType);
			}
			gen.Emit(OpCodes.Ret);
			return (Func<PropType>)getterMethod.CreateDelegate(typeof(Func<PropType>));
		}

		/// <summary>
		/// Creates a delegate which sets the value of a property. If emitting is not supported on the current platform, the delegate will use reflection to set the value.
		/// </summary>
		/// <typeparam name="InstanceType">The type of the instance to set a value on.</typeparam>
		/// <typeparam name="PropType">The type of the property to set a value to.</typeparam>
		/// <param name="propertyInfo">The <see cref="T:System.Reflection.PropertyInfo" /> instance describing the property to create a setter for.</param>
		/// <returns>A delegate which sets the value of the given property.</returns>
		/// <exception cref="T:System.ArgumentNullException">The propertyInfo parameter is null.</exception>
		public static ValueSetter<InstanceType, PropType> CreateInstancePropertySetter<InstanceType, PropType>(PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
			{
				throw new ArgumentNullException("fieldInfo");
			}
			propertyInfo = propertyInfo.DeAliasProperty();
			if (propertyInfo.GetIndexParameters().Length != 0)
			{
				throw new ArgumentException("Property must not have any index parameters");
			}
			MethodInfo setMethod = propertyInfo.GetSetMethod(nonPublic: true);
			if (setMethod == null)
			{
				throw new ArgumentException("Property must have a set method.");
			}
			if (setMethod.IsStatic)
			{
				throw new ArgumentException("Property cannot be static.");
			}
			if (EmitIsIllegalForMember(propertyInfo))
			{
				return delegate(ref InstanceType classInstance, PropType value)
				{
					if (typeof(InstanceType).IsValueType)
					{
						object obj = classInstance;
						propertyInfo.SetValue(obj, value, null);
						classInstance = (InstanceType)obj;
					}
					else
					{
						propertyInfo.SetValue(classInstance, value, null);
					}
				};
			}
			string methodName = propertyInfo.ReflectedType.FullName + ".set_" + propertyInfo.Name;
			DynamicMethod setterMethod = new DynamicMethod(methodName, null, new Type[2]
			{
				typeof(InstanceType).MakeByRefType(),
				typeof(PropType)
			}, restrictedSkipVisibility: true);
			ILGenerator gen = setterMethod.GetILGenerator();
			if (typeof(InstanceType).IsValueType)
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Callvirt, setMethod);
			}
			else
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldind_Ref);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Callvirt, setMethod);
			}
			gen.Emit(OpCodes.Ret);
			return (ValueSetter<InstanceType, PropType>)setterMethod.CreateDelegate(typeof(ValueSetter<InstanceType, PropType>));
		}

		/// <summary>
		/// Creates a delegate which gets the value of a property. If emitting is not supported on the current platform, the delegate will use reflection to get the value.
		/// </summary>
		/// <typeparam name="InstanceType">The type of the instance to get a value from.</typeparam>
		/// <typeparam name="PropType">The type of the property to get a value from.</typeparam>
		/// <param name="propertyInfo">The <see cref="T:System.Reflection.PropertyInfo" /> instance describing the property to create a getter for.</param>
		/// <returns>A delegate which gets the value of the given property.</returns>
		/// <exception cref="T:System.ArgumentNullException">The propertyInfo parameter is null.</exception>
		public static ValueGetter<InstanceType, PropType> CreateInstancePropertyGetter<InstanceType, PropType>(PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
			{
				throw new ArgumentNullException("propertyInfo");
			}
			propertyInfo = propertyInfo.DeAliasProperty();
			if (propertyInfo.GetIndexParameters().Length != 0)
			{
				throw new ArgumentException("Property must not have any index parameters");
			}
			MethodInfo getMethod = propertyInfo.GetGetMethod(nonPublic: true);
			if (getMethod == null)
			{
				throw new ArgumentException("Property must have a get method.");
			}
			if (getMethod.IsStatic)
			{
				throw new ArgumentException("Property cannot be static.");
			}
			if (EmitIsIllegalForMember(propertyInfo))
			{
				return delegate(ref InstanceType classInstance)
				{
					return (PropType)propertyInfo.GetValue(classInstance, null);
				};
			}
			string methodName = propertyInfo.ReflectedType.FullName + ".get_" + propertyInfo.Name;
			DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(PropType), new Type[1] { typeof(InstanceType).MakeByRefType() }, restrictedSkipVisibility: true);
			ILGenerator gen = getterMethod.GetILGenerator();
			if (typeof(InstanceType).IsValueType)
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Callvirt, getMethod);
			}
			else
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldind_Ref);
				gen.Emit(OpCodes.Callvirt, getMethod);
			}
			gen.Emit(OpCodes.Ret);
			return (ValueGetter<InstanceType, PropType>)getterMethod.CreateDelegate(typeof(ValueGetter<InstanceType, PropType>));
		}

		/// <summary>
		/// Creates a fast delegate method which calls a given parameterless instance method and returns the result.
		/// </summary>
		/// <typeparam name="InstanceType">The type of the class which the method is on.</typeparam>
		/// <typeparam name="ReturnType">The type which is returned by the given method info.</typeparam>
		/// <param name="methodInfo">The method info instance which is used.</param>
		/// <returns>A delegate which calls the method and returns the result, except it's hundreds of times faster than MethodInfo.Invoke.</returns>
		public static Func<InstanceType, ReturnType> CreateMethodReturner<InstanceType, ReturnType>(MethodInfo methodInfo)
		{
			if (methodInfo == null)
			{
				throw new ArgumentNullException("methodInfo");
			}
			if (methodInfo.IsStatic)
			{
				throw new ArgumentException("Given method '" + methodInfo.Name + "' is static when it has to be an instance method.");
			}
			methodInfo = methodInfo.DeAliasMethod();
			return (Func<InstanceType, ReturnType>)Delegate.CreateDelegate(typeof(Func<InstanceType, ReturnType>), methodInfo);
		}

		/// <summary>
		/// Creates a fast delegate method which calls a given parameterless static method.
		/// </summary>
		/// <param name="methodInfo">The method info instance which is used.</param>
		/// <returns>A delegate which calls the method and returns the result, except it's hundreds of times faster than MethodInfo.Invoke.</returns>
		public static Action CreateStaticMethodCaller(MethodInfo methodInfo)
		{
			if (methodInfo == null)
			{
				throw new ArgumentNullException("methodInfo");
			}
			if (!methodInfo.IsStatic)
			{
				throw new ArgumentException("Given method '" + methodInfo.Name + "' is an instance method when it has to be static.");
			}
			if (methodInfo.GetParameters().Length != 0)
			{
				throw new ArgumentException("Given method cannot have any parameters.");
			}
			methodInfo = methodInfo.DeAliasMethod();
			return (Action)Delegate.CreateDelegate(typeof(Action), methodInfo);
		}

		/// <summary>
		/// Creates a fast delegate method which calls a given parameterless weakly typed instance method.
		/// </summary>
		/// <param name="methodInfo">The method info instance which is used.</param>
		/// <returns>A delegate which calls the method and returns the result, except it's hundreds of times faster than MethodInfo.Invoke.</returns>
		public static Action<object, TArg1> CreateWeakInstanceMethodCaller<TArg1>(MethodInfo methodInfo)
		{
			if (methodInfo == null)
			{
				throw new ArgumentNullException("methodInfo");
			}
			if (methodInfo.IsStatic)
			{
				throw new ArgumentException("Given method '" + methodInfo.Name + "' is static when it has to be an instance method.");
			}
			ParameterInfo[] parameters = methodInfo.GetParameters();
			if (parameters.Length != 1)
			{
				throw new ArgumentException("Given method '" + methodInfo.Name + "' must have exactly one parameter.");
			}
			if (parameters[0].ParameterType != typeof(TArg1))
			{
				throw new ArgumentException("The first parameter of the method '" + methodInfo.Name + "' must be of type " + typeof(TArg1)?.ToString() + ".");
			}
			methodInfo = methodInfo.DeAliasMethod();
			if (EmitIsIllegalForMember(methodInfo))
			{
				return delegate(object classInstance, TArg1 arg)
				{
					methodInfo.Invoke(classInstance, new object[1] { arg });
				};
			}
			Type declaringType = methodInfo.DeclaringType;
			string methodName = methodInfo.ReflectedType.FullName + ".call_" + methodInfo.Name;
			DynamicMethod method = new DynamicMethod(methodName, null, new Type[2]
			{
				typeof(object),
				typeof(TArg1)
			}, restrictedSkipVisibility: true);
			ILGenerator gen = method.GetILGenerator();
			if (declaringType.IsValueType)
			{
				LocalBuilder loc = gen.DeclareLocal(declaringType);
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Unbox_Any, declaringType);
				gen.Emit(OpCodes.Stloc, loc);
				gen.Emit(OpCodes.Ldloca_S, loc);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Call, methodInfo);
			}
			else
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Castclass, declaringType);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Callvirt, methodInfo);
			}
			gen.Emit(OpCodes.Ret);
			return (Action<object, TArg1>)method.CreateDelegate(typeof(Action<object, TArg1>));
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static Action<object> CreateWeakInstanceMethodCaller(MethodInfo methodInfo)
		{
			if (methodInfo == null)
			{
				throw new ArgumentNullException("methodInfo");
			}
			if (methodInfo.IsStatic)
			{
				throw new ArgumentException("Given method '" + methodInfo.Name + "' is static when it has to be an instance method.");
			}
			if (methodInfo.GetParameters().Length != 0)
			{
				throw new ArgumentException("Given method cannot have any parameters.");
			}
			methodInfo = methodInfo.DeAliasMethod();
			if (EmitIsIllegalForMember(methodInfo))
			{
				return delegate(object classInstance)
				{
					methodInfo.Invoke(classInstance, null);
				};
			}
			Type declaringType = methodInfo.DeclaringType;
			string methodName = methodInfo.ReflectedType.FullName + ".call_" + methodInfo.Name;
			DynamicMethod method = new DynamicMethod(methodName, null, new Type[1] { typeof(object) }, restrictedSkipVisibility: true);
			ILGenerator gen = method.GetILGenerator();
			if (declaringType.IsValueType)
			{
				LocalBuilder loc = gen.DeclareLocal(declaringType);
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Unbox_Any, declaringType);
				gen.Emit(OpCodes.Stloc, loc);
				gen.Emit(OpCodes.Ldloca_S, loc);
				gen.Emit(OpCodes.Call, methodInfo);
			}
			else
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Castclass, declaringType);
				gen.Emit(OpCodes.Callvirt, methodInfo);
			}
			if (methodInfo.ReturnType != null && methodInfo.ReturnType != typeof(void))
			{
				gen.Emit(OpCodes.Pop);
			}
			gen.Emit(OpCodes.Ret);
			return (Action<object>)method.CreateDelegate(typeof(Action<object>));
		}

		/// <summary>
		/// Creates a fast delegate method which calls a given weakly typed instance method with one argument and returns a value.
		/// </summary>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <typeparam name="TArg1">The type of the first argument.</typeparam>
		/// <param name="methodInfo">The method info instance which is used.</param>
		/// <returns>
		/// A delegate which calls the method and returns the result, except it's hundreds of times faster than MethodInfo.Invoke.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">methodInfo</exception>
		/// <exception cref="T:System.ArgumentException">
		/// Given method ' + methodInfo.Name + ' is static when it has to be an instance method.
		/// or
		/// Given method ' + methodInfo.Name + ' must return type  + typeof(TResult) + .
		/// or
		/// Given method ' + methodInfo.Name + ' must have exactly one parameter.
		/// or
		/// The first parameter of the method ' + methodInfo.Name + ' must be of type  + typeof(TArg1) + .
		/// </exception>
		public static Func<object, TArg1, TResult> CreateWeakInstanceMethodCaller<TResult, TArg1>(MethodInfo methodInfo)
		{
			if (methodInfo == null)
			{
				throw new ArgumentNullException("methodInfo");
			}
			if (methodInfo.IsStatic)
			{
				throw new ArgumentException("Given method '" + methodInfo.Name + "' is static when it has to be an instance method.");
			}
			if (methodInfo.ReturnType != typeof(TResult))
			{
				throw new ArgumentException("Given method '" + methodInfo.Name + "' must return type " + typeof(TResult)?.ToString() + ".");
			}
			ParameterInfo[] parameters = methodInfo.GetParameters();
			if (parameters.Length != 1)
			{
				throw new ArgumentException("Given method '" + methodInfo.Name + "' must have exactly one parameter.");
			}
			if (!typeof(TArg1).InheritsFrom(parameters[0].ParameterType))
			{
				throw new ArgumentException("The first parameter of the method '" + methodInfo.Name + "' must be of type " + typeof(TArg1)?.ToString() + ".");
			}
			methodInfo = methodInfo.DeAliasMethod();
			if (EmitIsIllegalForMember(methodInfo))
			{
				return (object classInstance, TArg1 arg1) => (TResult)methodInfo.Invoke(classInstance, new object[1] { arg1 });
			}
			Type declaringType = methodInfo.DeclaringType;
			string methodName = methodInfo.ReflectedType.FullName + ".call_" + methodInfo.Name;
			DynamicMethod method = new DynamicMethod(methodName, typeof(TResult), new Type[2]
			{
				typeof(object),
				typeof(TArg1)
			}, restrictedSkipVisibility: true);
			ILGenerator gen = method.GetILGenerator();
			if (declaringType.IsValueType)
			{
				LocalBuilder loc = gen.DeclareLocal(declaringType);
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Unbox_Any, declaringType);
				gen.Emit(OpCodes.Stloc, loc);
				gen.Emit(OpCodes.Ldloca_S, loc);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Call, methodInfo);
			}
			else
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Castclass, declaringType);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Callvirt, methodInfo);
			}
			gen.Emit(OpCodes.Ret);
			return (Func<object, TArg1, TResult>)method.CreateDelegate(typeof(Func<object, TArg1, TResult>));
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static Func<object, TResult> CreateWeakInstanceMethodCallerFunc<TResult>(MethodInfo methodInfo)
		{
			if (methodInfo == null)
			{
				throw new ArgumentNullException("methodInfo");
			}
			if (methodInfo.IsStatic)
			{
				throw new ArgumentException("Given method '" + methodInfo.Name + "' is static when it has to be an instance method.");
			}
			if (methodInfo.ReturnType != typeof(TResult))
			{
				throw new ArgumentException("Given method '" + methodInfo.Name + "' must return type " + typeof(TResult)?.ToString() + ".");
			}
			ParameterInfo[] parameters = methodInfo.GetParameters();
			if (parameters.Length != 0)
			{
				throw new ArgumentException("Given method '" + methodInfo.Name + "' must have no parameter.");
			}
			methodInfo = methodInfo.DeAliasMethod();
			if (EmitIsIllegalForMember(methodInfo))
			{
				return (object classInstance) => (TResult)methodInfo.Invoke(classInstance, null);
			}
			Type declaringType = methodInfo.DeclaringType;
			string methodName = methodInfo.ReflectedType.FullName + ".call_" + methodInfo.Name;
			DynamicMethod method = new DynamicMethod(methodName, typeof(TResult), new Type[1] { typeof(object) }, restrictedSkipVisibility: true);
			ILGenerator gen = method.GetILGenerator();
			if (declaringType.IsValueType)
			{
				LocalBuilder loc = gen.DeclareLocal(declaringType);
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Unbox_Any, declaringType);
				gen.Emit(OpCodes.Stloc, loc);
				gen.Emit(OpCodes.Ldloca_S, loc);
				gen.Emit(OpCodes.Call, methodInfo);
			}
			else
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Castclass, declaringType);
				gen.Emit(OpCodes.Callvirt, methodInfo);
			}
			gen.Emit(OpCodes.Ret);
			return (Func<object, TResult>)method.CreateDelegate(typeof(Func<object, TResult>));
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static Func<object, TArg, TResult> CreateWeakInstanceMethodCallerFunc<TArg, TResult>(MethodInfo methodInfo)
		{
			if (methodInfo == null)
			{
				throw new ArgumentNullException("methodInfo");
			}
			if (methodInfo.IsStatic)
			{
				throw new ArgumentException("Given method '" + methodInfo.Name + "' is static when it has to be an instance method.");
			}
			if (methodInfo.ReturnType != typeof(TResult))
			{
				throw new ArgumentException("Given method '" + methodInfo.Name + "' must return type " + typeof(TResult)?.ToString() + ".");
			}
			ParameterInfo[] parameters = methodInfo.GetParameters();
			if (parameters.Length != 1)
			{
				throw new ArgumentException("Given method '" + methodInfo.Name + "' must have one parameter.");
			}
			if (!parameters[0].ParameterType.IsAssignableFrom(typeof(TArg)))
			{
				throw new ArgumentException("Given method '" + methodInfo.Name + "' has an invalid parameter type.");
			}
			methodInfo = methodInfo.DeAliasMethod();
			if (EmitIsIllegalForMember(methodInfo))
			{
				return (object classInstance, TArg arg) => (TResult)methodInfo.Invoke(classInstance, new object[1] { arg });
			}
			Type declaringType = methodInfo.DeclaringType;
			string methodName = methodInfo.ReflectedType.FullName + ".call_" + methodInfo.Name;
			DynamicMethod method = new DynamicMethod(methodName, typeof(TResult), new Type[2]
			{
				typeof(object),
				typeof(TArg)
			}, restrictedSkipVisibility: true);
			ILGenerator gen = method.GetILGenerator();
			if (declaringType.IsValueType)
			{
				LocalBuilder loc = gen.DeclareLocal(declaringType);
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Unbox_Any, declaringType);
				gen.Emit(OpCodes.Stloc, loc);
				gen.Emit(OpCodes.Ldloca_S, loc);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Call, methodInfo);
			}
			else
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Castclass, declaringType);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Callvirt, methodInfo);
			}
			gen.Emit(OpCodes.Ret);
			return (Func<object, TArg, TResult>)method.CreateDelegate(typeof(Func<object, TArg, TResult>));
		}

		/// <summary>
		/// Creates a fast delegate method which calls a given parameterless instance method on a reference type.
		/// </summary>
		/// <typeparam name="InstanceType">The type of the class which the method is on.</typeparam>
		/// <param name="methodInfo">The method info instance which is used.</param>
		/// <returns>A delegate which calls the method and returns the result, except it's hundreds of times faster than MethodInfo.Invoke.</returns>
		public static Action<InstanceType> CreateInstanceMethodCaller<InstanceType>(MethodInfo methodInfo)
		{
			if (methodInfo == null)
			{
				throw new ArgumentNullException("methodInfo");
			}
			if (methodInfo.IsStatic)
			{
				throw new ArgumentException("Given method '" + methodInfo.Name + "' is static when it has to be an instance method.");
			}
			if (methodInfo.GetParameters().Length != 0)
			{
				throw new ArgumentException("Given method cannot have any parameters.");
			}
			if (typeof(InstanceType).IsValueType)
			{
				throw new ArgumentException("This method does not work with struct instances; please use CreateInstanceRefMethodCaller instead.");
			}
			methodInfo = methodInfo.DeAliasMethod();
			return (Action<InstanceType>)Delegate.CreateDelegate(typeof(Action<InstanceType>), methodInfo);
		}

		/// <summary>
		/// Creates a fast delegate method which calls a given instance method with a given argument on a reference type.
		/// </summary>
		/// <typeparam name="InstanceType">The type of the class which the method is on.</typeparam>
		/// <typeparam name="Arg1">The type of the argument with which to call the method.</typeparam>
		/// <param name="methodInfo">The method info instance which is used.</param>
		/// <returns>A delegate which calls the method and returns the result, except it's hundreds of times faster than MethodInfo.Invoke.</returns>
		public static Action<InstanceType, Arg1> CreateInstanceMethodCaller<InstanceType, Arg1>(MethodInfo methodInfo)
		{
			if (methodInfo == null)
			{
				throw new ArgumentNullException("methodInfo");
			}
			if (methodInfo.IsStatic)
			{
				throw new ArgumentException("Given method '" + methodInfo.Name + "' is static when it has to be an instance method.");
			}
			if (methodInfo.GetParameters().Length != 1)
			{
				throw new ArgumentException("Given method must have only one parameter.");
			}
			if (typeof(InstanceType).IsValueType)
			{
				throw new ArgumentException("This method does not work with struct instances; please use CreateInstanceRefMethodCaller instead.");
			}
			methodInfo = methodInfo.DeAliasMethod();
			return (Action<InstanceType, Arg1>)Delegate.CreateDelegate(typeof(Action<InstanceType, Arg1>), methodInfo);
		}

		/// <summary>
		/// Creates a fast delegate method which calls a given parameterless instance method.
		/// </summary>
		/// <typeparam name="InstanceType">The type of the class which the method is on.</typeparam>
		/// <param name="methodInfo">The method info instance which is used.</param>
		/// <returns>A delegate which calls the method and returns the result, except it's hundreds of times faster than MethodInfo.Invoke.</returns>
		public static InstanceRefMethodCaller<InstanceType> CreateInstanceRefMethodCaller<InstanceType>(MethodInfo methodInfo)
		{
			if (methodInfo == null)
			{
				throw new ArgumentNullException("methodInfo");
			}
			if (methodInfo.IsStatic)
			{
				throw new ArgumentException("Given method '" + methodInfo.Name + "' is static when it has to be an instance method.");
			}
			if (methodInfo.GetParameters().Length != 0)
			{
				throw new ArgumentException("Given method cannot have any parameters.");
			}
			methodInfo = methodInfo.DeAliasMethod();
			if (EmitIsIllegalForMember(methodInfo))
			{
				return delegate(ref InstanceType instance)
				{
					object obj = instance;
					methodInfo.Invoke(obj, null);
					instance = (InstanceType)obj;
				};
			}
			Type declaringType = methodInfo.DeclaringType;
			string methodName = methodInfo.ReflectedType.FullName + ".call_" + methodInfo.Name;
			DynamicMethod method = new DynamicMethod(methodName, typeof(void), new Type[1] { typeof(InstanceType).MakeByRefType() }, restrictedSkipVisibility: true);
			ILGenerator gen = method.GetILGenerator();
			if (declaringType.IsValueType)
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Call, methodInfo);
			}
			else
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldind_Ref);
				gen.Emit(OpCodes.Callvirt, methodInfo);
			}
			gen.Emit(OpCodes.Ret);
			return (InstanceRefMethodCaller<InstanceType>)method.CreateDelegate(typeof(InstanceRefMethodCaller<InstanceType>));
		}

		/// <summary>
		/// Creates a fast delegate method which calls a given instance method with a given argument on a struct type.
		/// </summary>
		/// <typeparam name="InstanceType">The type of the class which the method is on.</typeparam>
		/// <typeparam name="Arg1">The type of the argument with which to call the method.</typeparam>
		/// <param name="methodInfo">The method info instance which is used.</param>
		/// <returns>A delegate which calls the method and returns the result, except it's hundreds of times faster than MethodInfo.Invoke.</returns>
		public static InstanceRefMethodCaller<InstanceType, Arg1> CreateInstanceRefMethodCaller<InstanceType, Arg1>(MethodInfo methodInfo)
		{
			if (methodInfo == null)
			{
				throw new ArgumentNullException("methodInfo");
			}
			if (methodInfo.IsStatic)
			{
				throw new ArgumentException("Given method '" + methodInfo.Name + "' is static when it has to be an instance method.");
			}
			if (methodInfo.GetParameters().Length != 1)
			{
				throw new ArgumentException("Given method must have only one parameter.");
			}
			methodInfo = methodInfo.DeAliasMethod();
			if (EmitIsIllegalForMember(methodInfo))
			{
				return delegate(ref InstanceType instance, Arg1 arg1)
				{
					object obj = instance;
					methodInfo.Invoke(obj, new object[1] { arg1 });
					instance = (InstanceType)obj;
				};
			}
			Type declaringType = methodInfo.DeclaringType;
			string methodName = methodInfo.ReflectedType.FullName + ".call_" + methodInfo.Name;
			DynamicMethod method = new DynamicMethod(methodName, typeof(void), new Type[2]
			{
				typeof(InstanceType).MakeByRefType(),
				typeof(Arg1)
			}, restrictedSkipVisibility: true);
			ILGenerator gen = method.GetILGenerator();
			if (declaringType.IsValueType)
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Call, methodInfo);
			}
			else
			{
				gen.Emit(OpCodes.Ldarg_0);
				gen.Emit(OpCodes.Ldind_Ref);
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Callvirt, methodInfo);
			}
			gen.Emit(OpCodes.Ret);
			return (InstanceRefMethodCaller<InstanceType, Arg1>)method.CreateDelegate(typeof(InstanceRefMethodCaller<InstanceType, Arg1>));
		}
	}
}

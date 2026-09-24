using System;
using System.Reflection;
using System.Reflection.Emit;
using UnityEditor;

namespace Sirenix.Reflection.Editor
{
	public static class SerializedProperty_Internals
	{
		public static readonly bool HasContentHash;

		public static readonly bool HasParent;

		public static readonly bool HasBoxedValue;

		private static readonly Func<SerializedProperty, uint> getContentHash;

		private static readonly Func<SerializedProperty, bool> callParent;

		private static readonly Func<SerializedProperty, object> getBoxedValue;

		private static readonly Action<SerializedProperty, object> setBoxedValue;

		static SerializedProperty_Internals()
		{
			PropertyInfo contentHashProp = typeof(SerializedProperty).GetProperty("contentHash", BindingFlags.Instance | BindingFlags.Public);
			if (contentHashProp != null && contentHashProp.PropertyType == typeof(uint))
			{
				MethodInfo getMethod = contentHashProp.GetGetMethod(nonPublic: true);
				DynamicMethod method = new DynamicMethod("SerializedProperty_Internals_getContentHash", typeof(uint), new Type[1] { typeof(SerializedProperty) }, restrictedSkipVisibility: true);
				ILGenerator il = method.GetILGenerator();
				il.Emit(OpCodes.Ldarg_0);
				if (getMethod.IsVirtual)
				{
					il.Emit(OpCodes.Callvirt, getMethod);
				}
				else
				{
					il.Emit(OpCodes.Call, getMethod);
				}
				il.Emit(OpCodes.Ret);
				getContentHash = (Func<SerializedProperty, uint>)method.CreateDelegate(typeof(Func<SerializedProperty, uint>));
				HasContentHash = true;
			}
			MethodInfo parentMethod = typeof(SerializedProperty).GetMethod("Parent", BindingFlags.Instance | BindingFlags.Public, null, Array.Empty<Type>(), null);
			if (parentMethod != null && parentMethod.ReturnType == typeof(bool))
			{
				DynamicMethod method2 = new DynamicMethod("SerializedProperty_Internals_callParent", typeof(bool), new Type[1] { typeof(SerializedProperty) }, restrictedSkipVisibility: true);
				ILGenerator il2 = method2.GetILGenerator();
				il2.Emit(OpCodes.Ldarg_0);
				if (parentMethod.IsVirtual)
				{
					il2.Emit(OpCodes.Callvirt, parentMethod);
				}
				else
				{
					il2.Emit(OpCodes.Call, parentMethod);
				}
				il2.Emit(OpCodes.Ret);
				callParent = (Func<SerializedProperty, bool>)method2.CreateDelegate(typeof(Func<SerializedProperty, bool>));
				HasParent = true;
			}
			PropertyInfo boxedValueProp = typeof(SerializedProperty).GetProperty("boxedValue", BindingFlags.Instance | BindingFlags.Public);
			if (!(boxedValueProp != null) || !(boxedValueProp.PropertyType == typeof(object)))
			{
				return;
			}
			MethodInfo setMethod = boxedValueProp.GetSetMethod(nonPublic: true);
			MethodInfo getMethod2 = boxedValueProp.GetGetMethod(nonPublic: true);
			if (setMethod != null && getMethod2 != null)
			{
				DynamicMethod method3 = new DynamicMethod("SerializedProperty_Internals_GetBoxedValue", typeof(object), new Type[1] { typeof(SerializedProperty) }, restrictedSkipVisibility: true);
				ILGenerator il3 = method3.GetILGenerator();
				il3.Emit(OpCodes.Ldarg_0);
				if (getMethod2.IsVirtual)
				{
					il3.Emit(OpCodes.Callvirt, getMethod2);
				}
				else
				{
					il3.Emit(OpCodes.Call, getMethod2);
				}
				il3.Emit(OpCodes.Ret);
				getBoxedValue = (Func<SerializedProperty, object>)method3.CreateDelegate(typeof(Func<SerializedProperty, object>));
				DynamicMethod method4 = new DynamicMethod("SerializedProperty_Internals_SetBoxedValue", typeof(void), new Type[2]
				{
					typeof(SerializedProperty),
					typeof(object)
				}, restrictedSkipVisibility: true);
				ILGenerator il4 = method4.GetILGenerator();
				il4.Emit(OpCodes.Ldarg_0);
				il4.Emit(OpCodes.Ldarg_1);
				if (getMethod2.IsVirtual)
				{
					il4.Emit(OpCodes.Callvirt, setMethod);
				}
				else
				{
					il4.Emit(OpCodes.Call, setMethod);
				}
				il4.Emit(OpCodes.Ret);
				setBoxedValue = (Action<SerializedProperty, object>)method4.CreateDelegate(typeof(Action<SerializedProperty, object>));
				HasBoxedValue = true;
			}
		}

		public static bool isValid(SerializedProperty property)
		{
			return property.isValid;
		}

		public static uint contentHash(SerializedProperty property)
		{
			if (getContentHash != null)
			{
				return getContentHash(property);
			}
			return 0u;
		}

		public static object GetBoxedValue(SerializedProperty property)
		{
			if (getBoxedValue != null)
			{
				return getBoxedValue(property);
			}
			return null;
		}

		public static void SetBoxedValue(SerializedProperty property, object obj)
		{
			if (setBoxedValue != null)
			{
				setBoxedValue(property, obj);
			}
		}

		public static bool Parent(SerializedProperty property)
		{
			if (callParent != null)
			{
				return callParent(property);
			}
			return false;
		}
	}
}

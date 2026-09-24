using System;
using System.Reflection;
using System.Reflection.Emit;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class EmitUtilsWIP
	{
		internal static Func<T> CreateConstructorCall<T>(Type type, ConstructorInfo ctor)
		{
			if (type.IsArray)
			{
				throw new ArgumentException();
			}
			if (ctor == null)
			{
				ctor = Sirenix.Utilities.TypeExtensions.FindIdealConstructor(type);
			}
			if (ctor == null)
			{
				throw new InvalidOperationException("No accessible constructor found for type " + type.FullName);
			}
			string methodName = FormatConstructor(ctor);
			DynamicMethod dynamicMethod = new DynamicMethod(methodName, MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, type, Type.EmptyTypes, typeof(EmitUtilities), skipVisibility: true);
			ILGenerator il = dynamicMethod.GetILGenerator();
			ParameterInfo[] parameters = ctor.GetParameters();
			ParameterInfo[] array = parameters;
			foreach (ParameterInfo param in array)
			{
				EmitILForParam(il, param);
			}
			il.Emit(OpCodes.Newobj, ctor);
			il.Emit(OpCodes.Ret);
			return (Func<T>)dynamicMethod.CreateDelegate(typeof(Func<T>));
		}

		internal static Func<object> CreateWeakConstructorCall(Type type, ConstructorInfo ctor)
		{
			return CreateConstructorCall<object>(type, ctor);
		}

		internal static void EmitILForParam(ILGenerator il, ParameterInfo param)
		{
			Type type = param.ParameterType;
			if (param.HasDefaultValue)
			{
				object value = param.DefaultValue;
				if (value == null)
				{
					il.Emit(OpCodes.Ldnull);
					return;
				}
				if (value is string str)
				{
					il.Emit(OpCodes.Ldstr, str);
					return;
				}
				Type valueType = value.GetType();
				if (valueType.IsEnum)
				{
					EmitEnumValue(il, valueType, value);
				}
				else if (type.IsPrimitive)
				{
					switch (Type.GetTypeCode(type))
					{
					case TypeCode.Boolean:
						il.Emit(((bool)value) ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
						break;
					case TypeCode.SByte:
					case TypeCode.Byte:
					case TypeCode.Int16:
					case TypeCode.UInt16:
					case TypeCode.Int32:
					case TypeCode.UInt32:
						il.Emit(OpCodes.Ldc_I4, Convert.ToInt32(value));
						break;
					case TypeCode.Int64:
					case TypeCode.UInt64:
						il.Emit(OpCodes.Ldc_I8, Convert.ToInt64(value));
						break;
					case TypeCode.Char:
						il.Emit(OpCodes.Ldc_I4, (char)value);
						break;
					case TypeCode.Single:
						il.Emit(OpCodes.Ldc_R4, (float)value);
						break;
					case TypeCode.Double:
						il.Emit(OpCodes.Ldc_R8, (double)value);
						break;
					}
					if (type == typeof(object))
					{
						il.Emit(OpCodes.Box, valueType);
					}
				}
			}
			else if (type.IsValueType)
			{
				if (type.IsPrimitive)
				{
					if (type == typeof(int) || type == typeof(uint) || type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort) || type == typeof(char) || type == typeof(bool))
					{
						il.Emit(OpCodes.Ldc_I4_0);
					}
					else if (type == typeof(long) || type == typeof(ulong))
					{
						il.Emit(OpCodes.Ldc_I8, 0L);
					}
					else if (type == typeof(float))
					{
						il.Emit(OpCodes.Ldc_R4, 0f);
					}
					else if (type == typeof(double))
					{
						il.Emit(OpCodes.Ldc_R8, 0.0);
					}
				}
				else if (type.IsEnum)
				{
					EmitEnumValue(il, type, 0);
				}
				else
				{
					LocalBuilder local = il.DeclareLocal(type);
					il.Emit(OpCodes.Ldloca_S, local);
					il.Emit(OpCodes.Initobj, type);
					il.Emit(OpCodes.Ldloc, local);
				}
			}
			else
			{
				il.Emit(OpCodes.Ldnull);
			}
		}

		internal static void EmitEnumValue(ILGenerator il, Type enumType, object value)
		{
			Type underlyingType = Enum.GetUnderlyingType(enumType);
			switch (Type.GetTypeCode(underlyingType))
			{
			case TypeCode.SByte:
			case TypeCode.Byte:
			case TypeCode.Int16:
			case TypeCode.UInt16:
			case TypeCode.Int32:
			case TypeCode.UInt32:
				il.Emit(OpCodes.Ldc_I4, Convert.ToInt32(value));
				break;
			case TypeCode.Int64:
			case TypeCode.UInt64:
				il.Emit(OpCodes.Ldc_I8, Convert.ToInt64(value));
				break;
			default:
				throw new NotSupportedException($"Unsupported enum underlying type: '{underlyingType}' for '{enumType}'");
			}
		}

		private static string FormatConstructor(ConstructorInfo ctor)
		{
			Type declaringType = ctor.DeclaringType;
			string typeName = ((declaringType.Assembly == typeof(object).Assembly) ? declaringType.FullName : ("[" + declaringType.Assembly.GetName().Name + "]" + declaringType.FullName));
			string paramString = "";
			ParameterInfo[] parameters = ctor.GetParameters();
			for (int i = 0; i < parameters.Length; i++)
			{
				if (i > 0)
				{
					paramString += ", ";
				}
				paramString += parameters[i].ParameterType.GetNiceName();
			}
			return typeName + "::.ctor(" + paramString + ")";
		}
	}
}

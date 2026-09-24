using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Sirenix.Serialization;
using Sirenix.Serialization.Utilities;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	public static class FastDeepCopier
	{
		public delegate void DeepCopierNoPolymorphism<T>(ref T from, ref T to, Dictionary<object, object> references);

		public delegate void DeepCopierWeakNoPolymorphism(object from, object to, Dictionary<object, object> references);

		public static class Accelerator<T>
		{
			private static readonly Type TypeOf_T = typeof(T);

			private static readonly bool CopyByRef = (object)TypeOf_T == TypeOf_String || typeof(UnityEngine.Object).IsAssignableFrom(TypeOf_T) || typeof(MemberInfo).IsAssignableFrom(TypeOf_T) || typeof(Module).IsAssignableFrom(TypeOf_T) || typeof(Assembly).IsAssignableFrom(TypeOf_T) || (object)TypeOf_T == TypeOf_InspectorProperty || typeof(PropertyTree).IsAssignableFrom(TypeOf_T);

			private static readonly bool IsObjectOrInterface = (object)TypeOf_T == TypeOf_Object || TypeOf_T.IsInterface;

			public static T DeepCopyWithManualReferences(T instance, Dictionary<object, object> references)
			{
				if (CopyByRef || instance == null)
				{
					return instance;
				}
				Type instanceType = instance.GetType();
				bool isPreciseType = (object)instanceType == TypeOf_T;
				if (!isPreciseType && IsObjectOrInterface && ((object)instanceType == TypeOf_String || instance is UnityEngine.Object || instance is MemberInfo || instance is Module || instance is Assembly || (object)instanceType == TypeOf_InspectorProperty || instance is PropertyTree))
				{
					return instance;
				}
				if (references != null && references.TryGetValue(instance, out var existingCopy))
				{
					return (T)existingCopy;
				}
				T copy;
				if (instance is Array array)
				{
					int rank = instanceType.GetArrayRank();
					if (rank == 1)
					{
						Array copyArr = Array.CreateInstance(instanceType.GetElementType(), array.Length);
						Type elementType = instanceType.GetElementType();
						copy = (T)(object)copyArr;
						if (elementType.IsPrimitive)
						{
							Buffer.BlockCopy(array, 0, copyArr, 0, array.Length * Marshal.SizeOf(elementType));
							return copy;
						}
					}
					else
					{
						int[] ranks = new int[rank];
						for (int i = 0; i < rank; i++)
						{
							ranks[i] = array.GetLength(i);
						}
						copy = (T)(object)Array.CreateInstance(instanceType.GetElementType(), ranks);
					}
				}
				else
				{
					copy = (T)FormatterServices.GetUninitializedObject(instanceType);
				}
				references?.Add(instance, copy);
				if (!isPreciseType)
				{
					DeepCopierWeakNoPolymorphism weakCopier = GetDeepCopierWeakNoPolymorphism(instanceType);
					object weakCopy = copy;
					weakCopier(instance, weakCopy, references);
					copy = (T)weakCopy;
				}
				else
				{
					DeepCopierNoPolymorphism<T> copier = GetDeepCopierNoPolymorphism<T>();
					copier(ref instance, ref copy, references);
				}
				return copy;
			}
		}

		private static readonly object LOCK = new object();

		private static readonly Dictionary<Type, DeepCopierWeakNoPolymorphism> DeepWeakCopiers = new Dictionary<Type, DeepCopierWeakNoPolymorphism>(Sirenix.Serialization.Utilities.FastTypeComparer.Instance);

		private static readonly Dictionary<Type, Delegate> DeepCopiers = new Dictionary<Type, Delegate>(Sirenix.Serialization.Utilities.FastTypeComparer.Instance);

		private static readonly Type[] CopyFromToArgSignature = new Type[3]
		{
			typeof(object),
			typeof(object),
			typeof(Dictionary<object, object>)
		};

		private static readonly MethodInfo Array_GetLength = typeof(Array).GetMethod("GetLength", BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(int) }, null);

		private static readonly MethodInfo Buffer_BlockCopy = typeof(Buffer).GetMethod("BlockCopy", BindingFlags.Static | BindingFlags.Public, null, new Type[5]
		{
			typeof(Array),
			typeof(int),
			typeof(Array),
			typeof(int),
			typeof(int)
		}, null);

		private static readonly MethodInfo FastDeepCopier_CopyMultiDimensionalArray = typeof(FastDeepCopier).GetMethod("CopyMultiDimensionalArray", BindingFlags.Static | BindingFlags.Public);

		private static readonly Action<ILGenerator> EmitLdArg0 = delegate(ILGenerator il)
		{
			il.Emit(OpCodes.Ldarg_0);
		};

		private static readonly Action<ILGenerator> EmitLdArg1 = delegate(ILGenerator il)
		{
			il.Emit(OpCodes.Ldarg_1);
		};

		private static readonly Action<ILGenerator> EmitLdArg0IndRef = delegate(ILGenerator il)
		{
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldind_Ref);
		};

		private static readonly Action<ILGenerator> EmitLdArg1IndRef = delegate(ILGenerator il)
		{
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldind_Ref);
		};

		private static int emitNameIncrement;

		private static object cachedReferenceDicts_LOCK = new object();

		private static readonly Dictionary<object, object>[] cachedReferenceDicts = new Dictionary<object, object>[4];

		private static readonly Type TypeOf_String = typeof(string);

		private static readonly Type TypeOf_Object = typeof(object);

		private static readonly Type TypeOf_InspectorProperty = typeof(InspectorProperty);

		public static Dictionary<object, object> ClaimCachedReferenceDict()
		{
			lock (cachedReferenceDicts_LOCK)
			{
				Dictionary<object, object>[] array = cachedReferenceDicts;
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] != null)
					{
						Dictionary<object, object> result = array[i];
						array[i] = null;
						result.Clear();
						return result;
					}
				}
				return new Dictionary<object, object>(ReferenceEqualityComparer<object>.Default);
			}
		}

		public static void ReleaseCachedReferenceDict(Dictionary<object, object> dict)
		{
			dict.Clear();
			lock (cachedReferenceDicts_LOCK)
			{
				Dictionary<object, object>[] array = cachedReferenceDicts;
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] == null)
					{
						array[i] = dict;
						break;
					}
				}
			}
		}

		private static bool ShouldCopyTypeByRef(Type type)
		{
			if ((object)type != TypeOf_String && !typeof(UnityEngine.Object).IsAssignableFrom(type) && !typeof(MemberInfo).IsAssignableFrom(type) && !typeof(Module).IsAssignableFrom(type) && !typeof(Assembly).IsAssignableFrom(type) && (object)type != TypeOf_InspectorProperty)
			{
				return typeof(PropertyTree).IsAssignableFrom(type);
			}
			return true;
		}

		public static T DeepCopy<T>(T instance, bool referenceTracking = true)
		{
			Dictionary<object, object> references = (referenceTracking ? ClaimCachedReferenceDict() : null);
			try
			{
				return Accelerator<T>.DeepCopyWithManualReferences(instance, references);
			}
			finally
			{
				if (references != null)
				{
					ReleaseCachedReferenceDict(references);
				}
			}
		}

		public static T DeepCopyWithManualReferences<T>(T instance, Dictionary<object, object> references)
		{
			return Accelerator<T>.DeepCopyWithManualReferences(instance, references);
		}

		public static void DeepCopyFromToStruct<T>(ref T from, ref T to, bool referenceTracking = true) where T : struct
		{
			Dictionary<object, object> references = (referenceTracking ? ClaimCachedReferenceDict() : null);
			try
			{
				DeepCopyFromToStructWithManualReferences(ref from, ref to, references);
			}
			finally
			{
				if (references != null)
				{
					ReleaseCachedReferenceDict(references);
				}
			}
		}

		public static void DeepCopyFromToStructWithManualReferences<T>(ref T from, ref T to, Dictionary<object, object> references) where T : struct
		{
			DeepCopierNoPolymorphism<T> copier = GetDeepCopierNoPolymorphism<T>();
			copier(ref from, ref to, references);
		}

		public static void DeepCopyFromToClass<T>(T from, T to, bool referenceTracking = true) where T : class
		{
			Dictionary<object, object> references = (referenceTracking ? ClaimCachedReferenceDict() : null);
			try
			{
				DeepCopyFromToClassWithManualReferences(from, to, references);
			}
			finally
			{
				if (references != null)
				{
					ReleaseCachedReferenceDict(references);
				}
			}
		}

		public static void DeepCopyFromToClassWithManualReferences<T>(T from, T to, Dictionary<object, object> references) where T : class
		{
			if (from == null || to == null)
			{
				throw new ArgumentException("Object to copy from or to cannot be null.");
			}
			if (from != to)
			{
				Type type = from.GetType();
				if ((object)type != to.GetType())
				{
					throw new ArgumentException("Object to copy from and to must be the same type; from was '" + from.GetType().GetNiceName() + "' and to was '" + from.GetType().GetNiceName() + "'.");
				}
				if ((object)type != typeof(T))
				{
					DeepCopierWeakNoPolymorphism weakCopier = GetDeepCopierWeakNoPolymorphism(type);
					weakCopier(from, to, references);
				}
				else
				{
					DeepCopierNoPolymorphism<T> copier = GetDeepCopierNoPolymorphism<T>();
					copier(ref from, ref to, references);
				}
			}
		}

		public static DeepCopierNoPolymorphism<T> GetDeepCopierNoPolymorphism<T>()
		{
			Type type = typeof(T);
			Delegate deepCopierDelegate;
			lock (LOCK)
			{
				if (!DeepCopiers.TryGetValue(type, out deepCopierDelegate))
				{
					DynamicMethod deepCopyMethod = new DynamicMethod("DeepCopy_" + type.GetNiceFullName() + "_" + emitNameIncrement++, typeof(void), new Type[3]
					{
						typeof(T).MakeByRefType(),
						typeof(T).MakeByRefType(),
						typeof(Dictionary<object, object>)
					}, restrictedSkipVisibility: true);
					ILGenerator il = deepCopyMethod.GetILGenerator();
					bool isValueType = type.IsValueType;
					EmitTypeCopyingLogic(type, il, isValueType ? EmitLdArg0 : EmitLdArg0IndRef, isValueType ? EmitLdArg1 : EmitLdArg1IndRef, isValueType);
					il.Emit(OpCodes.Ret);
					deepCopierDelegate = deepCopyMethod.CreateDelegate(typeof(DeepCopierNoPolymorphism<T>));
					DeepCopiers.Add(type, deepCopierDelegate);
				}
			}
			return deepCopierDelegate as DeepCopierNoPolymorphism<T>;
		}

		public static DeepCopierWeakNoPolymorphism GetDeepCopierWeakNoPolymorphism(Type type)
		{
			DeepCopierWeakNoPolymorphism deepCopier;
			lock (LOCK)
			{
				if (!DeepWeakCopiers.TryGetValue(type, out deepCopier))
				{
					DynamicMethod deepCopyMethodBuilder = new DynamicMethod("DeepCopyWeak_" + type.GetNiceFullName() + "_" + emitNameIncrement++, typeof(void), CopyFromToArgSignature, restrictedSkipVisibility: true);
					ILGenerator il = deepCopyMethodBuilder.GetILGenerator();
					if (type.IsValueType)
					{
						EmitTypeCopyingLogic(type, il, CreateLdArgUnboxGenerator(OpCodes.Ldarg_0, type), CreateLdArgUnboxGenerator(OpCodes.Ldarg_1, type), isValueType: true);
					}
					else
					{
						LocalBuilder from = il.DeclareLocal(type);
						LocalBuilder to = il.DeclareLocal(type);
						il.Emit(OpCodes.Ldarg_0);
						il.Emit(OpCodes.Castclass, type);
						il.Emit(OpCodes.Stloc, from);
						il.Emit(OpCodes.Ldarg_1);
						il.Emit(OpCodes.Castclass, type);
						il.Emit(OpCodes.Stloc, to);
						EmitTypeCopyingLogic(type, il, CreateLdLocGenerator(from), CreateLdLocGenerator(to), isValueType: true);
					}
					il.Emit(OpCodes.Ret);
					deepCopier = (DeepCopierWeakNoPolymorphism)deepCopyMethodBuilder.CreateDelegate(typeof(DeepCopierWeakNoPolymorphism));
					DeepWeakCopiers.Add(type, deepCopier);
				}
			}
			return deepCopier;
		}

		private static Action<ILGenerator> CreateLdLocGenerator(LocalBuilder local)
		{
			return delegate(ILGenerator il)
			{
				il.Emit(OpCodes.Ldloc, local);
			};
		}

		private static Action<ILGenerator> CreateLdArgUnboxGenerator(OpCode ldArg, Type type)
		{
			return delegate(ILGenerator il)
			{
				il.Emit(ldArg);
				il.Emit(OpCodes.Unbox, type);
			};
		}

		private static void EmitTypeCopyingLogic(Type type, ILGenerator il, Action<ILGenerator> emitLoadCopyFrom, Action<ILGenerator> emitLoadCopyTo, bool isValueType)
		{
			if (type.IsArray)
			{
				EmitArrayCopyingLogic(type, type.GetElementType(), il, emitLoadCopyFrom, emitLoadCopyTo);
			}
			else
			{
				EmitFieldCopyingLogic(type, il, emitLoadCopyFrom, emitLoadCopyTo, isValueType);
			}
		}

		private static void EmitArrayCreationLogic(Type arrayType, Type elementType, ILGenerator il, Action<ILGenerator> emitLoadCopyFrom)
		{
			int rank = arrayType.GetArrayRank();
			if (rank == 1)
			{
				emitLoadCopyFrom(il);
				il.Emit(OpCodes.Ldlen);
				il.Emit(OpCodes.Newarr, elementType);
				return;
			}
			Type[] constructorSignature = new Type[rank];
			for (int i = 0; i < rank; i++)
			{
				constructorSignature[i] = typeof(int);
			}
			ConstructorInfo ctor = arrayType.GetConstructor(constructorSignature);
			for (int j = 0; j < rank; j++)
			{
				emitLoadCopyFrom(il);
				il.Emit(OpCodes.Ldc_I4, j);
				il.Emit(OpCodes.Callvirt, Array_GetLength);
			}
			il.Emit(OpCodes.Newobj, ctor);
		}

		public static void CopyMultiDimensionalArray<T>(Array from, Array to, Dictionary<object, object> references)
		{
			Type arrayType = from.GetType();
			Type elementType = typeof(T);
			Func<T, Dictionary<object, object>, T> copier = null;
			if (elementType.IsValueType)
			{
				if (!StructCanBeCopiedDirectly(elementType))
				{
					DeepCopierNoPolymorphism<T> structCopier = GetDeepCopierNoPolymorphism<T>();
					copier = delegate(T val, Dictionary<object, object> refs)
					{
						T from2 = val;
						T to2 = default(T);
						structCopier(ref from2, ref to2, refs);
						return to2;
					};
				}
			}
			else
			{
				copier = DeepCopyWithManualReferences;
			}
			int ranks = arrayType.GetArrayRank();
			long[] rankSizes = new long[ranks];
			long[] indices = new long[ranks];
			for (int i = 0; i < ranks; i++)
			{
				rankSizes[i] = from.GetLength(i);
			}
			while (true)
			{
				bool allIndicesDoneIterating = true;
				for (int i2 = 0; i2 < indices.Length; i2++)
				{
					if (indices[i2] != rankSizes[i2] - 1)
					{
						allIndicesDoneIterating = false;
						break;
					}
				}
				T value = (T)from.GetValue(indices);
				to.SetValue((copier != null) ? copier(value, references) : value, indices);
				if (!allIndicesDoneIterating)
				{
					int i3 = indices.Length - 1;
					while (i3 >= 0 && ++indices[i3] >= rankSizes[i3])
					{
						indices[i3] = 0L;
						i3--;
					}
					continue;
				}
				break;
			}
		}

		private static void EmitArrayCopyingLogic(Type arrayType, Type elementType, ILGenerator il, Action<ILGenerator> loadFrom, Action<ILGenerator> loadTo)
		{
			if (arrayType.GetArrayRank() != 1)
			{
				MethodInfo copyMethod = FastDeepCopier_CopyMultiDimensionalArray.MakeGenericMethod(elementType);
				loadFrom(il);
				loadTo(il);
				il.Emit(OpCodes.Ldarg_2);
				il.Emit(OpCodes.Call, copyMethod);
				return;
			}
			if (elementType.IsPrimitive)
			{
				MethodInfo getType = typeof(object).GetMethod("GetType");
				loadFrom(il);
				il.Emit(OpCodes.Ldc_I4_0);
				loadTo(il);
				il.Emit(OpCodes.Ldc_I4_0);
				loadFrom(il);
				il.Emit(OpCodes.Ldlen);
				il.Emit(OpCodes.Ldc_I4, Marshal.SizeOf(elementType));
				il.Emit(OpCodes.Mul);
				il.Emit(OpCodes.Call, Buffer_BlockCopy);
				return;
			}
			LocalBuilder iLocal = il.DeclareLocal(typeof(int));
			Label loopLengthCheck = il.DefineLabel();
			Label loopBody = il.DefineLabel();
			il.Emit(OpCodes.Br, loopLengthCheck);
			il.Emit(OpCodes.Ldc_I4_0);
			il.Emit(OpCodes.Stloc, iLocal);
			il.MarkLabel(loopBody);
			if (elementType.IsValueType)
			{
				if (StructCanBeCopiedDirectly(elementType))
				{
					loadTo(il);
					il.Emit(OpCodes.Ldloc, iLocal);
					loadFrom(il);
					il.Emit(OpCodes.Ldloc, iLocal);
					il.Emit(OpCodes.Ldelem, elementType);
					il.Emit(OpCodes.Stelem, elementType);
				}
				else
				{
					EmitFieldCopyingLogic(elementType, il, delegate(ILGenerator iLGenerator)
					{
						loadFrom(iLGenerator);
						iLGenerator.Emit(OpCodes.Ldloc, iLocal);
						iLGenerator.Emit(OpCodes.Ldelema, elementType);
					}, delegate(ILGenerator iLGenerator)
					{
						loadTo(iLGenerator);
						iLGenerator.Emit(OpCodes.Ldloc, iLocal);
						iLGenerator.Emit(OpCodes.Ldelema, elementType);
					}, isValueType: true);
				}
			}
			else
			{
				MethodInfo deepCopyElementMethod = typeof(Accelerator<>).MakeGenericType(elementType).GetMethod("DeepCopyWithManualReferences", BindingFlags.Static | BindingFlags.Public);
				loadTo(il);
				il.Emit(OpCodes.Ldloc, iLocal);
				loadFrom(il);
				il.Emit(OpCodes.Ldloc, iLocal);
				il.Emit(OpCodes.Ldelem, elementType);
				il.Emit(OpCodes.Ldarg_2);
				il.Emit(OpCodes.Call, deepCopyElementMethod);
				il.Emit(OpCodes.Stelem, elementType);
			}
			il.Emit(OpCodes.Ldloc, iLocal);
			il.Emit(OpCodes.Ldc_I4_1);
			il.Emit(OpCodes.Add);
			il.Emit(OpCodes.Stloc, iLocal);
			il.MarkLabel(loopLengthCheck);
			il.Emit(OpCodes.Ldloc, iLocal);
			loadFrom(il);
			il.Emit(OpCodes.Ldlen);
			il.Emit(OpCodes.Conv_I4);
			il.Emit(OpCodes.Clt);
			il.Emit(OpCodes.Brtrue, loopBody);
		}

		private static void EmitFieldCopyingLogic(Type type, ILGenerator il, Action<ILGenerator> loadFrom, Action<ILGenerator> loadTo, bool isValueType)
		{
			MemberInfo[] fields = FormatterUtilities.GetSerializableMembers(type, SerializationPolicies.Everything);
			for (int i = 0; i < fields.Length; i++)
			{
				FieldInfo field = fields[i] as FieldInfo;
				if (field == null)
				{
					throw new Exception("SerializationPolicies.Everything is serializing a non-field member '" + fields[i].Name + "' on type '" + type.GetNiceName() + "'");
				}
				if (field.DeclaringType == typeof(UnityEngine.Object))
				{
					continue;
				}
				Type fieldType = field.FieldType;
				if (FormatterUtilities.IsPrimitiveType(fieldType))
				{
					loadTo(il);
					loadFrom(il);
					il.Emit(OpCodes.Ldfld, field);
					il.Emit(OpCodes.Stfld, field);
				}
				else if (fieldType.IsValueType)
				{
					if (StructCanBeCopiedDirectly(fieldType))
					{
						loadTo(il);
						loadFrom(il);
						il.Emit(OpCodes.Ldfld, field);
						il.Emit(OpCodes.Stfld, field);
						continue;
					}
					EmitFieldCopyingLogic(fieldType, il, delegate(ILGenerator iLGenerator)
					{
						loadFrom(iLGenerator);
						iLGenerator.Emit(OpCodes.Ldflda, field);
					}, delegate(ILGenerator iLGenerator)
					{
						loadTo(iLGenerator);
						iLGenerator.Emit(OpCodes.Ldflda, field);
					}, isValueType: true);
				}
				else if (fieldType.IsArray && fieldType.GetArrayRank() == 1)
				{
					Type elementType = fieldType.GetElementType();
					Label notNullCase = il.DefineLabel();
					Label done = il.DefineLabel();
					loadFrom(il);
					il.Emit(OpCodes.Ldfld, field);
					il.Emit(OpCodes.Ldnull);
					il.Emit(OpCodes.Ceq);
					il.Emit(OpCodes.Brfalse, notNullCase);
					loadTo(il);
					il.Emit(OpCodes.Ldnull);
					il.Emit(OpCodes.Stfld, field);
					il.Emit(OpCodes.Br, done);
					il.MarkLabel(notNullCase);
					Action<ILGenerator> loadArray = delegate(ILGenerator iLGenerator)
					{
						loadFrom(iLGenerator);
						iLGenerator.Emit(OpCodes.Ldfld, field);
					};
					loadTo(il);
					EmitArrayCreationLogic(fieldType, elementType, il, loadArray);
					il.Emit(OpCodes.Stfld, field);
					EmitArrayCopyingLogic(fieldType, elementType, il, loadArray, delegate(ILGenerator iLGenerator)
					{
						loadTo(iLGenerator);
						iLGenerator.Emit(OpCodes.Ldfld, field);
					});
					il.MarkLabel(done);
				}
				else
				{
					MethodInfo deepCopyMethod = typeof(Accelerator<>).MakeGenericType(fieldType).GetMethod("DeepCopyWithManualReferences", BindingFlags.Static | BindingFlags.Public);
					loadTo(il);
					loadFrom(il);
					il.Emit(OpCodes.Ldfld, field);
					il.Emit(OpCodes.Ldarg_2);
					il.Emit(OpCodes.Call, deepCopyMethod);
					il.Emit(OpCodes.Stfld, field);
				}
			}
		}

		private static bool StructCanBeCopiedDirectly(Type type)
		{
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				Type fieldType = fields[i].FieldType;
				if (!FormatterUtilities.IsPrimitiveType(fieldType))
				{
					if (!fieldType.IsValueType)
					{
						return false;
					}
					if (!StructCanBeCopiedDirectly(fieldType))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}

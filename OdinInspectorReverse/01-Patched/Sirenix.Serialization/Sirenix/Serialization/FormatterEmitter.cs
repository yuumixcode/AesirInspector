using System;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Utility class for emitting formatters using the <see cref="N:System.Reflection.Emit" /> namespace.
	/// <para />
	/// NOTE: Some platforms do not support emitting. Check whether you can emit on the current platform using <see cref="P:Sirenix.Serialization.Utilities.EmitUtilities.CanEmit" />.
	/// </summary>
	public static class FormatterEmitter
	{
		/// <summary>
		/// Base type for all AOT-emitted formatters.
		/// </summary>
		[EmittedFormatter]
		public abstract class AOTEmittedFormatter<T> : EasyBaseFormatter<T>
		{
		}

		/// <summary>
		/// Shortcut class that makes it easier to emit empty AOT formatters.
		/// </summary>
		public abstract class EmptyAOTEmittedFormatter<T> : AOTEmittedFormatter<T>
		{
			/// <summary>
			/// Skips the entry to read.
			/// </summary>
			protected override void ReadDataEntry(ref T value, string entryName, EntryType entryType, IDataReader reader)
			{
				reader.SkipEntry();
			}

			/// <summary>
			/// Does nothing at all.
			/// </summary>
			protected override void WriteDataEntries(ref T value, IDataWriter writer)
			{
			}
		}

		public delegate void ReadDataEntryMethodDelegate<T>(ref T value, string entryName, EntryType entryType, IDataReader reader);

		public delegate void WriteDataEntriesMethodDelegate<T>(ref T value, IDataWriter writer);

		[EmittedFormatter]
		public sealed class RuntimeEmittedFormatter<T> : EasyBaseFormatter<T>
		{
			public readonly ReadDataEntryMethodDelegate<T> Read;

			public readonly WriteDataEntriesMethodDelegate<T> Write;

			public RuntimeEmittedFormatter(ReadDataEntryMethodDelegate<T> read, WriteDataEntriesMethodDelegate<T> write)
			{
				Read = read;
				Write = write;
			}

			protected override void ReadDataEntry(ref T value, string entryName, EntryType entryType, IDataReader reader)
			{
				Read(ref value, entryName, entryType, reader);
			}

			protected override void WriteDataEntries(ref T value, IDataWriter writer)
			{
				Write(ref value, writer);
			}
		}

		/// <summary>
		/// Used for generating unique formatter helper type names.
		/// </summary>
		private static int helperFormatterNameId;

		/// <summary>
		/// The name of the pre-generated assembly that contains pre-emitted formatters for use on AOT platforms where emitting is not supported. Note that this assembly is not always present.
		/// </summary>
		public const string PRE_EMITTED_ASSEMBLY_NAME = "Sirenix.Serialization.AOTGenerated";

		/// <summary>
		/// The name of the runtime-generated assembly that contains runtime-emitted formatters for use on non-AOT platforms where emitting is supported. Note that this assembly is not always present.
		/// </summary>
		public const string RUNTIME_EMITTED_ASSEMBLY_NAME = "Sirenix.Serialization.RuntimeEmitted";

		private static readonly object LOCK = new object();

		private static readonly DoubleLookupDictionary<ISerializationPolicy, Type, IFormatter> Formatters = new DoubleLookupDictionary<ISerializationPolicy, Type, IFormatter>();

		private static AssemblyBuilder runtimeEmittedAssembly;

		private static ModuleBuilder runtimeEmittedModule;

		/// <summary>
		/// Gets an emitted formatter for a given type.
		/// <para />
		/// NOTE: Some platforms do not support emitting. On such platforms, this method logs an error and returns null. Check whether you can emit on the current platform using <see cref="P:Sirenix.Serialization.Utilities.EmitUtilities.CanEmit" />.
		/// </summary>
		/// <param name="type">The type to emit a formatter for.</param>
		/// <param name="policy">The serialization policy to use to determine which members the emitted formatter should serialize. If null, <see cref="P:Sirenix.Serialization.SerializationPolicies.Strict" /> is used.</param>
		/// <returns>The type of the emitted formatter.</returns>
		/// <exception cref="T:System.ArgumentNullException">The type argument is null.</exception>
		public static IFormatter GetEmittedFormatter(Type type, ISerializationPolicy policy)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (policy == null)
			{
				policy = SerializationPolicies.Strict;
			}
			IFormatter result = null;
			if (!Formatters.TryGetInnerValue(policy, type, out result))
			{
				lock (LOCK)
				{
					if (!Formatters.TryGetInnerValue(policy, type, out result))
					{
						EnsureRuntimeAssembly();
						try
						{
							result = CreateGenericFormatter(type, runtimeEmittedModule, policy);
						}
						catch (Exception exception)
						{
							Debug.LogError("The following error occurred while emitting a formatter for the type " + type.Name);
							Debug.LogException(exception);
						}
						Formatters.AddInner(policy, type, result);
					}
				}
			}
			return result;
		}

		private static void EnsureRuntimeAssembly()
		{
			if (runtimeEmittedAssembly == null)
			{
				AssemblyName assemblyName = new AssemblyName("Sirenix.Serialization.RuntimeEmitted");
				assemblyName.CultureInfo = CultureInfo.InvariantCulture;
				assemblyName.Flags = AssemblyNameFlags.None;
				assemblyName.ProcessorArchitecture = ProcessorArchitecture.MSIL;
				assemblyName.VersionCompatibility = AssemblyVersionCompatibility.SameDomain;
				runtimeEmittedAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			}
			if (runtimeEmittedModule == null)
			{
				bool emitSymbolInfo = true;
				runtimeEmittedModule = runtimeEmittedAssembly.DefineDynamicModule("Sirenix.Serialization.RuntimeEmitted", emitSymbolInfo);
			}
		}

		/// <summary>
		/// Emits a formatter for a given type into a given module builder, using a given serialization policy to determine which members to serialize.
		/// </summary>
		/// <param name="formattedType">Type to create a formatter for.</param>
		/// <param name="moduleBuilder">The module builder to emit a formatter into.</param>
		/// <param name="policy">The serialization policy to use for creating the formatter.</param>
		/// <returns>The fully constructed, emitted formatter type.</returns>
		public static Type EmitAOTFormatter(Type formattedType, ModuleBuilder moduleBuilder, ISerializationPolicy policy)
		{
			Dictionary<string, MemberInfo> serializableMembers = FormatterUtilities.GetSerializableMembersMap(formattedType, policy);
			string formatterName = moduleBuilder.Name + "." + formattedType.GetCompilableNiceFullName() + "__AOTFormatter";
			string formatterHelperName = moduleBuilder.Name + "." + formattedType.GetCompilableNiceFullName() + "__FormatterHelper";
			if (serializableMembers.Count == 0)
			{
				return moduleBuilder.DefineType(formatterName, TypeAttributes.Public | TypeAttributes.Sealed, typeof(EmptyAOTEmittedFormatter<>).MakeGenericType(formattedType)).CreateType();
			}
			BuildHelperType(moduleBuilder, formatterHelperName, formattedType, serializableMembers, out var serializerReadMethods, out var serializerWriteMethods, out var serializerFields, out var dictField, out var memberNames);
			TypeBuilder formatterType = moduleBuilder.DefineType(formatterName, TypeAttributes.Public | TypeAttributes.Sealed, typeof(AOTEmittedFormatter<>).MakeGenericType(formattedType));
			MethodInfo readBaseMethod = formatterType.BaseType.GetMethod("ReadDataEntry", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			MethodBuilder readMethod = formatterType.DefineMethod(readBaseMethod.Name, MethodAttributes.Family | MethodAttributes.Virtual, readBaseMethod.ReturnType, (from n in readBaseMethod.GetParameters()
				select n.ParameterType).ToArray());
			readBaseMethod.GetParameters().ForEach(delegate(ParameterInfo n)
			{
				readMethod.DefineParameter(n.Position, n.Attributes, n.Name);
			});
			EmitReadMethodContents(readMethod.GetILGenerator(), formattedType, dictField, serializerFields, memberNames, serializerReadMethods);
			MethodInfo writeBaseMethod = formatterType.BaseType.GetMethod("WriteDataEntries", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			MethodBuilder dynamicWriteMethod = formatterType.DefineMethod(writeBaseMethod.Name, MethodAttributes.Family | MethodAttributes.Virtual, writeBaseMethod.ReturnType, (from n in writeBaseMethod.GetParameters()
				select n.ParameterType).ToArray());
			writeBaseMethod.GetParameters().ForEach(delegate(ParameterInfo n)
			{
				dynamicWriteMethod.DefineParameter(n.Position + 1, n.Attributes, n.Name);
			});
			EmitWriteMethodContents(dynamicWriteMethod.GetILGenerator(), formattedType, serializerFields, memberNames, serializerWriteMethods);
			Type result = formatterType.CreateType();
			((AssemblyBuilder)moduleBuilder.Assembly).SetCustomAttribute(new CustomAttributeBuilder(typeof(RegisterFormatterAttribute).GetConstructor(new Type[2]
			{
				typeof(Type),
				typeof(int)
			}), new object[2] { formatterType, -1 }));
			return result;
		}

		private static IFormatter CreateGenericFormatter(Type formattedType, ModuleBuilder moduleBuilder, ISerializationPolicy policy)
		{
			Dictionary<string, MemberInfo> serializableMembers = FormatterUtilities.GetSerializableMembersMap(formattedType, policy);
			if (serializableMembers.Count == 0)
			{
				return (IFormatter)Activator.CreateInstance(typeof(EmptyTypeFormatter<>).MakeGenericType(formattedType));
			}
			string helperTypeName = moduleBuilder.Name + "." + formattedType.GetCompilableNiceFullName() + "___" + formattedType.Assembly.GetName().Name + "___FormatterHelper___" + Interlocked.Increment(ref helperFormatterNameId);
			BuildHelperType(moduleBuilder, helperTypeName, formattedType, serializableMembers, out var serializerReadMethods, out var serializerWriteMethods, out var serializerFields, out var dictField, out var memberNames);
			Type formatterType = typeof(RuntimeEmittedFormatter<>).MakeGenericType(formattedType);
			Type readDelegateType = typeof(ReadDataEntryMethodDelegate<>).MakeGenericType(formattedType);
			MethodInfo readDataEntryMethod = formatterType.GetMethod("ReadDataEntry", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			DynamicMethod dynamicReadMethod = new DynamicMethod("Dynamic_" + formattedType.GetCompilableNiceFullName(), null, (from n in readDataEntryMethod.GetParameters()
				select n.ParameterType).ToArray(), restrictedSkipVisibility: true);
			readDataEntryMethod.GetParameters().ForEach(delegate(ParameterInfo n)
			{
				dynamicReadMethod.DefineParameter(n.Position, n.Attributes, n.Name);
			});
			EmitReadMethodContents(dynamicReadMethod.GetILGenerator(), formattedType, dictField, serializerFields, memberNames, serializerReadMethods);
			Delegate del1 = dynamicReadMethod.CreateDelegate(readDelegateType);
			Type writeDelegateType = typeof(WriteDataEntriesMethodDelegate<>).MakeGenericType(formattedType);
			MethodInfo writeDataEntriesMethod = formatterType.GetMethod("WriteDataEntries", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			DynamicMethod dynamicWriteMethod = new DynamicMethod("Dynamic_Write_" + formattedType.GetCompilableNiceFullName(), null, (from n in writeDataEntriesMethod.GetParameters()
				select n.ParameterType).ToArray(), restrictedSkipVisibility: true);
			writeDataEntriesMethod.GetParameters().ForEach(delegate(ParameterInfo n)
			{
				dynamicWriteMethod.DefineParameter(n.Position + 1, n.Attributes, n.Name);
			});
			EmitWriteMethodContents(dynamicWriteMethod.GetILGenerator(), formattedType, serializerFields, memberNames, serializerWriteMethods);
			Delegate del2 = dynamicWriteMethod.CreateDelegate(writeDelegateType);
			return (IFormatter)Activator.CreateInstance(formatterType, del1, del2);
		}

		private static Type BuildHelperType(ModuleBuilder moduleBuilder, string helperTypeName, Type formattedType, Dictionary<string, MemberInfo> serializableMembers, out Dictionary<Type, MethodInfo> serializerReadMethods, out Dictionary<Type, MethodInfo> serializerWriteMethods, out Dictionary<Type, FieldBuilder> serializerFields, out FieldBuilder dictField, out Dictionary<MemberInfo, List<string>> memberNames)
		{
			TypeBuilder helperTypeBuilder = moduleBuilder.DefineType(helperTypeName, TypeAttributes.Public | TypeAttributes.Sealed);
			memberNames = new Dictionary<MemberInfo, List<string>>();
			foreach (KeyValuePair<string, MemberInfo> entry in serializableMembers)
			{
				if (!memberNames.TryGetValue(entry.Value, out var list))
				{
					list = new List<string>();
					memberNames.Add(entry.Value, list);
				}
				list.Add(entry.Key);
			}
			dictField = helperTypeBuilder.DefineField("SwitchLookup", typeof(Dictionary<string, int>), FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly);
			List<Type> neededSerializers = memberNames.Keys.Select((MemberInfo n) => FormatterUtilities.GetContainedType(n)).Distinct().ToList();
			serializerReadMethods = new Dictionary<Type, MethodInfo>(neededSerializers.Count);
			serializerWriteMethods = new Dictionary<Type, MethodInfo>(neededSerializers.Count);
			serializerFields = new Dictionary<Type, FieldBuilder>(neededSerializers.Count);
			foreach (Type t in neededSerializers)
			{
				string name = t.GetCompilableNiceFullName() + "__Serializer";
				int counter = 1;
				while (serializerFields.Values.Any((FieldBuilder n) => n.Name == name))
				{
					counter++;
					name = t.GetCompilableNiceFullName() + "__Serializer" + counter;
				}
				Type serializerType = typeof(Serializer<>).MakeGenericType(t);
				serializerReadMethods.Add(t, serializerType.GetMethod("ReadValue", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public));
				serializerWriteMethods.Add(t, serializerType.GetMethod("WriteValue", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public, null, new Type[3]
				{
					typeof(string),
					t,
					typeof(IDataWriter)
				}, null));
				serializerFields.Add(t, helperTypeBuilder.DefineField(name, serializerType, FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly));
			}
			MethodInfo addMethod = typeof(Dictionary<string, int>).GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
			ConstructorInfo dictionaryConstructor = typeof(Dictionary<string, int>).GetConstructor(Type.EmptyTypes);
			MethodInfo serializerGetMethod = typeof(Serializer).GetMethod("Get", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(Type) }, null);
			MethodInfo typeOfMethod = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(RuntimeTypeHandle) }, null);
			ConstructorBuilder staticConstructor = helperTypeBuilder.DefineTypeInitializer();
			ILGenerator gen = staticConstructor.GetILGenerator();
			gen.Emit(OpCodes.Newobj, dictionaryConstructor);
			int count = 0;
			foreach (KeyValuePair<MemberInfo, List<string>> memberName in memberNames)
			{
				foreach (string name2 in memberName.Value)
				{
					gen.Emit(OpCodes.Dup);
					gen.Emit(OpCodes.Ldstr, name2);
					gen.Emit(OpCodes.Ldc_I4, count);
					gen.Emit(OpCodes.Call, addMethod);
				}
				count++;
			}
			gen.Emit(OpCodes.Stsfld, dictField);
			foreach (KeyValuePair<Type, FieldBuilder> entry2 in serializerFields)
			{
				gen.Emit(OpCodes.Ldtoken, entry2.Key);
				gen.Emit(OpCodes.Call, typeOfMethod);
				gen.Emit(OpCodes.Call, serializerGetMethod);
				gen.Emit(OpCodes.Stsfld, entry2.Value);
			}
			gen.Emit(OpCodes.Ret);
			return helperTypeBuilder.CreateType();
		}

		private static void EmitReadMethodContents(ILGenerator gen, Type formattedType, FieldInfo dictField, Dictionary<Type, FieldBuilder> serializerFields, Dictionary<MemberInfo, List<string>> memberNames, Dictionary<Type, MethodInfo> serializerReadMethods)
		{
			MethodInfo skipMethod = typeof(IDataReader).GetMethod("SkipEntry", BindingFlags.Instance | BindingFlags.Public);
			MethodInfo tryGetValueMethod = typeof(Dictionary<string, int>).GetMethod("TryGetValue", BindingFlags.Instance | BindingFlags.Public);
			LocalBuilder lookupResult = gen.DeclareLocal(typeof(int));
			Label defaultLabel = gen.DefineLabel();
			Label switchLabel = gen.DefineLabel();
			Label endLabel = gen.DefineLabel();
			Label[] switchLabels = memberNames.Select((KeyValuePair<MemberInfo, List<string>> n) => gen.DefineLabel()).ToArray();
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Ldnull);
			gen.Emit(OpCodes.Ceq);
			gen.Emit(OpCodes.Brtrue, defaultLabel);
			gen.Emit(OpCodes.Ldsfld, dictField);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Ldloca, (short)lookupResult.LocalIndex);
			gen.Emit(OpCodes.Callvirt, tryGetValueMethod);
			gen.Emit(OpCodes.Brtrue, switchLabel);
			gen.Emit(OpCodes.Br, defaultLabel);
			gen.MarkLabel(switchLabel);
			gen.Emit(OpCodes.Ldloc, lookupResult);
			gen.Emit(OpCodes.Switch, switchLabels);
			int count = 0;
			foreach (MemberInfo member in memberNames.Keys)
			{
				Type memberType = FormatterUtilities.GetContainedType(member);
				PropertyInfo propInfo = member as PropertyInfo;
				FieldInfo fieldInfo = member as FieldInfo;
				gen.MarkLabel(switchLabels[count]);
				gen.Emit(OpCodes.Ldarg_0);
				if (!formattedType.IsValueType)
				{
					gen.Emit(OpCodes.Ldind_Ref);
				}
				gen.Emit(OpCodes.Ldsfld, serializerFields[memberType]);
				gen.Emit(OpCodes.Ldarg, (short)3);
				gen.Emit(OpCodes.Callvirt, serializerReadMethods[memberType]);
				if (fieldInfo != null)
				{
					gen.Emit(OpCodes.Stfld, fieldInfo.DeAliasField());
				}
				else
				{
					if (!(propInfo != null))
					{
						throw new NotImplementedException();
					}
					gen.Emit(OpCodes.Callvirt, propInfo.DeAliasProperty().GetSetMethod(nonPublic: true));
				}
				gen.Emit(OpCodes.Br, endLabel);
				count++;
			}
			gen.MarkLabel(defaultLabel);
			gen.Emit(OpCodes.Ldarg, (short)3);
			gen.Emit(OpCodes.Callvirt, skipMethod);
			gen.MarkLabel(endLabel);
			gen.Emit(OpCodes.Ret);
		}

		private static void EmitWriteMethodContents(ILGenerator gen, Type formattedType, Dictionary<Type, FieldBuilder> serializerFields, Dictionary<MemberInfo, List<string>> memberNames, Dictionary<Type, MethodInfo> serializerWriteMethods)
		{
			foreach (MemberInfo member in memberNames.Keys)
			{
				Type memberType = FormatterUtilities.GetContainedType(member);
				gen.Emit(OpCodes.Ldsfld, serializerFields[memberType]);
				gen.Emit(OpCodes.Ldstr, member.Name);
				if (member is FieldInfo)
				{
					FieldInfo fieldInfo = member as FieldInfo;
					if (formattedType.IsValueType)
					{
						gen.Emit(OpCodes.Ldarg_0);
						gen.Emit(OpCodes.Ldfld, fieldInfo.DeAliasField());
					}
					else
					{
						gen.Emit(OpCodes.Ldarg_0);
						gen.Emit(OpCodes.Ldind_Ref);
						gen.Emit(OpCodes.Ldfld, fieldInfo.DeAliasField());
					}
				}
				else
				{
					if (!(member is PropertyInfo))
					{
						throw new NotImplementedException();
					}
					PropertyInfo propInfo = member as PropertyInfo;
					if (formattedType.IsValueType)
					{
						gen.Emit(OpCodes.Ldarg_0);
						gen.Emit(OpCodes.Call, propInfo.DeAliasProperty().GetGetMethod(nonPublic: true));
					}
					else
					{
						gen.Emit(OpCodes.Ldarg_0);
						gen.Emit(OpCodes.Ldind_Ref);
						gen.Emit(OpCodes.Callvirt, propInfo.DeAliasProperty().GetGetMethod(nonPublic: true));
					}
				}
				gen.Emit(OpCodes.Ldarg_1);
				gen.Emit(OpCodes.Callvirt, serializerWriteMethods[memberType]);
			}
			gen.Emit(OpCodes.Ret);
		}
	}
}

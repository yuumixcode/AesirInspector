using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Sirenix.Serialization.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;

namespace Sirenix.Serialization.Editor
{
	public static class AOTSupportUtilities
	{
		/// <summary>
		/// Scans the project's build scenes and resources, plus their dependencies, for serialized types to support. Progress bars are shown during the scan.
		/// </summary>
		/// <param name="serializedTypes">The serialized types to support.</param>
		/// <param name="scanBuildScenes">Whether to scan the project's build scenes.</param>
		/// <param name="scanAllAssetBundles">Whether to scan all the project's asset bundles.</param>
		/// <param name="scanPreloadedAssets">Whether to scan the project's preloaded assets.</param>
		/// <param name="scanResources">Whether to scan the project's resources.</param>
		/// <param name="resourcesToScan">An optional list of the resource paths to scan. Only has an effect if the scanResources argument is true. All the resources will be scanned if null.</param>
		/// <returns>true if the scan succeeded, false if the scan failed or was cancelled</returns>
		public static bool ScanProjectForSerializedTypes(out List<Type> serializedTypes, bool scanBuildScenes = true, bool scanAllAssetBundles = true, bool scanPreloadedAssets = true, bool scanResources = true, List<string> resourcesToScan = null, bool scanAddressables = true)
		{
			using (AOTSupportScanner scanner = new AOTSupportScanner())
			{
				scanner.BeginScan();
				if (scanBuildScenes && !scanner.ScanBuildScenes(includeSceneDependencies: true, showProgressBar: true))
				{
					Debug.Log("Project scan canceled while scanning scenes and their dependencies.");
					serializedTypes = null;
					return false;
				}
				if (scanResources && !scanner.ScanAllResources(includeResourceDependencies: true, showProgressBar: true, resourcesToScan))
				{
					Debug.Log("Project scan canceled while scanning resources and their dependencies.");
					serializedTypes = null;
					return false;
				}
				if (scanAllAssetBundles && !scanner.ScanAllAssetBundles(showProgressBar: true))
				{
					Debug.Log("Project scan canceled while scanning asset bundles and their dependencies.");
					serializedTypes = null;
					return false;
				}
				if (scanPreloadedAssets && !scanner.ScanPreloadedAssets(showProgressBar: true))
				{
					Debug.Log("Project scan canceled while scanning preloaded assets and their dependencies.");
					serializedTypes = null;
					return false;
				}
				if (scanAddressables && !scanner.ScanAllAddressables(includeAssetDependencies: true, showProgressBar: true))
				{
					Debug.Log("Project scan canceled while scanning addressable assets and their dependencies.");
					serializedTypes = null;
					return false;
				}
				serializedTypes = scanner.EndScan();
			}
			return true;
		}

		/// <summary>
		/// Generates an AOT DLL, using the given parameters.
		/// </summary>
		public static void GenerateDLL(string dirPath, string assemblyName, List<Type> supportSerializedTypes, bool generateLinkXml = true)
		{
			if (!dirPath.EndsWith("/"))
			{
				dirPath += "/";
			}
			string newDllPath = dirPath + assemblyName;
			string fullDllPath = newDllPath + ".dll";
			AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName
			{
				Name = assemblyName
			}, (AssemblyBuilderAccess)2, dirPath);
			ModuleBuilder module = assembly.DefineDynamicModule(assemblyName);
			assembly.SetCustomAttribute(new CustomAttributeBuilder(typeof(EmittedAssemblyAttribute).GetConstructor(new Type[0]), new object[0]));
			FieldInfo modulesField = assembly.GetType().GetField("modules", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			FieldInfo manifestModuleField = assembly.GetType().GetField("manifest_module", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (modulesField != null)
			{
				modulesField.SetValue(assembly, new ModuleBuilder[1] { module });
			}
			if (manifestModuleField != null)
			{
				manifestModuleField.SetValue(assembly, module);
			}
			TypeBuilder type = module.DefineType(assemblyName + ".PreventCodeStrippingViaReferences", TypeAttributes.Abstract | TypeAttributes.Sealed);
			CustomAttributeBuilder attributeBuilder = new CustomAttributeBuilder(typeof(PreserveAttribute).GetConstructor(Type.EmptyTypes), new object[0]);
			type.SetCustomAttribute(attributeBuilder);
			ConstructorBuilder staticConstructor = type.DefineTypeInitializer();
			ILGenerator il = staticConstructor.GetILGenerator();
			LocalBuilder falseLocal = il.DeclareLocal(typeof(bool));
			il.Emit(OpCodes.Ldc_I4_0);
			il.Emit(OpCodes.Stloc, falseLocal);
			HashSet<Type> seenTypes = new HashSet<Type>();
			if (UnityVersion.Major == 2019 && UnityVersion.Minor == 2)
			{
				supportSerializedTypes.Add(typeof(DateTimeFormatter));
			}
			HashSet<Type> allTypesToSupport = new HashSet<Type>(supportSerializedTypes);
			foreach (Type typeToSupport in supportSerializedTypes)
			{
				RecursivelyAddExtraTypesToSupport(typeToSupport, allTypesToSupport);
			}
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly loadedAssembly in assemblies)
			{
				Type[] array = loadedAssembly.SafeGetTypes();
				foreach (Type loadedType in array)
				{
					if (!loadedType.IsAbstract && typeof(WeakBaseFormatter).IsAssignableFrom(loadedType))
					{
						GatherExtraTypesToSupportFromStaticFormatterFields(loadedType, allTypesToSupport);
					}
				}
			}
			supportSerializedTypes = allTypesToSupport.ToList();
			foreach (Type serializedType in supportSerializedTypes)
			{
				if (serializedType == null)
				{
					continue;
				}
				bool isAbstract = serializedType.IsAbstract || serializedType.IsInterface;
				if (serializedType.IsGenericType && (serializedType.IsGenericTypeDefinition || !serializedType.IsFullyConstructedGenericType()))
				{
					Debug.LogError("Skipping type '" + serializedType.GetNiceFullName() + "'! Type is a generic type definition, or its arguments contain generic parameters; type must be a fully constructed generic type.");
				}
				else
				{
					if (seenTypes.Contains(serializedType))
					{
						continue;
					}
					seenTypes.Add(serializedType);
					if (serializedType.IsValueType)
					{
						LocalBuilder local = il.DeclareLocal(serializedType);
						il.Emit(OpCodes.Ldloca, local);
						il.Emit(OpCodes.Initobj, serializedType);
					}
					else if (!isAbstract)
					{
						ConstructorInfo constructor = serializedType.GetConstructor(Type.EmptyTypes);
						if (constructor != null)
						{
							il.Emit(OpCodes.Newobj, constructor);
							il.Emit(OpCodes.Pop);
						}
					}
					if (!FormatterUtilities.IsPrimitiveType(serializedType) && !typeof(UnityEngine.Object).IsAssignableFrom(serializedType) && !isAbstract)
					{
						IFormatter actualFormatter = FormatterLocator.GetFormatter(serializedType, SerializationPolicies.Unity);
						actualFormatter.GetType().IsDefined<EmittedFormatterAttribute>();
						List<IFormatter> formatters = FormatterLocator.GetAllCompatiblePredefinedFormatters(serializedType, SerializationPolicies.Unity);
						foreach (IFormatter formatter in formatters)
						{
							ConstructorInfo formatterConstructor = formatter.GetType().GetConstructor(Type.EmptyTypes);
							if (formatterConstructor != null)
							{
								il.Emit(OpCodes.Newobj, formatterConstructor);
								il.Emit(OpCodes.Pop);
								continue;
							}
							formatterConstructor = formatter.GetType().GetConstructor(new Type[1] { typeof(Type) });
							if (formatterConstructor != null)
							{
								il.Emit(OpCodes.Ldnull);
								il.Emit(OpCodes.Newobj, formatterConstructor);
								il.Emit(OpCodes.Pop);
							}
						}
					}
					if (serializedType.IsValueType)
					{
						ConstructorInfo serializerConstructor = Serializer.Get(serializedType).GetType().GetConstructor(Type.EmptyTypes);
						il.Emit(OpCodes.Newobj, serializerConstructor);
						Label endLabel = il.DefineLabel();
						il.Emit(OpCodes.Ldloc, falseLocal);
						il.Emit(OpCodes.Brfalse, endLabel);
						Type baseSerializerType = typeof(Serializer<>).MakeGenericType(serializedType);
						MethodInfo readValueWeakMethod = baseSerializerType.GetMethod("ReadValueWeak", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(IDataReader) }, null);
						MethodInfo writeValueWeakMethod = baseSerializerType.GetMethod("WriteValueWeak", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public, null, new Type[3]
						{
							typeof(string),
							typeof(object),
							typeof(IDataWriter)
						}, null);
						il.Emit(OpCodes.Dup);
						il.Emit(OpCodes.Ldnull);
						il.Emit(OpCodes.Callvirt, readValueWeakMethod);
						il.Emit(OpCodes.Pop);
						il.Emit(OpCodes.Dup);
						il.Emit(OpCodes.Ldnull);
						il.Emit(OpCodes.Ldnull);
						il.Emit(OpCodes.Ldnull);
						il.Emit(OpCodes.Callvirt, writeValueWeakMethod);
						il.MarkLabel(endLabel);
						il.Emit(OpCodes.Pop);
					}
					else
					{
						ConstructorInfo serializerConstructor = typeof(ComplexTypeSerializer<>).MakeGenericType(serializedType).GetConstructor(Type.EmptyTypes);
						il.Emit(OpCodes.Newobj, serializerConstructor);
						il.Emit(OpCodes.Pop);
					}
				}
			}
			il.Emit(OpCodes.Ret);
			type.CreateType();
			if (!Directory.Exists(dirPath))
			{
				Directory.CreateDirectory(dirPath);
			}
			if (File.Exists(fullDllPath))
			{
				File.Delete(fullDllPath);
			}
			if (File.Exists(fullDllPath + ".meta"))
			{
				File.Delete(fullDllPath + ".meta");
			}
			try
			{
				AssetDatabase.Refresh();
			}
			catch (Exception)
			{
			}
			assembly.Save(assemblyName);
			File.Move(newDllPath, fullDllPath);
			if (generateLinkXml)
			{
				File.WriteAllText(dirPath + "link.xml", "<linker>\r\n       <assembly fullname=\"" + assemblyName + "\" preserve=\"all\"/>\r\n</linker>");
			}
			try
			{
				AssetDatabase.Refresh();
			}
			catch (Exception)
			{
			}
			PluginImporter pluginImporter = AssetImporter.GetAtPath(fullDllPath) as PluginImporter;
			if (pluginImporter != null)
			{
				pluginImporter.SetCompatibleWithEditor(enable: false);
				pluginImporter.SetCompatibleWithAnyPlatform(enable: true);
				pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, enable: false);
				if (!UnityVersion.IsVersionOrGreater(2019, 2))
				{
					pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux, enable: false);
					pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneLinuxUniversal, enable: false);
				}
				pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, enable: false);
				if (!UnityVersion.IsVersionOrGreater(2017, 3))
				{
					pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel, enable: false);
					pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneOSXIntel64, enable: false);
				}
				pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, enable: false);
				pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, enable: false);
				pluginImporter.SaveAndReimport();
			}
			AssetDatabase.SaveAssets();
		}

		private static void RecursivelyAddExtraTypesToSupport(Type typeToSupport, HashSet<Type> allTypesToSupport)
		{
			if (FormatterUtilities.IsPrimitiveType(typeToSupport))
			{
				return;
			}
			MemberInfo[] serializedMembers = FormatterUtilities.GetSerializableMembers(typeToSupport, SerializationPolicies.Unity);
			MemberInfo[] array = serializedMembers;
			foreach (MemberInfo member in array)
			{
				Type memberType = member.GetReturnType();
				if (AOTSupportScanner.AllowRegisterType(memberType) && allTypesToSupport.Add(memberType))
				{
					RecursivelyAddExtraTypesToSupport(memberType, allTypesToSupport);
				}
			}
			if (typeof(UnityEngine.Object).IsAssignableFrom(typeToSupport))
			{
				return;
			}
			List<IFormatter> formatters = FormatterLocator.GetAllCompatiblePredefinedFormatters(typeToSupport, SerializationPolicies.Unity);
			foreach (IFormatter formatter in formatters)
			{
				GatherExtraTypesToSupportFromStaticFormatterFields(formatter.GetType(), allTypesToSupport);
			}
		}

		private static void GatherExtraTypesToSupportFromStaticFormatterFields(Type formatterType, HashSet<Type> allTypesToSupport)
		{
			FieldInfo[] staticFormatterFields = formatterType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			FieldInfo[] array = staticFormatterFields;
			foreach (FieldInfo field in array)
			{
				Type[] parameters = field.FieldType.GetArgumentsOfInheritedOpenGenericClass(typeof(Serializer<>));
				if (parameters == null)
				{
					continue;
				}
				Type[] array2 = parameters;
				foreach (Type parameterType in array2)
				{
					if (allTypesToSupport.Add(parameterType))
					{
						RecursivelyAddExtraTypesToSupport(parameterType, allTypesToSupport);
					}
				}
			}
		}
	}
}

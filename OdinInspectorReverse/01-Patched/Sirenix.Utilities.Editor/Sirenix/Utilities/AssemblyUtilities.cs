using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities
{
	/// <summary>
	/// A utility class for finding types in various asssembly.
	/// </summary>
	[InitializeOnLoad]
	public static class AssemblyUtilities
	{
		internal const string AssemblyTypeFlagsObsoleteMessage = "AssemblyTypeFlags have been made obsolete, because they cannot be determined accurately for all assemblies. Use AssemblyUtilities.GetAssemblyCategory(assembly) instead.";

		private static string[] pluginAssemblyPrefixes;

		private static readonly Dictionary<Assembly, bool> IsDynamicCache;

		private static readonly object IS_DYNAMIC_CACHE_LOCK;

		private static readonly object ASSEMBLY_TYPE_FLAG_LOOKUP_LOCK;

		private static readonly object ASSEMBLY_CATEGORY_LOOKUP_LOCK;

		private static Assembly unityEngineAssembly;

		private static Assembly unityEditorAssembly;

		private static DirectoryInfo projectAssetsFolderDirectory;

		private static DirectoryInfo scriptAssembliesDirectory;

		private static DirectoryInfo mscorlibDirectory;

		private static DirectoryInfo unityEngineDirectory;

		[Obsolete("", false)]
		private static Dictionary<Assembly, AssemblyTypeFlags> assemblyTypeFlagLookup;

		private static Dictionary<Assembly, AssemblyCategory> assemblyCategoryLookup;

		static AssemblyUtilities()
		{
			pluginAssemblyPrefixes = new string[6] { "assembly-csharp-firstpass", "assembly-csharp-editor-firstpass", "assembly-unityscript-firstpass", "assembly-unityscript-editor-firstpass", "assembly-boo-firstpass", "assembly-boo-editor-firstpass" };
			IsDynamicCache = new Dictionary<Assembly, bool>(ReferenceEqualityComparer<Assembly>.Default);
			IS_DYNAMIC_CACHE_LOCK = new object();
			ASSEMBLY_TYPE_FLAG_LOOKUP_LOCK = new object();
			ASSEMBLY_CATEGORY_LOOKUP_LOCK = new object();
			unityEngineAssembly = typeof(UnityEngine.Object).Assembly;
			unityEditorAssembly = typeof(UnityEditor.Editor).Assembly;
			assemblyTypeFlagLookup = new Dictionary<Assembly, AssemblyTypeFlags>(100);
			assemblyCategoryLookup = new Dictionary<Assembly, AssemblyCategory>(100);
			string currentDir = Environment.CurrentDirectory.Replace("\\", "//").Replace("//", "/").TrimEnd(new char[1] { '/' });
			string dataPath = currentDir + "/Assets";
			string scriptAssembliesPath = currentDir + "/Library/ScriptAssemblies";
			projectAssetsFolderDirectory = new DirectoryInfo(dataPath);
			scriptAssembliesDirectory = new DirectoryInfo(scriptAssembliesPath);
			mscorlibDirectory = new DirectoryInfo(typeof(string).Assembly.GetAssemblyDirectory());
			unityEngineDirectory = new DirectoryInfo(typeof(UnityEngine.Object).Assembly.GetAssemblyDirectory());
			if (unityEngineDirectory.Parent.Name == "Managed")
			{
				unityEngineDirectory = unityEngineDirectory.Parent;
			}
		}

		public static AssemblyCategory GetAssemblyCategory(Assembly assembly)
		{
			if (assembly == null)
			{
				throw new NullReferenceException("assembly");
			}
			lock (ASSEMBLY_CATEGORY_LOOKUP_LOCK)
			{
				if (!assemblyCategoryLookup.TryGetValue(assembly, out var result))
				{
					result = GetAssemblyCategoryPrivate(assembly);
					assemblyCategoryLookup[assembly] = result;
				}
				return result;
			}
		}

		[Obsolete("AssemblyTypeFlags have been made obsolete, because they cannot be determined accurately for all assemblies. Use AssemblyUtilities.GetAssemblyCategory(assembly) instead.", false)]
		internal static AssemblyCategory LossyBadConvertAssemblyTypeFlagsToCategories(AssemblyTypeFlags flags)
		{
			AssemblyCategory result = AssemblyCategory.None;
			if (flags.HasFlag(AssemblyTypeFlags.UserTypes))
			{
				result |= AssemblyCategory.Scripts;
			}
			if (flags.HasFlag(AssemblyTypeFlags.PluginTypes))
			{
				result |= AssemblyCategory.ProjectSpecific;
			}
			if (flags.HasFlag(AssemblyTypeFlags.UnityTypes))
			{
				result |= AssemblyCategory.UnityEngine;
			}
			if (flags.HasFlag(AssemblyTypeFlags.UserEditorTypes))
			{
				result |= AssemblyCategory.Scripts;
			}
			if (flags.HasFlag(AssemblyTypeFlags.PluginEditorTypes))
			{
				result |= AssemblyCategory.ProjectSpecific;
			}
			if (flags.HasFlag(AssemblyTypeFlags.UnityEditorTypes))
			{
				result |= AssemblyCategory.UnityEngine;
			}
			if (flags.HasFlag(AssemblyTypeFlags.OtherTypes))
			{
				result |= AssemblyCategory.DotNetRuntime | AssemblyCategory.DynamicAssemblies | AssemblyCategory.Unknown;
			}
			return result;
		}

		private static AssemblyCategory GetAssemblyCategoryPrivate(Assembly assembly)
		{
			if (assembly.IsDynamic())
			{
				return AssemblyCategory.DynamicAssemblies;
			}
			string path = assembly.GetAssemblyDirectory();
			if (path != null && Directory.Exists(path))
			{
				DirectoryInfo pathDir = new DirectoryInfo(path);
				if (pathDir.FullName == scriptAssembliesDirectory.FullName)
				{
					return AssemblyCategory.Scripts;
				}
				if (File.Exists(assembly.Location + ".meta"))
				{
					return AssemblyCategory.ImportedAssemblies;
				}
				if (unityEngineDirectory.HasSubDirectory(pathDir))
				{
					return AssemblyCategory.UnityEngine;
				}
				if (mscorlibDirectory.HasSubDirectory(pathDir))
				{
					return AssemblyCategory.DotNetRuntime;
				}
			}
			string name = assembly.GetName().Name;
			if (name.StartsWith("UnityEngine.") || name.StartsWith("UnityEditor."))
			{
				return AssemblyCategory.UnityEngine;
			}
			return AssemblyCategory.Unknown;
		}

		[Obsolete("Reload is no longer supported.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Reload()
		{
		}

		/// <summary>
		/// Gets an <see cref="T:Sirenix.Utilities.ImmutableList" /> of all assemblies in the current <see cref="T:System.AppDomain" />.
		/// </summary>
		/// <returns>An <see cref="T:Sirenix.Utilities.ImmutableList" /> of all assemblies in the current <see cref="T:System.AppDomain" />.</returns>
		public static ImmutableList<Assembly> GetAllAssemblies()
		{
			return new ImmutableList<Assembly>(AppDomain.CurrentDomain.GetAssemblies());
		}

		/// <summary>
		/// Gets the <see cref="T:Sirenix.Utilities.AssemblyTypeFlags" /> for a given assembly.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <returns>The <see cref="T:Sirenix.Utilities.AssemblyTypeFlags" /> for a given assembly.</returns>
		/// <exception cref="T:System.NullReferenceException"><paramref name="assembly" /> is null.</exception>
		[Obsolete("AssemblyTypeFlags have been made obsolete, because they cannot be determined accurately for all assemblies. Use AssemblyUtilities.GetAssemblyCategory(assembly) instead.", false)]
		public static AssemblyTypeFlags GetAssemblyTypeFlag(this Assembly assembly)
		{
			if (assembly == null)
			{
				throw new NullReferenceException("assembly");
			}
			lock (ASSEMBLY_TYPE_FLAG_LOOKUP_LOCK)
			{
				if (!assemblyTypeFlagLookup.TryGetValue(assembly, out var result))
				{
					result = GetAssemblyTypeFlagNoLookup(assembly);
					assemblyTypeFlagLookup[assembly] = result;
				}
				return result;
			}
		}

		[Obsolete("", false)]
		private static AssemblyTypeFlags GetAssemblyTypeFlagNoLookup(Assembly assembly)
		{
			string name = assembly.FullName.ToLower(CultureInfo.InvariantCulture);
			AssemblyCategory category = GetAssemblyCategory(assembly);
			if (category <= AssemblyCategory.DotNetRuntime)
			{
				switch (category)
				{
				case AssemblyCategory.Scripts:
				{
					bool isEditor = name.Contains("-editor");
					if (name.StartsWithAnyOf(pluginAssemblyPrefixes))
					{
						if (!isEditor)
						{
							return AssemblyTypeFlags.PluginTypes;
						}
						return AssemblyTypeFlags.PluginEditorTypes;
					}
					if (!isEditor)
					{
						return AssemblyTypeFlags.UserTypes;
					}
					return AssemblyTypeFlags.UserEditorTypes;
				}
				case AssemblyCategory.ImportedAssemblies:
					return AssemblyTypeFlags.PluginTypes;
				case AssemblyCategory.UnityEngine:
					if (assembly.IsDependentOn(unityEditorAssembly))
					{
						return AssemblyTypeFlags.UnityEditorTypes;
					}
					return AssemblyTypeFlags.UnityTypes;
				}
			}
			else if (category != AssemblyCategory.DynamicAssemblies)
			{
				_ = 32;
				return AssemblyTypeFlags.OtherTypes;
			}
			return AssemblyTypeFlags.OtherTypes;
		}

		/// <summary>
		/// Determines whether an assembly is depended on another assembly.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <param name="otherAssembly">The other assembly.</param>
		/// <returns>
		///   <c>true</c> if <paramref name="assembly" /> has a reference in <paramref name="otherAssembly" /> or <paramref name="assembly" /> is the same as <paramref name="otherAssembly" />.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException"><paramref name="assembly" /> is null.</exception>
		/// <exception cref="T:System.NullReferenceException"><paramref name="otherAssembly" /> is null.</exception>
		public static bool IsDependentOn(this Assembly assembly, Assembly otherAssembly)
		{
			if (assembly == null)
			{
				throw new NullReferenceException("assembly");
			}
			if (otherAssembly == null)
			{
				throw new NullReferenceException("otherAssembly");
			}
			if (assembly == otherAssembly)
			{
				return true;
			}
			string otherName = otherAssembly.GetName().ToString();
			AssemblyName[] referencedAsssemblies = assembly.GetReferencedAssemblies();
			for (int i = 0; i < referencedAsssemblies.Length; i++)
			{
				if (otherName == referencedAsssemblies[i].ToString())
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Determines whether the assembly module is a of type <see cref="T:System.Reflection.Emit.ModuleBuilder" />.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <returns>
		///   <c>true</c> if the specified assembly of type <see cref="T:System.Reflection.Emit.ModuleBuilder" />; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">assembly</exception>
		public static bool IsDynamic(this Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			bool result;
			lock (IS_DYNAMIC_CACHE_LOCK)
			{
				if (!IsDynamicCache.TryGetValue(assembly, out result))
				{
					try
					{
						result = assembly.GetType().FullName.EndsWith("AssemblyBuilder") || assembly.Location == null || assembly.Location == "";
					}
					catch
					{
						result = true;
					}
					IsDynamicCache.Add(assembly, result);
				}
			}
			return result;
		}

		/// <summary>
		/// Gets the full file path to a given assembly's containing directory.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <returns>The full file path to a given assembly's containing directory, or <c>Null</c> if no file path was found.</returns>
		/// <exception cref="T:System.NullReferenceException"><paramref name="assembly" /> is Null.</exception>
		public static string GetAssemblyDirectory(this Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			string path = assembly.GetAssemblyFilePath();
			if (path == null)
			{
				return null;
			}
			try
			{
				return Path.GetDirectoryName(path);
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Gets the full directory path to a given assembly.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <returns>The full directory path in which a given assembly is located, or <c>Null</c> if no file path was found.</returns>
		public static string GetAssemblyFilePath(this Assembly assembly)
		{
			if (assembly == null)
			{
				return null;
			}
			if (assembly.IsDynamic())
			{
				return null;
			}
			if (assembly.CodeBase == null)
			{
				return null;
			}
			string filePrefix = "file:///";
			string path = assembly.CodeBase;
			if (path.StartsWith(filePrefix, StringComparison.InvariantCultureIgnoreCase))
			{
				path = path.Substring(filePrefix.Length);
				path = path.Replace('\\', '/');
				if (File.Exists(path))
				{
					return path;
				}
				if (!Path.IsPathRooted(path))
				{
					path = ((!File.Exists("/" + path)) ? Path.GetFullPath(path) : ("/" + path));
				}
				if (File.Exists(path))
				{
					return path;
				}
				try
				{
					path = assembly.Location;
				}
				catch
				{
					return null;
				}
				if (File.Exists(path))
				{
					return path;
				}
			}
			if (File.Exists(assembly.Location))
			{
				return assembly.Location;
			}
			return null;
		}

		/// <summary>
		/// Gets the type.
		/// </summary>
		/// <param name="fullName">The full name of the type, with or without any assembly information.</param>
		public static Type GetTypeByCachedFullName(string name)
		{
			return TwoWaySerializationBinder.Default.BindToType(name);
		}

		/// <summary>
		/// Get types from the current AppDomain with a specified <see cref="T:Sirenix.Utilities.AssemblyCategory" /> filter.
		/// </summary>
		/// <param name="assemblyFlags">The <see cref="T:Sirenix.Utilities.AssemblyCategory" /> filters.</param>
		/// <returns>Types from the current AppDomain with the specified <see cref="T:Sirenix.Utilities.AssemblyCategory" /> filters.</returns>
		public static IEnumerable<Type> GetTypes(AssemblyCategory assemblyFlags)
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				AssemblyCategory flag = GetAssemblyCategory(assembly);
				if ((flag & assemblyFlags) != AssemblyCategory.None)
				{
					Type[] array = assembly.SafeGetTypes();
					for (int j = 0; j < array.Length; j++)
					{
						yield return array[j];
					}
				}
			}
		}

		/// <summary>
		/// Get types from the current AppDomain with a specified <see cref="T:Sirenix.Utilities.AssemblyTypeFlags" /> filter.
		/// </summary>
		/// <param name="assemblyTypeFlags">The <see cref="T:Sirenix.Utilities.AssemblyTypeFlags" /> filters.</param>
		/// <returns>Types from the current AppDomain with the specified <see cref="T:Sirenix.Utilities.AssemblyTypeFlags" /> filters.</returns>
		[Obsolete("AssemblyTypeFlags have been made obsolete, because they cannot be determined accurately for all assemblies. Use AssemblyUtilities.GetTypes(AssemblyCategory assemblyFlags) instead.", false)]
		public static IEnumerable<Type> GetTypes(AssemblyTypeFlags assemblyTypeFlags)
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				AssemblyTypeFlags flag = assembly.GetAssemblyTypeFlag();
				if ((flag & assemblyTypeFlags) != AssemblyTypeFlags.None)
				{
					Type[] array = assembly.SafeGetTypes();
					for (int j = 0; j < array.Length; j++)
					{
						yield return array[j];
					}
				}
			}
		}

		private static bool StartsWithAnyOf(this string str, string[] values)
		{
			for (int i = 0; i < values.Length; i++)
			{
				if (str.StartsWith(values[i], StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}
	}
}

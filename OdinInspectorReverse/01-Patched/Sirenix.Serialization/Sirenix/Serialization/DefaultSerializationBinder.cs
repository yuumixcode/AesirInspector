using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Sirenix.Serialization.Utilities;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Provides a default, catch-all <see cref="T:Sirenix.Serialization.TwoWaySerializationBinder" /> implementation. This binder only includes assembly names, without versions and tokens, in order to increase compatibility.
	/// </summary>
	/// <seealso cref="T:Sirenix.Serialization.TwoWaySerializationBinder" />
	/// <seealso cref="T:Sirenix.Serialization.BindTypeNameToTypeAttribute" />
	public class DefaultSerializationBinder : TwoWaySerializationBinder
	{
		private static readonly object ASSEMBLY_LOOKUP_LOCK;

		private static readonly Dictionary<string, Assembly> assemblyNameLookUp;

		private static readonly Dictionary<string, Type> customTypeNameToTypeBindings;

		private static readonly object TYPETONAME_LOCK;

		private static readonly Dictionary<Type, string> nameMap;

		private static readonly object NAMETOTYPE_LOCK;

		private static readonly Dictionary<string, Type> typeMap;

		private static readonly object ASSEMBLY_REGISTER_QUEUE_LOCK;

		private static readonly List<Assembly> assembliesQueuedForRegister;

		private static readonly List<AssemblyLoadEventArgs> assemblyLoadEventsQueuedForRegister;

		static DefaultSerializationBinder()
		{
			ASSEMBLY_LOOKUP_LOCK = new object();
			assemblyNameLookUp = new Dictionary<string, Assembly>();
			customTypeNameToTypeBindings = new Dictionary<string, Type>();
			TYPETONAME_LOCK = new object();
			nameMap = new Dictionary<Type, string>(FastTypeComparer.Instance);
			NAMETOTYPE_LOCK = new object();
			typeMap = new Dictionary<string, Type>();
			ASSEMBLY_REGISTER_QUEUE_LOCK = new object();
			assembliesQueuedForRegister = new List<Assembly>();
			assemblyLoadEventsQueuedForRegister = new List<AssemblyLoadEventArgs>();
			AppDomain.CurrentDomain.AssemblyLoad += delegate(object sender, AssemblyLoadEventArgs args)
			{
				lock (ASSEMBLY_REGISTER_QUEUE_LOCK)
				{
					assemblyLoadEventsQueuedForRegister.Add(args);
				}
			};
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				lock (ASSEMBLY_REGISTER_QUEUE_LOCK)
				{
					assembliesQueuedForRegister.Add(assembly);
				}
			}
			lock (ASSEMBLY_LOOKUP_LOCK)
			{
				customTypeNameToTypeBindings["System.Reflection.MonoMethod"] = typeof(MethodInfo);
				customTypeNameToTypeBindings["System.Reflection.MonoMethod, mscorlib"] = typeof(MethodInfo);
			}
		}

		private static void RegisterAllQueuedAssembliesRepeating()
		{
			while (RegisterQueuedAssemblies())
			{
			}
			while (RegisterQueuedAssemblyLoadEvents())
			{
			}
		}

		private static bool RegisterQueuedAssemblies()
		{
			Assembly[] toRegister = null;
			lock (ASSEMBLY_REGISTER_QUEUE_LOCK)
			{
				if (assembliesQueuedForRegister.Count > 0)
				{
					toRegister = assembliesQueuedForRegister.ToArray();
					assembliesQueuedForRegister.Clear();
				}
			}
			if (toRegister == null)
			{
				return false;
			}
			for (int i = 0; i < toRegister.Length; i++)
			{
				RegisterAssembly(toRegister[i]);
			}
			return true;
		}

		private static bool RegisterQueuedAssemblyLoadEvents()
		{
			AssemblyLoadEventArgs[] toRegister = null;
			lock (ASSEMBLY_REGISTER_QUEUE_LOCK)
			{
				if (assemblyLoadEventsQueuedForRegister.Count > 0)
				{
					toRegister = assemblyLoadEventsQueuedForRegister.ToArray();
					assemblyLoadEventsQueuedForRegister.Clear();
				}
			}
			if (toRegister == null)
			{
				return false;
			}
			foreach (AssemblyLoadEventArgs args in toRegister)
			{
				Assembly assembly;
				try
				{
					assembly = args.LoadedAssembly;
				}
				catch
				{
					continue;
				}
				RegisterAssembly(assembly);
			}
			return true;
		}

		private static void RegisterAssembly(Assembly assembly)
		{
			string name;
			try
			{
				name = assembly.GetName().Name;
			}
			catch
			{
				return;
			}
			bool wasAdded = false;
			lock (ASSEMBLY_LOOKUP_LOCK)
			{
				if (!assemblyNameLookUp.ContainsKey(name))
				{
					assemblyNameLookUp.Add(name, assembly);
					wasAdded = true;
				}
			}
			if (!wasAdded)
			{
				return;
			}
			try
			{
				object[] customAttributes = assembly.SafeGetCustomAttributes(typeof(BindTypeNameToTypeAttribute), inherit: false);
				if (customAttributes == null)
				{
					return;
				}
				for (int i = 0; i < customAttributes.Length; i++)
				{
					if (customAttributes[i] is BindTypeNameToTypeAttribute attr && attr.NewType != null)
					{
						lock (ASSEMBLY_LOOKUP_LOCK)
						{
							customTypeNameToTypeBindings[attr.OldTypeName] = attr.NewType;
						}
					}
				}
			}
			catch
			{
			}
		}

		/// <summary>
		/// Bind a type to a name.
		/// </summary>
		/// <param name="type">The type to bind.</param>
		/// <param name="debugContext">The debug context to log to.</param>
		/// <returns>
		/// The name that the type has been bound to.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">The type argument is null.</exception>
		public override string BindToName(Type type, DebugContext debugContext = null)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			string result;
			lock (TYPETONAME_LOCK)
			{
				if (!nameMap.TryGetValue(type, out result))
				{
					if (!type.IsGenericType)
					{
						result = ((!type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false)) ? (type.FullName + ", " + type.Assembly.GetName().Name) : (type.FullName + ", " + type.Assembly.GetName().Name));
					}
					else
					{
						List<Type> toResolve = type.GetGenericArguments().ToList();
						HashSet<Assembly> assemblies = new HashSet<Assembly>();
						while (toResolve.Count > 0)
						{
							Type t = toResolve[0];
							if (t.IsGenericType)
							{
								toResolve.AddRange(t.GetGenericArguments());
							}
							assemblies.Add(t.Assembly);
							toResolve.RemoveAt(0);
						}
						result = type.FullName + ", " + type.Assembly.GetName().Name;
						foreach (Assembly ass in assemblies)
						{
							result = result.Replace(ass.FullName, ass.GetName().Name);
						}
					}
					nameMap.Add(type, result);
				}
			}
			return result;
		}

		/// <summary>
		/// Determines whether the specified type name is mapped.
		/// </summary>
		public override bool ContainsType(string typeName)
		{
			lock (NAMETOTYPE_LOCK)
			{
				return typeMap.ContainsKey(typeName);
			}
		}

		/// <summary>
		/// Binds a name to type.
		/// </summary>
		/// <param name="typeName">The name of the type to bind.</param>
		/// <param name="debugContext">The debug context to log to.</param>
		/// <returns>
		/// The type that the name has been bound to, or null if the type could not be resolved.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">The typeName argument is null.</exception>
		public override Type BindToType(string typeName, DebugContext debugContext = null)
		{
			if (typeName == null)
			{
				throw new ArgumentNullException("typeName");
			}
			RegisterAllQueuedAssembliesRepeating();
			Type result;
			lock (NAMETOTYPE_LOCK)
			{
				if (!typeMap.TryGetValue(typeName, out result))
				{
					result = ParseTypeName(typeName, debugContext);
					if (result == null)
					{
						debugContext?.LogWarning("Failed deserialization type lookup for type name '" + typeName + "'.");
					}
					typeMap.Add(typeName, result);
				}
			}
			return result;
		}

		private Type ParseTypeName(string typeName, DebugContext debugContext)
		{
			Type type;
			lock (ASSEMBLY_LOOKUP_LOCK)
			{
				if (customTypeNameToTypeBindings.TryGetValue(typeName, out type))
				{
					return type;
				}
			}
			type = Type.GetType(typeName);
			if (type != null)
			{
				return type;
			}
			type = ParseGenericAndOrArrayType(typeName, debugContext);
			if (type != null)
			{
				return type;
			}
			ParseName(typeName, out var typeStr, out var assemblyStr);
			if (!string.IsNullOrEmpty(typeStr))
			{
				lock (ASSEMBLY_LOOKUP_LOCK)
				{
					if (customTypeNameToTypeBindings.TryGetValue(typeStr, out type))
					{
						return type;
					}
				}
				if (assemblyStr != null)
				{
					Assembly assembly;
					lock (ASSEMBLY_LOOKUP_LOCK)
					{
						assemblyNameLookUp.TryGetValue(assemblyStr, out assembly);
					}
					if (assembly == null)
					{
						try
						{
							assembly = Assembly.Load(assemblyStr);
						}
						catch
						{
						}
					}
					if (assembly != null)
					{
						try
						{
							type = assembly.GetType(typeStr);
						}
						catch
						{
						}
						if (type != null)
						{
							return type;
						}
					}
				}
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (Assembly assembly in assemblies)
				{
					try
					{
						type = assembly.GetType(typeStr, throwOnError: false);
					}
					catch
					{
					}
					if (type != null)
					{
						return type;
					}
				}
			}
			return null;
		}

		private static void ParseName(string fullName, out string typeName, out string assemblyName)
		{
			typeName = null;
			assemblyName = null;
			int firstComma = fullName.IndexOf(',');
			if (firstComma < 0 || firstComma + 1 == fullName.Length)
			{
				typeName = fullName.Trim(',', ' ');
				return;
			}
			typeName = fullName.Substring(0, firstComma);
			int secondComma = fullName.IndexOf(',', firstComma + 1);
			if (secondComma < 0)
			{
				assemblyName = fullName.Substring(firstComma).Trim(',', ' ');
			}
			else
			{
				assemblyName = fullName.Substring(firstComma, secondComma - firstComma).Trim(',', ' ');
			}
		}

		private Type ParseGenericAndOrArrayType(string typeName, DebugContext debugContext)
		{
			if (!TryParseGenericAndOrArrayTypeName(typeName, out var actualTypeName, out var isGeneric, out var genericArgNames, out var isArray, out var arrayRank))
			{
				return null;
			}
			Type type = BindToType(actualTypeName, debugContext);
			if (type == null)
			{
				return null;
			}
			if (isGeneric)
			{
				if (!type.IsGenericType)
				{
					return null;
				}
				using Cache<List<Type>> argsCache = Cache<List<Type>>.Claim();
				List<Type> args = argsCache.Value;
				args.Clear();
				for (int i = 0; i < genericArgNames.Count; i++)
				{
					Type arg = BindToType(genericArgNames[i], debugContext);
					if (arg == null)
					{
						return null;
					}
					args.Add(arg);
				}
				Type[] argsArray = args.ToArray();
				if (!type.AreGenericConstraintsSatisfiedBy(argsArray))
				{
					if (debugContext != null)
					{
						string argsStr = "";
						Type[] array = argsArray;
						foreach (Type arg2 in array)
						{
							if (argsStr != "")
							{
								argsStr += ", ";
							}
							argsStr += arg2.GetNiceFullName();
						}
						debugContext.LogWarning("Deserialization type lookup failure: The generic type arguments '" + argsStr + "' do not satisfy the generic constraints of generic type definition '" + type.GetNiceFullName() + "'. All this parsed from the full type name string: '" + typeName + "'");
					}
					return null;
				}
				type = type.MakeGenericType(argsArray);
				args.Clear();
			}
			if (isArray)
			{
				type = ((arrayRank != 1) ? type.MakeArrayType(arrayRank) : type.MakeArrayType());
			}
			return type;
		}

		private static bool TryParseGenericAndOrArrayTypeName(string typeName, out string actualTypeName, out bool isGeneric, out List<string> genericArgNames, out bool isArray, out int arrayRank)
		{
			isGeneric = false;
			isArray = false;
			arrayRank = 0;
			bool parsingGenericArguments = false;
			genericArgNames = null;
			actualTypeName = null;
			for (int i = 0; i < typeName.Length; i++)
			{
				if (typeName[i] == '[')
				{
					char next = Peek(typeName, i, 1);
					if (next == ',' || next == ']')
					{
						if (actualTypeName == null)
						{
							actualTypeName = typeName.Substring(0, i);
						}
						isArray = true;
						arrayRank = 1;
						i++;
						if (next != ',')
						{
							continue;
						}
						while (true)
						{
							switch (next)
							{
							case ',':
								goto IL_005f;
							default:
								return false;
							case ']':
								break;
							}
							break;
							IL_005f:
							arrayRank++;
							next = Peek(typeName, i, 1);
							i++;
						}
					}
					else if (!isGeneric)
					{
						actualTypeName = typeName.Substring(0, i);
						isGeneric = true;
						parsingGenericArguments = true;
						genericArgNames = new List<string>();
					}
					else
					{
						if (!isGeneric || !ReadGenericArg(typeName, ref i, out var argName))
						{
							return false;
						}
						genericArgNames.Add(argName);
					}
				}
				else if (typeName[i] == ']')
				{
					if (!parsingGenericArguments)
					{
						return false;
					}
					parsingGenericArguments = false;
				}
				else if (typeName[i] == ',' && !parsingGenericArguments)
				{
					actualTypeName += typeName.Substring(i);
					break;
				}
			}
			return isArray | isGeneric;
		}

		private static char Peek(string str, int i, int ahead)
		{
			if (i + ahead < str.Length)
			{
				return str[i + ahead];
			}
			return '\0';
		}

		private static bool ReadGenericArg(string typeName, ref int i, out string argName)
		{
			argName = null;
			if (typeName[i] != '[')
			{
				return false;
			}
			int start = i + 1;
			int genericDepth = 0;
			while (i < typeName.Length)
			{
				if (typeName[i] == '[')
				{
					genericDepth++;
				}
				else if (typeName[i] == ']')
				{
					genericDepth--;
					if (genericDepth == 0)
					{
						int length = i - start;
						argName = typeName.Substring(start, length);
						return true;
					}
				}
				i++;
			}
			return false;
		}
	}
}

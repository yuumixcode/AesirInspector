using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Sirenix.Serialization.Utilities
{
	/// <summary>
	/// Type method extensions.
	/// </summary>
	internal static class TypeExtensions
	{
		private static readonly Func<float, float, bool> FloatEqualityComparerFunc = FloatEqualityComparer;

		private static readonly Func<double, double, bool> DoubleEqualityComparerFunc = DoubleEqualityComparer;

		private static readonly Func<Quaternion, Quaternion, bool> QuaternionEqualityComparerFunc = QuaternionEqualityComparer;

		private static readonly object GenericConstraintsSatisfaction_LOCK = new object();

		private static readonly Dictionary<Type, Type> GenericConstraintsSatisfactionInferredParameters = new Dictionary<Type, Type>();

		private static readonly Dictionary<Type, Type> GenericConstraintsSatisfactionResolvedMap = new Dictionary<Type, Type>();

		private static readonly HashSet<Type> GenericConstraintsSatisfactionProcessedParams = new HashSet<Type>();

		private static readonly HashSet<Type> GenericConstraintsSatisfactionTypesToCheck = new HashSet<Type>();

		private static readonly List<Type> GenericConstraintsSatisfactionTypesToCheck_ToAdd = new List<Type>();

		private static readonly Type GenericListInterface = typeof(IList<>);

		private static readonly Type GenericCollectionInterface = typeof(ICollection<>);

		private static readonly object WeaklyTypedTypeCastDelegates_LOCK = new object();

		private static readonly object StronglyTypedTypeCastDelegates_LOCK = new object();

		private static readonly DoubleLookupDictionary<Type, Type, Func<object, object>> WeaklyTypedTypeCastDelegates = new DoubleLookupDictionary<Type, Type, Func<object, object>>();

		private static readonly DoubleLookupDictionary<Type, Type, Delegate> StronglyTypedTypeCastDelegates = new DoubleLookupDictionary<Type, Type, Delegate>();

		private static readonly Type[] TwoLengthTypeArray_Cached = new Type[2];

		private static readonly Stack<Type> GenericArgumentsContainsTypes_ArgsToCheckCached = new Stack<Type>();

		private static HashSet<string> ReservedCSharpKeywords = new HashSet<string>
		{
			"abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
			"class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum",
			"event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto",
			"if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace",
			"new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public",
			"readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string",
			"struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
			"unsafe", "ushort", "using", "static", "void", "volatile", "while", "in", "get", "set",
			"var"
		};

		/// <summary>
		/// Type name alias lookup.
		/// TypeNameAlternatives["Single"] will give you "float", "UInt16" will give you "ushort", "Boolean[]" will give you "bool[]" etc..
		/// </summary>
		public static readonly Dictionary<string, string> TypeNameAlternatives = new Dictionary<string, string>
		{
			{ "Single", "float" },
			{ "Double", "double" },
			{ "SByte", "sbyte" },
			{ "Int16", "short" },
			{ "Int32", "int" },
			{ "Int64", "long" },
			{ "Byte", "byte" },
			{ "UInt16", "ushort" },
			{ "UInt32", "uint" },
			{ "UInt64", "ulong" },
			{ "Decimal", "decimal" },
			{ "String", "string" },
			{ "Char", "char" },
			{ "Boolean", "bool" },
			{ "Single[]", "float[]" },
			{ "Double[]", "double[]" },
			{ "SByte[]", "sbyte[]" },
			{ "Int16[]", "short[]" },
			{ "Int32[]", "int[]" },
			{ "Int64[]", "long[]" },
			{ "Byte[]", "byte[]" },
			{ "UInt16[]", "ushort[]" },
			{ "UInt32[]", "uint[]" },
			{ "UInt64[]", "ulong[]" },
			{ "Decimal[]", "decimal[]" },
			{ "String[]", "string[]" },
			{ "Char[]", "char[]" },
			{ "Boolean[]", "bool[]" }
		};

		private static readonly object CachedNiceNames_LOCK = new object();

		private static readonly Dictionary<Type, string> CachedNiceNames = new Dictionary<Type, string>();

		private static readonly Type VoidPointerType = typeof(void).MakePointerType();

		private static readonly Dictionary<Type, HashSet<Type>> PrimitiveImplicitCasts = new Dictionary<Type, HashSet<Type>>
		{
			{
				typeof(long),
				new HashSet<Type>
				{
					typeof(float),
					typeof(double),
					typeof(decimal)
				}
			},
			{
				typeof(int),
				new HashSet<Type>
				{
					typeof(long),
					typeof(float),
					typeof(double),
					typeof(decimal)
				}
			},
			{
				typeof(short),
				new HashSet<Type>
				{
					typeof(int),
					typeof(long),
					typeof(float),
					typeof(double),
					typeof(decimal)
				}
			},
			{
				typeof(sbyte),
				new HashSet<Type>
				{
					typeof(short),
					typeof(int),
					typeof(long),
					typeof(float),
					typeof(double),
					typeof(decimal)
				}
			},
			{
				typeof(ulong),
				new HashSet<Type>
				{
					typeof(float),
					typeof(double),
					typeof(decimal)
				}
			},
			{
				typeof(uint),
				new HashSet<Type>
				{
					typeof(long),
					typeof(ulong),
					typeof(float),
					typeof(double),
					typeof(decimal)
				}
			},
			{
				typeof(ushort),
				new HashSet<Type>
				{
					typeof(int),
					typeof(uint),
					typeof(long),
					typeof(ulong),
					typeof(float),
					typeof(double),
					typeof(decimal)
				}
			},
			{
				typeof(byte),
				new HashSet<Type>
				{
					typeof(short),
					typeof(ushort),
					typeof(int),
					typeof(uint),
					typeof(long),
					typeof(ulong),
					typeof(float),
					typeof(double),
					typeof(decimal)
				}
			},
			{
				typeof(char),
				new HashSet<Type>
				{
					typeof(ushort),
					typeof(int),
					typeof(uint),
					typeof(long),
					typeof(ulong),
					typeof(float),
					typeof(double),
					typeof(decimal)
				}
			},
			{
				typeof(bool),
				new HashSet<Type>()
			},
			{
				typeof(decimal),
				new HashSet<Type>()
			},
			{
				typeof(float),
				new HashSet<Type> { typeof(double) }
			},
			{
				typeof(double),
				new HashSet<Type>()
			},
			{
				typeof(IntPtr),
				new HashSet<Type>()
			},
			{
				typeof(UIntPtr),
				new HashSet<Type>()
			},
			{
				VoidPointerType,
				new HashSet<Type>()
			}
		};

		private static readonly HashSet<Type> ExplicitCastIntegrals = new HashSet<Type>
		{
			typeof(long),
			typeof(int),
			typeof(short),
			typeof(sbyte),
			typeof(ulong),
			typeof(uint),
			typeof(ushort),
			typeof(byte),
			typeof(char),
			typeof(decimal),
			typeof(float),
			typeof(double),
			typeof(IntPtr),
			typeof(UIntPtr)
		};

		private static string GetCachedNiceName(Type type)
		{
			string result;
			lock (CachedNiceNames_LOCK)
			{
				if (!CachedNiceNames.TryGetValue(type, out result))
				{
					result = CreateNiceName(type);
					CachedNiceNames.Add(type, result);
				}
			}
			return result;
		}

		private static string CreateNiceName(Type type)
		{
			if (type.IsArray)
			{
				int rank = type.GetArrayRank();
				return type.GetElementType().GetNiceName() + ((rank == 1) ? "[]" : "[,]");
			}
			if (type.InheritsFrom(typeof(Nullable<>)))
			{
				return type.GetGenericArguments()[0].GetNiceName() + "?";
			}
			if (type.IsByRef)
			{
				return "ref " + type.GetElementType().GetNiceName();
			}
			if (type.IsGenericParameter || !type.IsGenericType)
			{
				return type.TypeNameGauntlet();
			}
			StringBuilder builder = new StringBuilder();
			string name = type.Name;
			int index = name.IndexOf("`");
			if (index != -1)
			{
				builder.Append(name.Substring(0, index));
			}
			else
			{
				builder.Append(name);
			}
			builder.Append('<');
			Type[] args = type.GetGenericArguments();
			for (int i = 0; i < args.Length; i++)
			{
				Type arg = args[i];
				if (i != 0)
				{
					builder.Append(", ");
				}
				builder.Append(arg.GetNiceName());
			}
			builder.Append('>');
			return builder.ToString();
		}

		internal static bool HasCastDefined(this Type from, Type to, bool requireImplicitCast)
		{
			if (from.IsEnum)
			{
				return Enum.GetUnderlyingType(from).IsCastableTo(to);
			}
			if (to.IsEnum)
			{
				return Enum.GetUnderlyingType(to).IsCastableTo(from);
			}
			if ((from.IsPrimitive || from == VoidPointerType) && (to.IsPrimitive || to == VoidPointerType))
			{
				if (requireImplicitCast)
				{
					return PrimitiveImplicitCasts[from].Contains(to);
				}
				if (from == typeof(IntPtr))
				{
					if (to == typeof(UIntPtr))
					{
						return false;
					}
					if (to == VoidPointerType)
					{
						return true;
					}
				}
				else if (from == typeof(UIntPtr))
				{
					if (to == typeof(IntPtr))
					{
						return false;
					}
					if (to == VoidPointerType)
					{
						return true;
					}
				}
				if (ExplicitCastIntegrals.Contains(from))
				{
					return ExplicitCastIntegrals.Contains(to);
				}
				return false;
			}
			return from.GetCastMethod(to, requireImplicitCast) != null;
		}

		/// <summary>
		/// Checks whether a given string is a valid CSharp identifier name. This also checks full type names including namespaces.
		/// </summary>
		/// <param name="identifier">The identifier to check.</param>
		public static bool IsValidIdentifier(string identifier)
		{
			if (identifier == null || identifier.Length == 0)
			{
				return false;
			}
			int dotIndex = identifier.IndexOf('.');
			if (dotIndex >= 0)
			{
				string[] identifiers = identifier.Split(new char[1] { '.' });
				for (int i = 0; i < identifiers.Length; i++)
				{
					if (!IsValidIdentifier(identifiers[i]))
					{
						return false;
					}
				}
				return true;
			}
			if (ReservedCSharpKeywords.Contains(identifier))
			{
				return false;
			}
			if (!IsValidIdentifierStartCharacter(identifier[0]))
			{
				return false;
			}
			for (int j = 1; j < identifier.Length; j++)
			{
				if (!IsValidIdentifierPartCharacter(identifier[j]))
				{
					return false;
				}
			}
			return true;
		}

		private static bool IsValidIdentifierStartCharacter(char c)
		{
			if ((c < 'a' || c > 'z') && (c < 'A' || c > 'Z') && c != '_' && c != '@')
			{
				return char.IsLetter(c);
			}
			return true;
		}

		private static bool IsValidIdentifierPartCharacter(char c)
		{
			if ((c < 'a' || c > 'z') && (c < 'A' || c > 'Z'))
			{
				switch (c)
				{
				default:
					return char.IsLetter(c);
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				case '_':
					break;
				}
			}
			return true;
		}

		/// <summary>
		/// Determines whether a type can be casted to another type.
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <param name="requireImplicitCast">if set to <c>true</c> an implicit or explicit operator must be defined on the given type.</param>
		public static bool IsCastableTo(this Type from, Type to, bool requireImplicitCast = false)
		{
			if (from == null)
			{
				throw new ArgumentNullException("from");
			}
			if (to == null)
			{
				throw new ArgumentNullException("to");
			}
			if (from == to)
			{
				return true;
			}
			if (!to.IsAssignableFrom(from))
			{
				return from.HasCastDefined(to, requireImplicitCast);
			}
			return true;
		}

		/// <summary>
		/// If a type can be casted to another type, this provides a function to manually convert the type.
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <param name="requireImplicitCast">if set to <c>true</c> an implicit or explicit operator must be defined on the given type.</param>
		public static Func<object, object> GetCastMethodDelegate(this Type from, Type to, bool requireImplicitCast = false)
		{
			Func<object, object> result;
			lock (WeaklyTypedTypeCastDelegates_LOCK)
			{
				if (!WeaklyTypedTypeCastDelegates.TryGetInnerValue(from, to, out result))
				{
					MethodInfo method = from.GetCastMethod(to, requireImplicitCast);
					if (method != null)
					{
						result = (object obj) => method.Invoke(null, new object[1] { obj });
					}
					WeaklyTypedTypeCastDelegates.AddInner(from, to, result);
				}
			}
			return result;
		}

		/// <summary>
		/// If a type can be casted to another type, this provides a function to manually convert the type.
		/// </summary>
		/// <param name="requireImplicitCast">if set to <c>true</c> an implicit or explicit operator must be defined on the given type.</param>
		public static Func<TFrom, TTo> GetCastMethodDelegate<TFrom, TTo>(bool requireImplicitCast = false)
		{
			Delegate del;
			lock (StronglyTypedTypeCastDelegates_LOCK)
			{
				if (!StronglyTypedTypeCastDelegates.TryGetInnerValue(typeof(TFrom), typeof(TTo), out del))
				{
					MethodInfo method = typeof(TFrom).GetCastMethod(typeof(TTo), requireImplicitCast);
					if (method != null)
					{
						del = Delegate.CreateDelegate(typeof(Func<TFrom, TTo>), method);
					}
					StronglyTypedTypeCastDelegates.AddInner(typeof(TFrom), typeof(TTo), del);
				}
			}
			return (Func<TFrom, TTo>)del;
		}

		/// <summary>
		/// If a type can be casted to another type, this provides the method info of the method in charge of converting the type.
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <param name="requireImplicitCast">if set to <c>true</c> an implicit or explicit operator must be defined on the given type.</param>
		public static MethodInfo GetCastMethod(this Type from, Type to, bool requireImplicitCast = false)
		{
			IEnumerable<MethodInfo> fromMethods = from.GetAllMembers<MethodInfo>(BindingFlags.Static | BindingFlags.Public);
			foreach (MethodInfo method in fromMethods)
			{
				if ((method.Name == "op_Implicit" || (!requireImplicitCast && method.Name == "op_Explicit")) && method.GetParameters()[0].ParameterType.IsAssignableFrom(from) && to.IsAssignableFrom(method.ReturnType))
				{
					return method;
				}
			}
			IEnumerable<MethodInfo> toMethods = to.GetAllMembers<MethodInfo>(BindingFlags.Static | BindingFlags.Public);
			foreach (MethodInfo method2 in toMethods)
			{
				if ((method2.Name == "op_Implicit" || (!requireImplicitCast && method2.Name == "op_Explicit")) && method2.GetParameters()[0].ParameterType.IsAssignableFrom(from) && to.IsAssignableFrom(method2.ReturnType))
				{
					return method2;
				}
			}
			return null;
		}

		private static bool FloatEqualityComparer(float a, float b)
		{
			if (float.IsNaN(a) && float.IsNaN(b))
			{
				return true;
			}
			return a == b;
		}

		private static bool DoubleEqualityComparer(double a, double b)
		{
			if (double.IsNaN(a) && double.IsNaN(b))
			{
				return true;
			}
			return a == b;
		}

		private static bool QuaternionEqualityComparer(Quaternion a, Quaternion b)
		{
			if (a.x == b.x && a.y == b.y && a.z == b.z)
			{
				return a.w == b.w;
			}
			return false;
		}

		/// <summary>
		/// Gets an equality comparer delegate used to compare the equality of values of a given type. In order, this will be:
		///
		/// 1. The == operator, if one is defined on the type.
		/// 2. A delegate that uses <see cref="T:System.IEquatable`1" />, if the type implements that interface.
		/// 3. .NET's own <see cref="P:System.Collections.Generic.EqualityComparer`1.Default" />
		/// </summary>
		/// <remarks>
		/// <para>Note that in the special case of the type <see cref="T:UnityEngine.Quaternion" />, a special equality comparer is returned that only checks whether all the Quaternion components are equal.</para>
		/// <para>This is because, by default, Quaternion's equality operator is broken when operating on invalid quaternions; "default(Quaternion) == default(Quaternion)" evaluates to false, and this causes a multitude of problems.</para>
		/// <para>Special delegates are also returned for float and double, that consider float.NaN to be equal to float.NaN, and double.NaN to be equal to double.NaN.</para>
		/// </remarks>
		public static Func<T, T, bool> GetEqualityComparerDelegate<T>()
		{
			if (typeof(T) == typeof(float))
			{
				return (Func<T, T, bool>)(object)FloatEqualityComparerFunc;
			}
			if (typeof(T) == typeof(double))
			{
				return (Func<T, T, bool>)(object)DoubleEqualityComparerFunc;
			}
			if (typeof(T) == typeof(Quaternion))
			{
				return (Func<T, T, bool>)(object)QuaternionEqualityComparerFunc;
			}
			Func<T, T, bool> result = null;
			if (typeof(IEquatable<T>).IsAssignableFrom(typeof(T)))
			{
				result = ((!typeof(T).IsValueType) ? ((Func<T, T, bool>)delegate(T a, T b)
				{
					if ((object)a == (object)b)
					{
						return true;
					}
					return a != null && ((IEquatable<T>)(object)a).Equals(b);
				}) : ((Func<T, T, bool>)((T a, T b) => ((IEquatable<T>)(object)a).Equals(b))));
			}
			else
			{
				Type currentType = typeof(T);
				while (currentType != null && currentType != typeof(object))
				{
					MethodInfo equalityMethod = currentType.GetOperatorMethod(Operator.Equality, currentType, currentType);
					if (equalityMethod != null)
					{
						result = (Func<T, T, bool>)Delegate.CreateDelegate(typeof(Func<T, T, bool>), equalityMethod, throwOnBindFailure: true);
						break;
					}
					currentType = currentType.BaseType;
				}
			}
			if (result == null)
			{
				EqualityComparer<T> comparer = EqualityComparer<T>.Default;
				result = comparer.Equals;
			}
			return result;
		}

		/// <summary>
		/// Gets the first attribute of type T. Returns null in the no attribute of type T was found.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="inherit">If true, specifies to also search the ancestors of element for custom attributes.</param>
		public static T GetAttribute<T>(this Type type, bool inherit) where T : Attribute
		{
			object[] attrs = type.GetCustomAttributes(typeof(T), inherit);
			if (attrs.Length == 0)
			{
				return null;
			}
			return (T)attrs[0];
		}

		/// <summary>
		/// Determines whether a type implements or inherits from another type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="to">To.</param>
		public static bool ImplementsOrInherits(this Type type, Type to)
		{
			return to.IsAssignableFrom(type);
		}

		/// <summary>
		/// Determines whether a type implements an open generic interface or class such as IList&lt;&gt; or List&lt;&gt;.
		/// </summary>
		/// <param name="candidateType">Type of the candidate.</param>
		/// <param name="openGenericType">Type of the open generic type.</param>
		/// <returns></returns>
		public static bool ImplementsOpenGenericType(this Type candidateType, Type openGenericType)
		{
			if (openGenericType.IsInterface)
			{
				return candidateType.ImplementsOpenGenericInterface(openGenericType);
			}
			return candidateType.ImplementsOpenGenericClass(openGenericType);
		}

		/// <summary>
		/// Determines whether a type implements an open generic interface such as IList&lt;&gt;.
		/// </summary>
		/// <param name="candidateType">Type of the candidate.</param>
		/// <param name="openGenericInterfaceType">Type of the open generic interface.</param>
		/// <exception cref="T:System.ArgumentNullException"></exception>
		/// <exception cref="T:System.ArgumentException">Type " + openGenericInterfaceType.Name + " is not a generic type definition and an interface.</exception>
		public static bool ImplementsOpenGenericInterface(this Type candidateType, Type openGenericInterfaceType)
		{
			if (candidateType == openGenericInterfaceType)
			{
				return true;
			}
			if (candidateType.IsGenericType && candidateType.GetGenericTypeDefinition() == openGenericInterfaceType)
			{
				return true;
			}
			Type[] interfaces = candidateType.GetInterfaces();
			for (int i = 0; i < interfaces.Length; i++)
			{
				if (interfaces[i].ImplementsOpenGenericInterface(openGenericInterfaceType))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Determines whether a type implements an open generic class such as List&lt;&gt;.
		/// </summary>
		/// <param name="candidateType">Type of the candidate.</param>
		/// <param name="openGenericType">Type of the open generic interface.</param>
		public static bool ImplementsOpenGenericClass(this Type candidateType, Type openGenericType)
		{
			if (candidateType.IsGenericType && candidateType.GetGenericTypeDefinition() == openGenericType)
			{
				return true;
			}
			Type baseType = candidateType.BaseType;
			if (baseType != null && baseType.ImplementsOpenGenericClass(openGenericType))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets the generic arguments of an inherited open generic class or interface.
		/// </summary>
		/// <param name="candidateType">Type of the candidate.</param>
		/// <param name="openGenericType">The open generic type to get the arguments of.</param>
		public static Type[] GetArgumentsOfInheritedOpenGenericType(this Type candidateType, Type openGenericType)
		{
			if (openGenericType.IsInterface)
			{
				return candidateType.GetArgumentsOfInheritedOpenGenericInterface(openGenericType);
			}
			return candidateType.GetArgumentsOfInheritedOpenGenericClass(openGenericType);
		}

		/// <summary>
		/// Gets the generic arguments of an inherited open generic class.
		/// </summary>
		/// <param name="candidateType">Type of the candidate.</param>
		/// <param name="openGenericType">Type of the open generic class.</param>
		public static Type[] GetArgumentsOfInheritedOpenGenericClass(this Type candidateType, Type openGenericType)
		{
			if (candidateType.IsGenericType && candidateType.GetGenericTypeDefinition() == openGenericType)
			{
				return candidateType.GetGenericArguments();
			}
			Type baseType = candidateType.BaseType;
			if (baseType != null)
			{
				return baseType.GetArgumentsOfInheritedOpenGenericClass(openGenericType);
			}
			return null;
		}

		/// <summary>
		/// Gets the generic arguments of an inherited open generic interface.
		/// </summary>
		/// <param name="candidateType">Type of the candidate.</param>
		/// <param name="openGenericInterfaceType">Type of the open generic interface.</param>
		public static Type[] GetArgumentsOfInheritedOpenGenericInterface(this Type candidateType, Type openGenericInterfaceType)
		{
			if ((openGenericInterfaceType == GenericListInterface || openGenericInterfaceType == GenericCollectionInterface) && candidateType.IsArray)
			{
				return new Type[1] { candidateType.GetElementType() };
			}
			if (candidateType == openGenericInterfaceType)
			{
				return candidateType.GetGenericArguments();
			}
			if (candidateType.IsGenericType && candidateType.GetGenericTypeDefinition() == openGenericInterfaceType)
			{
				return candidateType.GetGenericArguments();
			}
			Type[] interfaces = candidateType.GetInterfaces();
			foreach (Type @interface in interfaces)
			{
				if (@interface.IsGenericType)
				{
					Type[] result = @interface.GetArgumentsOfInheritedOpenGenericInterface(openGenericInterfaceType);
					if (result != null)
					{
						return result;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the MethodInfo of a specific operator kind, with the given left and right operands. This overload is *far* faster than any of the other GetOperatorMethod implementations, and should be used whenever possible.
		/// </summary>
		public static MethodInfo GetOperatorMethod(this Type type, Operator op, Type leftOperand, Type rightOperand)
		{
			string methodName;
			switch (op)
			{
			case Operator.Equality:
				methodName = "op_Equality";
				break;
			case Operator.Inequality:
				methodName = "op_Inequality";
				break;
			case Operator.Addition:
				methodName = "op_Addition";
				break;
			case Operator.Subtraction:
				methodName = "op_Subtraction";
				break;
			case Operator.Multiply:
				methodName = "op_Multiply";
				break;
			case Operator.Division:
				methodName = "op_Division";
				break;
			case Operator.LessThan:
				methodName = "op_LessThan";
				break;
			case Operator.GreaterThan:
				methodName = "op_GreaterThan";
				break;
			case Operator.LessThanOrEqual:
				methodName = "op_LessThanOrEqual";
				break;
			case Operator.GreaterThanOrEqual:
				methodName = "op_GreaterThanOrEqual";
				break;
			case Operator.Modulus:
				methodName = "op_Modulus";
				break;
			case Operator.RightShift:
				methodName = "op_RightShift";
				break;
			case Operator.LeftShift:
				methodName = "op_LeftShift";
				break;
			case Operator.BitwiseAnd:
				methodName = "op_BitwiseAnd";
				break;
			case Operator.BitwiseOr:
				methodName = "op_BitwiseOr";
				break;
			case Operator.ExclusiveOr:
				methodName = "op_ExclusiveOr";
				break;
			case Operator.BitwiseComplement:
				methodName = "op_OnesComplement";
				break;
			case Operator.LogicalNot:
				methodName = "op_LogicalNot";
				break;
			case Operator.LogicalAnd:
			case Operator.LogicalOr:
				return null;
			default:
				throw new NotImplementedException();
			}
			Type[] types = TwoLengthTypeArray_Cached;
			lock (types)
			{
				types[0] = leftOperand;
				types[1] = rightOperand;
				try
				{
					MethodInfo result = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
					if (result != null && result.ReturnType != typeof(bool))
					{
						return null;
					}
					return result;
				}
				catch (AmbiguousMatchException)
				{
					MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					foreach (MethodInfo method in methods)
					{
						if (!(method.Name != methodName) && !(method.ReturnType != typeof(bool)))
						{
							ParameterInfo[] parameters = method.GetParameters();
							if (parameters.Length == 2 && parameters[0].ParameterType.IsAssignableFrom(leftOperand) && parameters[1].ParameterType.IsAssignableFrom(rightOperand))
							{
								return method;
							}
						}
					}
					return null;
				}
			}
		}

		/// <summary>
		/// Gets the MethodInfo of a specific operator type.
		/// </summary>
		public static MethodInfo GetOperatorMethod(this Type type, Operator op)
		{
			string methodName;
			switch (op)
			{
			case Operator.Equality:
				methodName = "op_Equality";
				break;
			case Operator.Inequality:
				methodName = "op_Inequality";
				break;
			case Operator.Addition:
				methodName = "op_Addition";
				break;
			case Operator.Subtraction:
				methodName = "op_Subtraction";
				break;
			case Operator.Multiply:
				methodName = "op_Multiply";
				break;
			case Operator.Division:
				methodName = "op_Division";
				break;
			case Operator.LessThan:
				methodName = "op_LessThan";
				break;
			case Operator.GreaterThan:
				methodName = "op_GreaterThan";
				break;
			case Operator.LessThanOrEqual:
				methodName = "op_LessThanOrEqual";
				break;
			case Operator.GreaterThanOrEqual:
				methodName = "op_GreaterThanOrEqual";
				break;
			case Operator.Modulus:
				methodName = "op_Modulus";
				break;
			case Operator.RightShift:
				methodName = "op_RightShift";
				break;
			case Operator.LeftShift:
				methodName = "op_LeftShift";
				break;
			case Operator.BitwiseAnd:
				methodName = "op_BitwiseAnd";
				break;
			case Operator.BitwiseOr:
				methodName = "op_BitwiseOr";
				break;
			case Operator.ExclusiveOr:
				methodName = "op_ExclusiveOr";
				break;
			case Operator.BitwiseComplement:
				methodName = "op_OnesComplement";
				break;
			case Operator.LogicalNot:
				methodName = "op_LogicalNot";
				break;
			case Operator.LogicalAnd:
			case Operator.LogicalOr:
				return null;
			default:
				throw new NotImplementedException();
			}
			return type.GetAllMembers<MethodInfo>(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault((MethodInfo m) => m.Name == methodName);
		}

		/// <summary>
		/// Gets the MethodInfo of a specific operator type.
		/// </summary>
		public static MethodInfo[] GetOperatorMethods(this Type type, Operator op)
		{
			string methodName;
			switch (op)
			{
			case Operator.Equality:
				methodName = "op_Equality";
				break;
			case Operator.Inequality:
				methodName = "op_Inequality";
				break;
			case Operator.Addition:
				methodName = "op_Addition";
				break;
			case Operator.Subtraction:
				methodName = "op_Subtraction";
				break;
			case Operator.Multiply:
				methodName = "op_Multiply";
				break;
			case Operator.Division:
				methodName = "op_Division";
				break;
			case Operator.LessThan:
				methodName = "op_LessThan";
				break;
			case Operator.GreaterThan:
				methodName = "op_GreaterThan";
				break;
			case Operator.LessThanOrEqual:
				methodName = "op_LessThanOrEqual";
				break;
			case Operator.GreaterThanOrEqual:
				methodName = "op_GreaterThanOrEqual";
				break;
			case Operator.Modulus:
				methodName = "op_Modulus";
				break;
			case Operator.RightShift:
				methodName = "op_RightShift";
				break;
			case Operator.LeftShift:
				methodName = "op_LeftShift";
				break;
			case Operator.BitwiseAnd:
				methodName = "op_BitwiseAnd";
				break;
			case Operator.BitwiseOr:
				methodName = "op_BitwiseOr";
				break;
			case Operator.ExclusiveOr:
				methodName = "op_ExclusiveOr";
				break;
			case Operator.BitwiseComplement:
				methodName = "op_OnesComplement";
				break;
			case Operator.LogicalNot:
				methodName = "op_LogicalNot";
				break;
			case Operator.LogicalAnd:
			case Operator.LogicalOr:
				return null;
			default:
				throw new NotImplementedException();
			}
			return (from x in type.GetAllMembers<MethodInfo>(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				where x.Name == methodName
				select x).ToArray();
		}

		/// <summary>
		/// Gets all members from a given type, including members from all base types if the <see cref="F:System.Reflection.BindingFlags.DeclaredOnly" /> flag isn't set.
		/// </summary>
		public static IEnumerable<MemberInfo> GetAllMembers(this Type type, BindingFlags flags = BindingFlags.Default)
		{
			Type currentType = type;
			if ((flags & BindingFlags.DeclaredOnly) == BindingFlags.DeclaredOnly)
			{
				MemberInfo[] members = currentType.GetMembers(flags);
				for (int i = 0; i < members.Length; i++)
				{
					yield return members[i];
				}
				yield break;
			}
			flags |= BindingFlags.DeclaredOnly;
			do
			{
				MemberInfo[] members = currentType.GetMembers(flags);
				for (int i = 0; i < members.Length; i++)
				{
					yield return members[i];
				}
				currentType = currentType.BaseType;
			}
			while (currentType != null);
		}

		/// <summary>
		/// Gets all members from a given type, including members from all base types.
		/// </summary>
		public static IEnumerable<MemberInfo> GetAllMembers(this Type type, string name, BindingFlags flags = BindingFlags.Default)
		{
			foreach (MemberInfo member in type.GetAllMembers(flags))
			{
				if (!(member.Name != name))
				{
					yield return member;
				}
			}
		}

		/// <summary>
		/// Gets all members of a specific type from a type, including members from all base types, if the <see cref="F:System.Reflection.BindingFlags.DeclaredOnly" /> flag isn't set.
		/// </summary>
		public static IEnumerable<T> GetAllMembers<T>(this Type type, BindingFlags flags = BindingFlags.Default) where T : MemberInfo
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (type == typeof(object))
			{
				yield break;
			}
			Type currentType = type;
			if ((flags & BindingFlags.DeclaredOnly) == BindingFlags.DeclaredOnly)
			{
				MemberInfo[] members = currentType.GetMembers(flags);
				foreach (MemberInfo member in members)
				{
					T found = member as T;
					if (found != null)
					{
						yield return found;
					}
				}
				yield break;
			}
			flags |= BindingFlags.DeclaredOnly;
			do
			{
				MemberInfo[] members = currentType.GetMembers(flags);
				foreach (MemberInfo member2 in members)
				{
					T found2 = member2 as T;
					if (found2 != null)
					{
						yield return found2;
					}
				}
				currentType = currentType.BaseType;
			}
			while (currentType != null);
		}

		/// <summary>
		/// Gets the generic type definition of an open generic base type.
		/// </summary>
		public static Type GetGenericBaseType(this Type type, Type baseType)
		{
			int count;
			return type.GetGenericBaseType(baseType, out count);
		}

		/// <summary>
		/// Gets the generic type definition of an open generic base type.
		/// </summary>
		public static Type GetGenericBaseType(this Type type, Type baseType, out int depthCount)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (baseType == null)
			{
				throw new ArgumentNullException("baseType");
			}
			if (!baseType.IsGenericType)
			{
				throw new ArgumentException("Type " + baseType.Name + " is not a generic type.");
			}
			if (!type.InheritsFrom(baseType))
			{
				throw new ArgumentException("Type " + type.Name + " does not inherit from " + baseType.Name + ".");
			}
			Type t = type;
			depthCount = 0;
			while (t != null && (!t.IsGenericType || t.GetGenericTypeDefinition() != baseType))
			{
				depthCount++;
				t = t.BaseType;
			}
			if (t == null)
			{
				throw new ArgumentException(type.Name + " is assignable from " + baseType.Name + ", but base type was not found?");
			}
			return t;
		}

		/// <summary>
		/// Returns a lazy enumerable of all the base types of this type including interfaces and classes
		/// </summary>
		public static IEnumerable<Type> GetBaseTypes(this Type type, bool includeSelf = false)
		{
			IEnumerable<Type> result = type.GetBaseClasses(includeSelf).Concat(type.GetInterfaces());
			if (includeSelf && type.IsInterface)
			{
				result.Concat(new Type[1] { type });
			}
			return result;
		}

		/// <summary>
		/// Returns a lazy enumerable of all the base classes of this type
		/// </summary>
		public static IEnumerable<Type> GetBaseClasses(this Type type, bool includeSelf = false)
		{
			if (!(type == null) && !(type.BaseType == null))
			{
				if (includeSelf)
				{
					yield return type;
				}
				Type current = type.BaseType;
				while (current != null)
				{
					yield return current;
					current = current.BaseType;
				}
			}
		}

		/// <summary>
		/// Used to filter out unwanted type names. Ex "int" instead of "Int32"
		/// </summary>
		private static string TypeNameGauntlet(this Type type)
		{
			string typeName = type.Name;
			string altTypeName = string.Empty;
			if (TypeNameAlternatives.TryGetValue(typeName, out altTypeName))
			{
				typeName = altTypeName;
			}
			return typeName;
		}

		/// <summary>
		/// Returns a nicely formatted name of a type.
		/// </summary>
		public static string GetNiceName(this Type type)
		{
			if (type.IsNested && !type.IsGenericParameter)
			{
				return type.DeclaringType.GetNiceName() + "." + GetCachedNiceName(type);
			}
			return GetCachedNiceName(type);
		}

		/// <summary>
		/// Returns a nicely formatted full name of a type.
		/// </summary>
		public static string GetNiceFullName(this Type type)
		{
			if (type.IsNested && !type.IsGenericParameter)
			{
				return type.DeclaringType.GetNiceFullName() + "." + GetCachedNiceName(type);
			}
			string result = GetCachedNiceName(type);
			if (type.Namespace != null)
			{
				result = type.Namespace + "." + result;
			}
			return result;
		}

		/// <summary>
		/// Gets the name of the compilable nice.
		/// </summary>
		/// <param name="type">The type.</param>
		public static string GetCompilableNiceName(this Type type)
		{
			return type.GetNiceName().Replace('<', '_').Replace('>', '_')
				.TrimEnd(new char[1] { '_' });
		}

		/// <summary>
		/// Gets the full name of the compilable nice.
		/// </summary>
		/// <param name="type">The type.</param>
		public static string GetCompilableNiceFullName(this Type type)
		{
			return type.GetNiceFullName().Replace('<', '_').Replace('>', '_')
				.TrimEnd(new char[1] { '_' });
		}

		/// <summary>
		/// Returns the first found custom attribute of type T on this type
		/// Returns null if none was found
		/// </summary>
		public static T GetCustomAttribute<T>(this Type type, bool inherit) where T : Attribute
		{
			object[] attrs = type.GetCustomAttributes(typeof(T), inherit);
			if (attrs.Length == 0)
			{
				return null;
			}
			return attrs[0] as T;
		}

		/// <summary>
		/// Returns the first found non-inherited custom attribute of type T on this type
		/// Returns null if none was found
		/// </summary>
		public static T GetCustomAttribute<T>(this Type type) where T : Attribute
		{
			return type.GetCustomAttribute<T>(inherit: false);
		}

		/// <summary>
		/// Gets all attributes of type T.
		/// </summary>
		/// <param name="type">The type.</param>
		public static IEnumerable<T> GetCustomAttributes<T>(this Type type) where T : Attribute
		{
			return type.GetCustomAttributes<T>(inherit: false);
		}

		/// <summary>
		/// Gets all attributes of type T.
		/// </summary>
		/// <param name="type">The type</param>
		/// <param name="inherit">If true, specifies to also search the ancestors of element for custom attributes.</param>
		public static IEnumerable<T> GetCustomAttributes<T>(this Type type, bool inherit) where T : Attribute
		{
			object[] attrs = type.GetCustomAttributes(typeof(T), inherit);
			for (int i = 0; i < attrs.Length; i++)
			{
				yield return attrs[i] as T;
			}
		}

		/// <summary>
		/// Returns true if the attribute whose type is specified by the generic argument is defined on this type
		/// </summary>
		public static bool IsDefined<T>(this Type type) where T : Attribute
		{
			return type.IsDefined(typeof(T), inherit: false);
		}

		/// <summary>
		/// Returns true if the attribute whose type is specified by the generic argument is defined on this type
		/// </summary>
		public static bool IsDefined<T>(this Type type, bool inherit) where T : Attribute
		{
			return type.IsDefined(typeof(T), inherit);
		}

		/// <summary>
		/// Determines whether a type inherits or implements another type. Also include support for open generic base types such as List&lt;&gt;.
		/// </summary>
		/// <param name="type"></param>
		public static bool InheritsFrom<TBase>(this Type type)
		{
			return type.InheritsFrom(typeof(TBase));
		}

		/// <summary>
		/// Determines whether a type inherits or implements another type. Also include support for open generic base types such as List&lt;&gt;.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="baseType"></param>
		public static bool InheritsFrom(this Type type, Type baseType)
		{
			if (baseType.IsAssignableFrom(type))
			{
				return true;
			}
			if (type.IsInterface && !baseType.IsInterface)
			{
				return false;
			}
			if (baseType.IsInterface)
			{
				return type.GetInterfaces().Contains(baseType);
			}
			Type t = type;
			while (t != null)
			{
				if (t == baseType)
				{
					return true;
				}
				if (baseType.IsGenericTypeDefinition && t.IsGenericType && t.GetGenericTypeDefinition() == baseType)
				{
					return true;
				}
				t = t.BaseType;
			}
			return false;
		}

		/// <summary>
		/// Gets the number of base types between given type and baseType.
		/// </summary>
		public static int GetInheritanceDistance(this Type type, Type baseType)
		{
			Type higherType;
			Type lowerType;
			if (type.IsAssignableFrom(baseType))
			{
				higherType = type;
				lowerType = baseType;
			}
			else
			{
				if (!baseType.IsAssignableFrom(type))
				{
					throw new ArgumentException("Cannot assign types '" + type.GetNiceName() + "' and '" + baseType.GetNiceName() + "' to each other.");
				}
				higherType = baseType;
				lowerType = type;
			}
			Type currentType = lowerType;
			int count = 0;
			if (higherType.IsInterface)
			{
				while (currentType != null && currentType != typeof(object))
				{
					count++;
					currentType = currentType.BaseType;
					Type[] interfaces = currentType.GetInterfaces();
					for (int i = 0; i < interfaces.Length; i++)
					{
						if (interfaces[i] == higherType)
						{
							currentType = null;
							break;
						}
					}
				}
			}
			else
			{
				while (currentType != higherType && currentType != null && currentType != typeof(object))
				{
					count++;
					currentType = currentType.BaseType;
				}
			}
			return count;
		}

		/// <summary>
		/// Determines whether a method has the specified parameter types.
		/// </summary>
		public static bool HasParamaters(this MethodInfo methodInfo, IList<Type> paramTypes, bool inherit = true)
		{
			ParameterInfo[] methodParams = methodInfo.GetParameters();
			if (methodParams.Length == paramTypes.Count)
			{
				for (int i = 0; i < methodParams.Length; i++)
				{
					if (inherit && !paramTypes[i].InheritsFrom(methodParams[i].ParameterType))
					{
						return false;
					}
					if (methodParams[i].ParameterType != paramTypes[i])
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// FieldInfo will return the fieldType, propertyInfo the PropertyType, MethodInfo the return type and EventInfo will return the EventHandlerType.
		/// </summary>
		/// <param name="memberInfo">The MemberInfo.</param>
		public static Type GetReturnType(this MemberInfo memberInfo)
		{
			FieldInfo fieldInfo = memberInfo as FieldInfo;
			if (fieldInfo != null)
			{
				return fieldInfo.FieldType;
			}
			PropertyInfo propertyInfo = memberInfo as PropertyInfo;
			if (propertyInfo != null)
			{
				return propertyInfo.PropertyType;
			}
			MethodInfo methodInfo = memberInfo as MethodInfo;
			if (methodInfo != null)
			{
				return methodInfo.ReturnType;
			}
			EventInfo eventInfo = memberInfo as EventInfo;
			if (eventInfo != null)
			{
				return eventInfo.EventHandlerType;
			}
			return null;
		}

		/// <summary>
		/// Gets the value contained in a given <see cref="T:System.Reflection.MemberInfo" />. Currently only <see cref="T:System.Reflection.FieldInfo" /> and <see cref="T:System.Reflection.PropertyInfo" /> is supported.
		/// </summary>
		/// <param name="member">The <see cref="T:System.Reflection.MemberInfo" /> to get the value of.</param>
		/// <param name="obj">The instance to get the value from.</param>
		/// <returns>The value contained in the given <see cref="T:System.Reflection.MemberInfo" />.</returns>
		/// <exception cref="T:System.ArgumentException">Can't get the value of the given <see cref="T:System.Reflection.MemberInfo" /> type.</exception>
		public static object GetMemberValue(this MemberInfo member, object obj)
		{
			if (member is FieldInfo)
			{
				return (member as FieldInfo).GetValue(obj);
			}
			if (member is PropertyInfo)
			{
				return (member as PropertyInfo).GetGetMethod(nonPublic: true).Invoke(obj, null);
			}
			throw new ArgumentException("Can't get the value of a " + member.GetType().Name);
		}

		/// <summary>
		/// Sets the value of a given MemberInfo. Currently only <see cref="T:System.Reflection.FieldInfo" /> and <see cref="T:System.Reflection.PropertyInfo" /> is supported.
		/// </summary>
		/// <param name="member">The <see cref="T:System.Reflection.MemberInfo" /> to set the value of.</param>
		/// <param name="obj">The object to set the value on.</param>
		/// <param name="value">The value to set.</param>
		/// <exception cref="T:System.ArgumentException">
		/// Property has no setter
		/// or
		/// Can't set the value of the given <see cref="T:System.Reflection.MemberInfo" /> type.
		/// </exception>
		public static void SetMemberValue(this MemberInfo member, object obj, object value)
		{
			if (member is FieldInfo)
			{
				(member as FieldInfo).SetValue(obj, value);
				return;
			}
			if (member is PropertyInfo)
			{
				MethodInfo method = (member as PropertyInfo).GetSetMethod(nonPublic: true);
				if (method != null)
				{
					method.Invoke(obj, new object[1] { value });
					return;
				}
				throw new ArgumentException("Property " + member.Name + " has no setter");
			}
			throw new ArgumentException("Can't set the value of a " + member.GetType().Name);
		}

		/// <summary>
		/// Tries to infer a set of valid generic parameters for a generic type definition, given a subset of known parameters.
		/// </summary>
		/// <param name="genericTypeDefinition">The generic type definition to attempt to infer parameters for.</param>
		/// <param name="inferredParams">The inferred parameters, if inferral was successful.</param>
		/// <param name="knownParameters">The known parameters to infer from.</param>
		/// <returns>True if the parameters could be inferred, otherwise, false.</returns>
		/// <exception cref="T:System.ArgumentNullException">
		/// genericTypeDefinition is null
		/// or
		/// knownParameters is null
		/// </exception>
		/// <exception cref="T:System.ArgumentException">The genericTypeDefinition parameter must be a generic type definition.</exception>
		public static bool TryInferGenericParameters(this Type genericTypeDefinition, out Type[] inferredParams, params Type[] knownParameters)
		{
			if (genericTypeDefinition == null)
			{
				throw new ArgumentNullException("genericTypeDefinition");
			}
			if (knownParameters == null)
			{
				throw new ArgumentNullException("knownParameters");
			}
			if (!genericTypeDefinition.IsGenericType)
			{
				throw new ArgumentException("The genericTypeDefinition parameter must be a generic type.");
			}
			lock (GenericConstraintsSatisfaction_LOCK)
			{
				Dictionary<Type, Type> matches = GenericConstraintsSatisfactionInferredParameters;
				matches.Clear();
				HashSet<Type> typesToCheck = GenericConstraintsSatisfactionTypesToCheck;
				typesToCheck.Clear();
				List<Type> typesToCheck_ToAdd = GenericConstraintsSatisfactionTypesToCheck_ToAdd;
				typesToCheck_ToAdd.Clear();
				for (int i = 0; i < knownParameters.Length; i++)
				{
					typesToCheck.Add(knownParameters[i]);
				}
				Type[] definitions = genericTypeDefinition.GetGenericArguments();
				if (!genericTypeDefinition.IsGenericTypeDefinition)
				{
					Type[] constructedParameters = definitions;
					genericTypeDefinition = genericTypeDefinition.GetGenericTypeDefinition();
					definitions = genericTypeDefinition.GetGenericArguments();
					int unknownCount = 0;
					for (int j = 0; j < constructedParameters.Length; j++)
					{
						if (!constructedParameters[j].IsGenericParameter && (!constructedParameters[j].IsGenericType || constructedParameters[j].IsFullyConstructedGenericType()))
						{
							matches[definitions[j]] = constructedParameters[j];
						}
						else
						{
							unknownCount++;
						}
					}
					if (unknownCount == knownParameters.Length)
					{
						int count = 0;
						for (int k = 0; k < constructedParameters.Length; k++)
						{
							if (constructedParameters[k].IsGenericParameter)
							{
								constructedParameters[k] = knownParameters[count++];
							}
						}
						if (genericTypeDefinition.AreGenericConstraintsSatisfiedBy(constructedParameters))
						{
							inferredParams = constructedParameters;
							return true;
						}
					}
				}
				if (definitions.Length == knownParameters.Length && genericTypeDefinition.AreGenericConstraintsSatisfiedBy(knownParameters))
				{
					inferredParams = knownParameters;
					return true;
				}
				Type[] array = definitions;
				foreach (Type typeArg in array)
				{
					Type[] constraints = typeArg.GetGenericParameterConstraints();
					Type[] array2 = constraints;
					foreach (Type constraint in array2)
					{
						foreach (Type parameter in typesToCheck)
						{
							if (!constraint.IsGenericType)
							{
								continue;
							}
							Type constraintDefinition = constraint.GetGenericTypeDefinition();
							Type[] constraintParams = constraint.GetGenericArguments();
							Type[] paramParams;
							if (parameter.IsGenericType && constraintDefinition == parameter.GetGenericTypeDefinition())
							{
								paramParams = parameter.GetGenericArguments();
							}
							else if (constraintDefinition.IsInterface && parameter.ImplementsOpenGenericInterface(constraintDefinition))
							{
								paramParams = parameter.GetArgumentsOfInheritedOpenGenericInterface(constraintDefinition);
							}
							else
							{
								if (!constraintDefinition.IsClass || !parameter.ImplementsOpenGenericClass(constraintDefinition))
								{
									continue;
								}
								paramParams = parameter.GetArgumentsOfInheritedOpenGenericClass(constraintDefinition);
							}
							matches[typeArg] = parameter;
							typesToCheck_ToAdd.Add(parameter);
							for (int n = 0; n < constraintParams.Length; n++)
							{
								if (constraintParams[n].IsGenericParameter)
								{
									matches[constraintParams[n]] = paramParams[n];
									typesToCheck_ToAdd.Add(paramParams[n]);
								}
							}
						}
						foreach (Type type in typesToCheck_ToAdd)
						{
							typesToCheck.Add(type);
						}
						typesToCheck_ToAdd.Clear();
					}
				}
				if (matches.Count == definitions.Length)
				{
					inferredParams = new Type[matches.Count];
					for (int num = 0; num < definitions.Length; num++)
					{
						inferredParams[num] = matches[definitions[num]];
					}
					if (genericTypeDefinition.AreGenericConstraintsSatisfiedBy(inferredParams))
					{
						return true;
					}
				}
				inferredParams = null;
				return false;
			}
		}

		/// <summary>
		/// <para>Checks whether an array of types satisfy the constraints of a given generic type definition.</para>
		/// <para>If this method returns true, the given parameters can be safely used with <see cref="M:System.Type.MakeGenericType(System.Type[])" /> with the given generic type definition.</para>
		/// </summary>
		/// <param name="genericType">The generic type definition to check.</param>
		/// <param name="parameters">The parameters to check validity for.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// genericType is null
		/// or
		/// types is null
		/// </exception>
		/// <exception cref="T:System.ArgumentException">The genericType parameter must be a generic type definition.</exception>
		public static bool AreGenericConstraintsSatisfiedBy(this Type genericType, params Type[] parameters)
		{
			if (genericType == null)
			{
				throw new ArgumentNullException("genericType");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			if (!genericType.IsGenericType)
			{
				throw new ArgumentException("The genericTypeDefinition parameter must be a generic type.");
			}
			return AreGenericConstraintsSatisfiedBy(genericType.GetGenericArguments(), parameters);
		}

		/// <summary>
		/// <para>Checks whether an array of types satisfy the constraints of a given generic method definition.</para>
		/// <para>If this method returns true, the given parameters can be safely used with <see cref="M:System.Reflection.MethodInfo.MakeGenericMethod(System.Type[])" /> with the given generic method definition.</para>
		/// </summary>
		/// <param name="genericType">The generic method definition to check.</param>
		/// <param name="parameters">The parameters to check validity for.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// genericType is null
		/// or
		/// types is null
		/// </exception>
		/// <exception cref="T:System.ArgumentException">The genericMethod parameter must be a generic method definition.</exception>
		public static bool AreGenericConstraintsSatisfiedBy(this MethodBase genericMethod, params Type[] parameters)
		{
			if (genericMethod == null)
			{
				throw new ArgumentNullException("genericMethod");
			}
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters");
			}
			if (!genericMethod.IsGenericMethod)
			{
				throw new ArgumentException("The genericMethod parameter must be a generic method.");
			}
			return AreGenericConstraintsSatisfiedBy(genericMethod.GetGenericArguments(), parameters);
		}

		public static bool AreGenericConstraintsSatisfiedBy(Type[] definitions, Type[] parameters)
		{
			if (definitions.Length != parameters.Length)
			{
				return false;
			}
			lock (GenericConstraintsSatisfaction_LOCK)
			{
				Dictionary<Type, Type> resolvedMap = GenericConstraintsSatisfactionResolvedMap;
				resolvedMap.Clear();
				for (int i = 0; i < definitions.Length; i++)
				{
					Type definition = definitions[i];
					Type parameter = parameters[i];
					if (!definition.GenericParameterIsFulfilledBy(parameter, resolvedMap))
					{
						return false;
					}
				}
				return true;
			}
		}

		public static bool GenericParameterIsFulfilledBy(this Type genericParameterDefinition, Type parameterType)
		{
			lock (GenericConstraintsSatisfaction_LOCK)
			{
				GenericConstraintsSatisfactionResolvedMap.Clear();
				return genericParameterDefinition.GenericParameterIsFulfilledBy(parameterType, GenericConstraintsSatisfactionResolvedMap);
			}
		}

		/// <summary>
		/// Before calling this method we must ALWAYS hold a lock on the GenericConstraintsSatisfaction_LOCK object, as that is an implicit assumption it works with.
		/// </summary>
		private static bool GenericParameterIsFulfilledBy(this Type genericParameterDefinition, Type parameterType, Dictionary<Type, Type> resolvedMap, HashSet<Type> processedParams = null)
		{
			if (genericParameterDefinition == null)
			{
				throw new ArgumentNullException("genericParameterDefinition");
			}
			if (parameterType == null)
			{
				throw new ArgumentNullException("parameterType");
			}
			if (resolvedMap == null)
			{
				throw new ArgumentNullException("resolvedMap");
			}
			if (!genericParameterDefinition.IsGenericParameter && genericParameterDefinition == parameterType)
			{
				return true;
			}
			if (!genericParameterDefinition.IsGenericParameter)
			{
				return false;
			}
			if (processedParams == null)
			{
				processedParams = GenericConstraintsSatisfactionProcessedParams;
				processedParams.Clear();
			}
			processedParams.Add(genericParameterDefinition);
			GenericParameterAttributes specialConstraints = genericParameterDefinition.GenericParameterAttributes;
			if (specialConstraints != GenericParameterAttributes.None)
			{
				if ((specialConstraints & GenericParameterAttributes.NotNullableValueTypeConstraint) == GenericParameterAttributes.NotNullableValueTypeConstraint)
				{
					if (!parameterType.IsValueType || (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(Nullable<>)))
					{
						return false;
					}
				}
				else if ((specialConstraints & GenericParameterAttributes.ReferenceTypeConstraint) == GenericParameterAttributes.ReferenceTypeConstraint && parameterType.IsValueType)
				{
					return false;
				}
				if ((specialConstraints & GenericParameterAttributes.DefaultConstructorConstraint) == GenericParameterAttributes.DefaultConstructorConstraint && (parameterType.IsAbstract || (!parameterType.IsValueType && parameterType.GetConstructor(Type.EmptyTypes) == null)))
				{
					return false;
				}
			}
			if (resolvedMap.ContainsKey(genericParameterDefinition) && !parameterType.IsAssignableFrom(resolvedMap[genericParameterDefinition]))
			{
				return false;
			}
			Type[] constraints = genericParameterDefinition.GetGenericParameterConstraints();
			for (int i = 0; i < constraints.Length; i++)
			{
				Type constraint = constraints[i];
				if (constraint.IsGenericParameter && resolvedMap.ContainsKey(constraint))
				{
					constraint = resolvedMap[constraint];
				}
				if (constraint.IsGenericParameter)
				{
					if (!constraint.GenericParameterIsFulfilledBy(parameterType, resolvedMap, processedParams))
					{
						return false;
					}
					continue;
				}
				if (constraint.IsClass || constraint.IsInterface || constraint.IsValueType)
				{
					if (constraint.IsGenericType)
					{
						Type constraintDefinition = constraint.GetGenericTypeDefinition();
						Type[] constraintParams = constraint.GetGenericArguments();
						Type[] paramParams;
						if (parameterType.IsGenericType && constraintDefinition == parameterType.GetGenericTypeDefinition())
						{
							paramParams = parameterType.GetGenericArguments();
						}
						else if (constraintDefinition.IsClass)
						{
							if (!parameterType.ImplementsOpenGenericClass(constraintDefinition))
							{
								return false;
							}
							paramParams = parameterType.GetArgumentsOfInheritedOpenGenericClass(constraintDefinition);
						}
						else
						{
							if (!parameterType.ImplementsOpenGenericInterface(constraintDefinition))
							{
								return false;
							}
							paramParams = parameterType.GetArgumentsOfInheritedOpenGenericInterface(constraintDefinition);
						}
						for (int j = 0; j < constraintParams.Length; j++)
						{
							Type c = constraintParams[j];
							Type p = paramParams[j];
							if (c.IsGenericParameter && resolvedMap.ContainsKey(c))
							{
								c = resolvedMap[c];
							}
							if (c.IsGenericParameter)
							{
								if (!processedParams.Contains(c) && !c.GenericParameterIsFulfilledBy(p, resolvedMap, processedParams))
								{
									return false;
								}
							}
							else if (c != p && !c.IsAssignableFrom(p))
							{
								return false;
							}
						}
					}
					else if (!constraint.IsAssignableFrom(parameterType))
					{
						return false;
					}
					continue;
				}
				throw new Exception("Unknown parameter constraint type! " + constraint.GetNiceName());
			}
			resolvedMap[genericParameterDefinition] = parameterType;
			return true;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static string GetGenericConstraintsString(this Type type, bool useFullTypeNames = false)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (!type.IsGenericTypeDefinition)
			{
				throw new ArgumentException("Type '" + type.GetNiceName() + "' is not a generic type definition!");
			}
			Type[] parameters = type.GetGenericArguments();
			string[] strings = new string[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				strings[i] = parameters[i].GetGenericParameterConstraintsString(useFullTypeNames);
			}
			return string.Join(" ", strings);
		}

		/// <summary>
		/// Formats a string with the specified generic parameter constraints on any given type. Example output: <c>where T : class</c>
		/// </summary>
		public static string GetGenericParameterConstraintsString(this Type type, bool useFullTypeNames = false)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (!type.IsGenericParameter)
			{
				throw new ArgumentException("Type '" + type.GetNiceName() + "' is not a generic parameter!");
			}
			StringBuilder sb = new StringBuilder();
			bool started = false;
			GenericParameterAttributes specialConstraints = type.GenericParameterAttributes;
			if ((specialConstraints & GenericParameterAttributes.NotNullableValueTypeConstraint) == GenericParameterAttributes.NotNullableValueTypeConstraint)
			{
				sb.Append("where ").Append(type.Name).Append(" : struct");
				started = true;
			}
			else if ((specialConstraints & GenericParameterAttributes.ReferenceTypeConstraint) == GenericParameterAttributes.ReferenceTypeConstraint)
			{
				sb.Append("where ").Append(type.Name).Append(" : class");
				started = true;
			}
			if ((specialConstraints & GenericParameterAttributes.DefaultConstructorConstraint) == GenericParameterAttributes.DefaultConstructorConstraint)
			{
				if (started)
				{
					sb.Append(", new()");
				}
				else
				{
					sb.Append("where ").Append(type.Name).Append(" : new()");
					started = true;
				}
			}
			Type[] constraints = type.GetGenericParameterConstraints();
			if (constraints.Length != 0)
			{
				foreach (Type constraint in constraints)
				{
					if (started)
					{
						sb.Append(", ");
						if (useFullTypeNames)
						{
							sb.Append(constraint.GetNiceFullName());
						}
						else
						{
							sb.Append(constraint.GetNiceName());
						}
						continue;
					}
					sb.Append("where ").Append(type.Name).Append(" : ");
					if (useFullTypeNames)
					{
						sb.Append(constraint.GetNiceFullName());
					}
					else
					{
						sb.Append(constraint.GetNiceName());
					}
					started = true;
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// Determines whether a generic type contains the specified generic argument constraints.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="types">The generic argument types.</param>
		public static bool GenericArgumentsContainsTypes(this Type type, params Type[] types)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (!type.IsGenericType)
			{
				return false;
			}
			bool[] typesSeen = new bool[types.Length];
			Type[] args = type.GetGenericArguments();
			Stack<Type> argsToCheck = GenericArgumentsContainsTypes_ArgsToCheckCached;
			lock (argsToCheck)
			{
				argsToCheck.Clear();
				for (int i = 0; i < args.Length; i++)
				{
					argsToCheck.Push(args[i]);
				}
				while (argsToCheck.Count > 0)
				{
					Type arg = argsToCheck.Pop();
					for (int j = 0; j < types.Length; j++)
					{
						Type lookingForType = types[j];
						if (lookingForType == arg)
						{
							typesSeen[j] = true;
						}
						else if (lookingForType.IsGenericTypeDefinition && arg.IsGenericType && !arg.IsGenericTypeDefinition && arg.GetGenericTypeDefinition() == lookingForType)
						{
							typesSeen[j] = true;
						}
					}
					bool allSeen = true;
					for (int k = 0; k < typesSeen.Length; k++)
					{
						if (!typesSeen[k])
						{
							allSeen = false;
							break;
						}
					}
					if (allSeen)
					{
						return true;
					}
					if (arg.IsGenericType)
					{
						Type[] genericArguments = arg.GetGenericArguments();
						foreach (Type innerArg in genericArguments)
						{
							argsToCheck.Push(innerArg);
						}
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Determines whether a type is a fully constructed generic type.
		/// </summary>
		public static bool IsFullyConstructedGenericType(this Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (type.IsGenericTypeDefinition)
			{
				return false;
			}
			if (type.HasElementType)
			{
				Type element = type.GetElementType();
				if (element.IsGenericParameter || !element.IsFullyConstructedGenericType())
				{
					return false;
				}
			}
			Type[] args = type.GetGenericArguments();
			foreach (Type arg in args)
			{
				if (arg.IsGenericParameter)
				{
					return false;
				}
				if (!arg.IsFullyConstructedGenericType())
				{
					return false;
				}
			}
			return !type.IsGenericTypeDefinition;
		}

		/// <summary>
		/// Determines whether a type is nullable by ensuring the type is neither a PrimitiveType, ValueType or an Enum.
		/// </summary>
		public static bool IsNullableType(this Type type)
		{
			if (!type.IsPrimitive && !type.IsValueType)
			{
				return !type.IsEnum;
			}
			return false;
		}

		/// <summary>
		/// Gets the enum bitmask in a ulong.
		/// </summary>
		/// <exception cref="T:System.ArgumentException">enumType</exception>
		public static ulong GetEnumBitmask(object value, Type enumType)
		{
			if (!enumType.IsEnum)
			{
				throw new ArgumentException("enumType");
			}
			try
			{
				return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
			}
			catch (OverflowException)
			{
				return (ulong)Convert.ToInt64(value, CultureInfo.InvariantCulture);
			}
		}

		public static Type[] SafeGetTypes(this Assembly assembly)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch
			{
				return Type.EmptyTypes;
			}
		}

		public static bool SafeIsDefined(this Assembly assembly, Type attribute, bool inherit)
		{
			try
			{
				return assembly.IsDefined(attribute, inherit);
			}
			catch
			{
				return false;
			}
		}

		public static object[] SafeGetCustomAttributes(this Assembly assembly, Type type, bool inherit)
		{
			try
			{
				return assembly.GetCustomAttributes(type, inherit);
			}
			catch
			{
				return new object[0];
			}
		}
	}
}

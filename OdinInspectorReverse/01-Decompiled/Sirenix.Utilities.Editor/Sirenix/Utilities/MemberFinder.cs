using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor;

namespace Sirenix.Utilities
{
	/// <summary>
	/// MemberFinder is obsolete, and has been replacted by <see cref="!:Sirenix.OdinInspector.Editor.ValueResolvers.ValueResolver" /> and <see cref="!:Sirenix.OdinInspector.Editor.ActionResolvers.ActionResolver" />. 
	/// Use cases that do not fit those utlities should use manual reflection that is hand-optimized for the best performance in the given case.
	/// <para />
	/// MemberFinder was a utility class often used by Odin drawers to find fields, methods, and
	/// properties while providing good user-friendly error messages based on the search criteria.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("MemberFinder is obsolete, due to performance issues and because its various uses have been replaced by the ValueResolver and ActionResolver utilities. Use cases that do not fit those utlities should use manual reflection that is hand-optimized for the best performance in the given case.", false)]
	public class MemberFinder
	{
		[Flags]
		private enum ConditionFlags
		{
			None = 0,
			IsStatic = 2,
			IsProperty = 4,
			IsInstance = 8,
			IsDeclaredOnly = 0x10,
			HasNoParamaters = 0x20,
			IsMethod = 0x40,
			IsField = 0x80,
			IsPublic = 0x100,
			IsNonPublic = 0x200
		}

		private Type type;

		private ConditionFlags conditionFlags;

		private string name;

		private Type returnType;

		private List<Type> paramTypes = new List<Type>();

		private bool returnTypeCanInherit;

		private bool returnTypeCanBeConverted;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Utilities.MemberFinder" /> class.
		/// </summary>
		[Obsolete("MemberFinder is obsolete, due to performance issues and because its various uses have been replaced by the ValueResolver and ActionResolver utilities. Use cases that do not fit those utlities should use manual reflection that is hand-optimized for the best performance in the given case.", false)]
		public MemberFinder()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Utilities.MemberFinder" /> class.
		/// </summary>
		[Obsolete("MemberFinder is obsolete, due to performance issues and because its various uses have been replaced by the ValueResolver and ActionResolver utilities. Use cases that do not fit those utlities should use manual reflection that is hand-optimized for the best performance in the given case.", false)]
		public MemberFinder(Type type)
		{
			InitializeFor(type);
		}

		/// <summary>
		/// <para>Find members of the given type, while providing good error messages based on the following search filters provided.</para>
		/// </summary>
		[Obsolete("MemberFinder is obsolete, due to performance issues and because its various uses have been replaced by the ValueResolver and ActionResolver utilities. Use cases that do not fit those utlities should use manual reflection that is hand-optimized for the best performance in the given case.", false)]
		public static MemberFinder Start<T>()
		{
			return new MemberFinder().InitializeFor(typeof(T));
		}

		/// <summary>
		/// <para>Find members of the given type, while providing good error messages based on the following search filters provided.</para>
		/// </summary>
		[Obsolete("MemberFinder is obsolete, due to performance issues and because its various uses have been replaced by the ValueResolver and ActionResolver utilities. Use cases that do not fit those utlities should use manual reflection that is hand-optimized for the best performance in the given case.", false)]
		public static MemberFinder Start(Type type)
		{
			return new MemberFinder().InitializeFor(type);
		}

		/// <summary>
		/// Can be true for both fields, properties and methods.
		/// </summary>
		/// <returns></returns>
		public MemberFinder HasNoParameters()
		{
			conditionFlags |= ConditionFlags.HasNoParamaters;
			return this;
		}

		/// <summary>
		/// Exclude members found in base-types.
		/// </summary>
		public MemberFinder IsDeclaredOnly()
		{
			conditionFlags |= ConditionFlags.IsDeclaredOnly;
			return this;
		}

		/// <summary>
		/// <para>Only include methods with the following parameter.</para>
		/// <para>Calling this will also exclude fields and properties.</para>
		/// <para>Parameter type inheritance is supported.</para>
		/// </summary>
		public MemberFinder HasParameters(Type param1)
		{
			conditionFlags |= ConditionFlags.IsMethod;
			paramTypes.Add(param1);
			return this;
		}

		/// <summary>
		/// <para>Only include methods with the following parameters.</para>
		/// <para>Calling this will also exclude fields and properties.</para>
		/// <para>Parameter type inheritance is supported.</para>
		/// </summary>
		public MemberFinder HasParameters(Type param1, Type param2)
		{
			conditionFlags |= ConditionFlags.IsMethod;
			paramTypes.Add(param1);
			paramTypes.Add(param2);
			return this;
		}

		/// <summary>
		/// <para>Only include methods with the following parameters.</para>
		/// <para>Calling this will also exclude fields and properties.</para>
		/// <para>Parameter type inheritance is supported.</para>
		/// </summary>
		public MemberFinder HasParameters(Type param1, Type param2, Type param3)
		{
			conditionFlags |= ConditionFlags.IsMethod;
			paramTypes.Add(param1);
			paramTypes.Add(param2);
			paramTypes.Add(param3);
			return this;
		}

		/// <summary>
		/// <para>Only include methods with the following parameters.</para>
		/// <para>Calling this will also exclude fields and properties.</para>
		/// <para>Parameter type inheritance is supported.</para>
		/// </summary>
		public MemberFinder HasParameters(Type param1, Type param2, Type param3, Type param4)
		{
			conditionFlags |= ConditionFlags.IsMethod;
			paramTypes.Add(param1);
			paramTypes.Add(param2);
			paramTypes.Add(param3);
			paramTypes.Add(param4);
			return this;
		}

		/// <summary>
		/// <para>Only include methods with the following parameters.</para>
		/// <para>Calling this will also exclude fields and properties.</para>
		/// <para>Parameter type inheritance is supported.</para>
		/// </summary>
		public MemberFinder HasParameters<T>()
		{
			return HasParameters(typeof(T));
		}

		/// <summary>
		/// <para>Only include methods with the following parameters.</para>
		/// <para>Calling this will also exclude fields and properties.</para>
		/// <para>Parameter type inheritance is supported.</para>
		/// </summary>
		public MemberFinder HasParameters<T1, T2>()
		{
			return HasParameters(typeof(T1), typeof(T2));
		}

		/// <summary>
		/// <para>Only include methods with the following parameters.</para>
		/// <para>Calling this will also exclude fields and properties.</para>
		/// <para>Parameter type inheritance is supported.</para>
		/// </summary>
		public MemberFinder HasParameters<T1, T2, T3>()
		{
			return HasParameters(typeof(T1), typeof(T2), typeof(T3));
		}

		/// <summary>
		/// <para>Only include methods with the following parameters.</para>
		/// <para>Calling this will also exclude fields and properties.</para>
		/// <para>Parameter type inheritance is supported.</para>
		/// </summary>
		public MemberFinder HasParameters<T1, T2, T3, T4>()
		{
			return HasParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
		}

		/// <summary>
		/// Determines whether [has return type] [the specified return type].
		/// </summary>
		public MemberFinder HasReturnType(Type returnType, bool inherit = false)
		{
			returnTypeCanInherit = inherit;
			this.returnType = returnType;
			return this;
		}

		/// <summary>
		/// Can be true for both fields, properties and methods.
		/// </summary>
		public MemberFinder HasReturnType<T>(bool inherit = false)
		{
			return HasReturnType(typeof(T), inherit);
		}

		public MemberFinder HasConvertableReturnType(Type type)
		{
			returnTypeCanInherit = true;
			returnTypeCanBeConverted = true;
			returnType = type;
			return this;
		}

		public MemberFinder HasConvertableReturnType<T>()
		{
			returnTypeCanInherit = true;
			returnTypeCanBeConverted = true;
			returnType = typeof(T);
			return this;
		}

		/// <summary>
		/// Calls IsField() and IsProperty().
		/// </summary>
		public MemberFinder IsFieldOrProperty()
		{
			IsField();
			IsProperty();
			return this;
		}

		/// <summary>
		/// Only include static members. By default, both static and non-static members are included.
		/// </summary>
		public MemberFinder IsStatic()
		{
			conditionFlags |= ConditionFlags.IsStatic;
			return this;
		}

		/// <summary>
		/// Only include non-static members. By default, both static and non-static members are included.
		/// </summary>
		public MemberFinder IsInstance()
		{
			conditionFlags |= ConditionFlags.IsInstance;
			return this;
		}

		/// <summary>
		/// Specify the name of the member.
		/// </summary>
		public MemberFinder IsNamed(string name)
		{
			this.name = name;
			return this;
		}

		/// <summary>
		/// <para>Excludes fields and methods if nether IsField() or IsMethod() is called. Otherwise includes properties.</para>
		/// <para>By default, all member types are included.</para>
		/// </summary>
		public MemberFinder IsProperty()
		{
			conditionFlags |= ConditionFlags.IsProperty;
			return this;
		}

		/// <summary>
		/// <para>Excludes fields and properties if nether IsField() or IsProperty() is called. Otherwise includes methods.</para>
		/// <para>By default, all member types are included.</para>
		/// </summary>
		public MemberFinder IsMethod()
		{
			conditionFlags |= ConditionFlags.IsMethod;
			return this;
		}

		/// <summary>
		/// <para>Excludes properties and methods if nether IsProperty() or IsMethod() is called. Otherwise includes fields.</para>
		/// <para>By default, all member types are included.</para>
		/// </summary>
		public MemberFinder IsField()
		{
			conditionFlags |= ConditionFlags.IsField;
			return this;
		}

		/// <summary>
		/// <para>Excludes non-public members if IsNonPublic() has not yet been called. Otherwise includes public members.</para>
		/// <para>By default, both public and non-public members are included.</para>
		/// </summary>
		public MemberFinder IsPublic()
		{
			conditionFlags |= ConditionFlags.IsPublic;
			return this;
		}

		/// <summary>
		/// <para>Excludes public members if IsPublic() has not yet been called. Otherwise includes non-public members.</para>
		/// <para>By default, both public and non-public members are included.</para>
		/// </summary>
		public MemberFinder IsNonPublic()
		{
			conditionFlags |= ConditionFlags.IsNonPublic;
			return this;
		}

		public bool IsNamed(object customDeleteFunction)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Excludes fields and properties, and only includes methods with a return type of void.
		/// </summary>
		public MemberFinder ReturnsVoid()
		{
			conditionFlags |= ConditionFlags.IsMethod;
			return HasReturnType(typeof(void));
		}

		/// <summary>
		/// <para>Gets the member based on the search filters provided</para>
		/// <para>Returns null if no member was found.</para>
		/// </summary>
		[Obsolete("MemberFinder is obsolete, due to performance issues and because its various uses have been replaced by the ValueResolver and ActionResolver utilities. Use cases that do not fit those utlities should use manual reflection that is hand-optimized for the best performance in the given case.", false)]
		public T GetMember<T>() where T : MemberInfo
		{
			string errorMessage = null;
			return GetMember<T>(out errorMessage);
		}

		/// <summary>
		/// <para>Gets the member based on the search filters provided, and provides a proper error message if no members was found.</para>
		/// </summary>
		[Obsolete("MemberFinder is obsolete, due to performance issues and because its various uses have been replaced by the ValueResolver and ActionResolver utilities. Use cases that do not fit those utlities should use manual reflection that is hand-optimized for the best performance in the given case.", false)]
		public T GetMember<T>(out string errorMessage) where T : MemberInfo
		{
			TryGetMember(out T memberInfo, out errorMessage);
			return memberInfo;
		}

		/// <summary>
		/// <para>Gets the member based on the search filters provided, and provides a proper error message if no members was found.</para>
		/// </summary>
		[Obsolete("MemberFinder is obsolete, due to performance issues and because its various uses have been replaced by the ValueResolver and ActionResolver utilities. Use cases that do not fit those utlities should use manual reflection that is hand-optimized for the best performance in the given case.", false)]
		public MemberInfo GetMember(out string errorMessage)
		{
			TryGetMember(out var memberInfo, out errorMessage);
			return memberInfo;
		}

		/// <summary>
		/// <para>Try gets the member based on the search filters provided, and provides a proper error message if no members was found.</para>
		/// </summary>
		[Obsolete("MemberFinder is obsolete, due to performance issues and because its various uses have been replaced by the ValueResolver and ActionResolver utilities. Use cases that do not fit those utlities should use manual reflection that is hand-optimized for the best performance in the given case.", false)]
		public bool TryGetMember<T>(out T memberInfo, out string errorMessage) where T : MemberInfo
		{
			MemberInfo m;
			bool result = TryGetMember(out m, out errorMessage);
			memberInfo = m as T;
			return result;
		}

		/// <summary>
		/// <para>Try gets the member based on the search filters provided, and provides a proper error message if no members was found.</para>
		/// </summary>
		[Obsolete("MemberFinder is obsolete, due to performance issues and because its various uses have been replaced by the ValueResolver and ActionResolver utilities. Use cases that do not fit those utlities should use manual reflection that is hand-optimized for the best performance in the given case.", false)]
		public bool TryGetMember(out MemberInfo memberInfo, out string errorMessage)
		{
			if (TryGetMembers(out var memberInfos, out errorMessage))
			{
				memberInfo = memberInfos[0];
				return true;
			}
			memberInfo = null;
			return false;
		}

		/// <summary>
		/// <para>Try gets all members based on the search filters provided, and provides a proper error message if no members was found.</para>
		/// </summary>
		[Obsolete("MemberFinder is obsolete, due to performance issues and because its various uses have been replaced by the ValueResolver and ActionResolver utilities. Use cases that do not fit those utlities should use manual reflection that is hand-optimized for the best performance in the given case.", false)]
		public bool TryGetMembers(out MemberInfo[] memberInfos, out string errorMessage)
		{
			IEnumerable<MemberInfo> tmpMemberInfos = Enumerable.Empty<MemberInfo>();
			BindingFlags bindingFlags = (HasCondition(ConditionFlags.IsDeclaredOnly) ? BindingFlags.DeclaredOnly : BindingFlags.FlattenHierarchy);
			bool hasNoParamaters = HasCondition(ConditionFlags.HasNoParamaters);
			bool isInstance = HasCondition(ConditionFlags.IsInstance);
			bool isStatic = HasCondition(ConditionFlags.IsStatic);
			bool isPublic = HasCondition(ConditionFlags.IsPublic);
			bool isNonPublic = HasCondition(ConditionFlags.IsNonPublic);
			bool isMethod = HasCondition(ConditionFlags.IsMethod);
			bool isField = HasCondition(ConditionFlags.IsField);
			bool isProperty = HasCondition(ConditionFlags.IsProperty);
			if (!isPublic && !isNonPublic)
			{
				isPublic = true;
				isNonPublic = true;
			}
			if (!isStatic && !isInstance)
			{
				isStatic = true;
				isInstance = true;
			}
			if (!(isField || isProperty || isMethod))
			{
				isMethod = true;
				isField = true;
				isProperty = true;
			}
			if (isInstance)
			{
				bindingFlags |= BindingFlags.Instance;
			}
			if (isStatic)
			{
				bindingFlags |= BindingFlags.Static;
			}
			if (isPublic)
			{
				bindingFlags |= BindingFlags.Public;
			}
			if (isNonPublic)
			{
				bindingFlags |= BindingFlags.NonPublic;
			}
			if (isMethod && isField && isProperty)
			{
				tmpMemberInfos = ((name != null) ? (from n in type.GetAllMembers(bindingFlags)
					where n.Name == name
					select n) : type.GetAllMembers(bindingFlags));
				if (hasNoParamaters)
				{
					tmpMemberInfos = tmpMemberInfos.Where((MemberInfo x) => !(x is MethodInfo) || (x as MethodInfo).GetParameters().Length == 0);
				}
			}
			else
			{
				if (isMethod)
				{
					IEnumerable<MethodInfo> methodInfos = ((name == null) ? type.GetAllMembers<MethodInfo>(bindingFlags) : (from x in type.GetAllMembers<MethodInfo>(bindingFlags)
						where x.Name == name
						select x));
					if (hasNoParamaters)
					{
						methodInfos = methodInfos.Where((MethodInfo x) => x.GetParameters().Length == 0);
					}
					else if (paramTypes.Count > 0)
					{
						methodInfos = methodInfos.Where((MethodInfo x) => x.HasParamaters(paramTypes));
					}
					tmpMemberInfos = methodInfos.OfType<MemberInfo>();
				}
				if (isField)
				{
					tmpMemberInfos = ((name != null) ? tmpMemberInfos.AppendWith((from n in type.GetAllMembers<FieldInfo>(bindingFlags)
						where n.Name == name
						select n).Cast<MemberInfo>()) : tmpMemberInfos.AppendWith(type.GetAllMembers<FieldInfo>(bindingFlags).Cast<MemberInfo>()));
				}
				if (isProperty)
				{
					tmpMemberInfos = ((name != null) ? tmpMemberInfos.AppendWith((from n in type.GetAllMembers<PropertyInfo>(bindingFlags)
						where n.Name == name
						select n).Cast<MemberInfo>()) : tmpMemberInfos.AppendWith(type.GetAllMembers<PropertyInfo>(bindingFlags).Cast<MemberInfo>()));
				}
			}
			if (this.returnType != null)
			{
				Type returnType = null;
				tmpMemberInfos = (returnTypeCanBeConverted ? tmpMemberInfos.Where((MemberInfo x) => (returnType = x.GetReturnType()) != null && ConvertUtility.CanConvert(returnType, this.returnType)) : ((!returnTypeCanInherit) ? tmpMemberInfos.Where((MemberInfo x) => (returnType = x.GetReturnType()) != null && returnType == this.returnType) : tmpMemberInfos.Where((MemberInfo x) => (returnType = x.GetReturnType()) != null && returnType.InheritsFrom(this.returnType))));
			}
			memberInfos = tmpMemberInfos.ToArray();
			if (memberInfos != null && memberInfos.Length != 0)
			{
				errorMessage = null;
				return true;
			}
			MemberInfo namedMember = ((name == null) ? null : type.GetMember(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).FirstOrDefault((MemberInfo t) => (t is MethodInfo && isMethod) || (t is FieldInfo && isField) || (t is PropertyInfo && isProperty)));
			if (namedMember != null)
			{
				string accessModifies = (namedMember.IsStatic() ? "Static " : "Non-static ");
				if (hasNoParamaters && namedMember is MethodInfo && (namedMember as MethodInfo).GetParameters().Length != 0)
				{
					errorMessage = accessModifies + "method " + name + " can not take parameters.";
					return false;
				}
				if (isMethod && paramTypes.Count > 0 && namedMember is MethodInfo && !(namedMember as MethodInfo).HasParamaters(paramTypes))
				{
					errorMessage = accessModifies + "method " + name + " must have the following parameters: " + string.Join(", ", paramTypes.Select((Type x) => x.GetNiceName()).ToArray()) + ".";
					return false;
				}
				if (this.returnType != null && this.returnType != namedMember.GetReturnType())
				{
					if (returnTypeCanBeConverted)
					{
						errorMessage = accessModifies + namedMember.MemberType.ToString().ToLower(CultureInfo.InvariantCulture) + " " + name + " must have a return type that can be cast to " + this.returnType.GetNiceName() + ".";
					}
					else if (returnTypeCanInherit)
					{
						errorMessage = accessModifies + namedMember.MemberType.ToString().ToLower(CultureInfo.InvariantCulture) + " " + name + " must have a return type that is assignable to " + this.returnType.GetNiceName() + ".";
					}
					else
					{
						errorMessage = accessModifies + namedMember.MemberType.ToString().ToLower(CultureInfo.InvariantCulture) + " " + name + " must have a return type of " + this.returnType.GetNiceName() + ".";
					}
					return false;
				}
			}
			int modCount = (isField ? 1 : 0) + (isProperty ? 1 : 0) + (isMethod ? 1 : 0);
			string strMemberTypes = (isField ? ("fields" + ((modCount-- <= 1) ? " " : ((modCount == 1) ? " or " : ", "))) : string.Empty) + (isProperty ? ("properties" + ((modCount-- <= 1) ? " " : ((modCount == 1) ? " or " : ", "))) : string.Empty) + (isMethod ? ("methods" + ((modCount-- <= 1) ? " " : ((modCount == 1) ? " or " : ", "))) : string.Empty);
			string strAccessModifiers = ((isPublic == isNonPublic) ? string.Empty : (isPublic ? "public " : "non-public ")) + ((isStatic == isInstance) ? string.Empty : (isStatic ? "static " : "non-static "));
			string strReturnType = ((this.returnType == null) ? " " : ("with a return type of " + this.returnType.GetNiceName() + " "));
			string strParameters = ((paramTypes.Count == 0) ? " " : (((strReturnType == " ") ? "" : "and ") + "with the parameter signature (" + string.Join(", ", paramTypes.Select((Type n) => n.GetNiceName()).ToArray()) + ") "));
			if (name == null)
			{
				errorMessage = "No " + strAccessModifiers + strMemberTypes + strReturnType + strParameters + "was found in " + type.GetNiceName() + ".";
				return false;
			}
			errorMessage = "No " + strAccessModifiers + strMemberTypes + "named " + name + " " + strReturnType + strParameters + "was found in " + type.GetNiceName() + ".";
			return false;
		}

		private MemberFinder InitializeFor(Type type)
		{
			this.type = type;
			Reset();
			return this;
		}

		private void Reset()
		{
			returnType = null;
			returnTypeCanInherit = false;
			returnTypeCanBeConverted = false;
			name = null;
			conditionFlags = ConditionFlags.None;
			paramTypes.Clear();
		}

		private bool HasCondition(ConditionFlags flag)
		{
			return (conditionFlags & flag) == flag;
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Contains meta-data information about a property in the inspector, that can be used to create an actual property instance.
	/// </summary>
	public sealed class InspectorPropertyInfo
	{
		private struct TypeSignature4
		{
			public Type T1;

			public Type T2;

			public Type T3;

			public Type T4;

			public int Hash;

			public TypeSignature4(Type t1, Type t2, Type t3, Type t4)
			{
				T1 = t1;
				T2 = t2;
				T3 = t3;
				T4 = t4;
				int result = 1;
				int hash = t1.GetHashCode();
				result = 137 * result + (hash ^ (hash >> 16));
				hash = t2.GetHashCode();
				result = 137 * result + (hash ^ (hash >> 16));
				hash = t3.GetHashCode();
				result = 137 * result + (hash ^ (hash >> 16));
				hash = t4.GetHashCode();
				result = 137 * result + (hash ^ (hash >> 16));
				Hash = result;
			}
		}

		private class TypeSignatureComparer : IEqualityComparer<TypeSignature4>
		{
			public bool Equals(TypeSignature4 x, TypeSignature4 y)
			{
				if (x.Hash != y.Hash)
				{
					return false;
				}
				if ((object)x.T1 != y.T1)
				{
					return false;
				}
				if ((object)x.T2 != y.T2)
				{
					return false;
				}
				if ((object)x.T3 != y.T3)
				{
					return false;
				}
				if ((object)x.T4 != y.T4)
				{
					return false;
				}
				return true;
			}

			public int GetHashCode(TypeSignature4 obj)
			{
				return obj.Hash;
			}
		}

		internal MemberInfo[] memberInfos;

		internal List<Attribute> attributes;

		private ImmutableList<Attribute> attributesImmutable;

		private Type typeOfOwner;

		private Type typeOfValue;

		private IValueGetterSetter getterSetter;

		internal InspectorPropertyInfo[] groupInfos;

		private bool isUnityPropertyOnly;

		private Delegate @delegate;

		private static readonly DoubleLookupDictionary<Type, Type, Func<MemberInfo, bool, IValueGetterSetter>> GetterSetterCreators = new DoubleLookupDictionary<Type, Type, Func<MemberInfo, bool, IValueGetterSetter>>(FastTypeComparer.Instance, FastTypeComparer.Instance);

		private static readonly Dictionary<TypeSignature4, Func<IValueGetterSetter, IValueGetterSetter>> AliasGetterSetterCreators = new Dictionary<TypeSignature4, Func<IValueGetterSetter, IValueGetterSetter>>(new TypeSignatureComparer());

		private static readonly Type[] GetterSetterConstructorSignature = new Type[2]
		{
			typeof(MemberInfo),
			typeof(bool)
		};

		private static readonly Type[] AliasGetterSetterConstructorSignature = new Type[1];

		internal bool IsShownInInspector;

		internal int DesignerDesiredIndex = -1;

		internal int DesignerHierarchyDepth;

		internal Type DesignerGroupOwner;

		internal bool DesignerIsFromCodeAndNotProcessor;

		internal HashSet<Type> DesignerAttributesFromCode;

		internal string DesignerId;

		/// <summary>
		/// The name of the property.
		/// </summary>
		public string PropertyName { get; internal set; }

		/// <summary>
		/// Gets a value indicating whether this InspectorPropertyInfo has any backing members.
		/// </summary>
		public bool HasBackingMembers
		{
			get
			{
				if (memberInfos != null)
				{
					return memberInfos.Length != 0;
				}
				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this InspectorPropertyInfo has only a single backing member.
		/// </summary>
		public bool HasSingleBackingMember
		{
			get
			{
				if (memberInfos != null)
				{
					return memberInfos.Length == 1;
				}
				return false;
			}
		}

		/// <summary>
		/// The member info of the property. If the property has many member infos, such as if it is a group property, the first member info of <see cref="P:Sirenix.OdinInspector.Editor.InspectorPropertyInfo.MemberInfos" /> is returned.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use GetMemberInfo() instead, and note that there might not be a member at all, even if there is a value.", true)]
		public MemberInfo MemberInfo => GetMemberInfo();

		/// <summary>
		/// Indicates which type of property it is.
		/// </summary>
		public PropertyType PropertyType { get; private set; }

		/// <summary>
		/// The serialization backend for this property.
		/// </summary>
		public SerializationBackend SerializationBackend { get; private set; }

		/// <summary>
		/// The type on which this property is declared.
		/// </summary>
		public Type TypeOfOwner => typeOfOwner;

		/// <summary>
		/// The base type of the value which this property represents. If there is no value, this will be null.
		/// </summary>
		public Type TypeOfValue => typeOfValue;

		/// <summary>
		/// Whether this property is editable or not.
		/// </summary>
		public bool IsEditable { get; internal set; }

		/// <summary>
		/// All member infos of the property. There will only be more than one member if it is an <see cref="!:InspectorPropertyGroupInfo" />.
		/// </summary>
		[Obsolete("Use GetMemberInfos() instead, and note that there might not be any members at all, even if there is a value.", true)]
		public MemberInfo[] MemberInfos => memberInfos;

		/// <summary>
		/// The order value of this property. Properties are (by convention) ordered by ascending order, IE, lower order values are shown first in the inspector. The final actual ordering of properties is decided upon by the property resolver.
		/// </summary>
		public float Order { get; set; }

		/// <summary>
		/// The attributes associated with this property.
		/// </summary>
		public ImmutableList<Attribute> Attributes
		{
			get
			{
				if (attributes == null)
				{
					return null;
				}
				if (attributesImmutable == null)
				{
					attributesImmutable = new ImmutableList<Attribute>(attributes);
				}
				return attributesImmutable;
			}
		}

		/// <summary>
		/// Whether this property only exists as a Unity <see cref="T:UnityEditor.SerializedProperty" />, and has no associated managed member to represent it.
		/// This case requires some special one-off custom behaviour in a few places.
		/// </summary>
		public bool IsUnityPropertyOnly => isUnityPropertyOnly;

		public static InspectorPropertyInfo CreateForDelegate(string name, float order, Type typeOfOwner, Delegate @delegate, params Attribute[] attributes)
		{
			return CreateForDelegate(name, order, typeOfOwner, @delegate, (IEnumerable<Attribute>)attributes);
		}

		public static InspectorPropertyInfo CreateForDelegate(string name, float order, Type typeOfOwner, Delegate @delegate, IEnumerable<Attribute> attributes)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (typeOfOwner == null)
			{
				throw new ArgumentNullException("typeOfOwner");
			}
			if ((object)@delegate == null)
			{
				throw new ArgumentNullException("@delegate");
			}
			for (int i = 0; i < name.Length; i++)
			{
				if (name[i] == '.')
				{
					throw new ArgumentException("Property names may not contain '.'; was given the name '" + name + "'.");
				}
			}
			InspectorPropertyInfo result = new InspectorPropertyInfo();
			result.memberInfos = new MemberInfo[0];
			result.typeOfOwner = typeOfOwner;
			result.Order = order;
			result.PropertyName = name;
			result.PropertyType = PropertyType.Method;
			result.SerializationBackend = SerializationBackend.None;
			if (attributes == null)
			{
				result.attributes = new List<Attribute>();
			}
			else
			{
				result.attributes = attributes.Where((Attribute attr) => attr != null).ToList();
			}
			result.@delegate = @delegate;
			return result;
		}

		public static InspectorPropertyInfo CreateForUnityProperty(string unityPropertyName, Type typeOfOwner, Type typeOfValue, bool isEditable, params Attribute[] attributes)
		{
			return CreateForUnityProperty(unityPropertyName, typeOfOwner, typeOfValue, isEditable, (IEnumerable<Attribute>)attributes);
		}

		public static InspectorPropertyInfo CreateForUnityProperty(string unityPropertyName, Type typeOfOwner, Type typeOfValue, bool isEditable, IEnumerable<Attribute> attributes)
		{
			if (unityPropertyName == null)
			{
				throw new ArgumentNullException("unityPropertyName");
			}
			if (typeOfOwner == null)
			{
				throw new ArgumentNullException("typeOfOwner");
			}
			if (typeOfValue == null)
			{
				throw new ArgumentNullException("typeOfValue");
			}
			for (int i = 0; i < unityPropertyName.Length; i++)
			{
				if (unityPropertyName[i] == '.')
				{
					throw new ArgumentException("Property names may not contain '.'; was given the name '" + unityPropertyName + "'.");
				}
			}
			InspectorPropertyInfo result = new InspectorPropertyInfo();
			result.memberInfos = new MemberInfo[0];
			result.typeOfOwner = typeOfOwner;
			result.typeOfValue = typeOfValue;
			result.PropertyName = unityPropertyName;
			result.PropertyType = PropertyType.Value;
			result.SerializationBackend = SerializationBackend.Unity;
			result.IsEditable = isEditable;
			if (attributes == null)
			{
				result.attributes = new List<Attribute>();
			}
			else
			{
				result.attributes = attributes.Where((Attribute attr) => attr != null).ToList();
			}
			result.isUnityPropertyOnly = true;
			return result;
		}

		public static InspectorPropertyInfo CreateValue(string name, float order, SerializationBackend serializationBackend, IValueGetterSetter getterSetter, params Attribute[] attributes)
		{
			return CreateValue(name, order, serializationBackend, getterSetter, (IList<Attribute>)attributes);
		}

		public static InspectorPropertyInfo CreateValue(string name, float order, SerializationBackend serializationBackend, IValueGetterSetter getterSetter, IList<Attribute> attributes)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (getterSetter == null)
			{
				throw new ArgumentNullException("getterSetter");
			}
			for (int i = 0; i < name.Length; i++)
			{
				if (name[i] == '.')
				{
					throw new ArgumentException("Property names may not contain '.'; was given the name '" + name + "'.");
				}
			}
			InspectorPropertyInfo result = new InspectorPropertyInfo();
			result.memberInfos = new MemberInfo[0];
			result.typeOfOwner = getterSetter.OwnerType;
			result.typeOfValue = getterSetter.ValueType;
			if (attributes == null)
			{
				result.attributes = new List<Attribute>();
			}
			else
			{
				int count = attributes.Count;
				result.attributes = new List<Attribute>(count + 4);
				for (int j = 0; j < count; j++)
				{
					Attribute attr = attributes[j];
					if (attr != null)
					{
						result.attributes.Add(attr);
					}
				}
			}
			result.PropertyName = name;
			result.PropertyType = PropertyType.Value;
			result.SerializationBackend = serializationBackend ?? SerializationBackend.None;
			result.IsEditable = !getterSetter.IsReadonly;
			result.Order = order;
			result.getterSetter = getterSetter;
			return result;
		}

		public static InspectorPropertyInfo CreateForMember(InspectorProperty parentProperty, MemberInfo member, bool allowEditable, params Attribute[] attributes)
		{
			List<Attribute> list = new List<Attribute>(attributes.Length);
			for (int i = 0; i < attributes.Length; i++)
			{
				list.Add(attributes[i]);
			}
			return CreateForMember(member, allowEditable, InspectorPropertyInfoUtility.GetSerializationBackend(parentProperty, member), list);
		}

		public static InspectorPropertyInfo CreateForMember(InspectorProperty parentProperty, MemberInfo member, bool allowEditable, IEnumerable<Attribute> attributes)
		{
			return CreateForMember(member, allowEditable, InspectorPropertyInfoUtility.GetSerializationBackend(parentProperty, member), attributes.ToList());
		}

		public static InspectorPropertyInfo CreateForMember(MemberInfo member, bool allowEditable, SerializationBackend serializationBackend, params Attribute[] attributes)
		{
			List<Attribute> list = new List<Attribute>(attributes.Length);
			for (int i = 0; i < attributes.Length; i++)
			{
				list.Add(attributes[i]);
			}
			return CreateForMember(member, allowEditable, serializationBackend, list);
		}

		public static InspectorPropertyInfo CreateForMember(MemberInfo member, bool allowEditable, SerializationBackend serializationBackend, IEnumerable<Attribute> attributes)
		{
			return CreateForMember(member, allowEditable, serializationBackend, attributes.ToList());
		}

		public static InspectorPropertyInfo CreateForMember(MemberInfo member, bool allowEditable, SerializationBackend serializationBackend, List<Attribute> attributes)
		{
			return CreateForMember(member, allowEditable, serializationBackend, attributes, isDesignerTree: false, null);
		}

		internal static InspectorPropertyInfo CreateForMember(MemberInfo member, bool allowEditable, SerializationBackend serializationBackend, List<Attribute> attributes, bool isDesignerTree, string nameOverride)
		{
			if (member == null)
			{
				throw new ArgumentNullException("member");
			}
			if (!(member is FieldInfo) && !(member is PropertyInfo) && !(member is MethodInfo))
			{
				throw new ArgumentException("Can only create inspector properties for field, property and method members.");
			}
			if (member is MethodInfo && serializationBackend != SerializationBackend.None)
			{
				throw new ArgumentException("Serialization backend can only be None for method members.");
			}
			if (member is MethodInfo && allowEditable)
			{
				allowEditable = false;
			}
			if (allowEditable && member is FieldInfo && (member as FieldInfo).IsLiteral)
			{
				allowEditable = false;
			}
			string name = null;
			if (nameOverride != null)
			{
				name = nameOverride;
			}
			else
			{
				if (member is MethodInfo)
				{
					MethodInfo mi = member as MethodInfo;
					ParameterInfo[] parameters = mi.GetParameters();
					if (parameters.Length != 0)
					{
						name = mi.GetNiceName();
					}
				}
				if (name == null)
				{
					name = member.Name;
				}
				for (int i = 0; i < name.Length; i++)
				{
					if (name[i] == '.')
					{
						name = name.Replace(".", ">");
						break;
					}
				}
				if (member.IsDefined(typeof(OmitFromPrefabModificationPathsAttribute), inherit: false))
				{
					name = "#" + name;
				}
			}
			InspectorPropertyInfo result = new InspectorPropertyInfo();
			result.memberInfos = new MemberInfo[1] { member };
			result.PropertyName = name;
			result.PropertyType = ((member is MethodInfo) ? PropertyType.Method : PropertyType.Value);
			result.SerializationBackend = serializationBackend ?? SerializationBackend.None;
			if (attributes == null)
			{
				result.attributes = new List<Attribute>();
			}
			else
			{
				result.attributes = attributes;
				for (int i2 = attributes.Count - 1; i2 >= 0; i2--)
				{
					Attribute attr = attributes[i2];
					if (attr == null)
					{
						attributes.RemoveAt(i2);
					}
					else if (attr is PropertyOrderAttribute orderAttr)
					{
						result.Order = orderAttr.Order;
					}
				}
			}
			result.typeOfOwner = member.DeclaringType;
			if (member is FieldInfo || member is PropertyInfo)
			{
				Type valueType = (result.typeOfValue = member.GetReturnType());
				Type declType = member.DeclaringType;
				if (isDesignerTree)
				{
					result.getterSetter = GetterSetterUtility.GetEmptyGetterSetter(declType, valueType);
				}
				else
				{
					result.getterSetter = GetEmittedGetterSetterCreator(declType, valueType)(member, !allowEditable);
				}
				result.IsEditable = allowEditable && !result.attributes.HasAttribute<ReadOnlyAttribute>() && !result.getterSetter.IsReadonly;
			}
			return result;
		}

		public static InspectorPropertyInfo CreateGroup(string name, Type typeOfOwner, float order, InspectorPropertyInfo[] groupInfos, params Attribute[] attributes)
		{
			return CreateGroup(name, typeOfOwner, order, groupInfos, (IEnumerable<Attribute>)attributes);
		}

		public static InspectorPropertyInfo CreateGroup(string name, Type typeOfOwner, float order, InspectorPropertyInfo[] groupInfos, IEnumerable<Attribute> attributes)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (typeOfOwner == null)
			{
				throw new ArgumentNullException("typeOfOwner");
			}
			if (groupInfos == null)
			{
				throw new ArgumentNullException("groupInfos");
			}
			for (int i = 0; i < name.Length; i++)
			{
				if (name[i] == '.')
				{
					throw new ArgumentException("Group names or paths may not contain '.'; was given the path/name '" + name + "'.");
				}
			}
			if (name.Length == 0 || name[0] != '#')
			{
				throw new ArgumentException("The first character in a property group name must be '#'; was given the name '" + name + "'.");
			}
			InspectorPropertyInfo result = new InspectorPropertyInfo();
			if (attributes == null)
			{
				result.attributes = new List<Attribute>();
			}
			else if (attributes is List<Attribute>)
			{
				result.attributes = (List<Attribute>)attributes;
			}
			else
			{
				result.attributes = new List<Attribute>(attributes);
			}
			result.Order = order;
			result.typeOfOwner = typeOfOwner;
			result.PropertyName = name;
			result.PropertyType = PropertyType.Group;
			result.SerializationBackend = SerializationBackend.None;
			result.IsEditable = false;
			result.groupInfos = groupInfos;
			result.UpdateMemberInfosForGroup();
			return result;
		}

		internal InspectorPropertyInfo(float order, Type typeOfOwner, string propertyName, PropertyType propertyType, SerializationBackend serializationBackend, bool isEditable)
		{
			Order = order;
			this.typeOfOwner = typeOfOwner;
			PropertyName = propertyName;
			PropertyType = propertyType;
			SerializationBackend = serializationBackend;
			IsEditable = isEditable;
		}

		private InspectorPropertyInfo()
		{
		}

		/// <summary>
		/// Returns a <see cref="T:System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			if (PropertyType == PropertyType.Group)
			{
				return GetAttribute<PropertyGroupAttribute>().GroupID + " (type: " + PropertyType.ToString() + ", order: " + Order + ")";
			}
			return PropertyName + " (type: " + PropertyType.ToString() + ", backend: " + SerializationBackend?.ToString() + ", order: " + Order + ")";
		}

		/// <summary>
		/// Gets the first attribute of a given type on this property.
		/// </summary>
		public T GetAttribute<T>() where T : Attribute
		{
			if (attributes != null)
			{
				for (int i = 0; i < attributes.Count; i++)
				{
					if (attributes[i] is T result)
					{
						return result;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the first attribute of a given type on this property, which is not contained in a given hashset.
		/// </summary>
		/// <param name="exclude">The attributes to exclude.</param>
		public T GetAttribute<T>(HashSet<Attribute> exclude) where T : Attribute
		{
			if (attributes != null)
			{
				for (int i = 0; i < attributes.Count; i++)
				{
					if (attributes[i] is T attr && (exclude == null || !exclude.Contains(attr)))
					{
						return attr;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Gets all attributes of a given type on the property.
		/// </summary>
		public IEnumerable<T> GetAttributes<T>() where T : Attribute
		{
			if (attributes == null)
			{
				yield break;
			}
			for (int i = 0; i < attributes.Count; i++)
			{
				if (attributes[i] is T result)
				{
					yield return result;
				}
			}
		}

		/// <summary>
		/// The <see cref="T:Sirenix.OdinInspector.Editor.InspectorPropertyInfo" />s of all the individual properties in this group.
		/// </summary>
		public InspectorPropertyInfo[] GetGroupInfos()
		{
			return groupInfos;
		}

		public MemberInfo GetMemberInfo()
		{
			if (memberInfos.Length != 0)
			{
				return memberInfos[0];
			}
			return null;
		}

		public MemberInfo[] GetMemberInfos()
		{
			return memberInfos;
		}

		public IValueGetterSetter GetGetterSetter()
		{
			return getterSetter;
		}

		/// <summary>
		/// Gets the property's method delegate, if there is one. Note that this is null if a method property is backed by an actual method member.
		/// </summary>
		public Delegate GetMethodDelegate()
		{
			return @delegate;
		}

		public bool TryGetStrongGetterSetter<TOwner, TValue>(out IValueGetterSetter<TOwner, TValue> result)
		{
			if (PropertyType != PropertyType.Value)
			{
				result = null;
				return false;
			}
			result = getterSetter as IValueGetterSetter<TOwner, TValue>;
			if (result != null)
			{
				return true;
			}
			result = (IValueGetterSetter<TOwner, TValue>)GetEmittedAliasGetterSetterCreator(typeof(TOwner), typeof(TValue), getterSetter.OwnerType, getterSetter.ValueType)(getterSetter);
			return result != null;
		}

		public List<Attribute> GetEditableAttributesList()
		{
			return attributes;
		}

		private static Func<MemberInfo, bool, IValueGetterSetter> GetEmittedGetterSetterCreator(Type ownerType, Type valueType)
		{
			if (!GetterSetterCreators.TryGetInnerValue(ownerType, valueType, out var result))
			{
				Type type = typeof(GetterSetter<, >).MakeGenericType(ownerType, valueType);
				ConstructorInfo constructor = type.GetConstructor(GetterSetterConstructorSignature);
				DynamicMethod method = new DynamicMethod("GetterSetterCreator<" + ownerType.GetNiceName() + ", " + valueType.GetNiceName() + ">", typeof(IValueGetterSetter), new Type[2]
				{
					typeof(MemberInfo),
					typeof(bool)
				});
				ILGenerator il = method.GetILGenerator();
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Newobj, constructor);
				il.Emit(OpCodes.Ret);
				result = (Func<MemberInfo, bool, IValueGetterSetter>)method.CreateDelegate(typeof(Func<MemberInfo, bool, IValueGetterSetter>));
				GetterSetterCreators.AddInner(ownerType, valueType, result);
			}
			return result;
		}

		private static Func<IValueGetterSetter, IValueGetterSetter> GetEmittedAliasGetterSetterCreator(Type ownerType, Type valueType, Type propertyOwnerType, Type propertyValueType)
		{
			TypeSignature4 signature = new TypeSignature4(ownerType, valueType, propertyOwnerType, propertyValueType);
			if (!AliasGetterSetterCreators.TryGetValue(signature, out var result))
			{
				Type type = typeof(AliasGetterSetter<, , , >).MakeGenericType(ownerType, valueType, propertyOwnerType, propertyValueType);
				AliasGetterSetterConstructorSignature[0] = typeof(IValueGetterSetter<, >).MakeGenericType(propertyOwnerType, propertyValueType);
				ConstructorInfo constructor = type.GetConstructor(AliasGetterSetterConstructorSignature);
				DynamicMethod method = new DynamicMethod("AliasGetterSetterCreator<" + ownerType.GetNiceName() + ", " + valueType.GetNiceName() + ">", typeof(IValueGetterSetter), new Type[1] { typeof(IValueGetterSetter) });
				ILGenerator il = method.GetILGenerator();
				if (propertyOwnerType.IsAssignableFrom(ownerType) && propertyValueType.IsAssignableFrom(valueType))
				{
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Castclass, AliasGetterSetterConstructorSignature[0]);
					il.Emit(OpCodes.Newobj, constructor);
					il.Emit(OpCodes.Ret);
				}
				else
				{
					il.Emit(OpCodes.Ldnull);
					il.Emit(OpCodes.Ret);
				}
				result = (Func<IValueGetterSetter, IValueGetterSetter>)method.CreateDelegate(typeof(Func<IValueGetterSetter, IValueGetterSetter>));
				AliasGetterSetterCreators.Add(signature, result);
			}
			return result;
		}

		public InspectorPropertyInfo CreateCopy()
		{
			InspectorPropertyInfo copy = new InspectorPropertyInfo();
			copy.memberInfos = memberInfos;
			copy.attributes = attributes.ToList();
			copy.typeOfOwner = typeOfOwner;
			copy.typeOfValue = typeOfValue;
			copy.getterSetter = getterSetter;
			copy.groupInfos = groupInfos;
			copy.isUnityPropertyOnly = isUnityPropertyOnly;
			copy.@delegate = @delegate;
			copy.PropertyName = PropertyName;
			copy.PropertyType = PropertyType;
			copy.SerializationBackend = SerializationBackend;
			copy.IsEditable = IsEditable;
			copy.Order = Order;
			return copy;
		}

		internal void UpdateOrderFromAttributes()
		{
			if (attributes == null)
			{
				return;
			}
			for (int i = 0; i < attributes.Count; i++)
			{
				if (attributes[i] is PropertyOrderAttribute orderAttr)
				{
					Order = orderAttr.Order;
				}
			}
		}

		internal void UpdateMemberInfosForGroup()
		{
			int memberInfosCount = 0;
			for (int i = 0; i < groupInfos.Length; i++)
			{
				memberInfosCount += groupInfos[i].GetMemberInfos().Length;
			}
			if (memberInfos == null || memberInfos.Length != memberInfosCount)
			{
				memberInfos = new MemberInfo[memberInfosCount];
			}
			memberInfosCount = 0;
			for (int j = 0; j < groupInfos.Length; j++)
			{
				MemberInfo[] groupMembers = groupInfos[j].GetMemberInfos();
				for (int k = 0; k < groupMembers.Length; k++)
				{
					memberInfos[memberInfosCount++] = groupMembers[k];
				}
			}
		}
	}
}

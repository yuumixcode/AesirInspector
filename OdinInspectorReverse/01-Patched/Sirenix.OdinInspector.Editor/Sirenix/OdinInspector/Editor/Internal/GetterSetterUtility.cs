using System;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class GetterSetterUtility
	{
		internal readonly struct GetterSetterKey : IEquatable<GetterSetterKey>
		{
			public readonly Type OwnerType;

			public readonly Type ValueType;

			public GetterSetterKey(Type ownerType, Type valueType)
			{
				OwnerType = ownerType;
				ValueType = valueType;
			}

			public bool Equals(GetterSetterKey other)
			{
				if ((object)OwnerType != other.OwnerType && !(OwnerType == other.OwnerType))
				{
					return false;
				}
				bool valueTypeEquality = (object)ValueType == other.ValueType;
				if (!valueTypeEquality)
				{
					valueTypeEquality = ValueType == other.ValueType;
				}
				return valueTypeEquality;
			}

			public override bool Equals(object obj)
			{
				if (obj is GetterSetterKey other)
				{
					return Equals(other);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return (((OwnerType != null) ? OwnerType.GetHashCode() : 0) * 397) ^ ((ValueType != null) ? ValueType.GetHashCode() : 0);
			}
		}

		internal static readonly Dictionary<GetterSetterKey, IValueGetterSetter> CachedEmptyGetterSetters = new Dictionary<GetterSetterKey, IValueGetterSetter>(128);

		public static IValueGetterSetter GetEmptyGetterSetter(Type ownerType, Type valueType)
		{
			GetterSetterKey key = new GetterSetterKey(ownerType, valueType);
			if (CachedEmptyGetterSetters.TryGetValue(key, out var getterSetter))
			{
				return getterSetter;
			}
			Type type = typeof(GetterSetter<, >).MakeGenericType(ownerType, valueType);
			return CachedEmptyGetterSetters[key] = (IValueGetterSetter)Activator.CreateInstance(type, nonPublic: true);
		}
	}
}

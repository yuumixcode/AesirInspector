using System;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	[Serializable]
	public struct UnitySerializableType : IEquatable<UnitySerializableType>, IEquatable<Type>
	{
		[SerializeField]
		public string TypeName;

		[NonSerialized]
		private Type type;

		public Type Type
		{
			get
			{
				if (type == null && !string.IsNullOrWhiteSpace(TypeName))
				{
					type = TwoWaySerializationBinder.Default.BindToType(TypeName);
				}
				return type;
			}
			set
			{
				if (type != value)
				{
					type = value;
					if (type == null)
					{
						TypeName = "";
					}
					else
					{
						TypeName = TwoWaySerializationBinder.Default.BindToName(type);
					}
				}
			}
		}

		public string GetNiceName()
		{
			return Type?.GetNiceName();
		}

		public string GetNiceFullName()
		{
			return Type?.GetNiceFullName();
		}

		public override int GetHashCode()
		{
			if (!(Type == null))
			{
				return Type.GetHashCode();
			}
			return 0;
		}

		public bool Equals(UnitySerializableType other)
		{
			return FastTypeComparer.Instance.Equals(Type, other.Type);
		}

		public bool Equals(Type other)
		{
			return FastTypeComparer.Instance.Equals(Type, other);
		}

		public override bool Equals(object obj)
		{
			if (obj is Type t1)
			{
				return Equals(t1);
			}
			if (obj is UnitySerializableType t2)
			{
				return Equals(t2);
			}
			return false;
		}

		public static implicit operator Type(UnitySerializableType type)
		{
			return type.Type;
		}

		public static implicit operator UnitySerializableType(Type type)
		{
			return new UnitySerializableType
			{
				Type = type
			};
		}

		public static bool operator ==(UnitySerializableType a, UnitySerializableType b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(UnitySerializableType a, UnitySerializableType b)
		{
			return !a.Equals(b);
		}

		public static bool operator ==(UnitySerializableType a, Type b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(UnitySerializableType a, Type b)
		{
			return !a.Equals(b);
		}

		public static bool operator ==(Type a, UnitySerializableType b)
		{
			return b.Equals(a);
		}

		public static bool operator !=(Type a, UnitySerializableType b)
		{
			return !b.Equals(a);
		}
	}
}

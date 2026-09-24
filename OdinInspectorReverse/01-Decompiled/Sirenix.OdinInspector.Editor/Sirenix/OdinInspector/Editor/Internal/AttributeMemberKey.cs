using System;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal struct AttributeMemberKey : IEquatable<AttributeMemberKey>
	{
		public Type Type;

		public string Name;

		public AttributeMemberKey(Type type, string name)
		{
			Type = type;
			Name = name;
		}

		public bool Equals(AttributeMemberKey other)
		{
			if (Type == other.Type)
			{
				return Name == other.Name;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is AttributeMemberKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((Type != null) ? Type.GetHashCode() : 0) * 397) ^ ((Name != null) ? Name.GetHashCode() : 0);
		}
	}
}

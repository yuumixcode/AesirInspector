using System;
using System.Reflection;

namespace Sirenix.OdinInspector.Editor.Validation
{
	internal struct FixIdentifier : IEquatable<FixIdentifier>
	{
		public string Name;

		public MethodInfo MethodInfo;

		public FixIdentifier(MethodInfo methodInfo)
		{
			MethodInfo = methodInfo;
			Name = null;
		}

		public FixIdentifier(string name, MethodInfo methodInfo)
		{
			Name = name;
			MethodInfo = methodInfo;
		}

		public override string ToString()
		{
			return Name;
		}

		public bool Equals(FixIdentifier other)
		{
			return object.Equals(MethodInfo, other.MethodInfo);
		}

		public override bool Equals(object obj)
		{
			if (obj is FixIdentifier other)
			{
				return Equals(other);
			}
			return false;
		}

		public static bool operator ==(FixIdentifier left, FixIdentifier right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(FixIdentifier left, FixIdentifier right)
		{
			return !left.Equals(right);
		}

		public override int GetHashCode()
		{
			if (!(MethodInfo != null))
			{
				return 0;
			}
			return MethodInfo.GetHashCode();
		}
	}
}

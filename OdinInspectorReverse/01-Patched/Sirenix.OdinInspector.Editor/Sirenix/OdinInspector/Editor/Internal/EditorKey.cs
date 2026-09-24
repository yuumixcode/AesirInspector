using System;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal readonly struct EditorKey : IEquatable<EditorKey>
	{
		public readonly Type Type;

		public readonly object Instance;

		public readonly string Path;

		public EditorKey(Type type, object instance, string path)
		{
			Type = type;
			Instance = instance;
			Path = path;
		}

		public bool Equals(EditorKey other)
		{
			bool typeEquality = (object)Type == other.Type;
			if (!typeEquality)
			{
				typeEquality = Type == other.Type;
			}
			if (typeEquality && object.Equals(Instance, other.Instance))
			{
				return Path == other.Path;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is EditorKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int hashCode = ((Type != null) ? Type.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ ((Instance != null) ? Instance.GetHashCode() : 0);
			return (hashCode * 397) ^ ((Path != null) ? Path.GetHashCode() : 0);
		}
	}
}

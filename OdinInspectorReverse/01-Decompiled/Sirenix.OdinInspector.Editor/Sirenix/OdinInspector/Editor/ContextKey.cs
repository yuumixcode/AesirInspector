using System;
using System.Runtime.InteropServices;
using Sirenix.Serialization;

namespace Sirenix.OdinInspector.Editor
{
	[StructLayout(LayoutKind.Explicit)]
	[AlwaysFormatsSelf]
	internal struct ContextKey : ISelfFormatter, IEquatable<ContextKey>
	{
		[FieldOffset(0)]
		public Guid Key1234;

		[FieldOffset(0)]
		public int Key1;

		[FieldOffset(4)]
		public int Key2;

		[FieldOffset(8)]
		public int Key3;

		[FieldOffset(12)]
		public int Key4;

		[FieldOffset(16)]
		public int Key5;

		private static readonly Serializer<object> ObjectSerializer = Serializer.Get<object>();

		public ContextKey(int key1, int key2, int key3, int key4, int key5)
		{
			Key1234 = default(Guid);
			Key1 = key1;
			Key2 = key2;
			Key3 = key3;
			Key4 = key4;
			Key5 = key5;
		}

		public void Deserialize(IDataReader reader)
		{
			reader.ReadGuid(out Key1234);
			reader.ReadInt32(out Key5);
		}

		public void Serialize(IDataWriter writer)
		{
			writer.WriteGuid(null, Key1234);
			writer.WriteInt32(null, Key5);
		}

		public override int GetHashCode()
		{
			return Key1 + Key2 + Key3 + Key4 + Key5;
		}

		public bool Equals(ContextKey other)
		{
			if (other.Key1234 == Key1234)
			{
				return Key5 == other.Key5;
			}
			return false;
		}
	}
}

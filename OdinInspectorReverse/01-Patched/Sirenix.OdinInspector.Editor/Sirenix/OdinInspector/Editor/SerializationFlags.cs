using System;

namespace Sirenix.OdinInspector.Editor
{
	[Flags]
	internal enum SerializationFlags
	{
		Public = 2,
		Field = 4,
		Property = 8,
		AutoProperty = 0x10,
		SerializedByUnity = 0x20,
		SerializedByOdin = 0x40,
		SerializeFieldAttribute = 0x80,
		OdinSerializeAttribute = 0x100,
		NonSerializedAttribute = 0x200,
		TypeSupportedByUnity = 0x400,
		DefaultSerializationPolicy = 0x800,
		SerializeReferenceAttribute = 0x1000
	}
}

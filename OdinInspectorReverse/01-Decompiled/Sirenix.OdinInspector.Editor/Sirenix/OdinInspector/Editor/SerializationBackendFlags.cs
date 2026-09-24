using System;

namespace Sirenix.OdinInspector.Editor
{
	[Flags]
	internal enum SerializationBackendFlags
	{
		None = 0,
		Unity = 1,
		Odin = 2,
		UnityAndOdin = 3
	}
}

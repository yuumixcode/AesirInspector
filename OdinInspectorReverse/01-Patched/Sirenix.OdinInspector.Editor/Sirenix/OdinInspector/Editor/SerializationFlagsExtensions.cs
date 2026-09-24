namespace Sirenix.OdinInspector.Editor
{
	internal static class SerializationFlagsExtensions
	{
		public static bool HasAny(this SerializationFlags e, SerializationFlags flags)
		{
			return (e & flags) != 0;
		}

		public static bool HasAll(this SerializationFlags e, SerializationFlags flags)
		{
			return (e & flags) == flags;
		}

		public static bool HasNone(this SerializationFlags e, SerializationFlags flags)
		{
			return (e & flags) == 0;
		}

		public static bool HasAny(this SerializationBackendFlags e, SerializationBackendFlags flags)
		{
			return (e & flags) != 0;
		}

		public static bool HasAll(this SerializationBackendFlags e, SerializationBackendFlags flags)
		{
			return (e & flags) == flags;
		}

		public static bool HasNone(this SerializationBackendFlags e, SerializationBackendFlags flags)
		{
			return (e & flags) == 0;
		}
	}
}

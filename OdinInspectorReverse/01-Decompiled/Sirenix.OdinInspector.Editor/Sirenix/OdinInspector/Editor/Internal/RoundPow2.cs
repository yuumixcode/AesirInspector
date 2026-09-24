namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class RoundPow2
	{
		public static int Round4(int x)
		{
			return (x + 3) & -4;
		}

		public static int Round8(int x)
		{
			return (x + 7) & -8;
		}

		public static int Round16(int x)
		{
			return (x + 15) & -16;
		}

		public static int Round32(int x)
		{
			return (x + 31) & -32;
		}

		public static int Round64(int x)
		{
			return (x + 63) & -64;
		}
	}
}

using System;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class ArrayUtils
	{
		public static T[] CreateOrEmpty<T>(int capacity)
		{
			if (capacity <= 0)
			{
				return Array.Empty<T>();
			}
			return new T[capacity];
		}

		public static void ResizeIfNeeded<T>(ref T[] array, int desiredSize)
		{
			if (desiredSize > array.Length)
			{
				int newSize = array.Length * 2;
				if (desiredSize > newSize)
				{
					newSize = DesignerUtils.Round32(desiredSize);
				}
				if (array.Length != 0)
				{
					Array.Resize(ref array, newSize);
				}
				else
				{
					array = new T[newSize];
				}
			}
		}
	}
}

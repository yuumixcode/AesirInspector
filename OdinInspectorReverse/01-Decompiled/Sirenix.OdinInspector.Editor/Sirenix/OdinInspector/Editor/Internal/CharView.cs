using System;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal readonly ref struct CharView
	{
		public readonly char[] Buffer;

		public readonly int Length;

		public char this[int index] => Buffer[index];

		public CharView(char[] buffer, int length)
		{
			Buffer = buffer;
			Length = length;
		}

		public CharSlice Slice(int start, int end)
		{
			int length = Math.Max(0, end - start);
			if (start < 0 || start + length > Length)
			{
				throw new ArgumentOutOfRangeException();
			}
			return new CharSlice(Buffer, start, length);
		}
	}
}

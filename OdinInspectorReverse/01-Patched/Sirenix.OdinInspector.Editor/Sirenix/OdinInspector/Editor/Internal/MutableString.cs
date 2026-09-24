namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class MutableString
	{
		public int Length;

		public char[] Buffer;

		public int Capacity => Buffer.Length;

		public MutableString(int capacity)
		{
			Length = 0;
			Buffer = ArrayUtils.CreateOrEmpty<char>(capacity);
		}

		public override string ToString()
		{
			if (Length <= 0)
			{
				return string.Empty;
			}
			return new string(Buffer, 0, Length);
		}

		public void Clear()
		{
			Length = 0;
		}

		public void Append(char c)
		{
			EnsureCapacityFor(1);
			Buffer[Length++] = c;
		}

		public void Append(string str)
		{
			if (!string.IsNullOrEmpty(str))
			{
				EnsureCapacityFor(str.Length);
				for (int i = 0; i < str.Length; i++)
				{
					Buffer[Length++] = str[i];
				}
			}
		}

		public void EnsureCapacityFor(int amount)
		{
			ArrayUtils.ResizeIfNeeded(ref Buffer, Length + amount);
		}
	}
}

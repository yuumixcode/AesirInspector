using System.Runtime.CompilerServices;

namespace Sirenix.Utilities.Editor
{
	public struct ArraySlice<T>
	{
		public struct Iterator
		{
			private int index;

			private ArraySlice<T> arr;

			public ref T Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
				get
				{
					return ref arr[index];
				}
			}

			public Iterator(ArraySlice<T> arr)
			{
				index = -1;
				this.arr = arr;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			public bool MoveNext()
			{
				index++;
				return index < arr.Length;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			public void Reset()
			{
				index = -1;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			public void Dispose()
			{
			}
		}

		public T[] OriginalArray;

		public int Offset;

		public int Length;

		public ref T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			get
			{
				return ref OriginalArray[index + Offset];
			}
		}

		public ArraySlice(T[] array, int offset, int length)
		{
			OriginalArray = array;
			Offset = offset;
			Length = length;
		}

		public Iterator GetEnumerator()
		{
			return new Iterator(this);
		}
	}
}

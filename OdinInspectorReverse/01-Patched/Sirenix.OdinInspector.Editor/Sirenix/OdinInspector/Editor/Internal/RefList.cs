using System;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor.Internal
{
	[Serializable]
	internal class RefList<TStruct> where TStruct : struct
	{
		/// <summary>
		///
		/// </summary>
		/// <remarks>Does not guarantee stability.</remarks>
		[Serializable]
		public readonly struct IndexRef
		{
			public readonly RefList<TStruct> Source;

			public readonly int Index;

			public static IndexRef Null => new IndexRef(null, -1);

			public bool IsNull
			{
				get
				{
					if (Source != null)
					{
						return Index < 0;
					}
					return true;
				}
			}

			public ref TStruct Ref => ref Source[Index];

			public IndexRef(RefList<TStruct> source, int index)
			{
				Source = source;
				Index = index;
			}
		}

		internal sealed class ItemComparer : IComparer<TStruct>
		{
			private readonly Comparison<TStruct> comparison;

			public ItemComparer(Comparison<TStruct> comparison)
			{
				this.comparison = comparison;
			}

			public int Compare(TStruct x, TStruct y)
			{
				return comparison(x, y);
			}
		}

		public static readonly RefList<TStruct> Empty = new RefList<TStruct>(0);

		public TStruct[] Items;

		public int Length;

		public int Capacity => Items.Length;

		public ref TStruct this[int index] => ref Items[index];

		public RefList(int capacity = 4)
		{
			Items = ((capacity > 0) ? new TStruct[capacity] : Array.Empty<TStruct>());
			Length = 0;
		}

		public IndexRef GetIndexRef(int index)
		{
			return new IndexRef(this, index);
		}

		public RefList<TStruct> CopyTrim()
		{
			TStruct[] resultItems = new TStruct[Length];
			Array.Copy(Items, 0, resultItems, 0, Length);
			return new RefList<TStruct>
			{
				Items = resultItems,
				Length = Length
			};
		}

		public void Add(ref TStruct item)
		{
			EnsureSpaceFor1();
			Items[Length++] = item;
		}

		public ref TStruct AddDefault()
		{
			EnsureSpaceFor1();
			int index = Length;
			Items[index] = default(TStruct);
			Length++;
			return ref Items[index];
		}

		public IndexRef AddAndGetHandle(ref TStruct item)
		{
			EnsureSpaceFor1();
			int index = Length;
			Items[index] = item;
			Length++;
			return GetIndexRef(index);
		}

		public IndexRef AddDefaultAndGetHandle()
		{
			EnsureSpaceFor1();
			int index = Length;
			Items[index] = new TStruct();
			Length++;
			return GetIndexRef(index);
		}

		public void RemoveAt(int index)
		{
			Length--;
			int moveCount = Length - index;
			if (moveCount > 0)
			{
				Array.Copy(Items, index + 1, Items, index, moveCount);
			}
		}

		public void Reset()
		{
			Length = 0;
		}

		public void Clear()
		{
			if (Length != 0)
			{
				Array.Clear(Items, 0, Length);
				Length = 0;
			}
		}

		public void Sort(Comparison<TStruct> comparison)
		{
			Array.Sort(Items, 0, Length, new ItemComparer(comparison));
		}

		public void SetCapacity(int capacity)
		{
			Array.Resize(ref Items, capacity);
		}

		public void EnsureSpaceFor1()
		{
			if (Length >= Capacity)
			{
				if (Capacity == 0)
				{
					Items = new TStruct[4];
				}
				else
				{
					SetCapacity(Capacity * 2);
				}
			}
		}
	}
}

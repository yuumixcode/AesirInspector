using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Validation;

namespace Sirenix.OdinValidator.Editor
{
	public class ValidationWorkItemQueue
	{
		private struct CircularBufferQueue
		{
			public int Count;

			public int CurrentIndex;

			public ItemRef[] Items;

			public CircularBufferQueue(int capacity)
			{
				Count = 0;
				CurrentIndex = 0;
				Items = new ItemRef[capacity];
			}

			public void Clear()
			{
				Count = 0;
				CurrentIndex = 0;
			}

			public ItemRef Peek()
			{
				if (Count == 0)
				{
					throw new InvalidOperationException("Can't peek when there are no items.");
				}
				return Items[CurrentIndex];
			}

			public ItemRef Dequeue()
			{
				if (Count == 0)
				{
					throw new InvalidOperationException("Can't dequeue when there are no items.");
				}
				int index = CurrentIndex;
				if (++CurrentIndex >= Items.Length)
				{
					CurrentIndex = 0;
				}
				Count--;
				return Items[index];
			}

			public void InsertFirst(ItemRef itemRef)
			{
				EnsureSpaceForOneMoreItem();
				Count++;
				if (CurrentIndex == 0)
				{
					CurrentIndex = Items.Length - 1;
				}
				else
				{
					CurrentIndex--;
				}
				Items[CurrentIndex] = itemRef;
			}

			public void Enqueue(ItemRef itemRef)
			{
				EnsureSpaceForOneMoreItem();
				int current = (CurrentIndex + Count++) % Items.Length;
				Items[current] = itemRef;
			}

			private void EnsureSpaceForOneMoreItem()
			{
				if (Count + 1 != Items.Length)
				{
					return;
				}
				if (CurrentIndex == 0)
				{
					Array.Resize(ref Items, Items.Length * 2);
					return;
				}
				ItemRef[] newItems = new ItemRef[Items.Length * 2];
				int newI = 0;
				for (int i = CurrentIndex; i < Items.Length; i++)
				{
					newItems[newI++] = Items[i];
				}
				for (int j = 0; j < CurrentIndex; j++)
				{
					newItems[newI++] = Items[j];
				}
				CurrentIndex = 0;
				Items = newItems;
			}
		}

		private struct ItemRef
		{
			public int Version;

			public int Index;
		}

		private struct ValidationWorkItemEntry
		{
			public int Version;

			public ValidationWorkItem Item;
		}

		private ulong[] entrySpace = new ulong[1];

		private ValidationWorkItemEntry[] entries = new ValidationWorkItemEntry[64];

		private Dictionary<ValidationWorkItem, ItemRef> keyToItem = new Dictionary<ValidationWorkItem, ItemRef>(default(ValidationWorkItem.Comparer));

		private HashSet<SceneReference> queuedSceneReferences = new HashSet<SceneReference>();

		private CircularBufferQueue itemQueue = new CircularBufferQueue(256);

		private int available = 64;

		private int count;

		public uint TotalResultsEstimate;

		private const ulong DeBruijnSequence = 251784493209109903uL;

		private static readonly int[] MultiplyDeBruijnBitPosition = new int[64]
		{
			0, 1, 17, 2, 18, 50, 3, 57, 47, 19,
			22, 51, 29, 4, 33, 58, 15, 48, 20, 27,
			25, 23, 52, 41, 54, 30, 38, 5, 43, 34,
			59, 8, 63, 16, 49, 56, 46, 21, 28, 32,
			14, 26, 24, 40, 53, 37, 42, 7, 62, 55,
			45, 31, 13, 39, 36, 6, 61, 44, 12, 35,
			60, 11, 10, 9
		};

		public int Count => count;

		public ICollection<SceneReference> AllQueuedSceneReferences => queuedSceneReferences;

		public bool Contains(ValidationWorkItem key)
		{
			return keyToItem.ContainsKey(key);
		}

		public bool Remove(ValidationWorkItem key)
		{
			if (keyToItem.TryGetValue(key, out var itemRef))
			{
				ValidationWorkItemEntry entry = entries[itemRef.Index];
				entrySpace[itemRef.Index >> 6] &= (ulong)(~(1L << itemRef.Index));
				count--;
				keyToItem.Remove(entry.Item);
				TotalResultsEstimate -= entry.Item.ResultCountEstimate;
				if (key.SceneContent.HasValue)
				{
					bool removed = queuedSceneReferences.Remove(entry.Item.SceneContent.Value);
				}
				return true;
			}
			return false;
		}

		public void Clear()
		{
			ulong[] space = entrySpace;
			ValidationWorkItemEntry[] entries = this.entries;
			for (int i = 0; i < space.Length; i++)
			{
				space[i] = 0uL;
			}
			ValidationWorkItemEntry defaultEntry = default(ValidationWorkItemEntry);
			for (int j = 0; j < entries.Length; j++)
			{
				entries[j] = defaultEntry;
			}
			itemQueue.Clear();
			keyToItem.Clear();
			queuedSceneReferences.Clear();
			count = 0;
			TotalResultsEstimate = 0u;
			available = this.entries.Length;
		}

		public ValidationWorkItem PeekMaybeInvalid()
		{
			if (count == 0)
			{
				throw new InvalidOperationException("Queue is empty.");
			}
			ItemRef itemRef = itemQueue.Peek();
			ValidationWorkItemEntry entry = entries[itemRef.Index];
			return entry.Item;
		}

		public ValidationWorkItem Dequeue()
		{
			if (count == 0)
			{
				throw new InvalidOperationException("Queue is empty.");
			}
			while (itemQueue.Count > 0)
			{
				ItemRef itemRef = itemQueue.Dequeue();
				ValidationWorkItemEntry entry = entries[itemRef.Index];
				if (entry.Version == itemRef.Version && (entrySpace[itemRef.Index >> 6] & (ulong)(1L << itemRef.Index)) != 0L)
				{
					entrySpace[itemRef.Index >> 6] &= (ulong)(~(1L << itemRef.Index));
					count--;
					available++;
					keyToItem.Remove(entry.Item);
					if (entry.Item.SceneContent.HasValue)
					{
						queuedSceneReferences.Remove(entry.Item.SceneContent.Value);
					}
					TotalResultsEstimate -= entry.Item.ResultCountEstimate;
					return entry.Item;
				}
			}
			throw new InvalidOperationException("Count said there were valid work items to dequeue, but there weren't.");
		}

		public bool Enqueue(ValidationWorkItem item)
		{
			return Add(item, insertFirst: false);
		}

		public bool InsertFirst(ValidationWorkItem item)
		{
			return Add(item, insertFirst: true);
		}

		private bool Add(ValidationWorkItem item, bool insertFirst)
		{
			if (keyToItem.TryGetValue(item, out var itemRef))
			{
				if (insertFirst)
				{
					Remove(item);
					Add(item, insertFirst);
					return false;
				}
				entries[itemRef.Index].Item = item;
				return false;
			}
			TotalResultsEstimate += item.ResultCountEstimate;
			if (available == 0)
			{
				ValidationWorkItemEntry entry = new ValidationWorkItemEntry
				{
					Item = item
				};
				itemRef.Index = entries.Length;
				int entrySpaceIndex = entrySpace.Length;
				available = entries.Length - 1;
				Array.Resize(ref entries, entries.Length * 2);
				Array.Resize(ref entrySpace, entries.Length / 64);
				entries[itemRef.Index] = entry;
				entrySpace[entrySpaceIndex] = 1uL;
				if (insertFirst)
				{
					itemQueue.InsertFirst(itemRef);
				}
				else
				{
					itemQueue.Enqueue(itemRef);
				}
				keyToItem.Add(item, itemRef);
				if (item.SceneContent.HasValue)
				{
					queuedSceneReferences.Add(item.SceneContent.Value);
				}
				count++;
				return true;
			}
			ulong[] space = entrySpace;
			for (int i = 0; i < space.Length; i++)
			{
				ulong bits = space[i];
				if (bits != ulong.MaxValue)
				{
					int availableIndex = MultiplyDeBruijnBitPosition[(~bits & (bits + 1)) * 251784493209109903L >> 58];
					itemRef.Index = i * 64 + availableIndex;
					itemRef.Version = ++entries[itemRef.Index].Version;
					entries[itemRef.Index].Item = item;
					if (insertFirst)
					{
						itemQueue.InsertFirst(itemRef);
					}
					else
					{
						itemQueue.Enqueue(itemRef);
					}
					keyToItem.Add(item, itemRef);
					if (item.SceneContent.HasValue)
					{
						queuedSceneReferences.Add(item.SceneContent.Value);
					}
					space[i] |= (ulong)(1L << availableIndex);
					available--;
					count++;
					return true;
				}
			}
			throw new Exception("Queue bucket was supposed to have available space but no space was marked available.");
		}
	}
}

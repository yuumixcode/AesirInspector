using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class PathLookup<T> where T : class
	{
		public class Node
		{
			public int Version;

			public Dictionary<StringSlice, Node> Children;

			public int ValueIndex = -1;

			public bool ValuesExistForChildren;

			private static StringSlice[] childrenToRemove = new StringSlice[2];

			private static int childrenToRemoveCount;

			public int CountChildren()
			{
				if (Children == null)
				{
					return 0;
				}
				int children = Children.Count;
				foreach (KeyValuePair<StringSlice, Node> item in Children.GFIterator())
				{
					children += item.Value.CountChildren();
				}
				return children;
			}

			public void Cleanup(int version)
			{
				if (Children == null)
				{
					return;
				}
				childrenToRemoveCount = 0;
				foreach (KeyValuePair<StringSlice, Node> pair in Children.GFIterator())
				{
					if (pair.Value.Version != version)
					{
						if (childrenToRemoveCount >= childrenToRemove.Length)
						{
							Array.Resize(ref childrenToRemove, childrenToRemove.Length * 2);
						}
						childrenToRemove[childrenToRemoveCount++] = pair.Key;
					}
				}
				for (int i = 0; i < childrenToRemoveCount; i++)
				{
					Children.Remove(childrenToRemove[i]);
				}
				foreach (Node child in Children.GFValueIterator())
				{
					child.Cleanup(version);
				}
			}
		}

		public int Version;

		public Node Root = new Node();

		public int NodeCount;

		public int ValueCount;

		public T[] Values = new T[32];

		public bool IsRebuilding;

		public int NodesUpdatedToLatestVersion;

		public bool IsDirty;

		public int ValueCountBeforeRebuild;

		public int NodeCountBeforeRebuild;

		public void BeginRebuild()
		{
			if (IsRebuilding)
			{
				throw new Exception("PathLookup is already rebuilding");
			}
			IsRebuilding = true;
			ValueCountBeforeRebuild = ValueCount;
			NodeCountBeforeRebuild = NodeCount;
			IsDirty = false;
			Version++;
			Root.Version = Version;
			Root.ValueIndex = -1;
			T[] values = Values;
			int count = ValueCount;
			for (int i = 0; i < count; i++)
			{
				values[i] = null;
			}
			ValueCount = 0;
			NodesUpdatedToLatestVersion = 0;
		}

		public void FinishRebuild()
		{
			if (!IsRebuilding)
			{
				throw new Exception("PathLookup is not rebuilding");
			}
			IsRebuilding = false;
			if (NodesUpdatedToLatestVersion != NodeCount || ValueCount != ValueCountBeforeRebuild || NodeCount != NodeCountBeforeRebuild)
			{
				IsDirty = true;
			}
			if (IsDirty)
			{
				CleanUp();
			}
		}

		public void CleanUp()
		{
			IsDirty = false;
			int newLength = Values.Length;
			while (ValueCount * 3 < newLength)
			{
				newLength /= 2;
			}
			if (newLength < ValueCount)
			{
				newLength = ValueCount;
			}
			if (newLength != Values.Length)
			{
				Array.Resize(ref Values, newLength);
			}
			Root.Cleanup(Version);
			NodeCount = Root.CountChildren();
		}

		public bool TryGetValue(StringSlice path, out StringSlice nearestPath, out bool childValuesExistForValue, out T value)
		{
			Node current = Root;
			StringSlice remainingPathSlice = path;
			nearestPath = default(StringSlice);
			value = null;
			childValuesExistForValue = false;
			StringSlice step;
			Node child;
			while (true)
			{
				if (current.Version != Version || current.Children == null)
				{
					nearestPath = ((remainingPathSlice.Index == path.Index) ? ((StringSlice)string.Empty) : path.Slice(0, remainingPathSlice.Index - path.Index - 1));
					return false;
				}
				int nextSeparator = remainingPathSlice.FirstIndexOf('.');
				step = ((nextSeparator == -1) ? remainingPathSlice : remainingPathSlice.Slice(0, nextSeparator));
				if (!current.Children.TryGetValue(step, out child))
				{
					nearestPath = ((step.Index == path.Index) ? ((StringSlice)string.Empty) : path.Slice(0, step.Index - path.Index - 1));
					return false;
				}
				if (nextSeparator == -1)
				{
					break;
				}
				current = child;
				remainingPathSlice = remainingPathSlice.Slice(nextSeparator + 1);
			}
			childValuesExistForValue = child.ValuesExistForChildren;
			if (child.ValueIndex == -1)
			{
				nearestPath = ((step.Index == path.Index) ? ((StringSlice)string.Empty) : path.Slice(0, step.Index - path.Index - 1));
				return false;
			}
			value = Values[child.ValueIndex];
			return true;
		}

		public void AddValue(StringSlice path, T value)
		{
			if (!IsRebuilding)
			{
				throw new Exception("Cannot add values to a PathLookup while it is not rebuilding");
			}
			int valueIndex = ValueCount;
			while (valueIndex >= Values.Length)
			{
				T[] values = Values;
				T[] newValues = new T[Math.Max(values.Length, 4) * 2];
				for (int i = 0; i < values.Length; i++)
				{
					newValues[i] = values[i];
				}
				Values = newValues;
			}
			Values[valueIndex] = value;
			ValueCount++;
			Node current = Root;
			StringSlice remainingPathSlice = path;
			Node child;
			while (true)
			{
				if (current.Version != Version)
				{
					current.Version = Version;
					current.ValueIndex = -1;
					NodesUpdatedToLatestVersion++;
				}
				current.ValuesExistForChildren = true;
				int nextSeparator = remainingPathSlice.FirstIndexOf('.');
				if (current.Children == null)
				{
					current.Children = new Dictionary<StringSlice, Node>(StringSliceEqualityComparer.Instance);
				}
				StringSlice step = ((nextSeparator == -1) ? remainingPathSlice : remainingPathSlice.Slice(0, nextSeparator));
				if (!current.Children.TryGetValue(step, out child))
				{
					child = new Node();
					NodeCount++;
					current.Children.Add(step, child);
				}
				if (child.Version != Version)
				{
					child.Version = Version;
					child.ValueIndex = -1;
					child.ValuesExistForChildren = false;
					NodesUpdatedToLatestVersion++;
				}
				if (nextSeparator == -1)
				{
					break;
				}
				current = child;
				remainingPathSlice = remainingPathSlice.Slice(nextSeparator + 1);
			}
			child.ValueIndex = valueIndex;
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.Reflection.Editor
{
	public struct GUILayoutGroup_Internal : IEquatable<GUILayoutGroup_Internal>, IList<GUILayoutEntry_Internal>, ICollection<GUILayoutEntry_Internal>, IEnumerable<GUILayoutEntry_Internal>, IEnumerable
	{
		internal UnityEngine.GUILayoutGroup group;

		public object Group => group;

		public ref Rect rect => ref group.rect;

		public ref bool consideredForMargin => ref group.consideredForMargin;

		public ref float minWidth => ref group.minWidth;

		public ref float maxWidth => ref group.maxWidth;

		public ref float minHeight => ref group.minHeight;

		public ref float maxHeight => ref group.maxHeight;

		public ref int stretchHeight => ref group.stretchHeight;

		public ref int stretchWidth => ref group.stretchWidth;

		public ref bool resetCoords => ref group.resetCoords;

		public ref bool IsVertical => ref group.isVertical;

		public ref bool IsWindow => ref group.isWindow;

		public GUILayoutEntry_Internal this[int index]
		{
			get
			{
				return new GUILayoutEntry_Internal(group.entries[index]);
			}
			set
			{
				group.entries[index] = value.entry;
			}
		}

		public int Count => group.entries.Count;

		public bool IsReadOnly => false;

		internal GUILayoutGroup_Internal(UnityEngine.GUILayoutGroup group)
		{
			this.group = group;
		}

		public override bool Equals(object obj)
		{
			if (obj is GUILayoutGroup_Internal @internal)
			{
				return Equals(@internal);
			}
			return false;
		}

		public bool Equals(GUILayoutGroup_Internal other)
		{
			return EqualityComparer<UnityEngine.GUILayoutGroup>.Default.Equals(group, other.group);
		}

		public override int GetHashCode()
		{
			return 742809708 + EqualityComparer<UnityEngine.GUILayoutGroup>.Default.GetHashCode(group);
		}

		public int IndexOf(GUILayoutEntry_Internal item)
		{
			return group.entries.IndexOf(item.entry);
		}

		public void Insert(int index, GUILayoutEntry_Internal item)
		{
			group.entries.Insert(index, item.entry);
		}

		public void RemoveAt(int index)
		{
			group.entries.RemoveAt(index);
		}

		public void Add(GUILayoutEntry_Internal item)
		{
			group.entries.Add(item.entry);
		}

		public void Clear()
		{
			group.entries.Clear();
		}

		public bool Contains(GUILayoutEntry_Internal item)
		{
			return group.entries.Contains(item.entry);
		}

		public void CopyTo(GUILayoutEntry_Internal[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(GUILayoutEntry_Internal item)
		{
			return group.entries.Remove(item.entry);
		}

		public IEnumerator<GUILayoutEntry_Internal> GetEnumerator()
		{
			foreach (UnityEngine.GUILayoutEntry item in group.entries)
			{
				yield return new GUILayoutEntry_Internal(item);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (UnityEngine.GUILayoutEntry item in group.entries)
			{
				yield return new GUILayoutEntry_Internal(item);
			}
		}

		public static bool operator ==(GUILayoutGroup_Internal left, GUILayoutGroup_Internal right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(GUILayoutGroup_Internal left, GUILayoutGroup_Internal right)
		{
			return !(left == right);
		}
	}
}

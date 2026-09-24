using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.Reflection.Editor
{
	public struct GUILayoutEntry_Internal : IEquatable<GUILayoutEntry_Internal>
	{
		internal UnityEngine.GUILayoutEntry entry;

		public object Entry => entry;

		public ref Rect rect => ref entry.rect;

		public ref float minWidth => ref entry.minWidth;

		public ref float maxWidth => ref entry.maxWidth;

		public ref float minHeight => ref entry.minHeight;

		public ref float maxHeight => ref entry.maxHeight;

		public ref int stretchHeight => ref entry.stretchHeight;

		public ref int stretchWidth => ref entry.stretchWidth;

		public ref bool consideredForMargin => ref entry.consideredForMargin;

		public bool IsLayoutGroup => entry is UnityEngine.GUILayoutGroup;

		public GUILayoutGroup_Internal AsLayoutGroup => new GUILayoutGroup_Internal((UnityEngine.GUILayoutGroup)entry);

		internal GUILayoutEntry_Internal(UnityEngine.GUILayoutEntry entry)
		{
			this.entry = entry;
		}

		public override bool Equals(object obj)
		{
			if (obj is GUILayoutEntry_Internal @internal)
			{
				return Equals(@internal);
			}
			return false;
		}

		public bool Equals(GUILayoutEntry_Internal other)
		{
			return EqualityComparer<UnityEngine.GUILayoutEntry>.Default.Equals(entry, other.entry);
		}

		public override int GetHashCode()
		{
			return 742809708 + EqualityComparer<UnityEngine.GUILayoutEntry>.Default.GetHashCode(entry);
		}

		public static bool operator ==(GUILayoutEntry_Internal left, GUILayoutEntry_Internal right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(GUILayoutEntry_Internal left, GUILayoutEntry_Internal right)
		{
			return !(left == right);
		}
	}
	public struct GUILayoutEntry_Internal<T> : IEquatable<GUILayoutEntry_Internal<T>> where T : class
	{
		internal CustomGUILayoutEntry<T> entry;

		public object Entry => entry;

		public ref T Value => ref entry.Value;

		public ref Rect rect => ref entry.rect;

		public ref float minWidth => ref entry.minWidth;

		public ref float maxWidth => ref entry.maxWidth;

		public ref float minHeight => ref entry.minHeight;

		public ref float maxHeight => ref entry.maxHeight;

		public ref int stretchHeight => ref entry.stretchHeight;

		public ref int stretchWidth => ref entry.stretchWidth;

		public ref bool consideredForMargin => ref entry.consideredForMargin;

		internal GUILayoutEntry_Internal(CustomGUILayoutEntry<T> entry)
		{
			this.entry = entry;
		}

		public void CalcWidth()
		{
			entry.CalcWidth();
		}

		public void CalcHeight()
		{
			entry.CalcHeight();
		}

		public void CalcWidth(float y, float height)
		{
			entry.SetVertical(y, height);
		}

		public void CalcHeight(float x, float width)
		{
			entry.SetHorizontal(x, width);
		}

		public static GUILayoutEntry_Internal<T> CreateCustom(T value, SetVertical<T> setVertical, SetHorizontal<T> setHorizontal, CalcWidth<T> calcWidth, CalcHeight<T> calcHeight)
		{
			CustomGUILayoutEntry<T> entry = new CustomGUILayoutEntry<T>(value, setVertical, setHorizontal, calcWidth, calcHeight);
			return new GUILayoutEntry_Internal<T>(entry);
		}

		public override bool Equals(object obj)
		{
			if (obj is GUILayoutEntry_Internal<T> @internal)
			{
				return Equals(@internal);
			}
			return false;
		}

		public bool Equals(GUILayoutEntry_Internal<T> other)
		{
			return EqualityComparer<UnityEngine.GUILayoutEntry>.Default.Equals(entry, other.entry);
		}

		public override int GetHashCode()
		{
			return 742809708 + EqualityComparer<UnityEngine.GUILayoutEntry>.Default.GetHashCode(entry);
		}

		public static bool operator ==(GUILayoutEntry_Internal<T> left, GUILayoutEntry_Internal<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(GUILayoutEntry_Internal<T> left, GUILayoutEntry_Internal<T> right)
		{
			return !(left == right);
		}
	}
}

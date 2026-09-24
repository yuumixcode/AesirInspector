using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.Reflection.Editor
{
	public struct ScrollViewState_Internal : IEquatable<ScrollViewState_Internal>
	{
		internal UnityEngine.ScrollViewState svs;

		public ref Rect position => ref svs.position;

		public ref Rect visibleRect => ref svs.visibleRect;

		public ref Rect viewRect => ref svs.viewRect;

		public ref Vector2 scrollPosition => ref svs.scrollPosition;

		public ref bool apply => ref svs.apply;

		internal ScrollViewState_Internal(UnityEngine.ScrollViewState svs)
		{
			this.svs = svs;
		}

		public override bool Equals(object obj)
		{
			if (obj is ScrollViewState_Internal @internal)
			{
				return Equals(@internal);
			}
			return false;
		}

		public bool Equals(ScrollViewState_Internal other)
		{
			return EqualityComparer<UnityEngine.ScrollViewState>.Default.Equals(svs, other.svs);
		}

		public override int GetHashCode()
		{
			return 742809708 + EqualityComparer<UnityEngine.ScrollViewState>.Default.GetHashCode(svs);
		}

		public static bool operator ==(ScrollViewState_Internal left, ScrollViewState_Internal right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ScrollViewState_Internal left, ScrollViewState_Internal right)
		{
			return !(left == right);
		}
	}
}

using UnityEngine;

namespace Sirenix.Reflection.Editor
{
	internal class CustomGUILayoutEntry<T> : UnityEngine.GUILayoutEntry where T : class
	{
		public T Value;

		private GUILayoutEntry_Internal<T> e;

		private CalcHeight<T> calcHeight;

		private SetVertical<T> setVertical;

		private SetHorizontal<T> setHorizontal;

		private CalcWidth<T> calcWidth;

		public CustomGUILayoutEntry(T value, SetVertical<T> setVertical, SetHorizontal<T> setHorizontal, CalcWidth<T> calcWidth, CalcHeight<T> calcHeight)
			: base(0f, 0f, 0f, 0f, null)
		{
			Value = value;
			this.setVertical = setVertical;
			this.setHorizontal = setHorizontal;
			this.calcWidth = calcWidth;
			this.calcHeight = calcHeight;
			consideredForMargin = false;
			e = new GUILayoutEntry_Internal<T>(this);
		}

		public override void CalcHeight()
		{
			calcHeight(ref e);
		}

		public override void CalcWidth()
		{
			calcWidth(ref e);
		}

		public override void SetHorizontal(float x, float width)
		{
			setHorizontal(ref e, x, width);
		}

		public override void SetVertical(float y, float height)
		{
			setVertical(ref e, y, height);
		}

		protected override void ApplyStyleSettings(GUIStyle style)
		{
			base.ApplyStyleSettings(GUIStyle.none);
		}
	}
}

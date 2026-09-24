using System;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// A cell of a <see cref="T:Sirenix.Utilities.Editor.GUITable" />
	/// </summary>
	public class GUITableCell
	{
		private Rect rect;

		/// <summary>
		/// The minimum width.
		/// </summary>
		public float MinWidth = 10f;

		/// <summary>
		/// <para>The width of the cell. Default is width is 0.</para>
		/// <para>The width the column is determained by the widest cell in the column.</para>
		/// <para>Width = 0 = auto.</para>
		/// </summary>
		public float Width;

		/// <summary>
		/// <para>The height of the cell. Default is height is 22.</para>
		/// <para>The height the column is determained by the tallest cell in the row.</para>
		/// </summary>
		public float Height = 22f;

		/// <summary>
		/// If true, the cell will expand vertically, covering all neighbour null cells.
		/// </summary>
		public bool SpanY;

		/// <summary>
		/// If true, the cell will expand horizontally, covering all neighbour null cells.
		/// </summary>
		public bool SpanX;

		public Action<Rect> OnGUI;

		/// <summary>
		/// The GUI style
		/// </summary>
		public Action<Rect> GUIStyle;

		internal GUITable Table;

		/// <summary>
		/// The table column index.
		/// </summary>
		public int X { get; internal set; }

		/// <summary>
		/// The table row index.
		/// </summary>
		public int Y { get; internal set; }

		/// <summary>
		/// Gets the rect.
		/// </summary>
		public Rect Rect
		{
			get
			{
				return rect;
			}
			internal set
			{
				rect = value;
			}
		}

		internal void Draw()
		{
			if (GUIStyle != null && Event.current.type == EventType.Repaint)
			{
				GUIStyle(rect);
			}
			if (OnGUI != null)
			{
				OnGUI(rect);
			}
		}
	}
}

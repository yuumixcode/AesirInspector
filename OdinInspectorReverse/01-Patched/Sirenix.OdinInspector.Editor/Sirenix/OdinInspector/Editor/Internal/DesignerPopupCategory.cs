using System;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal struct DesignerPopupCategory
	{
		public bool IsExpanded;

		public string Label;

		public RefList<DesignerPopupItem> Items;

		public DesignerPopupCategory(string label, bool isExpanded)
		{
			IsExpanded = isExpanded;
			Label = label;
			Items = RefList<DesignerPopupItem>.Empty;
		}

		public void EnsureItemsCapacity(int capacity)
		{
			if (Items.Capacity == 0)
			{
				Items = new RefList<DesignerPopupItem>(capacity);
			}
			else if (Items.Capacity < capacity)
			{
				Items.SetCapacity(DesignerUtils.Round8(capacity));
			}
		}

		public void Sort()
		{
			Items.Sort((DesignerPopupItem a, DesignerPopupItem b) => string.Compare(a.Label, b.Label, StringComparison.Ordinal));
		}

		public void Draw(Rect rect, int index, bool highlight)
		{
			Event e = Event.current;
			Rect interactRect = rect;
			if (SirenixEditorGUI.DoButton(interactRect, out var _, out var hovering, out var _))
			{
				IsExpanded = !IsExpanded;
				DesignerAttributePopup.NeedsScrollViewRebuild = true;
			}
			Color bgColor = ((highlight || hovering) ? Colors.AttributePopup.CategoryBgHover : Colors.AttributePopup.CategoryBg);
			EditorGUI.DrawRect(rect, bgColor);
			if (index != 0)
			{
				Color c = Colors.AttributePopup.CategoryBorder;
				c.a = GUI.color.a;
				GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, c, new Vector4(0f, 1f, 0f, IsExpanded ? 1 : 0), 0f);
			}
			Rect foldoutRect = rect.TakeFromLeft(rect.height);
			SdfIcons.DrawIcon(foldoutRect.AlignCenter(8f, 8f), IsExpanded ? SdfIconType.CaretDownFill : SdfIconType.CaretRightFill, Colors.Icons.Default);
			GUI.Label(rect.SubX(3f), Label, DesignerPopupItem.labelStyle);
		}
	}
}

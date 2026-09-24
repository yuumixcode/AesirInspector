using System;
using Sirenix.OdinInspector.Editor.Examples;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal struct DesignerPopupItem
	{
		public const int ItemHeight = 24;

		private static GUIStyle labelStyleBackingField;

		public string Label;

		public Type AttributeType;

		public bool HasExample;

		public static GUIStyle labelStyle
		{
			get
			{
				if (labelStyleBackingField == null)
				{
					labelStyleBackingField = new GUIStyle(EditorStyles.label);
					labelStyleBackingField.fontSize = 12;
				}
				return labelStyleBackingField;
			}
		}

		public DesignerPopupItem(OdinVisualDesignerAttributeItem attributeItem)
		{
			Label = attributeItem.Label;
			AttributeType = attributeItem.AttributeType;
			HasExample = AttributeExampleUtilities.HasExample(attributeItem.AttributeType);
		}

		public void Draw(Rect rect, DesignerAttributePopup popup, int indent, bool even, bool selected)
		{
			Event e = Event.current;
			Rect interactRect = rect;
			Color bgColor = (selected ? Colors.AttributePopup.SelectedAttribute : (e.IsHovering(interactRect) ? Colors.AttributePopup.HoveringAttribute : (even ? Colors.ListItemEven : Colors.ListItemOdd)));
			EditorGUI.DrawRect(rect, bgColor);
			rect.TakeFromLeft(indent);
			SdfIconType favoriteIcon = (GlobalConfig<OdinVisualDesignerConfig>.Instance.FavoriteAttributes.Contains(AttributeType) ? SdfIconType.StarFill : SdfIconType.Star);
			if (DesignerGUI.DrawSimpleIconButton(rect.TakeFromLeft(rect.height), favoriteIcon, 6))
			{
				bool favorited = GlobalConfig<OdinVisualDesignerConfig>.Instance.ToggleFavorite(AttributeType);
				DesignerAttributePopup.PopulateFavorites();
				DesignerAttributePopup.NeedsScrollViewRebuild = true;
				popup.FlipItemColors = !popup.FlipItemColors;
				if (favorited)
				{
					DesignerAttributePopup.ScrollView.ScrollPosition += 24f;
				}
				else
				{
					DesignerAttributePopup.ScrollView.ScrollPosition -= 24f;
				}
			}
			if (HasExample && DesignerGUI.DrawSimpleIconButton(rect.TakeFromRight(rect.height), SdfIconType.Info, 7))
			{
				DesignerAttributeExampleWindow.Open(UnityShims.Rect.Ctor(popup.position.position + new Vector2(popup.position.width + 10f, 0f), Vector2.zero), AttributeType);
			}
			if (GUI.Button(interactRect, GUIContent.none, GUIStyle.none) && popup.Editor.Context.SelectedNode != null)
			{
				popup.Editor.Context.AddAttributeToSelection(AttributeType);
				popup.GotoEditPage();
				DesignerAttributeExampleWindow.CloseAllDesignerAttributeExampleWindows();
				popup.ClearSearchField();
			}
			GUI.Label(rect, Label, labelStyle);
		}
	}
}

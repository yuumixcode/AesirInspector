using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerGroupGUI
	{
		public static void DrawGroup(Rect rect, Rect headerRect, DesignerEditorNode node, DesignerEditorContext context)
		{
			bool matchesSearch = context.SelectedFilteredNode != null && context.SelectedFilteredNode == node;
			Color borderColor = (matchesSearch ? Colors.Group.BorderHighlighted : Colors.Group.Border);
			SirenixEditorGUI.DrawRoundRect(rect, Colors.Group.Bg, 4f, borderColor, 1f);
			SirenixEditorGUI.DrawRoundRect(headerRect, Colors.Group.HeaderBg, 4f, 4f, 0f, 0f, borderColor, 1f);
			GUI.DrawTexture(headerRect.AddY(headerRect.height).SetHeight(8f), DesignerTextures.TopToBottomFade, ScaleMode.StretchToFill, alphaBlend: true, 1f, new Color(0f, 0f, 0f, 0.22f), 0f, 0f);
			string groupName = DesignerGUI.AttributeLabelCache.GetLabel(node.GroupAttribute.GetType());
			string finalLabel = (string.IsNullOrEmpty(node.Label) ? "" : (node.Label + "  ")) + "<size=10><color=grey>" + groupName + "</color></size>";
			GUI.Label(headerRect.Expand(-8f, 0f), finalLabel, matchesSearch ? DesignerStyles.Node.BoldLabelStyle : DesignerStyles.Node.LabelStyle);
			Rect editAttributesRect = rect.AlignTop(20f).AlignRight(20f).SubX(28f)
				.AddY(4f);
			Rect removeRect = rect.AlignTop(20f).AlignRight(20f).SubX(4f)
				.AddY(4f);
			if (DesignerGUI.DrawHaloButton(editAttributesRect, node.Path + "_edit", string.Empty, DesignerGUI.Tooltips.EditGroupParameters))
			{
				context.Select(node);
				Rect contextRect = GUIUtility.GUIToScreenRect(node.DrawRect);
				context.Editor.OpenPopup(contextRect.AlignTop(36f), node);
			}
			if (DesignerGUI.DrawHaloButton(removeRect, node.Path + "_close", string.Empty, DesignerGUI.Tooltips.EditGroupParameters))
			{
				context.BeginUndo();
				context.RemoveGroup(node);
				context.EndUndo(isEditorOutOfSync: true);
			}
			GUI.Label(editAttributesRect, GUIHelper.TempContent(string.Empty, "Edit Attributes"), GUIStyle.none);
			SdfIcons.DrawIcon(editAttributesRect.Padding(4f), SdfIconType.PencilFill, Colors.Icons.Default);
			SdfIcons.DrawIcon(removeRect.Padding(4f), SdfIconType.X, Colors.Icons.Default);
		}

		public static void DrawSubGroupOwner(Rect rect, Rect headerRect, DesignerEditorNode node, DesignerEditorContext context)
		{
			bool matchesSearch = context.SelectedFilteredNode != null && context.SelectedFilteredNode == node;
			Color borderColor = (matchesSearch ? Colors.Group.BorderHighlighted : Colors.Group.Border);
			SirenixEditorGUI.DrawRoundRect(rect, Colors.Group.Bg, 4f, borderColor, 1f);
			SirenixEditorGUI.DrawRoundRect(headerRect, Colors.Group.HeaderBg, 4f, 4f, 0f, 0f, borderColor, 1f);
			GUI.DrawTexture(headerRect.AddY(headerRect.height).SetHeight(8f), DesignerTextures.TopToBottomFade, ScaleMode.StretchToFill, alphaBlend: true, 1f, new Color(0f, 0f, 0f, 0.22f), 0f, 0f);
			Rect headerContentRect = headerRect.Expand(-8f, 0f);
			string groupName = DesignerGUI.AttributeLabelCache.GetLabel(node.GroupAttribute.GetType());
			string finalLabel = (string.IsNullOrEmpty(node.Label) ? "" : (node.Label + "  ")) + "<size=10><color=grey>" + groupName + "</color></size>";
			GUI.Label(headerContentRect, finalLabel, matchesSearch ? DesignerStyles.Node.BoldLabelStyle : DesignerStyles.Node.LabelStyle);
			Rect removeButtonRect = headerRect.TakeFromRight(headerContentRect.height).Padding(4f);
			Rect addButtonRect = headerRect.TakeFromRight(headerContentRect.height).Padding(4f);
			Rect editButtonRect = headerRect.TakeFromRight(headerContentRect.height).Padding(4f);
			if (DesignerGUI.DrawHaloButton(addButtonRect, node.Path + "_add_button_DrawSubGroupOwner", "", DesignerGUI.Tooltips.AddSubGroup))
			{
				string id = node.GetDesignerId();
				context.BeginUndo();
				GroupPatch subGroup = context.CreateGroup(DesignerRegistry.SubGroupMap[node.GroupAttribute.GetType()]);
				subGroup.ParentId = id;
				subGroup.DesiredIndex = node.Children.Count;
				context.EndUndo(isEditorOutOfSync: true);
			}
			SdfIcons.DrawIcon(addButtonRect.Padding(4f), SdfIconType.Plus);
			if (DesignerGUI.DrawHaloButton(editButtonRect, node.Path + "_edit_button_DrawSubGroupOwner", "", DesignerGUI.Tooltips.EditGroupParameters))
			{
				context.Select(node);
				Rect contextRect = GUIUtility.GUIToScreenRect(node.DrawRect);
				context.Editor.OpenPopup(contextRect.AlignTop(36f), node);
			}
			SdfIcons.DrawIcon(editButtonRect.Padding(4f), SdfIconType.PencilFill, Colors.Icons.Default);
			if (DesignerGUI.DrawHaloButton(removeButtonRect, node.Path + "_remove_button_DrawSubGroupOwner", "", DesignerGUI.Tooltips.EditGroupParameters))
			{
				context.BeginUndo();
				context.RemoveGroup(node);
				context.EndUndo(isEditorOutOfSync: true);
			}
			SdfIcons.DrawIcon(removeButtonRect.Padding(4f), SdfIconType.X, Colors.Icons.Default);
		}

		public static void DrawRow(DesignerEditorNode node, Rect rect)
		{
			DesignerGUI.DrawRoundBlur6(rect, new Color(0f, 0f, 0f, 0.0627451f));
			SirenixEditorGUI.DrawRoundRect(rect, Colors.Group.Bg, 4f, Colors.Node.Border, 1f);
		}

		public static void DrawColumn(DesignerEditorNode node, Rect rect, DesignerEditorContext context)
		{
			Rect sizeFieldRect = new Rect(rect.x, rect.y, rect.width, 20f).SubXMax(26f);
			Rect deleteRect = new Rect(rect.x, rect.y, rect.width, 20f).AlignRight(20f);
			if (DesignerGUI.DrawHaloButton(deleteRect, node.Path + "_close", string.Empty, DesignerGUI.Tooltips.EditGroupParameters))
			{
				context.BeginUndo();
				context.RemoveGroup(node);
				context.EndUndo(isEditorOutOfSync: true);
			}
			SdfIcons.DrawIcon(deleteRect.Padding(4f), SdfIconType.X, Colors.Icons.Default);
			node.Text = DesignerGUI.DoColumnSizeField(sizeFieldRect, GUIUtility.GetControlID(FocusType.Keyboard), node.Text, out var isConfirmed);
			if (!isConfirmed)
			{
				return;
			}
			if (!ColumnSizeParser.TryParse(node.Text, out var size))
			{
				size = ((ColumnGroupAttribute.ColumnSubGroupAttribute)node.GroupAttribute).Size;
			}
			else
			{
				context.BeginUndo();
				GroupPatch patch = node.GroupPatch;
				if (patch == null)
				{
					patch = context.CreateGroup(node);
				}
				FieldInfo field = typeof(ColumnGroupAttribute.ColumnSubGroupAttribute).GetField("Size");
				patch.AddAttributeDeltaChange(node.GroupAttribute.GetType(), field, size);
				context.EndUndo(isEditorOutOfSync: true);
			}
			node.Text = size.ToString();
		}
	}
}

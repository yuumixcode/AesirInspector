using System;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public class ColumnGroupAttributeDrawer : OdinGroupDrawer<ColumnGroupAttribute>
	{
		public struct Column
		{
			public float LabelWidth;

			public LayoutSize Size;

			public InspectorProperty Property;

			public Column(ColumnSize size, InspectorProperty property)
			{
				LabelWidth = 0f;
				switch (size.ColumnType)
				{
				case ColumnType.Auto:
					Size.Type = SizeMode.Auto;
					break;
				case ColumnType.Percent:
					Size.Type = SizeMode.Percentage;
					break;
				case ColumnType.Pixel:
					Size.Type = SizeMode.Pixels;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				Size.Value = size.Value;
				Property = property;
			}
		}

		public Column[] Columns;

		protected override void Initialize()
		{
			Columns = new Column[base.Property.Children.Count];
			for (int i = 0; i < base.Property.Children.Count; i++)
			{
				InspectorProperty current = base.Property.Children[i];
				ColumnGroupAttribute.ColumnSubGroupAttribute subGroup = current.GetAttribute<ColumnGroupAttribute.ColumnSubGroupAttribute>();
				Columns[i] = new Column(subGroup.Size, current);
			}
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			Rect rowRect = GUILayout_Internal.BeginSmartRow();
			bool isRowRectValid = Event.current.type == EventType.Repaint;
			float labelWidth = EditorGUIUtility.labelWidth;
			bool hasPadding = Columns.Length > 1;
			int lastIndex = Columns.Length - 1;
			for (int i = 0; i < Columns.Length; i++)
			{
				ref Column column = ref Columns[i];
				Vector2Int horizontalPadding = Vector2Int.zero;
				if (hasPadding)
				{
					if (i != 0)
					{
						horizontalPadding.x = 2;
					}
					if (i != lastIndex)
					{
						horizontalPadding.y = 2;
					}
				}
				Rect columnRect = GUILayout_Internal.BeginColumn(column.Size, horizontalPadding);
				if (isRowRectValid)
				{
					Columns[i].LabelWidth = labelWidth * (columnRect.width / rowRect.width);
				}
				if (column.LabelWidth > 0f)
				{
					GUIHelper.PushLabelWidth(column.LabelWidth);
					foreach (InspectorProperty property in column.Property.Children)
					{
						property.Draw();
					}
					GUIHelper.PopLabelWidth();
				}
				else
				{
					GUIHelper.PushLabelWidth(labelWidth / (float)Columns.Length);
					foreach (InspectorProperty property2 in column.Property.Children)
					{
						property2.Draw();
					}
					GUIHelper.PopLabelWidth();
				}
				GUILayout_Internal.EndColumn();
			}
			GUILayout_Internal.EndSmartRow();
		}
	}
}

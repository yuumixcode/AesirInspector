using Sirenix.OdinInspector.Editor.Examples.Internal;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.Utilities" })]
	[ShowOdinSerializedPropertiesInInspector]
	[AttributeExample(typeof(TableMatrixAttribute), Name = "Transpose")]
	internal class TransposeTableMatrixExample
	{
		[TableMatrix(HorizontalTitle = "Custom Cell Drawing", DrawElementMethod = "DrawColoredEnumElement", ResizableColumns = false, RowHeight = 16)]
		public bool[,] CustomCellDrawing;

		[TableMatrix(HorizontalTitle = "Transposed Custom Cell Drawing", DrawElementMethod = "DrawColoredEnumElement", ResizableColumns = false, RowHeight = 16, Transpose = true)]
		[ShowInInspector]
		[DoNotDrawAsReference]
		public bool[,] Transposed
		{
			get
			{
				return CustomCellDrawing;
			}
			set
			{
				CustomCellDrawing = value;
			}
		}

		private static bool DrawColoredEnumElement(Rect rect, bool value)
		{
			if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
			{
				value = !value;
				GUI.changed = true;
				Event.current.Use();
			}
			EditorGUI.DrawRect(rect.Padding(1f), value ? new Color(0.1f, 0.8f, 0.2f) : new Color(0f, 0f, 0f, 0.5f));
			return value;
		}

		[OnInspectorInit]
		private void CreateData()
		{
			CustomCellDrawing = new bool[15, 15];
			CustomCellDrawing[6, 5] = true;
			CustomCellDrawing[6, 6] = true;
			CustomCellDrawing[6, 7] = true;
			CustomCellDrawing[8, 5] = true;
			CustomCellDrawing[8, 6] = true;
			CustomCellDrawing[8, 7] = true;
			CustomCellDrawing[5, 9] = true;
			CustomCellDrawing[5, 10] = true;
			CustomCellDrawing[9, 9] = true;
			CustomCellDrawing[9, 10] = true;
			CustomCellDrawing[6, 11] = true;
			CustomCellDrawing[7, 11] = true;
			CustomCellDrawing[8, 11] = true;
		}
	}
}

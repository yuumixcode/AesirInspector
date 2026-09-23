using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    [AesirExample]
    public class TableMatrixExampleWithDrawElementMethodSO : OdinAttributeExampleSO<TableMatrixExampleWithDrawElementMethodSO>
    {
        [OdinSerialize]
        [Title("Parameter: DrawElementMethod (Rect rect, string value)")]
        [TableMatrix(DrawElementMethod = "DrawAsLabel")]
        public string[,] simpleDrawElement = new string[2, 3];

        [OdinSerialize]
        [Title("Parameter: DrawElementMethod (Rect rect, bool[,] table, int x, int y)")]
        [TableMatrix(DrawElementMethod = "DrawAsColoredRect")]
        public bool[,] coloredDrawElement = new bool[5, 5];

        [Title("Parameter: DrawElementMethod (Rect rect, bool[,] table, int x, int y)")]
        public Color TrueColor = new Color(0.11f, 0.77f, 0.5f, 1f);

        [Title("Parameter: DrawElementMethod (Rect rect, bool[,] table, int x, int y)")]
        public Color FalseColor = new Color(1f, 0.4f, 0.14f, 1f);

        [OdinSerialize]
        [Title("Parameter: DrawElementMethod (Rect rect, bool value)")]
        [TableMatrix(HorizontalTitle = "Custom Cell Drawing", DrawElementMethod = "DrawClickableElement",
            ResizableColumns = false, RowHeight = 16)]
        public bool[,] customCellDrawing = CreateSmileyMatrix();

        [TableMatrix(HorizontalTitle = "Transposed Custom Cell Drawing", DrawElementMethod = "DrawClickableElement",
            ResizableColumns = false, RowHeight = 16, Transpose = true)]
        [ShowInInspector]
        [DoNotDrawAsReference]
        public bool[,] TransposedCellDrawing
        {
            get { return customCellDrawing; }
            set { customCellDrawing = value; }
        }

        string DrawAsLabel(Rect rect, string value)
        {
            EditorGUI.LabelField(rect, value);
            return value;
        }

        bool DrawAsColoredRect(Rect rect, bool[,] table, int x, int y)
        {
            var value = table[x, y];
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                table[x, y] = !value;
            }

            EditorGUI.DrawRect(rect, value ? TrueColor : FalseColor);
            return value;
        }

        bool DrawClickableElement(Rect rect, bool value)
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

        static bool[,] CreateSmileyMatrix()
        {
            var matrix = new bool[15, 15];
            matrix[6, 5] = true;
            matrix[6, 6] = true;
            matrix[6, 7] = true;
            matrix[8, 5] = true;
            matrix[8, 6] = true;
            matrix[8, 7] = true;
            matrix[5, 9] = true;
            matrix[5, 10] = true;
            matrix[9, 9] = true;
            matrix[9, 10] = true;
            matrix[6, 11] = true;
            matrix[7, 11] = true;
            matrix[8, 11] = true;
            return matrix;
        }

        public override void AesirInspectorReset()
        {
            simpleDrawElement = new string[2, 3];
            coloredDrawElement = new bool[5, 5];
            TrueColor = new Color(0.11f, 0.77f, 0.5f, 1f);
            FalseColor = new Color(1f, 0.4f, 0.14f, 1f);
            customCellDrawing = CreateSmileyMatrix();
        }
    }
}

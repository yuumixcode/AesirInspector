using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    [AesirExample]
    public class TableMatrixExampleSO : OdinAttributeExampleSO<TableMatrixExampleSO>
    {
        [OdinSerialize]
        [FoldoutGroup("No Parameters")]
        [TableMatrix]
        public string[,] matrix = new string[2, 3];

        [OdinSerialize]
        [FoldoutGroup("Parameter: Transpose")]
        [TableMatrix(Transpose = true)]
        public string[,] transposedMatrix = new string[2, 3];

        [OdinSerialize]
        [FoldoutGroup("Parameter: IsReadOnly")]
        [TableMatrix(IsReadOnly = true)]
        public string[,] readOnlyMatrix = new string[2, 3];

        [OdinSerialize]
        [FoldoutGroup("Parameter: ResizableColumns")]
        [TableMatrix(ResizableColumns = false)]
        public string[,] fixedColumnsMatrix = new string[2, 3];

        [OdinSerialize]
        [FoldoutGroup("Parameter: HorizontalTitle")]
        [TableMatrix(HorizontalTitle = "Horizontal Title")]
        public string[,] horizontalTitleMatrix = new string[2, 3];

        [OdinSerialize]
        [FoldoutGroup("Parameter: VerticalTitle")]
        [TableMatrix(VerticalTitle = "Vertical Title")]
        public string[,] verticalTitleMatrix = new string[2, 3];

        [OdinSerialize]
        [FoldoutGroup("Parameter: RowHeight")]
        [TableMatrix(RowHeight = 40)]
        public string[,] rowHeightMatrix = new string[2, 3];

        [OdinSerialize]
        [FoldoutGroup("Parameter: SquareCells")]
        [TableMatrix(SquareCells = true)]
        public string[,] squareCellsMatrix = new string[2, 3];

        [OdinSerialize]
        [FoldoutGroup("Parameter: SquareCells")]
        [TableMatrix(HorizontalTitle = "Square Celled Matrix", SquareCells = true)]
        public Texture2D[,] squareCelledTextureMatrix = new Texture2D[8, 4];

        [OdinSerialize]
        [FoldoutGroup("Parameter: SquareCells")]
        [TableMatrix(SquareCells = true)]
        public Mesh[,] prefabMatrix = new Mesh[8, 4];

        [OdinSerialize]
        [FoldoutGroup("Parameter: HideColumnIndices")]
        [TableMatrix(HideColumnIndices = true)]
        public string[,] hideColumnIndicesMatrix = new string[2, 3];

        [OdinSerialize]
        [FoldoutGroup("Parameter: HideRowIndices")]
        [TableMatrix(HideRowIndices = true)]
        public string[,] hideRowIndicesMatrix = new string[2, 3];

        public override void AesirInspectorReset()
        {
            matrix = new string[2, 3];
            transposedMatrix = new string[2, 3];
            readOnlyMatrix = new string[2, 3];
            fixedColumnsMatrix = new string[2, 3];
            horizontalTitleMatrix = new string[2, 3];
            verticalTitleMatrix = new string[2, 3];
            rowHeightMatrix = new string[2, 3];
            squareCellsMatrix = new string[2, 3];
            squareCelledTextureMatrix = new Texture2D[8, 4];
            prefabMatrix = new Mesh[8, 4];
            hideColumnIndicesMatrix = new string[2, 3];
            hideRowIndicesMatrix = new string[2, 3];
        }
    }
}

using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// InlineProperty 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class InlinePropertyExampleSO : AttributeExampleSO<InlinePropertyExampleSO>
    {
        [Title("No Parameters")]
        public Vector3 MyVector3 = new Vector3(1f, 2f, 3f);

        [Title("No Parameters")]
        public SimpleData data = new SimpleData { name = "Example", id = 1 };

        [Title("Parameter: LabelWidth")]
        public Vector3Int MyVector3Int = new Vector3Int { X = 1, Y = 2, Z = 3 };

        [Title("Parameter: LabelWidth")]
        [InlineProperty(LabelWidth = 50)]
        public Vector2Int position = new Vector2Int { x = 10, y = 20 };

        [Title("Parameter: LabelWidth")]
        [InlineProperty(LabelWidth = 13)]
        public Vector2Int MyVector2Int = new Vector2Int { x = 5, y = 10 };

        public override void AesirInspectorReset()
        {
            MyVector3 = new Vector3(1f, 2f, 3f);
            data = new SimpleData { name = "Example", id = 1 };
            MyVector3Int = new Vector3Int { X = 1, Y = 2, Z = 3 };
            position = new Vector2Int { x = 10, y = 20 };
            MyVector2Int = new Vector2Int { x = 5, y = 10 };
        }

        [Serializable]
        [InlineProperty(LabelWidth = 13)]
        public struct Vector3Int
        {
            [HorizontalGroup()]
            public int X;

            [HorizontalGroup()]
            public int Y;

            [HorizontalGroup()]
            public int Z;
        }

        [Serializable]
        public struct Vector2Int
        {
            [HorizontalGroup]
            public int x;

            [HorizontalGroup]
            public int y;
        }

        [Serializable]
        [InlineProperty]
        public class SimpleData
        {
            [HorizontalGroup]
            [HideLabel]
            public string name;

            [HorizontalGroup(Width = 60)]
            [HideLabel]
            public int id;
        }
    }
}

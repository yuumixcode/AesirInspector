using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TableColumnWidth 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class TableColumnWidthExampleSO : AttributeExampleSO<TableColumnWidthExampleSO>
    {
        [TableList]
        public List<MyItem> List = CreateDefaultList();

        static List<MyItem> CreateDefaultList() => new List<MyItem>
        {
            new MyItem { ID = 1, Name = "Item A", Description = "固定 60px 且不可调整的 ID 列" },
            new MyItem { ID = 2, Name = "Item B", Description = "可调整的 160px 名称列" },
            new MyItem { ID = 3, Name = "Item C", Description = "未标注宽度的列自动分配剩余空间" }
        };

        public override void AesirInspectorReset() => List = CreateDefaultList();

        [Serializable]
        public class MyItem
        {
            [TableColumnWidth(60, Resizable = false)]
            public int ID;

            [TableColumnWidth(160)]
            public string Name;

            public string Description;
        }
    }
}

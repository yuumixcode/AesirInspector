using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    [AesirExample]
    public class TableListExampleSO : AttributeExampleSO<TableListExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        [TableList]
        public List<Item> plainItems = new List<Item>
        {
            new Item { ID = 1, Name = "Item A" },
            new Item { ID = 2, Name = "Item B" },
            new Item { ID = 3, Name = "Item C" }
        };

        [FoldoutGroup("Parameter: ShowIndexLabels")]
        [TableList(ShowIndexLabels = true)]
        public List<Item> items = new List<Item>
        {
            new Item { ID = 1, Name = "Apple", Price = 1.2f },
            new Item { ID = 2, Name = "Banana", Price = 0.8f },
            new Item { ID = 3, Name = "Orange", Price = 1.5f }
        };

        [FoldoutGroup("Parameter: DrawScrollView, MinScrollViewHeight, MaxScrollViewHeight")]
        [TableList(DrawScrollView = true, MaxScrollViewHeight = 200, MinScrollViewHeight = 100)]
        public List<Item> scrollableItems = CreateScrollableItems();

        [FoldoutGroup("Parameter: AlwaysExpanded, HideToolbar, DrawScrollView")]
        [TableList(HideToolbar = true, AlwaysExpanded = true, DrawScrollView = false)]
        public List<Item> simpleTable = new List<Item>
        {
            new Item { ID = 1, Name = "Item A" },
            new Item { ID = 2, Name = "Item B" }
        };

        [FoldoutGroup("Parameter: ShowPaging")]
        [TableList(ShowPaging = true)]
        public List<Item> tableWithPaging = new List<Item>
        {
            new Item { ID = 1, Name = "Page Item 1" },
            new Item { ID = 2, Name = "Page Item 2" }
        };

        [FoldoutGroup("Combining With TableColumnWidth")]
        [TableList(ShowIndexLabels = true)]
        public List<CustomColumnsItem> customColumnTable = new List<CustomColumnsItem>
        {
            new CustomColumnsItem { Description = "First item", A = "a1", B = "b1", C = "c1" },
            new CustomColumnsItem { Description = "Second item", A = "a2", B = "b2", C = "c2" }
        };

        static List<Item> CreateScrollableItems()
        {
            var list = new List<Item>();
            for (var i = 0; i < 10; i++)
            {
                list.Add(new Item { ID = i, Name = "Item " + i });
            }

            return list;
        }

        public override void AesirInspectorReset()
        {
            plainItems = new List<Item>
            {
                new Item { ID = 1, Name = "Item A" },
                new Item { ID = 2, Name = "Item B" },
                new Item { ID = 3, Name = "Item C" }
            };
            items = new List<Item>
            {
                new Item { ID = 1, Name = "Apple", Price = 1.2f },
                new Item { ID = 2, Name = "Banana", Price = 0.8f },
                new Item { ID = 3, Name = "Orange", Price = 1.5f }
            };
            scrollableItems = CreateScrollableItems();
            simpleTable = new List<Item>
            {
                new Item { ID = 1, Name = "Item A" },
                new Item { ID = 2, Name = "Item B" }
            };
            tableWithPaging = new List<Item>
            {
                new Item { ID = 1, Name = "Page Item 1" },
                new Item { ID = 2, Name = "Page Item 2" }
            };
            customColumnTable = new List<CustomColumnsItem>
            {
                new CustomColumnsItem { Description = "First item", A = "a1", B = "b1", C = "c1" },
                new CustomColumnsItem { Description = "Second item", A = "a2", B = "b2", C = "c2" }
            };
        }

        [Serializable]
        public class Item
        {
            [TableColumnWidth(50, false)]
            public int ID;

            [PreviewField(Height = 40)]
            [TableColumnWidth(50, false)]
            public Texture2D Icon;

            public string Name;

            public float Price;
        }

        [Serializable]
        public class CustomColumnsItem
        {
            [PreviewField(Alignment = ObjectFieldAlignment.Center)]
            [TableColumnWidth(57, false)]
            public Texture Icon;

            [TextArea]
            public string Description;

            [LabelWidth(22f)]
            [VerticalGroup("Combined Column")]
            public string A;

            [VerticalGroup("Combined Column")]
            [LabelWidth(22f)]
            public string B;

            [VerticalGroup("Combined Column")]
            [LabelWidth(22f)]
            public string C;

            [TableColumnWidth(60)]
            [Button]
            [VerticalGroup("Actions")]
            public void Test1() { }

            [Button]
            [VerticalGroup("Actions")]
            [TableColumnWidth(60)]
            public void Test2() { }
        }
    }
}

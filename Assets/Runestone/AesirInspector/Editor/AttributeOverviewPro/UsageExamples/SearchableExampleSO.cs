using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    [AesirExample]
    [Searchable]
    public class SearchableExampleSO : AttributeExampleSO<SearchableExampleSO>
    {
        [Title("No Parameters")]
        [Searchable]
        public List<Item> items = new List<Item>
        {
            new Item { name = "Apple", value = 10, tags = new List<string> { "Fruit", "Red" } },
            new Item { name = "Banana", value = 20, tags = new List<string> { "Fruit", "Yellow" } },
            new Item { name = "Carrot", value = 5, tags = new List<string> { "Vegetable", "Orange" } }
        };

        [Title("No Parameters")]
        public List<string> strings = CreateStrings();

        [Title("No Parameters")]
        public List<ExampleStruct> searchableList = CreateStructList();

        [Title("Parameter: Recursive (False)")]
        [Searchable(Recursive = false)]
        public List<Item> nonRecursiveItems = new List<Item>
        {
            new Item { name = "Apple", value = 10, tags = new List<string> { "Fruit", "Red" } },
            new Item { name = "Banana", value = 20, tags = new List<string> { "Fruit", "Yellow" } }
        };

        [Title("Parameter: FuzzySearch (False)")]
        [Searchable(FuzzySearch = false)]
        public List<Item> exactMatchItems = new List<Item>
        {
            new Item { name = "Apple", value = 10 },
            new Item { name = "Banana", value = 20 }
        };

        [Title("Parameter: FilterOptions")]
        [Searchable(FilterOptions = SearchFilterOptions.ISearchFilterableInterface)]
        public List<FilterableStruct> customFiltering = CreateFilterableList();

        [Title("Combining With Nested Class")]
        [Searchable]
        public ExampleClass searchableClass = new ExampleClass();

        static List<string> CreateStrings()
        {
            var list = new List<string>();
            for (var i = 1; i <= 10; i++)
            {
                list.Add("Str Element " + i);
            }
            return list;
        }

        static List<ExampleStruct> CreateStructList()
        {
            var list = new List<ExampleStruct>();
            for (var i = 1; i <= 10; i++)
            {
                list.Add(new ExampleStruct(i));
            }
            return list;
        }

        static List<FilterableStruct> CreateFilterableList()
        {
            var list = new List<FilterableStruct>();
            for (var i = 1; i <= 10; i++)
            {
                list.Add(new FilterableStruct(i));
            }
            return list;
        }

        public override void AesirInspectorReset()
        {
            items = new List<Item>
            {
                new Item { name = "Apple", value = 10, tags = new List<string> { "Fruit", "Red" } },
                new Item { name = "Banana", value = 20, tags = new List<string> { "Fruit", "Yellow" } },
                new Item { name = "Carrot", value = 5, tags = new List<string> { "Vegetable", "Orange" } }
            };
            strings = CreateStrings();
            searchableList = CreateStructList();
            nonRecursiveItems = new List<Item>
            {
                new Item { name = "Apple", value = 10, tags = new List<string> { "Fruit", "Red" } },
                new Item { name = "Banana", value = 20, tags = new List<string> { "Fruit", "Yellow" } }
            };
            exactMatchItems = new List<Item>
            {
                new Item { name = "Apple", value = 10 },
                new Item { name = "Banana", value = 20 }
            };
            customFiltering = CreateFilterableList();
            searchableClass = new ExampleClass();
        }

        [Serializable]
        public class Item
        {
            public string name;
            public int value;
            public List<string> tags;
        }

        public enum ExampleEnum
        {
            One,
            Two,
            Three,
            Four,
            Five
        }

        [Serializable]
        public struct ExampleStruct
        {
            public string Name;

            public int Number;

            public ExampleEnum Enum;

            public ExampleStruct(int nr)
            {
                this = default(ExampleStruct);
                Name = "Element " + nr;
                Number = nr;
                Enum = (ExampleEnum)(nr % 5);
            }
        }

        [Serializable]
        public class ExampleClass
        {
            public string SomeString = "Saehrimnir is a tasty delicacy";

            public int SomeInt = 13579;

            public DataContainer DataContainerOne = new DataContainer
            {
                Name = "Example Data Set One"
            };

            public DataContainer DataContainerTwo = new DataContainer
            {
                Name = "Example Data Set Two"
            };
        }

        [Serializable]
        [Searchable]
        public class DataContainer
        {
            public string Name;

            public List<ExampleStruct> Data = CreateStructList();
        }

        [Serializable]
        public struct FilterableStruct : ISearchFilterable
        {
            public int Number;

            [EnableGUI]
            [DisplayAsString]
            [ShowInInspector]
            public int Square => Number * Number;

            public FilterableStruct(int nr)
            {
                Number = nr;
            }

            public bool IsMatch(string searchString)
            {
                return searchString.Contains(Square.ToString());
            }
        }
    }
}

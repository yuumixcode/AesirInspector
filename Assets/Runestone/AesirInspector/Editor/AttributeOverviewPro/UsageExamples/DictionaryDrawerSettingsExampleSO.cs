using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    [AesirExample]
    public class DictionaryDrawerSettingsExampleSO : OdinAttributeExampleSO<DictionaryDrawerSettingsExampleSO>
    {
        public enum SomeEnum
        {
            First,
            Second,
            Third,
            Fourth,
            AndSoOn
        }

        [OdinSerialize]
        [Title("No Parameters")]
        public Dictionary<int, Material> intMaterialLookup = new Dictionary<int, Material>
        {
            { 1, null },
            { 7, null }
        };

        [OdinSerialize]
        [Title("No Parameters")]
        public Dictionary<string, string> stringStringDictionary = new Dictionary<string, string>
        {
            { "One", "Number one" },
            { "Seven", "Number seven" }
        };

        [OdinSerialize]
        [Title("Parameter: KeyLabel, ValueLabel, KeyColumnWidth")]
        [DictionaryDrawerSettings(KeyLabel = "ID", ValueLabel = "Name", KeyColumnWidth = 60)]
        public Dictionary<int, string> simpleDictionary = new Dictionary<int, string>
        {
            { 1, "Alice" },
            { 2, "Bob" }
        };

        [OdinSerialize]
        [Title("Parameter: KeyLabel, ValueLabel, KeyColumnWidth")]
        [DictionaryDrawerSettings(KeyLabel = "Custom Key Name", ValueLabel = "Custom Value Label")]
        public Dictionary<SomeEnum, MyCustomType> customLabels = new Dictionary<SomeEnum, MyCustomType>
        {
            { SomeEnum.First, new MyCustomType { SomeMember = 1 } },
            { SomeEnum.Second, new MyCustomType { SomeMember = 2 } }
        };

        [OdinSerialize]
        [Title("Parameter: DisplayMode")]
        [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.Foldout)]
        public Dictionary<string, List<int>> complexDictionary = new Dictionary<string, List<int>>
        {
            { "Group A", new List<int> { 1, 2, 3 } },
            { "Group B", new List<int> { 4, 5 } }
        };

        [OdinSerialize]
        [Title("Parameter: DisplayMode")]
        [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        public Dictionary<string, List<int>> stringListDictionary = new Dictionary<string, List<int>>
        {
            { "Numbers", new List<int> { 1, 2, 3, 4 } }
        };

        [OdinSerialize]
        [Title("Parameter: IsReadOnly")]
        [DictionaryDrawerSettings(IsReadOnly = true)]
        public Dictionary<string, int> readOnlyDictionary = new Dictionary<string, int>
        {
            { "Fixed Key", 100 }
        };

        public override void AesirInspectorReset()
        {
            intMaterialLookup = new Dictionary<int, Material>
            {
                { 1, null },
                { 7, null }
            };
            stringStringDictionary = new Dictionary<string, string>
            {
                { "One", "Number one" },
                { "Seven", "Number seven" }
            };
            simpleDictionary = new Dictionary<int, string> { { 1, "Alice" }, { 2, "Bob" } };
            customLabels = new Dictionary<SomeEnum, MyCustomType>
            {
                { SomeEnum.First, new MyCustomType { SomeMember = 1 } },
                { SomeEnum.Second, new MyCustomType { SomeMember = 2 } }
            };
            complexDictionary = new Dictionary<string, List<int>>
            {
                { "Group A", new List<int> { 1, 2, 3 } },
                { "Group B", new List<int> { 4, 5 } }
            };
            stringListDictionary = new Dictionary<string, List<int>>
            {
                { "Numbers", new List<int> { 1, 2, 3, 4 } }
            };
            readOnlyDictionary = new Dictionary<string, int> { { "Fixed Key", 100 } };
        }

        [InlineProperty(LabelWidth = 90)]
        public struct MyCustomType
        {
            public int SomeMember;

            public GameObject SomePrefab;
        }
    }
}

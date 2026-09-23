using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEngine;
using ObjectFieldAlignment = Sirenix.OdinInspector.ObjectFieldAlignment;

namespace Runestone.AesirInspector.Editor
{
    [AesirExample]
    public class ListDrawerSettingsExampleSO : AttributeExampleSO<ListDrawerSettingsExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        public List<float> floatList = new List<float> { 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f };

        [FoldoutGroup("Parameter: IsReadOnly")]
        [ListDrawerSettings(IsReadOnly = true)]
        public int[] readOnlyList = { 1, 2, 3 };

        [FoldoutGroup("Parameter: IsReadOnly")]
        [ReadOnly]
        public int[] readOnlyArray = { 1, 2, 3 };

        [FoldoutGroup("Parameter: NumberOfItemsPerPage")]
        [ListDrawerSettings(NumberOfItemsPerPage = 5, ShowItemCount = true)]
        public List<int> pagedList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        [FoldoutGroup("Parameter: ListElementLabelName, ShowIndexLabels")]
        [ListDrawerSettings(ListElementLabelName = "name", ShowIndexLabels = true)]
        public List<SomeStruct> namedElements = new List<SomeStruct>
        {
            new SomeStruct { name = "First" },
            new SomeStruct { name = "Second" }
        };

        [FoldoutGroup("Parameter: DraggableItems, ShowFoldout, ShowPaging, ShowItemCount, HideRemoveButton")]
        [ListDrawerSettings(DraggableItems = false, ShowFoldout = false, ShowIndexLabels = true, ShowPaging = false,
            ShowItemCount = false, HideRemoveButton = true)]
        public int[] moreListSettings = { 1, 2, 3 };

        [FoldoutGroup("Parameter: DraggableItems, ShowFoldout, ShowPaging, ShowItemCount, HideRemoveButton")]
        [ListDrawerSettings(DraggableItems = false, HideRemoveButton = true)]
        public List<int> restrictedList = new List<int> { 1, 2, 3 };

        [FoldoutGroup("Parameter: OnBeginListElementGUI, OnEndListElementGUI")]
        [ListDrawerSettings(OnBeginListElementGUI = "BeginDrawListElement", OnEndListElementGUI = "EndDrawListElement")]
        public ListElementStruct[] injectListElementGUI =
        {
            new ListElementStruct { SomeString = "Element 1" },
            new ListElementStruct { SomeString = "Element 2" },
            new ListElementStruct { SomeString = "Element 3" }
        };

        [FoldoutGroup("Parameter: OnTitleBarGUI")]
        [ListDrawerSettings(OnTitleBarGUI = "DrawRefreshButton")]
        public List<int> customButtons = new List<int> { 10, 20, 30 };

        [FoldoutGroup("Parameter: CustomAddFunction")]
        [ListDrawerSettings(CustomAddFunction = "CustomAddFunction")]
        public List<int> customAddBehaviour = new List<int> { 1 };

        [FoldoutGroup("Parameter: ElementColor")]
        [ListDrawerSettings(ElementColor = "lightblue")]
        public List<int> coloredList = new List<int> { 1, 2, 3 };

        [FoldoutGroup("Member Reference ($)")]
        public SomeOtherStruct[] someOtherStructList =
        {
            new SomeOtherStruct(),
            new SomeOtherStruct()
        };

        [FoldoutGroup("Combining With Range")]
        [Range(0f, 1f)]
        public float[] floatRangeArray = { 0.5f, 0.25f };

        void BeginDrawListElement(int index)
        {
            SirenixEditorGUI.BeginBox(injectListElementGUI[index].SomeString, false);
        }

        void EndDrawListElement(int index)
        {
            SirenixEditorGUI.EndBox();
        }

        void DrawRefreshButton()
        {
            if (SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh))
            {
                Debug.Log(customButtons.Count.ToString());
            }
        }

        int CustomAddFunction()
        {
            return customAddBehaviour.Count;
        }

        public override void AesirInspectorReset()
        {
            floatList = new List<float> { 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f };
            readOnlyList = new[] { 1, 2, 3 };
            readOnlyArray = new[] { 1, 2, 3 };
            pagedList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            namedElements = new List<SomeStruct>
            {
                new SomeStruct { name = "First" },
                new SomeStruct { name = "Second" }
            };
            moreListSettings = new[] { 1, 2, 3 };
            restrictedList = new List<int> { 1, 2, 3 };
            injectListElementGUI = new[]
            {
                new ListElementStruct { SomeString = "Element 1" },
                new ListElementStruct { SomeString = "Element 2" },
                new ListElementStruct { SomeString = "Element 3" }
            };
            customButtons = new List<int> { 10, 20, 30 };
            customAddBehaviour = new List<int> { 1 };
            coloredList = new List<int> { 1, 2, 3 };
            someOtherStructList = new[]
            {
                new SomeOtherStruct(),
                new SomeOtherStruct()
            };
            floatRangeArray = new[] { 0.5f, 0.25f };
        }

        [Serializable]
        public struct SomeStruct
        {
            public string name;
            public int value;
        }

        [Serializable]
        public struct ListElementStruct
        {
            public string SomeString;

            public int One;

            public int Two;

            public int Three;
        }

        [Serializable]
        public struct SomeOtherStruct
        {
            [HideLabel]
            [PreviewField(50f, ObjectFieldAlignment.Left)]
            [PropertyOrder(-1f)]
            [HorizontalGroup("Split", 55f, 0, 0, 0f)]
            public GameObject SomeObject;

            [FoldoutGroup("Split/$Name", false, 0f)]
            public int A;

            [FoldoutGroup("Split/$Name", false, 0f)]
            public int B;

            [FoldoutGroup("Split/$Name", false, 0f)]
            public int C;

            [FoldoutGroup("Split/$Name", false, 0f)]
            public int Two;

            [FoldoutGroup("Split/$Name", false, 0f)]
            public int Three;

            string Name
            {
                get
                {
                    if (!SomeObject)
                    {
                        return "Null";
                    }
                    return SomeObject.name;
                }
            }
        }
    }
}

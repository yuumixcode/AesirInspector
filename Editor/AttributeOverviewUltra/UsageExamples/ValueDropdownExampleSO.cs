using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ValueDropdown 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class ValueDropdownExampleSO : AttributeExampleSO<ValueDropdownExampleSO>
    {
        static readonly IEnumerable<KeyCode> KeyCodes = Enumerable.Range(48, 10).Cast<KeyCode>();

        static readonly IEnumerable FriendlyTextureSizes = new ValueDropdownList<int>
        {
            { "Small", 256 },
            { "Medium", 512 },
            { "Large", 1024 }
        };

        static readonly int[] TextureSizes = new int[3] { 256, 512, 1024 };

        [FoldoutGroup("No Parameters")]
        [ValueDropdown("TextureSizes")]
        public int SomeSize1 = 256;

        [FoldoutGroup("No Parameters")]
        [ValueDropdown("FriendlyTextureSizes")]
        public int SomeSize2 = 512;

        [FoldoutGroup("No Parameters")]
        [ValueDropdown("KeyCodes")]
        public KeyCode FilteredEnum;

        [FoldoutGroup("Parameter: AppendNextDrawer, DisableGUIInAppendedDrawer")]
        [ValueDropdown("FriendlyTextureSizes", AppendNextDrawer = true, DisableGUIInAppendedDrawer = true)]
        public int SomeSize3 = 1024;

        [FoldoutGroup("Parameter: AppendNextDrawer")]
        [ValueDropdown("GetListOfMonoBehaviours", AppendNextDrawer = true)]
        public MonoBehaviour SomeMonoBehaviour;

        [FoldoutGroup("Parameter: ExpandAllMenuItems")]
        [ValueDropdown("TreeViewOfInts", ExpandAllMenuItems = true)]
        public List<int> IntTreview = new List<int> { 1, 2, 7 };

        [FoldoutGroup("Parameter: IsUniqueList")]
        [ValueDropdown("GetAllSceneObjects", IsUniqueList = true)]
        public List<GameObject> UniqueGameobjectList;

        [FoldoutGroup("Parameter: IsUniqueList")]
        [ValueDropdown("GetAllScriptableObjects", IsUniqueList = true)]
        public List<ScriptableObject> UniqueScriptableObjectList;

        [FoldoutGroup(
            "Parameter: IsUniqueList, DropdownTitle, DrawDropdownForListElements, ExcludeExistingValuesInList")]
        [ValueDropdown("GetAllSceneObjects", IsUniqueList = true, DropdownTitle = "Select Scene Object",
            DrawDropdownForListElements = false, ExcludeExistingValuesInList = true)]
        public List<GameObject> UniqueGameobjectListMode2;

        readonly IEnumerable TreeViewOfInts = new ValueDropdownList<int>
        {
            { "Node 1/Node 1.1", 1 },
            { "Node 1/Node 1.2", 2 },
            { "Node 2/Node 2.1", 3 },
            { "Node 3/Node 3.1", 4 },
            { "Node 3/Node 3.2", 5 },
            { "Node 1/Node 3.1/Node 3.1.1", 6 },
            { "Node 1/Node 3.1/Node 3.1.2", 7 }
        };

        IEnumerable<MonoBehaviour> GetListOfMonoBehaviours() =>
            FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        static IEnumerable GetAllSceneObjects()
        {
            Func<Transform, string> getPath = null;
            getPath = x => !x ? "" : getPath(x.parent) + "/" + x.gameObject.name;
            var objects =
                FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var result = new ValueDropdownList<GameObject>();
            for (var i = 0; i < objects.Length; i++)
            {
                var go = objects[i];
                result.Add(getPath(go.transform), go);
            }

            return result;
        }

        static IEnumerable GetAllScriptableObjects()
        {
            var result = new ValueDropdownList<ScriptableObject>();
            foreach (var guid in AssetDatabase.FindAssets("t:ScriptableObject"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                result.Add(path, AssetDatabase.LoadAssetAtPath<ScriptableObject>(path));
            }

            return result;
        }

        public override void AesirInspectorReset()
        {
            SomeSize1 = 256;
            SomeSize2 = 512;
            FilteredEnum = KeyCode.None;
            SomeSize3 = 1024;
            SomeMonoBehaviour = null;
            IntTreview = new List<int> { 1, 2, 7 };
            UniqueGameobjectList = new List<GameObject>();
            UniqueScriptableObjectList = new List<ScriptableObject>();
            UniqueGameobjectListMode2 = new List<GameObject>();
        }
    }
}

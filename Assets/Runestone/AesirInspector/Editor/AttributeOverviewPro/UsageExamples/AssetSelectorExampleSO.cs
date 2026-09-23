using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// AssetSelector 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class AssetSelectorExampleSO : AttributeExampleSO<AssetSelectorExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        [AssetSelector]
        public Material anyAllMaterials;

        [FoldoutGroup("No Parameters")]
        [AssetSelector]
        public Material[] listOfAllMaterials;

        [FoldoutGroup("Parameter: FlattenTreeView")]
        [AssetSelector(FlattenTreeView = true)]
        public Material noTreeView;

        [FoldoutGroup("Parameter: Paths")]
        [AssetSelector(Paths = "Assets/Prototype/AttributeBankPrototype")]
        public ScriptableObject scriptableObjectsFromFolder;

        [FoldoutGroup("Parameter: Paths")]
        [AssetSelector(Paths = "Assets/Prototype/AttributeBankPrototype|Assets/Plugins/Sirenix")]
        public Material scriptableObjectsFromMultipleFolders;

        [FoldoutGroup("Parameter: Filter")]
        [AssetSelector(Filter = "name t:type l:label")]
        public UnityEngine.Object assetDatabaseSearchFilters;

        [FoldoutGroup("Parameter: DisableListAddButtonBehaviour")]
        [AssetSelector(DisableListAddButtonBehaviour = true)]
        public List<GameObject> disableListAddButtonBehaviour;

        [FoldoutGroup("Parameter: DrawDropdownForListElements")]
        [AssetSelector(DrawDropdownForListElements = false)]
        public List<GameObject> disableListElementBehaviour;

        [FoldoutGroup("Parameter: ExcludeExistingValuesInList")]
        [AssetSelector(ExcludeExistingValuesInList = true)]
        public List<GameObject> excludeExistingValuesInList;

        [FoldoutGroup("Parameter: IsUniqueList")]
        [AssetSelector(IsUniqueList = false)]
        public List<GameObject> disableUniqueListBehaviour;

        [FoldoutGroup("Parameter: ExpandAllMenuItems")]
        [AssetSelector(ExpandAllMenuItems = false)]
        public List<GameObject> expandAllMenuItems;

        [FoldoutGroup("Parameter: DropdownWidth, DropdownHeight, DropdownTitle")]
        [AssetSelector(DropdownWidth = 600, DropdownHeight = 300, DropdownTitle = "Dropdown Title")]
        public GameObject customDropdownTitle;

        public override void AesirInspectorReset()
        {
            anyAllMaterials = null;
            listOfAllMaterials = null;
            noTreeView = null;
            scriptableObjectsFromFolder = null;
            scriptableObjectsFromMultipleFolders = null;
            assetDatabaseSearchFilters = null;
            disableListAddButtonBehaviour = null;
            disableListElementBehaviour = null;
            excludeExistingValuesInList = null;
            disableUniqueListBehaviour = null;
            expandAllMenuItems = null;
            customDropdownTitle = null;
        }
    }
}

using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// MenuItemViewer 可视化面板
    /// </summary>
    public class MenuItemViewerSO : ScriptableObject, IAesirInspectorReset
    {
        static readonly string ConfigName = typeof(MenuItemViewerSO).GetNiceFullName();

        static MenuItemViewerSO _instance;

        /// <summary>
        /// 单例访问。解析结果按域缓存：缺失资产的解析会执行 CreateAsset 与 AssetDatabase.Refresh，
        /// 每次都重新解析会把这类重操作带进绘制回调等高频路径。
        /// </summary>
        public static MenuItemViewerSO Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = ScriptableObjectSafeEditorUtility.GetOrCreateEditorScriptableObject<MenuItemViewerSO>(
                    ConfigName, AesirInspectorPaths.MiniToolsAssetsFolderPath, "MenuItemViewer");
                return _instance;
            }
        }

        #region Event Functions

        void OnEnable()
        {
            bilingualHeaderControl = new BilingualHeaderControl("菜单项查看器", "MenuItem Viewer",
                "查看项目内所有 MenuItem，便于规划菜单路径。",
                "Browse all MenuItems in the project to help plan menu paths.",
                AesirInspectorWebLinks.GitUrl);
        }

        #endregion

        #region IAesirInspectorReset Members

        public void AesirInspectorReset()
        {
            assemblyFilter = null;
            menuItemInfos = null;
        }

        #endregion

        [PropertySpace(8, 8)]
        [BilingualButton("收集菜单项", "Collect MenuItems", ButtonSizes.Large, icon: SdfIconType.Search)]
        public void CollectMenuItems()
        {
            menuItemInfos = MenuItemViewerController.GetAllMenuItems(assemblyFilter);
        }

        #region Serialized Fields

        public BilingualHeaderControl bilingualHeaderControl;

        [PropertySpace]
        [SerializeReference]
        [BilingualTitle("程序集过滤器", "Assembly Filter")]
        [HideLabel]
        public IAssemblyFilter assemblyFilter;

        [PropertyOrder(10)]
        [Searchable(FilterOptions = SearchFilterOptions.ISearchFilterableInterface)]
        public List<MenuItemInfo> menuItemInfos;

        #endregion
    }
}

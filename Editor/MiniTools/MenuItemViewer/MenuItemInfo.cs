using System;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// 存储 UnityEditor.MenuItem 特性的参数信息
    /// </summary>
    [Serializable]
    public class MenuItemInfo : ISearchFilterable
    {
        const string MetaGroupName = "MenuItemInfoMeta";

        /// <summary>
        /// 供搜索使用的方法名，不显示在面板中
        /// </summary>
        [SerializeField]
        [HideInInspector]
        string methodName;

        public MenuItemInfo(MenuItem menuItem, MethodInfo method)
        {
            MenuPath = menuItem.menuItem;
            Priority = menuItem.priority;
            IsValidateFunction = menuItem.validate;
            methodName = method.Name;
            ClassName = method.DeclaringType?.Name;
            FullMethodSignature = $"{ClassName}.{method.Name}()";
            Method = method;
            Assembly = method.DeclaringType?.Assembly;
        }

        #region Displayed Fields

        /// <summary>
        /// 菜单路径独占一行并自动换行：与其它列同行时列宽被固定，
        /// 而 DisplayAsString 超宽会静默截断，长路径必须换行才能完整可见
        /// </summary>
        [WrappedText(LabelWidth = 76)]
        [BilingualText("菜单项", "Menu Path")]
        public string MenuPath;

        [HorizontalGroup(MetaGroupName, Width = 0.5f)]
        [LabelWidth(60)]
        [EnableGUI]
        [DisplayAsString]
        [BilingualText("优先级", "Priority")]
        public int Priority;

        [HorizontalGroup(MetaGroupName, Width = 0.5f)]
        [LabelWidth(62)]
        [EnableGUI]
        [DisplayAsString]
        [BilingualText("校验", "Validate")]
        public bool IsValidateFunction;

        #endregion

        #region Runtime Fields

        public string MethodName => methodName;

        public string ClassName { get; set; }

        public string FullMethodSignature { get; set; }

        public MethodInfo Method { get; set; }

        public Assembly Assembly { get; set; }

        #endregion

        #region ISearchFilterable Members

        public bool IsMatch(string searchString) =>
            MenuPath.ToLower().Contains(searchString.ToLower()) ||
            methodName.ToLower().Contains(searchString.ToLower());

        #endregion
    }
}

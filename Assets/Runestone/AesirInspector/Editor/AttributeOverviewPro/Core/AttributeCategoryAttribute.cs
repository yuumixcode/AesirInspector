using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// 为特性面板 SO 标记所属的 Odin 特性分类。
    /// [已弃用] 菜单分类归属现以 Odin 官方注册表（AesirAttributeRegistry）为准，
    /// 本特性仅作为历史标注保留，不参与 Ultra 窗口的目录构建。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AttributeCategoryAttribute : Attribute
    {
        public AttributeCategoryAttribute(AesirAttributeCategory category) => Category = category;

        /// <summary>
        /// 所属特性分类。
        /// </summary>
        public AesirAttributeCategory Category { get; }
    }
}

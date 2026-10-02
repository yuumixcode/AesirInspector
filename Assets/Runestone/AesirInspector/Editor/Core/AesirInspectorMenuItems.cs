namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Aesir Inspector 所有 MenuItem 菜单路径和优先级的统一管理。
    /// Unity 中 MenuItem 的顺序由 priority 参数（一个整数）决定，核心规则是：数字越小，位置越靠上。若不设置，默认值为 1000。
    /// 父菜单的 priority 由首次被编译的子菜单项决定。
    /// <para>
    /// 例外：Bootstrap 程序集里的 <c>Check Odin Dependency</c> 不能引用本类（未安装 Odin 时本程序集不存在），
    /// 其路径与优先级内联在 <c>AesirInspectorOdinDependencyMenu</c> 中，并固定排在 Inspector 子菜单最下方。
    /// </para>
    /// </summary>
    public static class AesirInspectorMenuItems
    {
        #region Menu Roots

        public const string ToolsAesirRoot = "Tools/Aesir";
        public const string ToolsAesirInspectorRoot = "Tools/Aesir/Inspector";

        #endregion

        #region Tools Menu

        /// <summary>
        /// 打开 Preferences 窗口的菜单路径。
        /// </summary>
        public const string Preferences = ToolsAesirInspectorRoot + "/Preferences";

        /// <summary>
        /// Preferences 菜单项优先级。
        /// </summary>
        public const int PreferencesOrder = -880;

        /// <summary>
        /// Preferences 窗口标题。
        /// </summary>
        public const string PreferencesWindowName = "Preferences";

        /// <summary>
        /// 打开 Attribute Overview Pro 窗口的菜单路径。
        /// </summary>
        public const string AttributeOverviewPro = ToolsAesirInspectorRoot + "/Attribute Overview Pro";

        /// <summary>
        /// Attribute Overview Pro 菜单项优先级。
        /// </summary>
        public const int AttributeOverviewProOrder = -900;

        /// <summary>
        /// Attribute Overview Pro 窗口标题。
        /// </summary>
        public const string AttributeOverviewProWindowName = "Attribute Overview Pro";

        /// <summary>
        /// 打开 Mini Tools 窗口的菜单路径。
        /// </summary>
        public const string MiniTools = ToolsAesirInspectorRoot + "/Mini Tools";

        /// <summary>
        /// Mini Tools 菜单项优先级。
        /// </summary>
        public const int MiniToolsOrder = -885;

        /// <summary>
        /// 打开 Plugin Config Solutions 示例窗口的菜单路径。
        /// </summary>
        public const string SamplePluginConfigSolutions =
            ToolsAesirInspectorRoot + "/Samples/Plugin Config Solutions";

        /// <summary>
        /// Plugin Config Solutions 示例菜单项优先级。
        /// </summary>
        public const int SamplePluginConfigSolutionsOrder = -800;

        /// <summary>
        /// Plugin Config Solutions 示例窗口标题。
        /// </summary>
        public const string SamplePluginConfigSolutionsWindowName = "Plugin Config Solutions";

        #endregion
    }
}

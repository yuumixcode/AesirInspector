namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Aesir Inspector 所有 MenuItem 菜单路径和优先级的统一管理。
    /// Unity 中 MenuItem 的顺序由 priority 参数（一个整数）决定，核心规则是：数字越小，位置越靠上。若不设置，默认值为 1000。
    /// 父菜单的 priority 由首次被编译的子菜单项决定。
    /// </summary>
    public static class AesirInspectorMenuItems
    {
        #region Menu Roots

        public const string ToolsAesirRoot = "Tools/Aesir";
        public const string ToolsAesirInspectorRoot = "Tools/Aesir/Inspector";

        #endregion

        #region Tools Menu

        /// <summary>
        /// 打开 Getting Started 窗口的菜单路径。
        /// </summary>
        public const string GettingStarted = ToolsAesirRoot + "/Getting Started";

        /// <summary>
        /// Getting Started 菜单项优先级。
        /// </summary>
        public const int GettingStartedOrder = -980;

        /// <summary>
        /// Getting Started 窗口标题。
        /// </summary>
        public const string GettingStartedWindowName = "Getting Started";

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
        /// 打开 Attribute Overview Ultra 窗口的菜单路径。
        /// </summary>
        public const string AttributeOverviewUltra = ToolsAesirInspectorRoot + "/Attribute Overview Ultra";

        /// <summary>
        /// Attribute Overview Ultra 菜单项优先级。
        /// </summary>
        public const int AttributeOverviewUltraOrder = -900;

        /// <summary>
        /// Attribute Overview Ultra 窗口标题。
        /// </summary>
        public const string AttributeOverviewUltraWindowName = "Attribute Overview Ultra";

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

        /// <summary>
        /// 打开 RuntimeInitializeLoadType 示例窗口的菜单路径。
        /// </summary>
        public const string SampleRuntimeInitializeOnLoad =
            ToolsAesirInspectorRoot + "/Samples/RuntimeInitializeLoadType";

        /// <summary>
        /// RuntimeInitializeLoadType 示例菜单项优先级。
        /// </summary>
        public const int SampleRuntimeInitializeOnLoadOrder = -795;

        /// <summary>
        /// RuntimeInitializeLoadType 示例窗口标题。
        /// </summary>
        public const string SampleRuntimeInitializeOnLoadWindowName = "RuntimeInitializeLoadType";

        #endregion
    }
}

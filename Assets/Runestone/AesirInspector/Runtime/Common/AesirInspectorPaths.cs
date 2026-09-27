namespace Runestone.AesirInspector
{
    /// <summary>
    /// Aesir Inspector 在编辑器中使用的路径。
    /// </summary>
    public static class AesirInspectorPaths
    {
        /// <summary>
        /// Aesir Inspector 的编辑器阶段资源的文件夹路径。
        /// 位于 Unity 特殊文件夹 Editor Default Resources 下：其中的资产只在编辑器阶段可用，不会包含在构建中。
        /// </summary>
        public const string EditorDefaultResourcesPath = "Assets/Editor Default Resources/AesirInspectorData";

        /// <summary>
        /// Preferences 配置资产路径
        /// </summary>
        public const string PreferencesAssetsFolderPath = EditorDefaultResourcesPath + "/Preferences";

        /// <summary>
        /// Attribute Overview 数据资产存放文件夹路径（仅 Pro 状态存储，无 Pro 资产）。
        /// </summary>
        public const string AttributeOverviewDataPath = EditorDefaultResourcesPath + "/AttributeOverviewPro";

        /// <summary>
        /// Attribute Overview Pro 状态存储资产路径。
        /// 存储面板选中记录与示例数据快照。
        /// </summary>
        public const string AttributeOverviewProStateStorePath =
            AttributeOverviewDataPath + "/ProStateStore.asset";

        /// <summary>
        /// MiniTools 资源的存放路径
        /// </summary>
        public const string MiniToolsAssetsFolderPath = EditorDefaultResourcesPath + "/MiniTools";
    }
}

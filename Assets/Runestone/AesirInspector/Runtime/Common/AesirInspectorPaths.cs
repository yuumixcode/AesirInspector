namespace Runestone.AesirInspector
{
    /// <summary>
    /// Aesir Inspector 在编辑器中使用的路径。
    /// </summary>
    public static class AesirInspectorPaths
    {
        /// <summary>
        /// Aesir Inspector 的编辑器阶段资源的文件夹路径
        /// </summary>
        public const string EditorDefaultResourcesPath = "Assets/Editor Default Resources/Aesir Inspector";

        /// <summary>
        /// Preferences 配置资产路径
        /// </summary>
        public const string PreferencesAssetsFolderPath = EditorDefaultResourcesPath + "/Preferences";

        /// <summary>
        /// Attribute Overview Pro 数据库资产存放文件夹路径
        /// </summary>
        public const string AttributeOverviewDatabasePath =
            EditorDefaultResourcesPath + "/Attribute Overview Pro";

        /// <summary>
        /// [已弃用] PanelSO 现作为数据库子资产存储。仅保留用于旧资产迁移清理。
        /// </summary>
        public const string AttributePanelsPath =
            EditorDefaultResourcesPath + "/Attribute Overview Pro/Panels";

        /// <summary>
        /// [已弃用] ExampleSO 现按序列化方式分别存入 Unity/Odin 容器。仅保留用于旧资产迁移清理。
        /// </summary>
        public const string AttributeExamplesPath =
            EditorDefaultResourcesPath + "/Attribute Overview Pro/Attribute Examples";

        /// <summary>
        /// Unity 原生序列化的 ExampleSO 容器文件路径。
        /// </summary>
        public const string AttributeExamplesUnityPath =
            AttributeOverviewDatabasePath + "/UnityExamples.asset";

        /// <summary>
        /// Odin 序列化的 ExampleSO 容器文件路径。
        /// </summary>
        public const string AttributeExamplesOdinPath = AttributeOverviewDatabasePath + "/OdinExamples.asset";

        /// <summary>
        /// Attribute Overview Ultra 状态银行资产路径。
        /// 存储面板选中记录与示例调试状态快照，替代 Pro 的示例子资产持久化。
        /// </summary>
        public const string AttributeOverviewUltraStateBankPath =
            AttributeOverviewDatabasePath + "/UltraStateBank.asset";

        /// <summary>
        /// MiniTools 资源的存放路径
        /// </summary>
        public const string MiniToolsAssetsFolderPath = EditorDefaultResourcesPath + "/MiniTools";
    }
}

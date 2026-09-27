using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// 编辑器数据目录的幂等迁移。0.19.0 → 0.20.0 期间数据路径发生了两次重命名，这里按顺序补齐，
    /// 保证升级用户的数据无损（全部使用 <see cref="AssetDatabase.MoveAsset" />，资产 GUID 保持不变，
    /// 因此 EditorBuildSettings 中的配置引用与状态存储中的用户数据都不会丢失）：
    /// <list type="number">
    /// <item><c>Assets/Editor Default Resources/Aesir Inspector</c> → <c>Assets/Editor Default Resources/AesirInspectorData</c></item>
    /// <item><c>…/AesirInspectorData/Attribute Overview</c> → <c>…/AesirInspectorData/AttributeOverviewPro</c></item>
    /// <item><c>…/AttributeOverviewPro/UltraStateStore.asset</c> → <c>…/AttributeOverviewPro/ProStateStore.asset</c></item>
    /// </list>
    /// 迁移必须在任何 AesirInspectorPaths 消费者解析之前完成，否则新路径会先自动创建出第二份资产。
    /// </summary>
    [InitializeOnLoad]
    internal static class AesirInspectorDataFolderMigration
    {
        /// <summary>0.19.0 及之前的根数据目录。</summary>
        const string LegacyRootFolderPath = "Assets/Editor Default Resources/Aesir Inspector";

        /// <summary>0.20.0 使用的根数据目录（后续子目录迁移以此为基准）。</summary>
        const string IntermediateRootFolderPath = "Assets/Editor Default Resources/AesirInspectorData";

        /// <summary>旧版 Attribute Overview 子目录名（含空格，无 Pro 后缀）。</summary>
        const string LegacyAttributeOverviewFolderName = "Attribute Overview";

        /// <summary>旧版状态存储资产名。</summary>
        const string LegacyStateStoreFileName = "UltraStateStore.asset";

        static AesirInspectorDataFolderMigration()
        {
            Migrate();
        }

        /// <summary>
        /// 幂等迁移：每一步都只在「旧路径存在且新路径不存在」时执行，可重复调用。
        /// </summary>
        internal static void Migrate()
        {
            var migrated = false;
            migrated |= TryMove(LegacyRootFolderPath, AesirInspectorPaths.EditorDefaultResourcesPath);
            migrated |= TryMove(IntermediateRootFolderPath + "/" + LegacyAttributeOverviewFolderName,
                AesirInspectorPaths.AttributeOverviewDataPath);
            migrated |= TryMove(AesirInspectorPaths.AttributeOverviewDataPath + "/" + LegacyStateStoreFileName,
                AesirInspectorPaths.AttributeOverviewProStateStorePath);

            if (migrated)
            {
                AssetDatabase.SaveAssets();
            }
        }

        static bool TryMove(string legacyPath, string newPath)
        {
            if (string.Equals(legacyPath, newPath, System.StringComparison.Ordinal))
            {
                return false;
            }

            if (!Exists(legacyPath) || Exists(newPath))
            {
                return false;
            }

            var error = AssetDatabase.MoveAsset(legacyPath, newPath);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogWarning(
                    $"[Aesir Inspector] 编辑器数据迁移失败（{legacyPath} → {newPath}）：{error}。请手动移动该目录或资产。");
                return false;
            }

            Debug.Log($"[Aesir Inspector] 编辑器数据已迁移：{legacyPath} → {newPath}");
            return true;
        }

        static bool Exists(string assetPath) =>
            !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(assetPath));
    }
}

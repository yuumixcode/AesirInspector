using System.Diagnostics;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 关于 Path 路径的编辑器安全工具类。仅在编辑器阶段可用，打包后调用自动剔除。
    /// </summary>
    public static class PathSafeEditorUtility
    {
        /// <summary>
        /// 确保 Assets 目录下的相对路径的文件夹存在，如果不存在则递归创建。仅在编辑器阶段可用，打包后自动剔除。
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void EnsureDirectoryExists(string relativePath)
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(relativePath))
            {
                return;
            }

            var fullPath = PathUtility.ToUnityPath(relativePath.Trim());
            if (!Directory.Exists(fullPath))
            {
                // Directory 默认递归创建
                Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh();
            }
#endif
        }

        /// <summary>
        /// 确保 Assets 目录下的文件夹在 AssetDatabase 中真实存在（逐级创建）。
        /// 与 <see cref="EnsureDirectoryExists" /> 的差异：这里通过 AssetDatabase.CreateFolder 逐级创建，
        /// 创建后可以立即作为 AssetDatabase.CreateAsset 的父目录使用（无需额外的 AssetDatabase.Refresh）。
        /// 仅在编辑器阶段可用，打包后自动剔除。
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void EnsureAssetFolderExists(string relativePath)
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(relativePath))
            {
                return;
            }

            var unityPath = PathUtility.ToUnityPath(relativePath.Trim()).TrimEnd('/');
            if (AssetDatabase.IsValidFolder(unityPath))
            {
                return;
            }

            var segments = unityPath.Split('/');
            if (segments.Length < 2 || segments[0] != "Assets")
            {
                // 只处理工程 Assets 下的相对路径，其余情况交给调用方自行处理。
                return;
            }

            var current = segments[0];
            for (var i = 1; i < segments.Length; i++)
            {
                var next = current + "/" + segments[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[i]);
                }

                current = next;
            }
#endif
        }
    }
}

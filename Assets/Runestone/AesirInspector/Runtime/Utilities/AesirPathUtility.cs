using System;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// Aesir Inspector 包自身位置的路径解析工具。
    /// 这里只做纯字符串处理（不依赖 Unity API），便于单元测试；
    /// 依赖 AssetDatabase 的解析流程见编辑器程序集中的 <c>AesirPackagePaths</c>。
    /// </summary>
    public static class AesirPathUtility
    {
        /// <summary>包名（UPM 安装时包位于 Packages/&lt;包名&gt; 下）。</summary>
        public const string PackageName = "cn.runestone.aesir-inspector";

        /// <summary>开发安装（包位于 Assets 下）时的默认包根路径。</summary>
        public const string DefaultPackageRootPath = "Assets/Runestone/AesirInspector/";

        /// <summary>UPM 安装时的包根路径。</summary>
        public const string UpmPackageRootPath = "Packages/" + PackageName + "/";

        /// <summary>包内示例资产目录相对包根的路径段。</summary>
        public const string ExampleAssetsFolderSegment = "Editor/ExampleAssets/";

        /// <summary>包位置锚点资产相对包根的路径（其 GUID 固定，与安装位置无关）。</summary>
        public const string LookupAssetRelativePath = ExampleAssetsFolderSegment + "AesirPathLookup.asset";

        /// <summary>
        /// 从锚点资产的 Unity 路径（<c>Assets/…</c> 或 <c>Packages/…</c>）截出包根路径（以 <c>/</c> 结尾）。
        /// 例如 <c>Packages/cn.runestone.aesir-inspector/Editor/ExampleAssets/AesirPathLookup.asset</c>
        /// 解析为 <c>Packages/cn.runestone.aesir-inspector/</c>。
        /// </summary>
        public static bool TryExtractPackageRoot(string anchorAssetPath, out string packageRootPath)
        {
            packageRootPath = null;
            if (string.IsNullOrEmpty(anchorAssetPath))
            {
                return false;
            }

            var index = anchorAssetPath.IndexOf(ExampleAssetsFolderSegment, StringComparison.Ordinal);
            if (index <= 0)
            {
                return false;
            }

            packageRootPath = anchorAssetPath.Substring(0, index);
            return true;
        }

        /// <summary>
        /// 判断给定路径是否正是包内的锚点资产（避免误用其它同名资产）。
        /// </summary>
        public static bool IsLookupAssetPath(string assetPath)
        {
            return !string.IsNullOrEmpty(assetPath)
                && assetPath.EndsWith(LookupAssetRelativePath, StringComparison.Ordinal);
        }
    }
}

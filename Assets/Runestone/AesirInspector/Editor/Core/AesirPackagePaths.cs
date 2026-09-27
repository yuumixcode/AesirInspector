using System.IO;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// 解析 Aesir Inspector 包自身的安装位置，使包内资产在 Assets 安装与 UPM（Packages）安装下都能被定位。
    /// 机制与 Odin 的 <c>SirenixAssetPaths</c> 一致：锚点资产的 GUID 固定，与资产位置无关。
    /// 结果按域缓存，域重载后重新解析。
    /// </summary>
    public static class AesirPackagePaths
    {
        /// <summary>锚点资产的固定 GUID，必须与 <c>AesirPathLookup.asset.meta</c> 保持一致。</summary>
        public const string LookupAssetGuid = "a5ae7fe4edbe4eaf946864be232d4bfc";

        /// <summary>锚点资产的名称。</summary>
        public const string LookupAssetName = "AesirPathLookup.asset";

        static bool _resolved;
        static string _packageRootPath;

        /// <summary>
        /// 是否成功解析出包根路径。
        /// </summary>
        public static bool IsResolved
        {
            get
            {
                EnsureResolved();
                return _packageRootPath != null;
            }
        }

        /// <summary>
        /// 包根路径（以 <c>/</c> 结尾）。未解析成功时回落为默认开发安装路径。
        /// </summary>
        public static string PackageRootPath
        {
            get
            {
                EnsureResolved();
                return _packageRootPath ?? AesirPathUtility.DefaultPackageRootPath;
            }
        }

        /// <summary>包内示例资产目录。</summary>
        public static string ExampleAssetsPath =>
            PackageRootPath + AesirPathUtility.ExampleAssetsFolderSegment;

        /// <summary>示例资产目录下的 ScriptableObject 目录。</summary>
        public static string ScriptableObjectsPath => ExampleAssetsPath + "ScriptableObjects";

        /// <summary>示例资产目录下的 Primary 材质目录。</summary>
        public static string MaterialsPrimaryPath => ExampleAssetsPath + "Materials/Primary";

        /// <summary>示例资产目录下的 Secondary 材质目录。</summary>
        public static string MaterialsSecondaryPath => ExampleAssetsPath + "Materials/Secondary";

        static void EnsureResolved()
        {
            if (_resolved)
            {
                return;
            }

            _resolved = true;
            _packageRootPath = ResolvePackageRoot();
        }

        static string ResolvePackageRoot()
        {
            // 快通道 1：开发安装（包位于 Assets 下），无需访问 AssetDatabase。
            if (File.Exists(AesirPathUtility.DefaultPackageRootPath + AesirPathUtility.LookupAssetRelativePath))
            {
                return AesirPathUtility.DefaultPackageRootPath;
            }

            // 快通道 2：UPM 安装（包位于 Packages 下，包名固定）。
            if (File.Exists(AesirPathUtility.UpmPackageRootPath + AesirPathUtility.LookupAssetRelativePath))
            {
                return AesirPathUtility.UpmPackageRootPath;
            }

            // 主通道：通过固定 GUID 定位锚点资产，GUID 与安装位置无关。
            var guidAssetPath = AssetDatabase.GUIDToAssetPath(LookupAssetGuid);
            if (AesirPathUtility.TryExtractPackageRoot(guidAssetPath, out var rootFromGuid))
            {
                return rootFromGuid;
            }

            // 兜底 1：按锚点资产类型查找（GUID 被意外改变时仍可定位）。
            foreach (var guid in AssetDatabase.FindAssets("t:AesirPathLookupScriptableObject"))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (AesirPathUtility.IsLookupAssetPath(assetPath)
                    && AesirPathUtility.TryExtractPackageRoot(assetPath, out var rootFromType))
                {
                    LogSlowFallbackWarning();
                    return rootFromType;
                }
            }

            // 兜底 2：全量扫描（非常规安装位置才会走到，比较慢）。
            foreach (var assetPath in AssetDatabase.GetAllAssetPaths())
            {
                if (AesirPathUtility.IsLookupAssetPath(assetPath)
                    && AesirPathUtility.TryExtractPackageRoot(assetPath, out var rootFromScan))
                {
                    LogSlowFallbackWarning();
                    return rootFromScan;
                }
            }

            Debug.LogWarning("[Aesir Inspector] 未能定位包目录：锚点资产 " + AesirPathUtility.LookupAssetRelativePath +
                             " 缺失，包内示例资产路径回落为默认值 " + AesirPathUtility.DefaultPackageRootPath + "。");
            return null;
        }

        static void LogSlowFallbackWarning()
        {
            Debug.LogWarning("[Aesir Inspector] 锚点资产 " + LookupAssetName +
                             " 未在其固定 GUID 上找到，已通过慢速回退方式定位包目录，这会增加项目重载时间。" +
                             "请重新导入 Aesir Inspector 包以恢复 " + LookupAssetName + " 的 GUID。");
        }
    }
}

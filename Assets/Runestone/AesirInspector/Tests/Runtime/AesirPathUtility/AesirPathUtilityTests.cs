using NUnit.Framework;

namespace Runestone.AesirInspector.Tests
{
    /// <summary>
    /// 关于 AesirPathUtility 包路径解析的测试：
    /// 验证包根路径的截取逻辑对「Assets 安装」与「UPM（Packages）安装」两种前缀都成立。
    /// </summary>
    public class AesirPathUtilityTests
    {
        const string AssetsAnchorPath =
            "Assets/Runestone/AesirInspector/Editor/ExampleAssets/AesirPathLookup.asset";

        const string UpmAnchorPath =
            "Packages/cn.runestone.aesir-inspector/Editor/ExampleAssets/AesirPathLookup.asset";

        /// <summary>
        /// Assets 安装（开发副本）时，包根应解析为 Assets/Runestone/AesirInspector/。
        /// </summary>
        [Test]
        public void TryExtractPackageRoot_AssetsInstall_ReturnsAssetsPackageRoot()
        {
            var success = AesirPathUtility.TryExtractPackageRoot(AssetsAnchorPath, out var root);
            Assert.IsTrue(success);
            Assert.AreEqual(AesirPathUtility.DefaultPackageRootPath, root);
            Assert.AreEqual("Assets/Runestone/AesirInspector/", root);
        }

        /// <summary>
        /// UPM 安装时，包根应解析为 Packages/&lt;包名&gt;/。
        /// </summary>
        [Test]
        public void TryExtractPackageRoot_UpmInstall_ReturnsUpmPackageRoot()
        {
            var success = AesirPathUtility.TryExtractPackageRoot(UpmAnchorPath, out var root);
            Assert.IsTrue(success);
            Assert.AreEqual(AesirPathUtility.UpmPackageRootPath, root);
            Assert.AreEqual("Packages/" + AesirPathUtility.PackageName + "/", root);
        }

        /// <summary>
        /// 解析以第一处「Editor/ExampleAssets/」为界，之前的路径即包根。
        /// </summary>
        [Test]
        public void TryExtractPackageRoot_RepeatedSegment_ReturnsRootBeforeFirstSegment()
        {
            const string path =
                "Assets/Some/Editor/ExampleAssets/Editor/ExampleAssets/AesirPathLookup.asset";
            var success = AesirPathUtility.TryExtractPackageRoot(path, out var root);
            Assert.IsTrue(success);
            Assert.AreEqual("Assets/Some/", root);
        }

        /// <summary>
        /// 空路径、缺少目录段、或目录段位于最前（无法推导包根）时应返回 false 并输出 null。
        /// </summary>
        [TestCase(null)]
        [TestCase("")]
        [TestCase("Assets/Runestone/AesirInspector/Editor/AesirPathLookup.asset")]
        [TestCase("Editor/ExampleAssets/AesirPathLookup.asset")]
        public void TryExtractPackageRoot_InvalidPath_ReturnsFalse(string path)
        {
            var success = AesirPathUtility.TryExtractPackageRoot(path, out var root);
            Assert.IsFalse(success);
            Assert.IsNull(root);
        }

        /// <summary>
        /// 只有包内锚点资产的完整相对路径才算命中，避免误用其它同名资产。
        /// </summary>
        [Test]
        public void IsLookupAssetPath_MatchesOnlyAnchorAsset()
        {
            Assert.IsTrue(AesirPathUtility.IsLookupAssetPath(AssetsAnchorPath));
            Assert.IsTrue(AesirPathUtility.IsLookupAssetPath(UpmAnchorPath));
            Assert.IsFalse(AesirPathUtility.IsLookupAssetPath(
                "Assets/Runestone/AesirInspector/Editor/ExampleAssets/Other.asset"));
            Assert.IsFalse(AesirPathUtility.IsLookupAssetPath(""));
            Assert.IsFalse(AesirPathUtility.IsLookupAssetPath(null));
        }
    }
}

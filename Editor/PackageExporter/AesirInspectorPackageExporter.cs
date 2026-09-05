using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    [Summary("导出当前版本的 .unitypackage，输出到工程根目录 Builds/ 下，供本地分发与 GitHub Release 使用")]
    public static class AesirInspectorPackageExporter
    {
        const string PackagePath = "Assets/Runestone/AesirInspector";
        const string OutputFolderName = "Builds";

        public const string ExportMenu = AesirInspectorMenuItems.ToolsAesirInspectorRoot + "/Export Package";

        [MenuItem(ExportMenu)]
        public static void ExportFromMenu()
        {
            var outputPath = ExportCurrentVersion();
            EditorUtility.RevealInFinder(outputPath);
        }

        [Summary("供 Unity 命令行 -executeMethod 调用：读取 package.json 版本号，导出 AesirInspector-<version>.unitypackage 到 Builds/ 目录")]
        public static string ExportCurrentVersion()
        {
            var version = ParsePackageVersion();
            var outputPath = Path.Combine(GetProjectRoot(), OutputFolderName, $"AesirInspector-{version}.unitypackage");
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            AssetDatabase.ExportPackage(PackagePath, outputPath, ExportPackageOptions.Default | ExportPackageOptions.Recurse);
            Debug.Log($"[AesirInspector] Package exported: {outputPath}");
            return outputPath;
        }

        static string GetProjectRoot()
        {
            return Directory.GetCurrentDirectory();
        }

        static string ParsePackageVersion()
        {
            var packageJsonPath = Path.Combine(GetProjectRoot(), PackagePath, "package.json");
            var json = File.ReadAllText(packageJsonPath);
            var match = Regex.Match(json, "\"version\"\\s*:\\s*\"([^\"]+)\"");
            if (!match.Success)
            {
                throw new InvalidOperationException($"[AesirInspector] version not found in {packageJsonPath}");
            }
            return match.Groups[1].Value;
        }
    }
}

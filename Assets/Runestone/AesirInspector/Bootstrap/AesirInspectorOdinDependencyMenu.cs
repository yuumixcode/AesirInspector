using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Runestone.AesirInspector.Bootstrap
{
    /// <summary>
    /// Odin Inspector 依赖检查菜单。
    /// <para>
    /// 本程序集刻意<b>不带</b> ODIN_INSPECTOR 约束，也不引用 AesirInspector 的其它程序集：
    /// 未安装 Odin 时其余程序集会被整体跳过编译，只有这里仍能编译并运行，从而给出明确提示，
    /// 而不是让用户面对一个「装了却毫无反应」的包。检测逻辑与安装方式无关，
    /// UPM（Packages）与 Assets 目录两种安装模式都适用。
    /// </para>
    /// </summary>
    internal static class AesirInspectorOdinDependencyMenu
    {
        // 本程序集不能引用 AesirInspectorMenuItems（它所在的程序集带 ODIN_INSPECTOR 约束，
        // 未安装 Odin 时并不存在），因此这里内联菜单路径与优先级。
        const string MenuPath = "Tools/Aesir/Inspector/Check Odin Dependency";
        const int MenuOrder = -975;

        const string OdinAttributesAssemblyName = "Sirenix.OdinInspector.Attributes";
        const string SirenixUtilitiesAssemblyName = "Sirenix.Utilities";

        [MenuItem(MenuPath, false, MenuOrder)]
        static void CheckOdinDependency()
        {
            var odinAvailable = IsOdinAvailable();
            EditorUtility.DisplayDialog(BuildTitle(odinAvailable), BuildMessage(odinAvailable), GetOkButtonText());
        }

        /// <summary>
        /// 是否检测到 Odin Inspector：先看 Odin 写入的 ODIN_INSPECTOR 宏，再用类型反射二次确认程序集确实存在。
        /// </summary>
        static bool IsOdinAvailable()
        {
#if ODIN_INSPECTOR
            return FindLoadedAssembly(OdinAttributesAssemblyName) != null
                   || FindLoadedAssembly(SirenixUtilitiesAssemblyName) != null;
#else
            return false;
#endif
        }

        static Assembly FindLoadedAssembly(string assemblyName) =>
            AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == assemblyName);

        static bool IsChinese =>
            Application.systemLanguage == SystemLanguage.ChineseSimplified
            || Application.systemLanguage == SystemLanguage.ChineseTraditional;

        static string GetOkButtonText() => IsChinese ? "确定" : "OK";

        static string BuildTitle(bool odinAvailable) => IsChinese
            ? (odinAvailable ? "Aesir Inspector 依赖检查" : "Aesir Inspector 缺少依赖")
            : (odinAvailable ? "Aesir Inspector Dependency Check" : "Aesir Inspector Is Missing a Dependency");

        static string BuildMessage(bool odinAvailable)
        {
            var installMode = IsInstalledViaUpm() ? "UPM（Packages）" : "Assets 目录";

            if (odinAvailable)
            {
                return IsChinese
                    ? "已检测到 Odin Inspector，Aesir Inspector 可以正常编译。\n\n" +
                      $"Aesir Inspector 安装方式：{installMode}\nOdin Inspector：已检测到"
                    : "Odin Inspector was detected; Aesir Inspector can be compiled normally.\n\n" +
                      $"Aesir Inspector install mode: {installMode}\nOdin Inspector: detected";
            }

            return IsChinese
                ? "本项目已安装 Aesir Inspector，但未检测到 Odin Inspector，因此 Aesir Inspector 不会参与编译。\n\n" +
                  "请先安装 Odin Inspector 3.3.x+，再重新打开工程。\n" +
                  $"当前 Aesir Inspector 安装方式：{installMode}"
                : "Aesir Inspector is installed in this project, but Odin Inspector was not detected, " +
                  "so Aesir Inspector will not be compiled.\n\n" +
                  "Please install Odin Inspector 3.3.x+ and reopen the project.\n" +
                  $"Current Aesir Inspector install mode: {installMode}";
        }

        /// <summary>
        /// 本程序集自身的安装方式（用于提示信息；与包内 InstallationChecker 的判定口径一致）。
        /// </summary>
        static bool IsInstalledViaUpm()
        {
            try
            {
                return PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly()) != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// 自动确保 <c>AESIR_INSPECTOR</c> 脚本宏定义符号存在。
    /// <para>
    /// 通过 <see cref="InitializeOnLoadAttribute" /> 在编辑器加载时自动执行，
    /// 供 Aesir 系列其他插件（如 Aesir Architecture）通过 <c>#if AESIR_INSPECTOR</c>
    /// 检测本插件是否安装，从而在编译期禁用自身提供的重复功能。
    /// </para>
    /// <remarks>
    /// 宏必须写入<b>每一个构建目标</b>：Aesir Architecture 的 <c>#if !AESIR_INSPECTOR</c> 是编译期判断，
    /// 若只写当前激活平台，切换到其它平台后那份代码会重新参与编译，导致菜单与功能重复。
    /// 这里遍历 <see cref="BuildTargetGroup" /> 并用 <see cref="NamedBuildTarget.FromBuildTargetGroup" />
    /// 显式构造目标（不再反射 NamedBuildTarget 的静态字段，避免依赖 Unity 的内部字段布局）。
    /// 仅在实际添加符号时记录日志，避免每次重载都输出。
    /// </remarks>
    [InitializeOnLoad]
    internal static class EnsureAesirInspectorDefine
    {
        const string Symbol = "AESIR_INSPECTOR";

        static EnsureAesirInspectorDefine()
        {
            var added = false;
            foreach (var target in CollectTargets())
            {
                added |= EnsureSymbol(target);
            }

            if (added)
            {
                Debug.Log("[Aesir Inspector] 已添加宏定义符号: " + Symbol);
            }
        }

        /// <summary>
        /// 枚举所有有效的脚本编译目标（按目标名去重；跳过 Unknown / Server）。
        /// </summary>
        static IEnumerable<NamedBuildTarget> CollectTargets()
        {
            var seen = new HashSet<string>();
            foreach (BuildTargetGroup group in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (group == BuildTargetGroup.Unknown || !TryGetTarget(group, out var target))
                {
                    continue;
                }

                if (target == NamedBuildTarget.Unknown || target == NamedBuildTarget.Server)
                {
                    continue;
                }

                if (seen.Add(target.TargetName))
                {
                    yield return target;
                }
            }
        }

        static bool TryGetTarget(BuildTargetGroup group, out NamedBuildTarget target)
        {
            target = NamedBuildTarget.Unknown;
            try
            {
                target = NamedBuildTarget.FromBuildTargetGroup(group);
                return true;
            }
            catch (Exception)
            {
                // 未安装对应构建模块的平台可能抛异常，直接跳过。
                return false;
            }
        }

        static bool EnsureSymbol(NamedBuildTarget target)
        {
            var current = PlayerSettings.GetScriptingDefineSymbols(target);
            if (ContainsSymbol(current, Symbol))
            {
                return false;
            }

            var newSymbols = string.IsNullOrEmpty(current) ? Symbol : current + ";" + Symbol;
            PlayerSettings.SetScriptingDefineSymbols(target, newSymbols);
            return true;
        }

        static bool ContainsSymbol(string symbols, string symbol)
        {
            if (string.IsNullOrEmpty(symbols))
            {
                return false;
            }

            foreach (var part in symbols.Split(';'))
            {
                if (part.Trim() == symbol)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

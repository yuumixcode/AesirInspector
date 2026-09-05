using System.Collections.Generic;
using System.Reflection;
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
    /// </summary>
    /// <remarks>
    /// 与 <c>Runestone.AesirArchitecture.Editor.EnsureAesirArchitectureDefine</c> 机制一致；
    /// 本程序集不依赖 Architecture，因此内联了符号添加逻辑而非复用其工具类。
    /// 仅在实际添加符号时记录日志，避免每次重载都输出。
    /// </remarks>
    [InitializeOnLoad]
    internal static class EnsureAesirInspectorDefine
    {
        const string Symbol = "AESIR_INSPECTOR";

        static NamedBuildTarget[] _validTargets;

        static EnsureAesirInspectorDefine()
        {
            var added = false;
            foreach (var target in ValidTargets)
            {
                var current = PlayerSettings.GetScriptingDefineSymbols(target);
                if (ContainsSymbol(current, Symbol))
                {
                    continue;
                }

                var newSymbols = string.IsNullOrEmpty(current) ? Symbol : current + ";" + Symbol;
                PlayerSettings.SetScriptingDefineSymbols(target, newSymbols);
                added = true;
            }

            if (added)
            {
                Debug.Log($"[Aesir Inspector] 已添加宏定义符号: {Symbol}");
            }
        }

        static NamedBuildTarget[] ValidTargets
        {
            get
            {
                if (_validTargets != null)
                {
                    return _validTargets;
                }

                var list = new List<NamedBuildTarget>();
                var fields = typeof(NamedBuildTarget).GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (var field in fields)
                {
                    if (field.Name == "Unknown" || field.Name == "Server")
                    {
                        continue;
                    }

                    list.Add((NamedBuildTarget)field.GetValue(null));
                }

                _validTargets = list.ToArray();
                return _validTargets;
            }
        }

        static bool ContainsSymbol(string symbols, string symbol)
        {
            if (string.IsNullOrEmpty(symbols))
            {
                return false;
            }

            var parts = symbols.Split(';');
            foreach (var part in parts)
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
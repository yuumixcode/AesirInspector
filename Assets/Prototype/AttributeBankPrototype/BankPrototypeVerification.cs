using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Runestone.BankPrototype
{
    /// <summary>
    /// 方案 A 关键风险点验证 harness。通过 Codely Bridge / 菜单调用，返回结构化报告。
    /// 覆盖：往返正确性、Unity 引用 GUID 持久化、磁盘落盘证据、版本漂移容错、批量性能、可绘制性。
    /// </summary>
    public static class BankPrototypeVerification
    {
        const string BankPath = "Assets/Prototype/AttributeBankPrototype/TestBank.asset";
        const string MaterialPath = "Assets/Prototype/AttributeBankPrototype/TestMaterial.mat";

        [MenuItem("Tools/Bank Prototype/Run Verification")]
        public static void RunFromMenu() => _ = RunAll();

        public static string RunAll()
        {
            var report = "===== AttributeBank 原型验证 =====\n";
            EnsureTestAssets(out var bank, out var material);

            report += Scenario1_UnitySerializedRoundTrip(bank, material);
            report += Scenario2_OdinSerializedRoundTrip(bank, material);
            report += Scenario3_DiskPersistence(bank, material);
            report += Scenario4_CorruptionFallback(bank);
            report += Scenario5_Timing(bank);
            report += Scenario6_OdinEditorDrawable(bank);

            report += "===== 完成 =====\n";
            UnityEngine.Debug.Log(report);
            return report;
        }

        static void EnsureTestAssets(out ExampleStateBankSO bank, out Material material)
        {
            material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ??
                              Shader.Find("Sprites/Default");
                material = new Material(shader) { name = "TestMaterial" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }

            bank = AssetDatabase.LoadAssetAtPath<ExampleStateBankSO>(BankPath);
            if (bank == null)
            {
                bank = ScriptableObject.CreateInstance<ExampleStateBankSO>();
                AssetDatabase.CreateAsset(bank, BankPath);
            }

            AssetDatabase.SaveAssets();
        }

        static string Scenario1_UnitySerializedRoundTrip(ExampleStateBankSO bank, Material material)
        {
            var key = "S1/ProtoUnityExampleSO";

            // 造脏数据
            var original = ScriptableObject.CreateInstance<ProtoUnityExampleSO>();
            original.dynamicButtonName = "中文名测试";
            original.tweakValue = 777;
            original.notes = new List<string> { "a", "b", "c" };
            original.curve = new AnimationCurve(new Keyframe(0, 5), new Keyframe(2, -3));
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.red, 0), new GradientColorKey(Color.blue, 1) },
                new[] { new GradientAlphaKey(1, 0) });
            var so = new SerializedObject(original);
            so.FindProperty("colorGradient").gradientValue = gradient;
            so.FindProperty("referencedMaterial").objectReferenceValue = material;
            so.ApplyModifiedPropertiesWithoutUndo();

            bank.SaveState(key, original);

            // 恢复到全新实例
            var restored = ScriptableObject.CreateInstance<ProtoUnityExampleSO>();
            if (!bank.TryRestoreState(key, restored))
            {
                return "[S1][FAIL] TryRestoreState 返回 false\n";
            }

            var pass = true;
            pass &= restored.dynamicButtonName == "中文名测试";
            pass &= restored.tweakValue == 777;
            pass &= restored.notes.Count == 3 && restored.notes[2] == "c";
            pass &= Mathf.Approximately(restored.curve[1].value, -3f);
            pass &= restored.ReferencedMaterial == material; // Unity 资产引用往返
            pass &= restored.ColorGradient.colorKeys.Length == 2 &&
                    restored.ColorGradient.colorKeys[1].color == Color.blue;

            return $"[S1] Unity 序列化往返（中文/曲线/渐变/Material 引用）: {(pass ? "PASS" : "FAIL")}\n";
        }

        static string Scenario2_OdinSerializedRoundTrip(ExampleStateBankSO bank, Material material)
        {
            var key = "S2/ProtoOdinExampleSO";

            var original = ScriptableObject.CreateInstance<ProtoOdinExampleSO>();
            original.statDictionary["attack"] = 999;
            original.statDictionary["new_stat"] = 1;
            original.materialLibrary["mat_A"] = material; // 字典值里的资产引用

            bank.SaveState(key, original);

            var restored = ScriptableObject.CreateInstance<ProtoOdinExampleSO>();
            if (!bank.TryRestoreState(key, restored))
            {
                return "[S2][FAIL] TryRestoreState 返回 false\n";
            }

            var pass = true;
            pass &= restored.statDictionary.Count == 3;
            pass &= restored.statDictionary["attack"] == 999;
            pass &= restored.materialLibrary.TryGetValue("mat_A", out var mat) && mat == material;

            return $"[S2] Odin 序列化往返（字典 + 字典内 Material 引用）: {(pass ? "PASS" : "FAIL")}\n";
        }

        static string Scenario3_DiskPersistence(ExampleStateBankSO bank, Material material)
        {
            // S1/S2 的状态已在银行里，落盘
            AssetDatabase.SaveAssets();

            var yaml = File.ReadAllText(BankPath);
            var hasBytes = yaml.Contains("SerializedBytes");
            var hasMaterialRef = Regex.IsMatch(yaml, @"guid:\s*[0-9a-f]{32}");

            // 尽力模拟冷加载：卸载未使用资产后重读
            var bankPath = BankPath;
            Resources.UnloadUnusedAssets();
            var reloaded = AssetDatabase.LoadAssetAtPath<ExampleStateBankSO>(bankPath);
            var coldLoadOk = reloaded != null && reloaded.EntryCount >= 2;
            if (coldLoadOk)
            {
                var restored = ScriptableObject.CreateInstance<ProtoOdinExampleSO>();
                reloaded.TryRestoreState("S2/ProtoOdinExampleSO", restored);
                coldLoadOk &= restored.statDictionary["attack"] == 999;
                coldLoadOk &= restored.materialLibrary["mat_A"] == material;
            }

            var pass = hasBytes && hasMaterialRef && coldLoadOk;
            return $"[S3] 磁盘持久化（YAML含数据:{hasBytes} / 含GUID引用:{hasMaterialRef} / 冷加载恢复:{coldLoadOk}）: " +
                   $"{(pass ? "PASS" : "FAIL")}\n";
        }

        static string Scenario4_CorruptionFallback(ExampleStateBankSO bank)
        {
            var key = "S4/Corrupt";
            var instance = ScriptableObject.CreateInstance<ProtoButtonExampleSO>();
            instance.dynamicButtonName = "before corrupt";
            bank.SaveState(key, instance);

            // 注入垃圾字节，模拟版本漂移 / 数据损坏
            bank.CorruptEntryForTest(key);

            var target = ScriptableObject.CreateInstance<ProtoButtonExampleSO>();
            bool noThrow;
            bool restoredOk;
            try
            {
                restoredOk = bank.TryRestoreState(key, target);
                noThrow = true;
            }
            catch (Exception)
            {
                noThrow = false;
                restoredOk = false;
            }

            // 核心期望：校验和把垃圾字节拦下（返回 false）+ 实例保持默认态，不产生半态
            var pass = noThrow && !restoredOk && target.dynamicButtonName == "Click Me!";

            // S4b 跨类型恢复：A 类型状态灌 C 类型实例 → 类型名校验干净拒绝
            var crossKey = "S4/CrossType";
            var unityExample = ScriptableObject.CreateInstance<ProtoUnityExampleSO>();
            unityExample.dynamicButtonName = "cross type data";
            bank.SaveState(crossKey, unityExample);
            var crossTarget = ScriptableObject.CreateInstance<ProtoButtonExampleSO>();
            var crossNoThrow = true;
            bool crossRejected;
            try
            {
                crossRejected = !bank.TryRestoreState(crossKey, crossTarget);
            }
            catch (Exception)
            {
                crossNoThrow = false;
                crossRejected = false;
            }

            pass &= crossNoThrow && crossRejected && crossTarget.dynamicButtonName == "Click Me!";
            bank.RemoveState(crossKey);

            return $"[S4] 损坏/漂移容错（垃圾字节不炸:{noThrow} / 校验和拦截:{!restoredOk} / 实例保持默认态:{target.dynamicButtonName == "Click Me!"} / " +
                   $"跨类型干净拒绝:{crossRejected}）: {(pass ? "PASS" : "FAIL")}\n";
        }

        static string Scenario5_Timing(ExampleStateBankSO bank)
        {
            const int count = 123;
            var types = new[]
            {
                typeof(ProtoUnityExampleSO), typeof(ProtoOdinExampleSO), typeof(ProtoButtonExampleSO)
            };

            // 银行模式：创建 + 快照 ×123
            var sw = Stopwatch.StartNew();
            var instances = new ScriptableObject[count];
            for (var i = 0; i < count; i++)
            {
                instances[i] = ScriptableObject.CreateInstance(types[i % 3]);
                bank.SaveState($"S5/{i}", instances[i]);
            }

            sw.Stop();
            var bankSaveMs = sw.ElapsedMilliseconds;

            // 银行模式：恢复 ×123
            sw = Stopwatch.StartNew();
            var okCount = 0;
            for (var i = 0; i < count; i++)
            {
                var restored = ScriptableObject.CreateInstance(types[i % 3]);
                if (bank.TryRestoreState($"S5/{i}", restored))
                {
                    okCount++;
                }
            }

            sw.Stop();
            var bankRestoreMs = sw.ElapsedMilliseconds;

            // 清理 S5 条目，保持银行干净
            for (var i = 0; i < count; i++)
            {
                bank.RemoveState($"S5/{i}");
            }

            // 现行模式对照：CreateInstance + AddObjectToAsset ×123 + SaveAssets（临时容器）
            var containerPath = "Assets/Prototype/AttributeBankPrototype/TimingContainer.asset";
            var container = ScriptableObject.CreateInstance<ProtoUnityExampleSO>();
            container.name = "TimingContainer";
            AssetDatabase.CreateAsset(container, containerPath);
            sw = Stopwatch.StartNew();
            for (var i = 0; i < count; i++)
            {
                var sub = ScriptableObject.CreateInstance(types[i % 3]);
                sub.name = $"Sub{i}";
                AssetDatabase.AddObjectToAsset(sub, containerPath);
            }

            AssetDatabase.SaveAssets();
            sw.Stop();
            var subAssetMs = sw.ElapsedMilliseconds;
            AssetDatabase.DeleteAsset(containerPath);

            return $"[S5] 性能对照（{count} 个示例）: 银行快照={bankSaveMs}ms(含序列化) / 银行恢复={bankRestoreMs}ms / " +
                   $"现行子资产模式(CreateInstance+AddObjectToAsset+SaveAssets)={subAssetMs}ms\n";
        }

        static string Scenario6_OdinEditorDrawable(ExampleStateBankSO bank)
        {
            var instance = bank.RestoreOrCreate("S2/ProtoOdinExampleSO", typeof(ProtoOdinExampleSO));
            var editor = UnityEditor.Editor.CreateEditor(instance);
            var isOdinEditor = editor != null && editor.GetType().Name.Contains("OdinEditor");

            return $"[S6] 内存实例可被 Odin Editor 绘制（{editor?.GetType().Name}, isOdin={isOdinEditor}）: " +
                   $"{(isOdinEditor ? "PASS" : "FAIL")}\n";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Attribute Overview Ultra 的内存面板注册中心。
    /// 替代 Pro 的资产数据库：TypeCache 扫描面板类型 → CreateInstance 内存实例化 →
    /// 按需把示例替换为内存实例（状态从 UltraStateBankSO 恢复）。
    /// 全程零 AssetDatabase 写操作。
    /// </summary>
    public class UltraPanelDatabase
    {
        /// <summary>
        /// 分类展示顺序（与 Pro 窗口一致，Essentials 首位）。
        /// </summary>
        internal static readonly AesirAttributeCategory[] CategoryOrder =
        {
            AesirAttributeCategory.Essentials,
            AesirAttributeCategory.Buttons,
            AesirAttributeCategory.Collections,
            AesirAttributeCategory.Groups,
            AesirAttributeCategory.Conditionals,
            AesirAttributeCategory.Numbers,
            AesirAttributeCategory.TypeSpecifics,
            AesirAttributeCategory.Validation,
            AesirAttributeCategory.Misc,
            AesirAttributeCategory.Meta,
            AesirAttributeCategory.Unity,
            AesirAttributeCategory.Debug
        };

        readonly UltraStateBankSO _bank;

        List<AbstractAttributePanelSO> _panels;

        public UltraPanelDatabase(UltraStateBankSO bank) => _bank = bank;

        /// <summary>
        /// 已实例化的全部内存面板（未构建时为 null）。
        /// </summary>
        public IReadOnlyList<AbstractAttributePanelSO> Panels => _panels;

        /// <summary>
        /// 扫描并实例化全部面板（幂等，每窗口生命周期仅构建一次）。
        /// Initialize 由菜单构建需要中文名而提前手动调用；
        /// 首次绘制时 Odin 树会再次触发 [OnInspectorInit]，届时示例会被 SwapPanelExamplesToMemory 统一替换。
        /// </summary>
        public void BuildAllPanels()
        {
            if (_panels != null)
            {
                return;
            }

            var panelTypes = TypeCache.GetTypesDerivedFrom<AbstractAttributePanelSO>()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .ToArray();

            _panels = new List<AbstractAttributePanelSO>(panelTypes.Length);
            foreach (var type in panelTypes)
            {
                var panel = (AbstractAttributePanelSO)ScriptableObject.CreateInstance(type);
                panel.Initialize();
                _panels.Add(panel);
            }
        }

        /// <summary>
        /// 获取面板所属分类；无标记返回 None。
        /// </summary>
        public static AesirAttributeCategory CategoryOf(AbstractAttributePanelSO panel) =>
            panel.GetType().GetCustomAttribute<AttributeCategoryAttribute>()?.Category ??
            AesirAttributeCategory.None;

        /// <summary>
        /// 面板是否属于指定分类（Flags 语义，与 Pro 的 FilterPanels 一致）。
        /// </summary>
        public static bool MatchesCategory(AbstractAttributePanelSO panel, AesirAttributeCategory category)
        {
            var attr = panel.GetType().GetCustomAttribute<AttributeCategoryAttribute>();
            return attr != null && attr.Category.HasFlagFast(category);
        }

        /// <summary>
        /// 把面板的示例预览项替换为内存实例，并按银行记录恢复面板选中的示例。
        /// 必须在面板的 [OnInspectorInit]（SetData 完成后）之后调用——窗口在 DrawEditor 首帧处触发。
        /// </summary>
        public void SwapPanelExamplesToMemory(AbstractAttributePanelSO panel)
        {
            var items = panel.ExamplePreviewItemsForUltra;
            if (items == null || items.Length == 0)
            {
                return;
            }

            ScriptableObject firstMemory = null;
            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }

                var current = item.ExampleType == AttributeExampleType.UnitySerialized
                    ? (ScriptableObject)item.UnitySerializedExample
                    : item.OdinSerializedExample;
                if (current == null)
                {
                    continue;
                }

                var exampleType = current.GetType();
                var memory = _bank.RestoreOrCreateExample(exampleType.Name, exampleType);
                if (item.ExampleType == AttributeExampleType.UnitySerialized)
                {
                    item.InitializeUnitySerializedExample(item.ItemName, memory);
                }
                else
                {
                    item.InitializeOdinSerializedExample(item.ItemName, (SerializedScriptableObject)memory);
                }

                firstMemory ??= memory;
            }

            if (firstMemory == null)
            {
                return;
            }

            // 恢复上次选中：按记录的类型名匹配预览项
            if (_bank.TryGetPanelSelection(panel.GetType().Name, out var selectedTypeName))
            {
                foreach (var item in items)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    var example = item.ExampleType == AttributeExampleType.UnitySerialized
                        ? (ScriptableObject)item.UnitySerializedExample
                        : item.OdinSerializedExample;
                    if (example != null && example.GetType().Name == selectedTypeName)
                    {
                        panel.CurrentSelectedExample = example;
                        return;
                    }
                }
            }

            panel.CurrentSelectedExample = firstMemory;
        }

        /// <summary>
        /// 判断面板首个示例是否仍是资产引用（说明 [OnInspectorInit] 重跑替换了内存实例，需要重新替换）。
        /// </summary>
        public static bool PanelNeedsExampleSwap(AbstractAttributePanelSO panel)
        {
            var items = panel.ExamplePreviewItemsForUltra;
            if (items == null || items.Length == 0)
            {
                return false;
            }

            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }

                var example = item.ExampleType == AttributeExampleType.UnitySerialized
                    ? (ScriptableObject)item.UnitySerializedExample
                    : item.OdinSerializedExample;
                return example != null && AssetDatabase.Contains(example);
            }

            return false;
        }

        /// <summary>
        /// 释放全部内存面板的语言订阅并销毁实例（窗口关闭时调用，防止静态事件持有已卸载对象）。
        /// </summary>
        public void ReleaseAll()
        {
            if (_panels == null)
            {
                return;
            }

            foreach (var panel in _panels)
            {
                if (panel == null)
                {
                    continue;
                }

                panel.ReleaseLanguageSubscription();
                UnityEngine.Object.DestroyImmediate(panel);
            }

            _panels = null;
        }
    }
}

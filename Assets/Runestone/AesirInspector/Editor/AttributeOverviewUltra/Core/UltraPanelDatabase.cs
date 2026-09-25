using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Attribute Overview Ultra 的内存面板注册中心。
    /// 替代 Pro 时代的面板资产集合（Pro 已移除）：TypeCache 扫描面板类型 → CreateInstance 内存实例化；
    /// 示例由各 Data 构造器经 UltraStateStoreSO 状态存储路由取得内存单例（含用户数据恢复）。
    /// 目录结构（分类/显示名/排序）由 AesirAttributeRegistry 从 Odin 官方注册表提供。
    /// 全程零 AssetDatabase 写操作。
    /// </summary>
    public class UltraPanelDatabase
    {
        readonly UltraStateStoreSO _stateStore;

        List<AbstractAttributePanelSO> _panels;

        public UltraPanelDatabase(UltraStateStoreSO stateStore) => _stateStore = stateStore;

        /// <summary>
        /// 已实例化的全部内存面板（未构建时为 null）。
        /// </summary>
        public IReadOnlyList<AbstractAttributePanelSO> Panels => _panels;

        /// <summary>
        /// 扫描并实例化全部面板（幂等，每窗口生命周期仅构建一次）。
        /// Initialize 由菜单构建需要中文名而提前手动调用；
        /// [OnInspectorInit] 重跑会把面板选中重置为初始示例，由窗口在 DrawEditor 的
        /// Layout 开头经 RestorePanelSelection 按状态存储记录恢复。
        /// </summary>
        public void BuildAllPanels()
        {
            if (_panels != null)
            {
                return;
            }

            var panelTypes = TypeCache.GetTypesDerivedFrom<AbstractAttributePanelSO>()
                .Where(t => !t.IsAbstract && !t.IsInterface).ToArray();

            _panels = new List<AbstractAttributePanelSO>(panelTypes.Length);
            foreach (var type in panelTypes)
            {
                var panel = (AbstractAttributePanelSO)ScriptableObject.CreateInstance(type);
                panel.Initialize();
                _panels.Add(panel);
            }
        }

        /// <summary>
        /// 示例 SO 的 Instance 后端已直接经 UltraStateStoreSO 状态存储路由返回内存单例（含数据恢复），
        /// 面板无需再替换示例引用。但 [OnInspectorInit] 重跑时 Internal_SetData 会把面板选中重置为初始示例，
        /// 因此选中恢复由本方法承担：状态存储有记录且与当前选中不一致时，恢复为记录的示例。
        /// </summary>
        public bool PanelSelectionNeedsRestore(AbstractAttributePanelSO panel)
        {
            if (!_stateStore.TryGetPanelSelection(panel.GetType().Name, out var selectedTypeName))
            {
                return false;
            }

            var current = panel.CurrentSelectedExample;
            return current == null || current.GetType().Name != selectedTypeName;
        }

        /// <summary>
        /// 按状态存储记录恢复面板选中的示例（无记录或已一致时为空操作）。
        /// 属于状态变更，只允许在 DrawEditor 的 Layout 事件开头调用，遵守 IMGUI 两遍布局契约。
        /// 记录指向的示例已不在本面板（示例被移除或改名）时丢弃该记录：
        /// 否则 <see cref="PanelSelectionNeedsRestore" /> 恒为 true，窗口会陷入每帧重绘。
        /// </summary>
        public void RestorePanelSelection(AbstractAttributePanelSO panel)
        {
            if (!PanelSelectionNeedsRestore(panel) ||
                !_stateStore.TryGetPanelSelection(panel.GetType().Name, out var selectedTypeName))
            {
                return;
            }

            var items = panel.ExamplePreviewItemsForUltra;
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    var example = item.ExampleType == AttributeExampleType.UnitySerialized
                        ? item.UnitySerializedExample
                        : item.OdinSerializedExample;
                    if (example != null && example.GetType().Name == selectedTypeName)
                    {
                        panel.CurrentSelectedExample = example;
                        return;
                    }
                }
            }

            _stateStore.RemovePanelSelection(panel.GetType().Name);
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
                Object.DestroyImmediate(panel);
            }

            _panels = null;
        }
    }
}

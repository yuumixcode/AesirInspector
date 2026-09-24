using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Attribute Overview Ultra 窗口。
    /// 与 Pro 的根本差异：面板与示例均为 CreateInstance 内存实例，
    /// 用户调试状态通过 UltraStateBankSO 快照持久化，Project 中零子资产。
    /// </summary>
    public class AttributeOverviewUltraWindow : OdinMenuEditorWindow
    {
        /// <summary>
        /// 右侧内容区最小宽度：保证参数表格（最小列宽合计 540px）不被迫压窄。
        /// 窗口更窄时右侧所有介绍内容作为整体保持该宽度，
        /// 由 OdinEditorWindow 自带的整体 ScrollView 出横向滚动条——单一滚动条，内部块不各自滚动。
        /// </summary>
        const float MinContentWidth = 560f;

        /// <summary>
        /// 菜单列常规宽度（可拖拽调整的实际存储值）。
        /// </summary>
        const float NormalMenuWidth = 230f;

        /// <summary>
        /// 菜单列宽度下限：极窄窗口下菜单收缩到此值，剩余宽度全部让给内容区。
        /// </summary>
        const float MinMenuWidth = 150f;

        UltraStateBankSO _bank;
        UltraPanelDatabase _database;
        float _menuWidth = NormalMenuWidth;

        /// <summary>
        /// 右侧内容整体宽度缓存。只在 Layout 事件重算，Repaint 等其余事件复用，
        /// 保证 IMGUI 两遍布局契约（两遍间布局参数必须一致）。
        /// </summary>
        float _contentWidthCache = MinContentWidth;

        /// <summary>
        /// 已订阅示例选中事件的面板集合（DrawEditor 首帧触发，幂等防护）。
        /// </summary>
        readonly HashSet<AbstractAttributePanelSO> _swappedPanels =
            new HashSet<AbstractAttributePanelSO>();

        [MenuItem(AesirInspectorMenuItems.AttributeOverviewUltra, false,
            AesirInspectorMenuItems.AttributeOverviewUltraOrder)]
        public static void OpenWindow()
        {
            var window = GetWindow<AttributeOverviewUltraWindow>(
                AesirInspectorMenuItems.AttributeOverviewUltraWindowName);
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1050, 750);
            window.Show();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _bank = UltraStateBankSO.LoadOrCreateBank();
            _database = new UltraPanelDatabase(_bank);
            WindowPadding = new Vector4(15, 15, 15, 5);

            // 窗口允许自由缩窄：内容区不足 MinContentWidth 时由整体横向滚动兜底，
            // 菜单宽度由 MenuWidth getter 自动收缩到 MinMenuWidth
            minSize = new Vector2(MinMenuWidth + 30f + 240f, 320f);

            AesirInspectorLanguageSettingsSO.LanguageChanged -= OnLanguageChanged;
            AesirInspectorLanguageSettingsSO.LanguageChanged += OnLanguageChanged;
        }

        /// <summary>
        /// 菜单列宽度：可自由拖拽（真 setter），下限 MinMenuWidth；
        /// 极窄窗口下内容区由整体横向滚动兜底。
        /// </summary>
        public override float MenuWidth
        {
            get
            {
                var maxMenu = Mathf.Max(MinMenuWidth, position.width - 30f - MinContentWidth);
                return Mathf.Clamp(_menuWidth, MinMenuWidth, maxMenu);
            }
            set => _menuWidth = value;
        }

        /// <summary>
        /// 允许拖拽分隔条调整菜单宽度。
        /// </summary>
        public override bool ResizableMenuWidth
        {
            get => true;
            set { }
        }

        protected override void OnDisable()
        {
            AesirInspectorLanguageSettingsSO.LanguageChanged -= OnLanguageChanged;
            SnapshotAll();
            base.OnDisable();
            _database?.ReleaseAll();
            _swappedPanels.Clear();
        }

        void OnLanguageChanged()
        {
            // 面板内部渲染器已各自订阅并刷新内容；窗口层只需请求重绘使其立即可见
            Repaint();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            _database.BuildAllPanels();

            var tree = new OdinMenuTree
            {
                Config =
                {
                    DrawSearchToolbar = true,
                    SearchTerm = ""
                },
                DefaultMenuStyle = new OdinMenuStyle
                {
                    Height = 24
                }
            };

            tree.Config.SearchFunction = menuItem =>
            {
                var str = menuItem.Name.ToLower().Replace(" ", "");
                var searchStr = tree.Config.SearchTerm.ToLower().Replace(" ", "");
                return str.Contains(searchStr);
            };

            if (_database.Panels == null)
            {
                return tree;
            }

            // 目录结构（分类归属/多分类/显示名/顺序）复刻 Odin 官方注册表：
            // 分类按官方 CategoryComparer 权重排序（同级字母序），分类内保持官方注册表顺序。
            var entries = AesirAttributeRegistry.BuildMenuEntries(_database.Panels);

            foreach (var categoryGroup in entries
                         .GroupBy(e => e.Category)
                         .OrderBy(g => AesirAttributeRegistry.GetCategorySortOrder(g.Key))
                         .ThenBy(g => g.Key, StringComparer.Ordinal))
            {
                foreach (var entry in categoryGroup)
                {
                    tree.AddObjectAtPath(entry.Category + "/" + entry.DisplayName, entry.Panel);
                }
            }

            return tree;
        }

        protected override void DrawEditor(int index)
        {
            var targets = CurrentDrawingTargets;
            if (targets != null && index < targets.Count &&
                targets[index] is AbstractAttributePanelSO panel)
            {
                if (Event.current.type == EventType.Layout)
                {
                    // 首帧登记订阅（幂等）；[OnInspectorInit] 重跑会把面板选中重置为初始示例，
                    // 每个布局周期开头按银行记录恢复——本次 Layout 与配对 Repaint 看到同一状态，
                    // 遵守 IMGUI 布局契约（两遍之间换状态会报 "Getting control N's position"）
                    if (_swappedPanels.Add(panel))
                    {
                        SubscribePanel(panel);
                    }

                    _database.RestorePanelSelection(panel);
                }
                else if (_database.PanelSelectionNeedsRestore(panel))
                {
                    // Repaint 等事件中检测到需恢复（上个 Layout 内被 [OnInspectorInit] 重置）：
                    // 绝不在两遍之间变更状态——保持一致绘制，排队下一周期再恢复
                    Repaint();
                }
            }

            // 右侧所有介绍内容作为一个整体计算宽度：
            // 空间富足 = max(最小占位, 视口宽) → 内容贴满可视区、无横向滚动条；
            // 窗口小于最小占位 → 内容保持最小宽，由 OdinEditorWindow 自带的整体 ScrollView
            // 出唯一一条横向滚动条（滚动的是整个右侧，而不是使用提示等单个块各自滚动）。
            // 只在 Layout 事件重算：position/MenuWidth 在两遍间必须取同一值，否则布局树错乱
            if (Event.current.type == EventType.Layout)
            {
                // 46 = WindowPadding 左右 15+15 + 竖滚动条 16（内容普遍超一屏）
                _contentWidthCache = Mathf.Max(MinContentWidth, position.width - MenuWidth - 46f);
            }

            GUILayout.BeginVertical(GUILayout.Width(_contentWidthCache));
            base.DrawEditor(index);
            GUILayout.EndVertical();
        }

        void SubscribePanel(AbstractAttributePanelSO panel)
        {
            panel.ExampleSelectionChanged -= OnPanelExampleSelectionChanged;
            panel.ExampleSelectionChanged += OnPanelExampleSelectionChanged;
        }

        void OnPanelExampleSelectionChanged(ScriptableObject selected)
        {
            // 切换示例时立即快照该面板，防止未关窗异常丢失调试状态
            var panel = _swappedPanels.FirstOrDefault(p =>
                p != null && p.CurrentSelectedExample == selected);
            if (panel != null)
            {
                SnapshotPanel(panel);
            }
        }

        /// <summary>
        /// 快照单个面板：选中记录 + 全部内存示例状态。
        /// </summary>
        void SnapshotPanel(AbstractAttributePanelSO panel)
        {
            if (panel == null)
            {
                return;
            }

            _bank.SavePanelSelection(panel.GetType().Name,
                panel.CurrentSelectedExample != null
                    ? panel.CurrentSelectedExample.GetType().Name
                    : null);

            var items = panel.ExamplePreviewItemsForUltra;
            if (items == null)
            {
                return;
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
                // 全部示例均为银行路由的内存实例，直接快照
                if (example != null)
                {
                    _bank.SaveExampleState(example.GetType().Name, example);
                }
            }
        }

        void SnapshotAll()
        {
            if (_bank == null || _database?.Panels == null)
            {
                return;
            }

            var anyChange = false;
            foreach (var panel in _database.Panels)
            {
                if (panel == null)
                {
                    continue;
                }

                panel.ExampleSelectionChanged -= OnPanelExampleSelectionChanged;
                if (_swappedPanels.Contains(panel))
                {
                    SnapshotPanel(panel);
                    anyChange = true;
                }
            }

            if (anyChange)
            {
                try
                {
                    AssetDatabase.SaveAssets();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[AttributeOverviewUltra] 快照落盘失败: {e.Message}");
                }
            }
        }
    }
}

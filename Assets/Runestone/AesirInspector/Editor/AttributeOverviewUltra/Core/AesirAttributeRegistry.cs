using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Examples;
using Sirenix.Utilities;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Ultra 菜单的一个条目：面板实例 + 所属分类 + 显示名。
    /// 同一个面板可因官方多分类注册而出现多个条目（如 ButtonGroup 同时在 Buttons 与 Groups）。
    /// </summary>
    internal readonly struct AesirAttributeMenuEntry
    {
        public readonly string Category;

        public readonly AbstractAttributePanelSO Panel;

        public readonly string DisplayName;

        public AesirAttributeMenuEntry(string category, AbstractAttributePanelSO panel, string displayName)
        {
            Category = category;
            Panel = panel;
            DisplayName = displayName;
        }
    }

    /// <summary>
    /// Aesir 面板 ↔ Odin 官方特性注册表的映射与目录结构数据源。
    /// 分类归属、显示名、排序规则全部取自 Odin 官方注册表（AttributeExampleUtilities）与
    /// 官方 AttributesExampleWindow 的实现，保证 Ultra 左侧目录结构与官方逐字一致，且随 Odin 升级零维护。
    /// </summary>
    internal static class AesirAttributeRegistry
    {
        /// <summary>
        /// Aesir 自有特性分类名（Odin 官方注册表之外的特性，如双语特性）。
        /// </summary>
        public const string AesirCustomsCategory = "Aesir Customs";

        /// <summary>
        /// 分类排序权重，复刻 Odin 官方 AttributesExampleWindow.CategoryComparer.Order。
        /// 未列出的分类权重为 0，同级按分类名字母序。
        /// Aesir Customs 置顶（官方注册表之外的自家特性优先展示）。
        /// </summary>
        static readonly Dictionary<string, int> CategorySortOrder = new Dictionary<string, int>
        {
            { AesirCustomsCategory, -20 },
            { "Essentials", -10 },
            { "Misc", 8 },
            { "Meta", 9 },
            { "Unity", 10 },
            { "Debug", 50 }
        };

        /// <summary>
        /// Aesir 面板类型 → Odin 特性类型。目录结构（分类、显示名）以官方注册表为准。
        /// 未映射的面板归入 <see cref="AesirCustomsCategory"/>。
        /// </summary>
        static readonly Dictionary<Type, Type> PanelToOdinAttribute = new Dictionary<Type, Type>
        {
            { typeof(AssetListAttributePanelSO), typeof(AssetListAttribute) },
            { typeof(AssetSelectorAttributePanelSO), typeof(AssetSelectorAttribute) },
            { typeof(AssetsOnlyAttributePanelSO), typeof(AssetsOnlyAttribute) },
            { typeof(BoxGroupAttributePanelSO), typeof(BoxGroupAttribute) },
            { typeof(ButtonAttributePanelSO), typeof(ButtonAttribute) },
            { typeof(ButtonGroupAttributePanelSO), typeof(ButtonGroupAttribute) },
            { typeof(ChildGameObjectOnlyAttributePanelSO), typeof(ChildGameObjectsOnlyAttribute) },
            { typeof(ColorPaletteAttributePanelSO), typeof(ColorPaletteAttribute) },
            { typeof(CustomContextMenuAttributePanelSO), typeof(CustomContextMenuAttribute) },
            { typeof(CustomValueDrawerPanelSO), typeof(CustomValueDrawerAttribute) },
            { typeof(DelayedPropertyAttributePanelSO), typeof(DelayedPropertyAttribute) },
            { typeof(DetailInfoBoxAttributePanelSO), typeof(DetailedInfoBoxAttribute) },
            { typeof(DictionaryDrawerSettingsAttributePanelSO), typeof(DictionaryDrawerSettings) },
            { typeof(DisableContextMenuAttributePanelSO), typeof(DisableContextMenuAttribute) },
            { typeof(DisableIfAttributePanelSO), typeof(DisableIfAttribute) },
            { typeof(DisableInAttributePanelSO), typeof(DisableInAttribute) },
            { typeof(DisableInEditorModeAttributePanelSO), typeof(DisableInEditorModeAttribute) },
            { typeof(DisableInInlineEditorsAttributePanelSO), typeof(DisableInInlineEditorsAttribute) },
            { typeof(DisableInPlayModeAttributePanelSO), typeof(DisableInPlayModeAttribute) },
            { typeof(DisallowModificationsInAttributePanelSO), typeof(DisallowModificationsInAttribute) },
            { typeof(DisplayAsStringAttributePanelSO), typeof(DisplayAsStringAttribute) },
            { typeof(DrawWithUnityAttributePanelSO), typeof(DrawWithUnityAttribute) },
            { typeof(EnableGUIAttributePanelSO), typeof(EnableGUIAttribute) },
            { typeof(EnableIfAttributePanelSO), typeof(EnableIfAttribute) },
            { typeof(EnableInAttributePanelSO), typeof(EnableInAttribute) },
            { typeof(EnumPagingAttributePanelSO), typeof(EnumPagingAttribute) },
            { typeof(EnumToggleButtonsAttributePanelSO), typeof(EnumToggleButtonsAttribute) },
            { typeof(FilePathAttributePanelSO), typeof(FilePathAttribute) },
            { typeof(FolderPathAttributePanelSO), typeof(FolderPathAttribute) },
            { typeof(FoldoutGroupAttributePanelSO), typeof(FoldoutGroupAttribute) },
            { typeof(GUIColorAttributePanelSO), typeof(GUIColorAttribute) },
            { typeof(HideDuplicateReferenceBoxAttributePanelSO), typeof(HideDuplicateReferenceBoxAttribute) },
            { typeof(HideIfAttributePanelSO), typeof(HideIfAttribute) },
            { typeof(HideIfGroupAttributePanelSO), typeof(HideIfGroupAttribute) },
            { typeof(HideInAttributePanelSO), typeof(HideInAttribute) },
            { typeof(HideInEditorModeAttributePanelSO), typeof(HideInEditorModeAttribute) },
            { typeof(HideInInlineEditorsAttributePanelSO), typeof(HideInInlineEditorsAttribute) },
            { typeof(HideInPlayModeAttributePanelSO), typeof(HideInPlayModeAttribute) },
            { typeof(HideInTablesAttributePanelSO), typeof(HideInTablesAttribute) },
            { typeof(HideLabelAttributePanelSO), typeof(HideLabelAttribute) },
            { typeof(HideMonoScriptAttributePanelSO), typeof(HideMonoScriptAttribute) },
            { typeof(HideNetworkBehaviourFieldsAttributePanelSO), typeof(HideNetworkBehaviourFieldsAttribute) },
            { typeof(HideReferenceObjectPickerAttributePanelSO), typeof(HideReferenceObjectPickerAttribute) },
            { typeof(HorizontalGroupAttributePanelSO), typeof(HorizontalGroupAttribute) },
            { typeof(ImageAttributePanelSO), typeof(ImageAttribute) },
            { typeof(IndentAttributePanelSO), typeof(IndentAttribute) },
            { typeof(InfoBoxAttributePanelSO), typeof(InfoBoxAttribute) },
            { typeof(InlineButtonAttributePanelSO), typeof(InlineButtonAttribute) },
            { typeof(InlineEditorAttributePanelSO), typeof(InlineEditorAttribute) },
            { typeof(InlinePropertyAttributePanelSO), typeof(InlinePropertyAttribute) },
            { typeof(LabelTextAttributePanelSO), typeof(LabelTextAttribute) },
            { typeof(LabelWidthAttributePanelSO), typeof(LabelWidthAttribute) },
            { typeof(ListDrawerSettingsAttributePanelSO), typeof(ListDrawerSettingsAttribute) },
            { typeof(MaxValueAttributePanelSO), typeof(MaxValueAttribute) },
            { typeof(MinMaxSliderAttributePanelSO), typeof(MinMaxSliderAttribute) },
            { typeof(MinValueAttributePanelSO), typeof(MinValueAttribute) },
            { typeof(MultiLinePropertyAttributePanelSO), typeof(MultiLinePropertyAttribute) },
            { typeof(MultilineAttributePanelSO), typeof(UnityEngine.MultilineAttribute) },
            { typeof(OnCollectionChangedAttributePanelSO), typeof(OnCollectionChangedAttribute) },
            { typeof(OnInspectorDisposeAttributePanelSO), typeof(OnInspectorDisposeAttribute) },
            { typeof(OnInspectorGUIAttributePanelSO), typeof(OnInspectorGUIAttribute) },
            { typeof(OnInspectorInitAttributePanelSO), typeof(OnInspectorInitAttribute) },
            { typeof(OnStateUpdateAttributePanelSO), typeof(OnStateUpdateAttribute) },
            { typeof(OnValueChangedAttributePanelSO), typeof(OnValueChangedAttribute) },
            { typeof(PolymorphicDrawerSettingsAttributePanelSO), typeof(PolymorphicDrawerSettingsAttribute) },
            { typeof(PreviewFieldAttributePanelSO), typeof(PreviewFieldAttribute) },
            { typeof(ProgressBarAttributePanelSO), typeof(ProgressBarAttribute) },
            { typeof(PropertyOrderAttributePanelSO), typeof(PropertyOrderAttribute) },
            { typeof(PropertyRangeAttributePanelSO), typeof(PropertyRangeAttribute) },
            { typeof(PropertySpaceAttributePanelSO), typeof(PropertySpaceAttribute) },
            { typeof(PropertyTooltipAttributePanelSO), typeof(PropertyTooltipAttribute) },
            { typeof(RangeAttributePanelSO), typeof(UnityEngine.RangeAttribute) },
            { typeof(ReadOnlyAttributePanelSO), typeof(ReadOnlyAttribute) },
            { typeof(RequiredAttributePanelSO), typeof(RequiredAttribute) },
            { typeof(RequiredInAttributePanelSO), typeof(RequiredInAttribute) },
            { typeof(RequiredListLengthAttributePanelSO), typeof(RequiredListLengthAttribute) },
            { typeof(ResponsiveButtonGroupAttributePanelSO), typeof(ResponsiveButtonGroupAttribute) },
            { typeof(SceneObjectsOnlyAttributePanelSO), typeof(SceneObjectsOnlyAttribute) },
            { typeof(SearchableAttributePanelSO), typeof(SearchableAttribute) },
            { typeof(ShowDrawerChainAttributePanelSO), typeof(ShowDrawerChainAttribute) },
            { typeof(ShowIfAttributePanelSO), typeof(ShowIfAttribute) },
            { typeof(ShowIfGroupAttributePanelSO), typeof(ShowIfGroupAttribute) },
            { typeof(ShowInAttributePanelSO), typeof(ShowInAttribute) },
            { typeof(ShowInInlineEditorsAttributePanelSO), typeof(ShowInInlineEditorsAttribute) },
            { typeof(ShowInInspectorAttributePanelSO), typeof(ShowInInspectorAttribute) },
            { typeof(ShowPropertyResolverAttributePanelSO), typeof(ShowPropertyResolverAttribute) },
            { typeof(SpaceAttributePanelSO), typeof(UnityEngine.SpaceAttribute) },
            { typeof(SuffixLabelAttributePanelSO), typeof(SuffixLabelAttribute) },
            { typeof(SuppressInvalidAttributeErrorAttributePanelSO), typeof(SuppressInvalidAttributeErrorAttribute) },
            { typeof(TabGroupAttributePanelSO), typeof(TabGroupAttribute) },
            { typeof(TableColumnWidthAttributePanelSO), typeof(TableColumnWidthAttribute) },
            { typeof(TableListAttributePanelSO), typeof(TableListAttribute) },
            { typeof(TableMatrixAttributePanelSO), typeof(TableMatrixAttribute) },
            { typeof(TextAreaAttributePanelSO), typeof(UnityEngine.TextAreaAttribute) },
            { typeof(TitleAttributePanelSO), typeof(TitleAttribute) },
            { typeof(TitleGroupAttributePanelSO), typeof(TitleGroupAttribute) },
            { typeof(ToggleAttributePanelSO), typeof(ToggleAttribute) },
            { typeof(ToggleGroupAttributePanelSO), typeof(ToggleGroupAttribute) },
            { typeof(ToggleLeftAttributePanelSO), typeof(ToggleLeftAttribute) },
            { typeof(TypeDrawerSettingsAttributePanelSO), typeof(TypeDrawerSettingsAttribute) },
            { typeof(TypeFilterAttributePanelSO), typeof(TypeFilterAttribute) },
            { typeof(TypeInfoBoxAttributePanelSO), typeof(TypeInfoBoxAttribute) },
            { typeof(TypeRegistryItemAttributePanelSO), typeof(TypeRegistryItemAttribute) },
            { typeof(TypeSelectorSettingsAttributePanelSO), typeof(TypeSelectorSettingsAttribute) },
            { typeof(UnitAttributePanelSO), typeof(UnitAttribute) },
            { typeof(ValidateInputAttributePanelSO), typeof(ValidateInputAttribute) },
            { typeof(ValueDropdownAttributePanelSO), typeof(ValueDropdownAttribute) },
            { typeof(VerticalGroupAttributePanelSO), typeof(VerticalGroupAttribute) },
            { typeof(WrapAttributePanelSO), typeof(WrapAttribute) }
        };

        /// <summary>
        /// 官方特性类型 → Aesir 面板类型（映射表反查，供按官方注册表顺序遍历使用）。
        /// </summary>
        static readonly Dictionary<Type, Type> OdinAttributeToPanel =
            PanelToOdinAttribute.ToDictionary(pair => pair.Value, pair => pair.Key);

        /// <summary>
        /// 构建菜单条目（按 Odin 官方注册表顺序，支持一个面板多分类）。
        /// </summary>
        public static List<AesirAttributeMenuEntry> BuildMenuEntries(
            IReadOnlyList<AbstractAttributePanelSO> panels)
        {
            var panelByType = new Dictionary<Type, AbstractAttributePanelSO>();
            if (panels != null)
            {
                foreach (var panel in panels)
                {
                    if (panel != null)
                    {
                        panelByType[panel.GetType()] = panel;
                    }
                }
            }

            var entries = new List<AesirAttributeMenuEntry>();
            var mappedPanelTypes = new HashSet<Type>();

            // 1) 官方注册表顺序（分类内顺序即官方顺序）
            foreach (var attributeType in AttributeExampleUtilities.GetAllOdinAttributes())
            {
                if (!OdinAttributeToPanel.TryGetValue(attributeType, out var panelType) ||
                    !panelByType.TryGetValue(panelType, out var panel))
                {
                    continue;
                }

                mappedPanelTypes.Add(panelType);
                var displayName = GetOfficialDisplayName(attributeType);

                foreach (var category in AttributeExampleUtilities.GetAttributeCategories(attributeType))
                {
                    entries.Add(new AesirAttributeMenuEntry(category, panel, displayName));
                }
            }

            // 2) Aesir 自有特性（无官方注册）→ Aesir Customs（按显示名字母序，风格与官方分类一致）
            var customEntries = new List<AesirAttributeMenuEntry>();
            foreach (var pair in panelByType)
            {
                if (mappedPanelTypes.Contains(pair.Key))
                {
                    continue;
                }

                customEntries.Add(new AesirAttributeMenuEntry(AesirCustomsCategory, pair.Value,
                    GetCustomPanelDisplayName(pair.Key)));
            }

            customEntries.Sort((a, b) => string.CompareOrdinal(a.DisplayName, b.DisplayName));
            entries.AddRange(customEntries);

            return entries;
        }

        /// <summary>
        /// 分类排序权重（复刻官方 CategoryComparer，未列出为 0）。
        /// </summary>
        public static int GetCategorySortOrder(string category) =>
            CategorySortOrder.TryGetValue(category, out var order) ? order : 0;

        /// <summary>
        /// 官方显示名规则：GetNiceName → 去掉 "Attribute" → 拆 PascalCase。
        /// 与 Odin 官方 AttributesExampleWindow 的实现完全一致（如 GUIColor 保持连写）。
        /// </summary>
        static string GetOfficialDisplayName(Type attributeType) =>
            attributeType.GetNiceName().Replace("Attribute", "").SplitPascalCase();

        /// <summary>
        /// Aesir 自有面板的显示名：去掉面板类后缀后拆 PascalCase（与官方风格一致）。
        /// </summary>
        static string GetCustomPanelDisplayName(Type panelType)
        {
            var name = panelType.Name;
            const string attributePanelSuffix = "AttributePanelSO";
            const string panelSuffix = "PanelSO";

            if (name.EndsWith(attributePanelSuffix, StringComparison.Ordinal))
            {
                name = name.Substring(0, name.Length - attributePanelSuffix.Length);
            }
            else if (name.EndsWith(panelSuffix, StringComparison.Ordinal))
            {
                name = name.Substring(0, name.Length - panelSuffix.Length);
            }

            return name.SplitPascalCase();
        }
    }
}

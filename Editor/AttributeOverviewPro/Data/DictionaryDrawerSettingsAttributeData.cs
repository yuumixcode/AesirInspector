using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// DictionaryDrawerSettings 特性的介绍数据。
    /// </summary>
    internal class DictionaryDrawerSettingsAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("DictionaryDrawerSettings", "DictionaryDrawerSettings",
                "DictionaryDrawerSettings 特性用于自定义字典（Dictionary）在 Inspector 中的绘制方式。",
                "The DictionaryDrawerSettings attribute is used to customize how dictionaries are drawn in the Inspector.",
                OdinInspectorDocumentationLinks.DictionaryDrawerSettingsUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("用于自定义字典在 Inspector 中的绘制；字典必须使用 Odin 序列化（[OdinSerialize]）才能在检视器中编辑。", "Customizes how a dictionary is drawn in the Inspector; the dictionary must use Odin serialization ([OdinSerialize]) to be editable there."),
            new BilingualData("KeyLabel / ValueLabel 自定义键值列标签（默认 \"Key\" / \"Value\"），KeyColumnWidth 设置键列宽度（默认 130）。", "KeyLabel / ValueLabel override the key and value column labels (default \"Key\" / \"Value\"), and KeyColumnWidth sets the key column width (default 130)."),
            new BilingualData("DisplayMode 控制绘制方式（OneLine、Foldout、CollapsedFoldout、ExpandedFoldout），IsReadOnly 可禁止在面板中增删条目。", "DisplayMode controls the drawing style (OneLine, Foldout, CollapsedFoldout, ExpandedFoldout), and IsReadOnly prevents adding or removing entries in the Inspector.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "KeyLabel",
                new BilingualData("键列的标签文本。", "The label for the key column.")),
            new ParameterValue(typeof(string).FullName, "ValueLabel",
                new BilingualData("值列的标签文本。", "The label for the value column.")),
            new ParameterValue(typeof(float).FullName, "KeyColumnWidth",
                new BilingualData("键列的宽度。", "The width of the key column.")),
            new ParameterValue(typeof(DictionaryDisplayOptions).FullName, "DisplayMode",
                new BilingualData("字典的显示模式。", "The display mode for the dictionary.")),
            new ParameterValue(typeof(bool).FullName, "IsReadOnly",
                new BilingualData("是否为只读模式。", "Whether the dictionary is read-only."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeOdinSerializedExample("Basic Usage",
                DictionaryDrawerSettingsExampleSO.Instance)
        };
    }
}

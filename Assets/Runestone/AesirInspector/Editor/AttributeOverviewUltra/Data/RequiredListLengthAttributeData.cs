namespace Runestone.AesirInspector.Editor
{
    internal class RequiredListLengthAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("RequiredListLength", "RequiredListLength",
                "RequiredListLength 特性用于限制列表的最小和/或最大长度。",
                "The RequiredListLength attribute is used to restrict the minimum and/or maximum length of a list.");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("仅适用于实现 IList 的集合（数组 / List）；字典等非列表集合不会被校验。",
                "Only applies to collections implementing IList (arrays / List); non-list collections such as dictionaries are not validated."),
            new BilingualData("单个数值参数表示固定长度（同时作为最小和最大值）；两个参数分别指定最小 / 最大长度，传 null 可跳过某一侧限制。",
                "A single numeric argument sets a fixed length (used as both minimum and maximum); two arguments set the minimum and maximum separately, and passing null skips that side of the restriction."),
            new BilingualData("长度也支持 $ 成员引用或 @ 表达式动态解析（如 \"@this.otherList.Count\"）。",
                "The length can also be resolved dynamically from a $ member reference or an @ expression (such as \"@this.otherList.Count\")."),
            new BilingualData("校验失败时提供一键修正列表长度的按钮；列表本身为 null 时会报告 \"is required\"。",
                "When validation fails it offers a one-click button to fix the list length; a null list itself reports \"is required\".")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = null;

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = null;

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                RequiredListLengthExampleSO.Instance)
        };
    }
}

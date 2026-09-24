using System;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HideInTables 特性的介绍数据。
    /// </summary>
    internal class HideInTablesAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("HideInTables", "HideInTables",
                "HideInTables 特性用于阻止成员在 TableList 绘制的表格中显示为列。",
                "The HideInTables attribute prevents a member from showing up as a column in tables drawn by TableList.",
                "https://odininspector.com/attributes/hide-in-tables-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("无参数，作用于成员本身。",
                "Takes no parameters and applies to the member itself."),
            new BilingualData("仅在 TableList 表格中生效，普通 Inspector 绘制不受影响。",
                "Only takes effect inside TableList tables; regular inspector drawing is unaffected.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = Array.Empty<ParameterValue>();

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
            Array.Empty<ResolvedStringParameterValue>();

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Hide In Tables",
                HideInTablesExampleSO.Instance)
        };
    }
}

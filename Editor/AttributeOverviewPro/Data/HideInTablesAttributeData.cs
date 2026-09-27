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
            new BilingualData("无参数，直接标注在成员上，仅在 TableList 绘制的表格中生效。",
                "Takes no parameters, is applied directly to a member, and only takes effect inside tables drawn by TableList."),
            new BilingualData("同一成员在普通 Inspector 中仍会显示，该特性只隐藏表格中的列。",
                "The same member is still shown in a regular inspector; this attribute only hides the column in the table.")
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

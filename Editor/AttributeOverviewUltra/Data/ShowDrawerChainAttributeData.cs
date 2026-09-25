namespace Runestone.AesirInspector.Editor
{
    internal class ShowDrawerChainAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("ShowDrawerChain", "ShowDrawerChain",
                "ShowDrawerChain 特性用于在检查器中显示属性的绘制链，便于调试。",
                "The ShowDrawerChain attribute is used to display the property's drawer chain in the inspector for debugging purposes.");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("列出该属性当前使用的所有 Prepend / Append / Value 绘制器，用于调试绘制链。",
                "Lists all Prepend / Append / Value drawers currently used by the property, for debugging the drawer chain."),
            new BilingualData("自定义绘制器以绿色高亮；未被调用的绘制器显示为灰色，便于看出哪些绘制器真正生效。",
                "Custom drawers are highlighted in green, while drawers that were never called appear greyed out, so you can see which ones actually take effect."),
            new BilingualData("可在链中勾选 / 取消勾选单个绘制器来临时跳过（SkipWhenDrawing），该状态不会被保存。",
                "Individual drawers can be toggled in the chain to skip them temporarily (SkipWhenDrawing); the state is not saved.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = null;

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                ShowDrawerChainExampleSO.Instance)
        };
    }
}

namespace Runestone.AesirInspector.Editor
{
    internal class SuppressInvalidAttributeErrorAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("SuppressInvalidAttributeError", "SuppressInvalidAttributeError",
                "SuppressInvalidAttributeError 特性用于抑制不适用特性产生的错误消息。",
                "The SuppressInvalidAttributeError attribute is used to suppress error messages from incompatible attributes.");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("抑制特性被用在不支持的类型上时弹出的 Inspector 错误提示。",
                "Suppresses the inspector error shown when an attribute is applied to a value it does not support."),
            new BilingualData("典型场景：泛型字段上组合只在部分类型有效的特性，如 [SuppressInvalidAttributeError, Range(0, 10)] public T Value;。",
                "Typical case: combining an attribute that is valid for only some types on a generic field, such as [SuppressInvalidAttributeError, Range(0, 10)] public T Value;."),
            new BilingualData("只隐藏错误提示，不会让不兼容的特性真正生效。",
                "It only hides the error message; it does not make the incompatible attribute take effect.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = { };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = null;

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                SuppressInvalidAttributeErrorExampleSO.Instance)
        };
    }
}

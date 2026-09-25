namespace Runestone.AesirInspector.Editor
{
    internal class TypeInfoBoxAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("TypeInfoBox", "TypeInfoBox", "TypeInfoBox 特性在类的内部的最上方绘制一个 InfoBox。",
                "The TypeInfoBox attribute draws an InfoBox at the top of a class.",
                "https://odininspector.com/attributes/type-info-box-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("在类型的最顶部绘制一个 InfoBox，无需再借助 PropertyOrder 和 OnInspectorGUI 特性。",
                "Draws an InfoBox at the very top of a type without needing the PropertyOrder and OnInspectorGUI attributes."),
            new BilingualData("只能标注在类、结构体或接口上，不能标注字段或属性；与 [InfoBox] 的区别是它作用于整个类型而非单个成员。",
                "Can only be applied to classes, structs or interfaces, not to fields or properties; unlike [InfoBox] it targets the whole type rather than a single member."),
            new BilingualData("既可用于可序列化类，也可用于 MonoBehaviour、ScriptableObject 等类型；message 支持 $/@ 字符串解析。",
                "It works on serializable classes as well as MonoBehaviour and ScriptableObject types, and message supports $/@ string resolution.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(string).FullName, "message",
                new BilingualData("顶部 InfoBox 的消息内容。", "The message content of the top InfoBox."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                TypeInfoBoxExampleSO.Instance)
        };
    }
}

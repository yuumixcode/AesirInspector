using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// CustomValueDrawer 特性的介绍数据，包含标题、参数说明、解析字符串参数和案例预览项。
    /// </summary>
    internal class CustomValueDrawerAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Custom Value Drawer", "Custom Value Drawer",
                "使用 CustomValueDrawer 特性，代替声明一个 Attribute，同时声明一个对应 Drawer 类的流程。CustomValueDrawer 支持撤销，重做，多选。",
                "Instead of making a new attribute, and a new drawer, for a one-time thing, you can with this attribute, make a method that acts as a custom property drawer. These drawers will out of the box have support for undo/redo and multi-selection.",
                OdinInspectorDocumentationLinks.CustomValueDrawerUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("用一个方法代替「新建特性 + 新建 Drawer」的流程，方法充当该字段的自定义绘制器；绘制器自带撤销 / 重做与多选支持。",
                "Replaces the \"new attribute + new drawer\" workflow with a single method that acts as the field's custom drawer; the drawer supports undo / redo and multi-selection out of the box."),
            new BilingualData(
                "Action 支持 $ 成员引用与 @ 表达式；方法通常接收 (T value, GUIContent label) 并返回 T，也可只接收 value 或额外接收 callNextDrawer。",
                "Action supports $ member references and @ expressions; the method usually takes (T value, GUIContent label) and returns T, but may also take only value or additionally a callNextDrawer."),
            new BilingualData("方法的返回值会写回字段；在方法内调用 callNextDrawer(label) 可先绘制默认绘制器。",
                "The method's return value is written back to the field; calling callNextDrawer(label) inside the method first draws the default drawer.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[1]
        {
            new ParameterValue(typeof(string).FullName, "Action",
                new BilingualData("设置自定义绘制方法或表达式。该方法通常接收 (T value, GUIContent label) 并返回 T",
                    "A resolved string that defines the custom drawer action to take, such as an expression or method invocation."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Action", ResolverType.ValueResolver, "T", "None",
                new List<ParameterValue>
                {
                    new ParameterValue("T", "$value",
                        new BilingualData("代表应用此特性的成员当前值，类型为成员类型",
                            "Representing the member that has attribute applied to it.")),
                    new ParameterValue("GUIContent", "$label",
                        new BilingualData("代表成员的标签", "Representing the label of the member.")),
                    new ParameterValue("InspectorProperty", "$property",
                        new BilingualData("代表此成员的 Odin 属性实例",
                            "Representing the Odin property instance for this member."))
                })
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Parameters",
                CustomValueDrawerExampleSO.Instance)
        };
    }
}

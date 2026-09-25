using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// OnStateUpdate 特性的介绍数据。
    /// </summary>
    internal class OnStateUpdateAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("On State Update", "在状态更新时",
                "OnStateUpdate 特性允许你在属性的状态（如可见性、是否禁用等）更新时执行代码。它在属性被绘制之前执行。",
                "The OnStateUpdate attribute allows you to execute code whenever the state of a property (like visibility, whether it's disabled, etc.) is updated. it runs before the property is drawn.",
                OdinInspectorDocumentationLinks.OnStateUpdateUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("在属性状态更新时执行（通常每帧至少一次），即使属性当前不可见也会被调用。",
                "Runs whenever the property's state is updated (generally at least once per frame) and is invoked even when the property is not visible."),
            new BilingualData(
                "可通过 $property 访问当前属性并修改其 State（如 Visible、Enabled、Expanded），用于替代 [ShowIf] 等一次性状态逻辑。",
                "Use $property to access the current property and modify its State (such as Visible, Enabled or Expanded); this replaces one-off state logic like [ShowIf]."),
            new BilingualData("也可用 @#(成员).State 修改其他属性的状态；标注在方法上时方法可接收 InspectorProperty 参数。",
                "Another property's state can be changed with @#(member).State; when placed on a method, the method may take an InspectorProperty parameter.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[1]
        {
            new ParameterValue(typeof(string).FullName, "Action",
                new BilingualData("要执行的操作或方法名。", "The action or method name to execute."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Action", ResolverType.ActionResolver, "void", "None",
                new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                OnStateUpdateExampleSO.Instance)
        };
    }
}

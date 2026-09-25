using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// OnInspectorGUI 特性的介绍数据。
    /// </summary>
    internal class OnInspectorGUIAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("On Inspector GUI", "在检查器 GUI 时",
                "OnInspectorGUI 特性允许你在 Inspector 中执行自定义的 GUI 代码。你可以将其应用于字段、属性或方法。",
                "The OnInspectorGUI attribute allows you to execute custom GUI code in the inspector. You can apply it to fields, properties, or methods.",
                OdinInspectorDocumentationLinks.OnInspectorGuiUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("在检查器绘制该成员时执行自定义 GUI 代码；可直接标注在方法上，也可标注在字段或属性上指定回调。",
                "Runs custom GUI code while the inspector draws the member; it can be placed directly on a method, or on a field or property with a callback."),
            new BilingualData("单参数形式默认 append 为 true，即绘制在成员之后；需要绘制在成员之前时传入 false。",
                "The single-argument form defaults to append: true, drawing after the member; pass false to draw before it."),
            new BilingualData(
                "可用两个参数同时指定前置与后置回调，如 [OnInspectorGUI(\"Prepend\", \"Append\")]；该特性继承自 ShowInInspector，因此可作用于私有成员。",
                "Two arguments can supply both a prepend and an append callback, e.g. [OnInspectorGUI(\"Prepend\", \"Append\")]; the attribute inherits from ShowInInspector, so private members work as well.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[1]
        {
            new ParameterValue(typeof(string).FullName, "Action",
                new BilingualData("要执行的 GUI 操作或方法名。", "The GUI action or method name to execute."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Action", ResolverType.ActionResolver, "void", "None",
                new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                OnInspectorGUIExampleSO.Instance)
        };
    }
}

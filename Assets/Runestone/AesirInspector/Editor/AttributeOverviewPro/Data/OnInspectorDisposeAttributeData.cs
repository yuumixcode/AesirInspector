using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    internal class OnInspectorDisposeAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("OnInspectorDispose", "OnInspectorDispose",
                "OnInspectorDispose 特性用于在属性即将从 Inspector 中移除或释放时执行清理代码。",
                "The OnInspectorDispose attribute is used to execute cleanup code when a property is about to be removed or disposed from the Inspector.",
                OdinInspectorDocumentationLinks.OnInspectorDisposeUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("在属性的绘制器被释放时执行，常用于取消订阅、释放资源等清理操作。",
                "Executes when the property's drawers are disposed; typically used for unsubscribing events, releasing resources and other cleanup."),
            new BilingualData("属性树重建时可能被多次调用（如多态属性类型变化），因此清理逻辑应可重复执行。",
                "It may run several times as the property tree is rebuilt (for example when a polymorphic property's type changes), so the cleanup logic must be repeatable."),
            new BilingualData("可直接标注在无参方法上，也可标注在成员上并用字符串指定方法名或 @ 表达式。",
                "Can be placed directly on a parameterless method, or on a member with a string naming a method or an @ expression.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[1]
        {
            new ParameterValue(typeof(string).FullName, "Action",
                new BilingualData("释放时要执行的操作或方法名。", "The action or method name to execute on dispose."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Action", ResolverType.ActionResolver, typeof(void).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Action",
                OnInspectorDisposeExampleSO.Instance)
        };
    }
}

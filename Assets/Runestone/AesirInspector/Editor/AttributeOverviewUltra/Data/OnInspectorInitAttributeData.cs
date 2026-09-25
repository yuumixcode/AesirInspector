using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    internal class OnInspectorInitAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("OnInspectorInit", "OnInspectorInit",
                "OnInspectorInit 特性用于在属性即将在 Inspector 中首次绘制之前执行初始化代码。",
                "The OnInspectorInit attribute is used to execute initialization code just before a property is drawn in the Inspector for the first time.",
                OdinInspectorDocumentationLinks.OnInspectorInitUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("在属性首次绘制前执行初始化；由于 Odin 属性系统惰性求值，位于未展开折叠组中的属性不会执行，展开后才触发。",
                "Runs initialization before the property is first drawn; because Odin's property system is lazily evaluated, a property inside a collapsed foldout does not run it until the foldout is expanded."),
            new BilingualData("属性树重建（如多态属性类型变化、重新选中对象）会使其再次执行，初始化逻辑应可重复运行。",
                "It runs again whenever the property tree is rebuilt (for example when a polymorphic property's type changes or the object is reselected), so the initialization logic must be repeatable."),
            new BilingualData("可直接标注在无参方法上，也可标注在成员上并用字符串指定方法名或 @ 表达式。",
                "Can be placed directly on a parameterless method, or on a member with a string naming a method or an @ expression.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[1]
        {
            new ParameterValue(typeof(string).FullName, "Action",
                new BilingualData("初始化时要执行的操作或方法名。",
                    "The action or method name to execute on initialization."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Action", ResolverType.ActionResolver, typeof(void).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Action",
                OnInspectorInitExampleSO.Instance)
        };
    }
}

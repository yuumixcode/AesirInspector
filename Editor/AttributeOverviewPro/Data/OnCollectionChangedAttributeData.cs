using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    internal class OnCollectionChangedAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("OnCollectionChanged", "OnCollectionChanged",
                "OnCollectionChanged 特性用于在集合（如 List、Dictionary 等）的内容发生更改时触发方法。",
                "The OnCollectionChanged attribute is used to trigger methods when the contents of a collection (such as List, Dictionary, etc.) are changed.",
                OdinInspectorDocumentationLinks.OnCollectionChangedUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("仅在通过检查器修改集合时触发，脚本修改不会触发回调。",
                "Only fires when the collection is modified through the inspector; changes made by script do not trigger the callbacks."),
            new BilingualData("适用于任何具备集合解析器的集合，如数组、List、Dictionary、HashSet、Stack、LinkedList（Dictionary/HashSet 需要 Odin 序列化）。",
                "Works for any collection with a collection resolver, such as arrays, List, Dictionary, HashSet, Stack and LinkedList (Dictionary/HashSet require Odin serialization)."),
            new BilingualData("回调方法可接收 CollectionChangeInfo 获取变更详情；它是仅编辑器可用的结构体（命名空间 Sirenix.OdinInspector.Editor），必须放在 #if UNITY_EDITOR 中。",
                "Callback methods can take a CollectionChangeInfo for details about the change; it is an editor-only struct (namespace Sirenix.OdinInspector.Editor) and must be wrapped in #if UNITY_EDITOR.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[2]
        {
            new ParameterValue(typeof(string).FullName, "Before",
                new BilingualData("集合更改前要执行的操作或方法名。",
                    "The action or method name to execute before the collection changes.")),
            new ParameterValue(typeof(string).FullName, "After",
                new BilingualData("集合更改后要执行的操作或方法名。",
                    "The action or method name to execute after the collection changes."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("Before", ResolverType.ActionResolver, typeof(void).FullName,
                "None", new List<ParameterValue>()),
            new ResolvedStringParameterValue("After", ResolverType.ActionResolver, typeof(void).FullName,
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                OnCollectionChangedExampleSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Before",
                OnCollectionChangedExampleWithBeforeSO.Instance),
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("After",
                OnCollectionChangedExampleWithAfterSO.Instance)
        };
    }
}

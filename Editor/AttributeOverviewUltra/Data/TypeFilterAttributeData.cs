using System.Collections.Generic;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TypeFilter 特性的介绍数据。
    /// </summary>
    internal class TypeFilterAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("Type Filter", "类型过滤器",
                "TypeFilter 特性为属性提供了一个下拉列表，用于选择并实例化不同的类型。这在处理多态性时非常有用。",
                "The TypeFilter attribute provides a dropdown for a property to select and instantiate different types. This is very useful when dealing with polymorphism.",
                OdinInspectorDocumentationLinks.TypeFilterUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData(
                "为字段提供类型下拉列表：FilterGetter 指向返回类型集合的成员或表达式（数组、List 等 IList/IEnumerable<Type> 均可）。",
                "Provides a type dropdown for a field: FilterGetter points to a member or expression that returns a type collection (arrays, Lists and other IList/IEnumerable<Type> values all work)."),
            new BilingualData("选中类型后 Odin 会直接实例化该类型，并在下拉框下方以折叠面板绘制它的所有子成员。",
                "When a type is selected Odin instantiates it directly and draws all of its child members in a foldout below the dropdown."),
            new BilingualData("常与接口或抽象类字段搭配使用，在运行时选择具体的实现类。",
                "Commonly used with interface or abstract class fields to pick a concrete implementation at runtime.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } = new ParameterValue[1]
        {
            new ParameterValue(typeof(string).FullName, "FilterGetter",
                new BilingualData("返回可选类型列表的方法或字段名。",
                    "The name of the method or field that returns the list of selectable types."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } =
        {
            new ResolvedStringParameterValue("FilterGetter", ResolverType.ValueResolver, "IEnumerable<Type>",
                "None", new List<ParameterValue>())
        };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                TypeFilterExampleSO.Instance)
        };
    }
}

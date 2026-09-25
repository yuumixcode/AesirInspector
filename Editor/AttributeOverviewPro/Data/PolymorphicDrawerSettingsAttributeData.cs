using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    internal class PolymorphicDrawerSettingsAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("PolymorphicDrawerSettings", "PolymorphicDrawerSettings",
                "PolymorphicDrawerSettings 特性用于设置多态字段的绘制样式。",
                "The PolymorphicDrawerSettings attribute configures the drawing style of polymorphic fields.",
                "https://odininspector.com/attributes/polymorphic-drawer-settings-attribute");

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("用于配置由 Odin 绘制的多态字段（接口、抽象类）；Unity 默认无法序列化这类类型，需要 Odin 序列化（如 SerializedMonoBehaviour 或 [OdinSerialize]）。",
                "Configures how Odin draws polymorphic fields (interfaces, abstract classes); Unity cannot serialize such types by default, so Odin serialization is required (e.g. SerializedMonoBehaviour or [OdinSerialize])."),
            new BilingualData("若接口使用 EditorOnly 序列化模式，构建时会剔除 Odin 序列化的数据，导致运行时数据丢失，需特别注意。",
                "If interfaces use the EditorOnly serialization mode, Odin-serialized data is stripped from builds and lost at runtime, so be careful."),
            new BilingualData("CreateInstanceFunction 需指向一个接收单个 Type 参数（名为 type）并返回 object 的方法，且不会用于 UnityEngine.Object 类型；未指定时由 NonDefaultConstructorPreference（默认 ConstructIdeal）决定实例构造方式。",
                "CreateInstanceFunction must point to a method taking a single Type parameter named 'type' and returning object, and is not called for UnityEngine.Object types; when unset, NonDefaultConstructorPreference (default ConstructIdeal) decides how instances are constructed.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "ShowBaseType",
                new BilingualData("是否显示基类字段，默认为 false。",
                    "Whether to display the base type field. Defaults to false.")),
            new ParameterValue(typeof(bool).FullName, "ReadOnlyIfNotNullReference",
                new BilingualData("如果引用不为空，是否只读，默认为 false。",
                    "Whether to make the field read-only if the reference is not null. Defaults to false.")),
            new ParameterValue(typeof(string).FullName, "CreateInstanceFunction",
                new BilingualData("自定义创建实例的函数名，默认为 null。",
                    "Custom function name for creating instances. Defaults to null.")),
            new ParameterValue(typeof(NonDefaultConstructorPreference).FullName,
                "NonDefaultConstructorPreference",
                new BilingualData("没有默认构造函数的处理设置。",
                    "Preference for handling types without default constructors."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeOdinSerializedExample("Basic Usage",
                PolymorphicDrawerSettingsExampleSO.Instance)
        };
    }
}

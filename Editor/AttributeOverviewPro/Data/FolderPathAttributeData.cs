namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// FolderPath 特性的介绍数据。
    /// </summary>
    internal class FolderPathAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("FolderPath", "FolderPath",
                "FolderPath 特性在字符串属性上绘制一个文件夹选择器，方便用户选择文件夹路径。",
                "The FolderPath attribute draws a folder picker for string properties, making it easy for users to select folder paths.",
                OdinInspectorDocumentationLinks.FolderPathUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("默认绘制相对于 Unity 项目根目录的路径，并强制使用正斜杠；AbsolutePath 可改为绝对路径，UseBackslashes 可改用反斜杠。",
                "By default the path is drawn relative to the Unity project root and forward slashes are enforced; AbsolutePath switches to absolute paths and UseBackslashes switches to backslashes."),
            new BilingualData("ParentFolder 支持 $ 成员引用（例如 ParentFolder = \"$DynamicParent\"），可以动态改变相对基准目录。",
                "ParentFolder supports $ member references (for example ParentFolder = \"$DynamicParent\"), so the base folder can change dynamically."),
            new BilingualData("支持从 Project 窗口直接拖放文件夹到该字段，也可以用于 string[] 等集合的元素。",
                "Folders can be dragged and dropped from the Project window onto the field, and the attribute also works on elements of collections such as string[]."),
            new BilingualData("RequireExistingPath 为 true 时，路径不存在会报校验错误；与 FilePath 的区别是它只提供文件夹选择器。",
                "With RequireExistingPath set to true, a non-existing path raises a validation error; unlike FilePath it only offers a folder picker.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "AbsolutePath",
                new BilingualData("是否使用绝对路径。", "Whether to use absolute paths.")),
            new ParameterValue(typeof(string).FullName, "ParentFolder",
                new BilingualData("相对路径的父文件夹。", "The parent folder for relative paths.")),
            new ParameterValue(typeof(bool).FullName, "RequireExistingPath",
                new BilingualData("选定的路径是否必须存在。", "Whether the selected path must exist.")),
            new ParameterValue(typeof(bool).FullName, "UseBackslashes",
                new BilingualData("是否使用反斜杠。", "Whether to use backslashes."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                FolderPathExampleSO.Instance)
        };
    }
}

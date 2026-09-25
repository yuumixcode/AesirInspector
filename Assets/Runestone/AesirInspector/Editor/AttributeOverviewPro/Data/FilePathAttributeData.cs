namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// FilePath 特性的介绍数据。
    /// </summary>
    internal class FilePathAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("FilePath", "FilePath", "FilePath 特性在字符串属性上绘制一个文件选择器，方便用户选择文件路径。",
                "The FilePath attribute draws a file picker for string properties, making it easy for users to select file paths.",
                OdinInspectorDocumentationLinks.FilePathUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("默认绘制相对于 Unity 项目根目录的路径，并强制使用正斜杠；AbsolutePath 可改为绝对路径，UseBackslashes 可改用反斜杠。", "By default the path is drawn relative to the Unity project root and forward slashes are enforced; AbsolutePath switches to absolute paths and UseBackslashes switches to backslashes."),
            new BilingualData("ParentFolder 与 Extensions 都支持 $ 成员引用（例如 ParentFolder = \"$DynamicParent\"），可以动态改变相对基准目录与允许的扩展名。", "Both ParentFolder and Extensions support $ member references (for example ParentFolder = \"$DynamicParent\"), so the base folder and allowed extensions can change dynamically."),
            new BilingualData("支持从 Project 窗口直接拖放文件到该字段，也可以用于 string[] 等集合的元素。", "Files can be dragged and dropped from the Project window onto the field, and the attribute also works on elements of collections such as string[]."),
            new BilingualData("RequireExistingPath 为 true 时，路径不存在会报校验错误。", "With RequireExistingPath set to true, a non-existing path raises a validation error.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "AbsolutePath",
                new BilingualData("是否使用绝对路径。", "Whether to use absolute paths.")),
            new ParameterValue(typeof(string).FullName, "Extensions",
                new BilingualData("允许的文件扩展名（用逗号分隔）。", "Allowed file extensions (comma separated).")),
            new ParameterValue(typeof(string).FullName, "ParentFolder",
                new BilingualData("相对路径的父文件夹。", "The parent folder for relative paths.")),
            new ParameterValue(typeof(bool).FullName, "RequireExistingPath",
                new BilingualData("选定的路径是否必须存在。", "Whether the selected path must exist.")),
            new ParameterValue(typeof(bool).FullName, "IncludeFileExtension",
                new BilingualData("选定的路径是否包含扩展名。",
                    "Whether the selected path should include the file extension.")),
            new ParameterValue(typeof(bool).FullName, "UseBackslashes",
                new BilingualData("是否使用反斜杠。", "Whether to use backslashes."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                FilePathExampleSO.Instance)
        };
    }
}

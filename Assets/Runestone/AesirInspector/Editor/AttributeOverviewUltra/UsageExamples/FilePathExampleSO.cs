using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// FilePath 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class FilePathExampleSO : AttributeExampleSO<FilePathExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        [FilePath]
        public string UnityProjectPath;

        [FoldoutGroup("Parameter: Extensions")]
        [FilePath(Extensions = "cs")]
        public string ScriptFiles;

        [FoldoutGroup("Parameter: Extensions")]
        [FilePath(Extensions = "cs, lua")]
        public string scriptPath;

        [FoldoutGroup("Parameter: AbsolutePath")]
        [FilePath(AbsolutePath = true)]
        public string AbsolutePath;

        [FoldoutGroup("Parameter: ParentFolder")]
        [FilePath(ParentFolder = "Assets/Plugins/Sirenix")]
        public string RelativeToParentPath;

        [FoldoutGroup("Parameter: ParentFolder")]
        [FilePath(ParentFolder = "Assets/Resources")]
        public string ResourcePath;

        [FoldoutGroup("Parameter: RequireExistingPath")]
        [FilePath(RequireExistingPath = true)]
        public string ExistingPath;

        [FoldoutGroup("Parameter: UseBackslashes")]
        [FilePath(UseBackslashes = true)]
        public string Backslashes;

        [FoldoutGroup("Parameter: IncludeFileExtension")]
        [FilePath(IncludeFileExtension = false)]
        public string noExtensionPath;

        [FoldoutGroup("Member Reference ($)")]
        [FilePath(ParentFolder = "$DynamicParent", Extensions = "$DynamicExtensions")]
        public string DynamicFilePath;

        [FoldoutGroup("Member Reference ($)")]
        public string DynamicParent = "Assets/Plugins/Sirenix";

        [FoldoutGroup("Member Reference ($)")]
        public string DynamicExtensions = "cs, unity, jpg";

        [FoldoutGroup("Lists")]
        [FilePath(ParentFolder = "Assets/Plugins/Sirenix/Demos/Odin Inspector")]
        public string[] ListOfFiles;

        public override void AesirInspectorReset()
        {
            UnityProjectPath = "";
            ScriptFiles = "";
            scriptPath = "";
            AbsolutePath = "";
            RelativeToParentPath = "";
            ResourcePath = "";
            ExistingPath = "";
            Backslashes = "";
            noExtensionPath = "";
            DynamicFilePath = "";
            DynamicParent = "Assets/Plugins/Sirenix";
            DynamicExtensions = "cs, unity, jpg";
            ListOfFiles = new string[0];
        }
    }
}

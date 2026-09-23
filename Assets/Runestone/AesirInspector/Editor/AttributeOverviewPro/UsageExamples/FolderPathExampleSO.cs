using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// FolderPath 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class FolderPathExampleSO : AttributeExampleSO<FolderPathExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        [FolderPath]
        public string UnityProjectPath;

        [FoldoutGroup("Parameter: AbsolutePath")]
        [FolderPath(AbsolutePath = true)]
        public string AbsolutePath;

        [FoldoutGroup("Parameter: ParentFolder")]
        [FolderPath(ParentFolder = "Assets/Plugins/Sirenix")]
        public string RelativeToParentPath;

        [FoldoutGroup("Parameter: ParentFolder")]
        [FolderPath(ParentFolder = "Assets/Resources")]
        public string ResourcePath;

        [FoldoutGroup("Parameter: ParentFolder")]
        [FolderPath(ParentFolder = "Assets/Runestone")]
        public string relativePath;

        [FoldoutGroup("Parameter: RequireExistingPath")]
        [FolderPath(RequireExistingPath = true)]
        public string ExistingPath;

        [FoldoutGroup("Parameter: UseBackslashes")]
        [FolderPath(UseBackslashes = true)]
        public string Backslashes;

        [FoldoutGroup("Member Reference ($)")]
        [FolderPath(ParentFolder = "$DynamicParent")]
        public string DynamicFolderPath;

        [FoldoutGroup("Member Reference ($)")]
        public string DynamicParent = "Assets/Plugins/Sirenix";

        [FoldoutGroup("Lists")]
        [FolderPath(ParentFolder = "Assets/Plugins/Sirenix")]
        public string[] ListOfFolders;

        public override void AesirInspectorReset()
        {
            UnityProjectPath = "";
            AbsolutePath = "";
            RelativeToParentPath = "";
            ResourcePath = "";
            relativePath = "";
            ExistingPath = "";
            Backslashes = "";
            DynamicFolderPath = "";
            DynamicParent = "Assets/Plugins/Sirenix";
            ListOfFolders = new string[0];
        }
    }
}

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(FilePathAttribute), "FilePath attribute provides a neat interface for assigning paths to strings.\nIt also supports drag and drop from the project folder.")]
	internal class FilePathExamples
	{
		[FilePath]
		public string UnityProjectPath;

		[FilePath(ParentFolder = "Assets/Plugins/Sirenix")]
		public string RelativeToParentPath;

		[FilePath(ParentFolder = "Assets/Resources")]
		public string ResourcePath;

		[BoxGroup("Conditions", true, false, 0f)]
		[FilePath(Extensions = "cs")]
		public string ScriptFiles;

		[FilePath(AbsolutePath = true)]
		[BoxGroup("Conditions", true, false, 0f)]
		public string AbsolutePath;

		[FilePath(RequireExistingPath = true)]
		[BoxGroup("Conditions", true, false, 0f)]
		public string ExistingPath;

		[FilePath(UseBackslashes = true)]
		[BoxGroup("Conditions", true, false, 0f)]
		public string Backslashes;

		[BoxGroup("Member referencing", true, false, 0f)]
		[FilePath(ParentFolder = "$DynamicParent", Extensions = "$DynamicExtensions")]
		public string DynamicFilePath;

		[BoxGroup("Member referencing", true, false, 0f)]
		public string DynamicParent = "Assets/Plugins/Sirenix";

		[BoxGroup("Member referencing", true, false, 0f)]
		public string DynamicExtensions = "cs, unity, jpg";

		[BoxGroup("Lists", true, false, 0f)]
		[FilePath(ParentFolder = "Assets/Plugins/Sirenix/Demos/Odin Inspector")]
		public string[] ListOfFiles;
	}
}

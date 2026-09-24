namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(FoldoutGroupAttribute))]
	internal class FoldoutGroupAttributeExamples
	{
		[FoldoutGroup("Group 1", 0f)]
		public int A;

		[FoldoutGroup("Group 1", 0f)]
		public int B;

		[FoldoutGroup("Group 1", 0f)]
		public int C;

		[FoldoutGroup("Collapsed group", false, 0f)]
		public int D;

		[FoldoutGroup("Collapsed group", 0f)]
		public int E;

		[FoldoutGroup("$GroupTitle", true, 0f)]
		public int One;

		[FoldoutGroup("$GroupTitle", 0f)]
		public int Two;

		public string GroupTitle = "Dynamic group title";
	}
}

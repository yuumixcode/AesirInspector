namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(IndentAttribute))]
	internal class IndentExamples
	{
		[Indent(1)]
		[Title("Nicely organize your properties.", null, TitleAlignments.Left, true, true)]
		public int A;

		[Indent(2)]
		public int B;

		[Indent(3)]
		public int C;

		[Indent(4)]
		public int D;

		[Indent(1)]
		[Title("Using the Indent attribute", null, TitleAlignments.Left, true, true)]
		public int E;

		[Indent(0)]
		public int F;

		[Indent(-1)]
		public int G;
	}
}

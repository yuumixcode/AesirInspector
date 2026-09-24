namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(HideInPlayModeAttribute))]
	internal class HideInPlayModeExamples
	{
		public int AlwaysVisible;

		[Title("Hidden in play mode", null, TitleAlignments.Left, true, true)]
		[HideInPlayMode]
		public int A;

		[HideInPlayMode]
		public int B;
	}
}

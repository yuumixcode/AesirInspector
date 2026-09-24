namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(BoxGroupAttribute), Order = 10f)]
	[AttributeExample(typeof(HorizontalGroupAttribute), Order = 10f)]
	[AttributeExample(typeof(TitleGroupAttribute), Order = 10f)]
	[AttributeExample(typeof(VerticalGroupAttribute), Order = 10f)]
	internal class MultipleStackedBoxesExample
	{
		[VerticalGroup("Multiple Stacked Boxes/Split/Left", 0f)]
		[TitleGroup("Multiple Stacked Boxes", null, TitleAlignments.Left, true, true, false, 0f)]
		[HorizontalGroup("Multiple Stacked Boxes/Split", 0f, 0, 0, 0f)]
		[BoxGroup("Multiple Stacked Boxes/Split/Left/Box A", true, false, 0f)]
		public int BoxA;

		[BoxGroup("Multiple Stacked Boxes/Split/Left/Box B", true, false, 0f)]
		public int BoxB;

		[VerticalGroup("Multiple Stacked Boxes/Split/Right", 0f)]
		[BoxGroup("Multiple Stacked Boxes/Split/Right/Box C", true, false, 0f)]
		public int BoxC;

		[VerticalGroup("Multiple Stacked Boxes/Split/Right", 0f)]
		[BoxGroup("Multiple Stacked Boxes/Split/Right/Box C", true, false, 0f)]
		public int BoxD;

		[VerticalGroup("Multiple Stacked Boxes/Split/Right", 0f)]
		[BoxGroup("Multiple Stacked Boxes/Split/Right/Box C", true, false, 0f)]
		public int BoxE;
	}
}

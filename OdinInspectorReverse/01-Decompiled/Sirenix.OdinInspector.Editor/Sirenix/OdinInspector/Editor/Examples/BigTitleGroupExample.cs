namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ButtonGroupAttribute), Order = 10f)]
	[AttributeExample(typeof(TitleGroupAttribute), Order = 10f)]
	[AttributeExample(typeof(BoxGroupAttribute), Order = 10f)]
	internal class BigTitleGroupExample
	{
		[TitleGroup("Titles/First Title", null, TitleAlignments.Left, true, true, false, 0f)]
		[BoxGroup("Titles", true, false, 0f, ShowLabel = false)]
		public int A;

		[TitleGroup("Titles/Boxed/Second Title", null, TitleAlignments.Left, true, true, false, 0f)]
		[BoxGroup("Titles/Boxed", true, false, 0f)]
		public int B;

		[TitleGroup("Titles/Boxed/Second Title", null, TitleAlignments.Left, true, true, false, 0f)]
		public int C;

		[ButtonGroup("Titles/Horizontal Buttons/Buttons", 0f)]
		[TitleGroup("Titles/Horizontal Buttons", null, TitleAlignments.Left, true, true, false, 0f)]
		public void FirstButton()
		{
		}

		[ButtonGroup("Titles/Horizontal Buttons/Buttons", 0f)]
		public void SecondButton()
		{
		}
	}
}

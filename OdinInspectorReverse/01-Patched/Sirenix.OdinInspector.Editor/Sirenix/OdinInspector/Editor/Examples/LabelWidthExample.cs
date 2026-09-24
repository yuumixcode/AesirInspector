namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(LabelWidthAttribute), "Change the width of the label for your property.")]
	internal class LabelWidthExample
	{
		public int DefaultWidth;

		[LabelWidth(50f)]
		public int Thin;

		[LabelWidth(250f)]
		public int Wide;
	}
}

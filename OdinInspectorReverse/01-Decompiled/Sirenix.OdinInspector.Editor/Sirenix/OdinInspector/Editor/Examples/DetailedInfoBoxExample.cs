namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(DetailedInfoBoxAttribute))]
	internal class DetailedInfoBoxExample
	{
		[DetailedInfoBox("Click the DetailedInfoBox...", "... to reveal more information!\nThis allows you to reduce unnecessary clutter in your editors, and still have all the relavant information available when required.", InfoMessageType.Info, null)]
		public int Field;
	}
}

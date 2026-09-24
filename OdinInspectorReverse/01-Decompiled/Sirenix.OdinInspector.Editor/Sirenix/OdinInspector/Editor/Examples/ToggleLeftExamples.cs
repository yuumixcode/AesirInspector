namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ToggleLeftAttribute))]
	internal class ToggleLeftExamples
	{
		[InfoBox("Draws the toggle button before the label for a bool property.", InfoMessageType.Info, null)]
		[ToggleLeft]
		public bool LeftToggled;

		[EnableIf("LeftToggled")]
		public int A;

		[EnableIf("LeftToggled")]
		public bool B;

		[EnableIf("LeftToggled")]
		public bool C;
	}
}

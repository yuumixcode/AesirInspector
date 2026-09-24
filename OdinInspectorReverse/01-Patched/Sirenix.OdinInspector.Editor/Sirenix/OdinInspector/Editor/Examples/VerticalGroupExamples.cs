namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(VerticalGroupAttribute), "VerticalGroup, similar to HorizontalGroup, groups properties together vertically in the inspector.\nBy itself it doesn't do much, but combined with other groups, like HorizontalGroup, it can be very useful. It can also be used in TableLists to create columns.")]
	internal class VerticalGroupExamples
	{
		[VerticalGroup("Split/Left", 0f)]
		[HorizontalGroup("Split", 0f, 0, 0, 0f)]
		public InfoMessageType First;

		[VerticalGroup("Split/Left", 0f)]
		public InfoMessageType Second;

		[HideLabel]
		[VerticalGroup("Split/Right", 0f)]
		public int A;

		[HideLabel]
		[VerticalGroup("Split/Right", 0f)]
		public int B;
	}
}

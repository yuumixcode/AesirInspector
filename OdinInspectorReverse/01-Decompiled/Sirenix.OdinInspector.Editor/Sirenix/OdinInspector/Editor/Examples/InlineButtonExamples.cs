using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(InlineButtonAttribute))]
	internal class InlineButtonExamples
	{
		[InlineButton("A", null)]
		public int InlineButton;

		[InlineButton("A", null)]
		[InlineButton("B", "Custom Button Name")]
		public int ChainedButtons;

		[InlineButton("C", SdfIconType.Dice6Fill, "Random")]
		public int IconButton;

		private void A()
		{
			Debug.Log("A");
		}

		private void B()
		{
			Debug.Log("B");
		}

		private void C()
		{
			Debug.Log("C");
		}
	}
}

using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(MinMaxSliderAttribute), "Uses a Vector2 where x is the min knob and y is the max knob.")]
	internal class MinMaxSliderExamples
	{
		[MinMaxSlider(-10f, 10f, false)]
		public Vector2 MinMaxValueSlider = new Vector2(-7f, -2f);

		[MinMaxSlider(-10f, 10f, true)]
		public Vector2 WithFields = new Vector2(-3f, 4f);

		[MinMaxSlider("DynamicRange", true)]
		[InfoBox("You can also assign the min max values dynamically by referring to members.", InfoMessageType.Info, null)]
		public Vector2 DynamicMinMax = new Vector2(25f, 50f);

		[MinMaxSlider("Min", 10f, true)]
		public Vector2 DynamicMin = new Vector2(2f, 7f);

		[InfoBox("You can also use attribute expressions with the @ symbol.", InfoMessageType.Info, null)]
		[MinMaxSlider("@DynamicRange.x", "@DynamicRange.y * 10f", true)]
		public Vector2 Expressive = new Vector2(0f, 450f);

		public Vector2 DynamicRange = new Vector2(0f, 50f);

		public float Min => DynamicRange.x;

		public float Max => DynamicRange.y;
	}
}

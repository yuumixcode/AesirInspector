using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(MinValueAttribute))]
	[AttributeExample(typeof(MaxValueAttribute))]
	internal class MinMaxValueValueExamples
	{
		[MinValue(0.0)]
		[Title("Int", null, TitleAlignments.Left, true, true)]
		public int IntMinValue0;

		[MaxValue(0.0)]
		public int IntMaxValue0;

		[MinValue(0.0)]
		[Title("Float", null, TitleAlignments.Left, true, true)]
		public float FloatMinValue0;

		[MaxValue(0.0)]
		public float FloatMaxValue0;

		[Title("Vectors", null, TitleAlignments.Left, true, true)]
		[MinValue(0.0)]
		public Vector3 Vector3MinValue0;

		[MaxValue(0.0)]
		public Vector3 Vector3MaxValue0;
	}
}

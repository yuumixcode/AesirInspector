using System;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(HorizontalGroupAttribute))]
	internal class HorizontalGroupAttributeExamples
	{
		[Serializable]
		[HideLabel]
		public struct SomeFieldType
		{
			[ListDrawerSettings(ShowIndexLabels = true)]
			[LabelText("@$property.Parent.NiceName")]
			public float[] x;
		}

		[HorizontalGroup(0f, 0, 0, 0f)]
		public SomeFieldType Left1;

		[HorizontalGroup(0f, 0, 0, 0f)]
		public SomeFieldType Right1;

		[HorizontalGroup("row2", 0f, 0, 0, 0f, MarginRight = 0.4f)]
		public SomeFieldType Left2;

		[HorizontalGroup("row2", 0f, 0, 0, 0f)]
		public SomeFieldType Right2;

		[HorizontalGroup("row1", 0f, 0, 0, 0f, Width = 0.25f)]
		public SomeFieldType Left3;

		[HorizontalGroup("row1", 0f, 0, 0, 0f, Width = 150f)]
		public SomeFieldType Center3;

		[HorizontalGroup("row1", 0f, 0, 0, 0f)]
		public SomeFieldType Right3;

		[HorizontalGroup("row3", 0f, 0, 0, 0f, Gap = 3f)]
		public SomeFieldType Left4;

		[HorizontalGroup("row3", 0f, 0, 0, 0f)]
		public SomeFieldType Center4;

		[HorizontalGroup("row3", 0f, 0, 0, 0f)]
		public SomeFieldType Right4;

		[HorizontalGroup("row4", 0f, 0, 0, 0f, Title = "Horizontal Group Title")]
		public SomeFieldType Left5;

		[HorizontalGroup("row4", 0f, 0, 0, 0f)]
		public SomeFieldType Center5;

		[HorizontalGroup("row4", 0f, 0, 0, 0f)]
		public SomeFieldType Right5;
	}
}

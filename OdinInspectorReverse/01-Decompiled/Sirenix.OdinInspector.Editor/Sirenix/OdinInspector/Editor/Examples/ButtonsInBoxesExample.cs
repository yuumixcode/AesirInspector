namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(BoxGroupAttribute), Order = 10f)]
	[AttributeExample(typeof(FoldoutGroupAttribute), Order = 10f)]
	[AttributeExample(typeof(HorizontalGroupAttribute), Order = 10f)]
	internal class ButtonsInBoxesExample
	{
		[Button(ButtonSizes.Large)]
		[FoldoutGroup("Buttons in Boxes", 0f)]
		[HorizontalGroup("Buttons in Boxes/Horizontal", 0f, 0, 0, 0f)]
		[BoxGroup("Buttons in Boxes/Horizontal/One", true, false, 0f)]
		public void Button1()
		{
		}

		[BoxGroup("Buttons in Boxes/Horizontal/Two", true, false, 0f)]
		[Button(ButtonSizes.Large)]
		public void Button2()
		{
		}

		[BoxGroup("Buttons in Boxes/Horizontal/Double", true, false, 0f)]
		[HorizontalGroup("Buttons in Boxes/Horizontal", 0f, 0, 0, 0f, Width = 60f)]
		[Button]
		public void Accept()
		{
		}

		[Button]
		[BoxGroup("Buttons in Boxes/Horizontal/Double", true, false, 0f)]
		public void Cancel()
		{
		}
	}
}

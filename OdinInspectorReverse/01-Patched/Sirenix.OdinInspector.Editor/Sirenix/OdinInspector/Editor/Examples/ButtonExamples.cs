namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ButtonAttribute))]
	internal class ButtonExamples
	{
		public string ButtonName = "Dynamic button name";

		public bool Toggle;

		[Button("$ButtonName")]
		private void DefaultSizedButton()
		{
			Toggle = !Toggle;
		}

		[Button("@\"Expression label: \" + DateTime.Now.ToString(\"HH:mm:ss\")")]
		public void ExpressionLabel()
		{
			Toggle = !Toggle;
		}

		[Button("Name of button")]
		private void NamedButton()
		{
			Toggle = !Toggle;
		}

		[Button(ButtonSizes.Small)]
		private void SmallButton()
		{
			Toggle = !Toggle;
		}

		[Button(ButtonSizes.Medium)]
		private void MediumSizedButton()
		{
			Toggle = !Toggle;
		}

		[Button(ButtonSizes.Large)]
		[GUIColor(0.4f, 0.8f, 1f, 1f)]
		[HorizontalGroup("Split", 0.5f, 0, 0, 0f)]
		[DisableIf("Toggle")]
		private void FanzyButton1()
		{
			Toggle = !Toggle;
		}

		[GUIColor(0f, 1f, 0f, 1f)]
		[Button(ButtonSizes.Large)]
		[HideIf("Toggle", true)]
		[VerticalGroup("Split/right", 0f)]
		private void FanzyButton2()
		{
			Toggle = !Toggle;
		}

		[ShowIf("Toggle", true)]
		[VerticalGroup("Split/right", 0f)]
		[Button(ButtonSizes.Large)]
		[GUIColor(1f, 0.2f, 0f, 1f)]
		private void FanzyButton3()
		{
			Toggle = !Toggle;
		}

		[Button(ButtonSizes.Gigantic)]
		private void GiganticButton()
		{
			Toggle = !Toggle;
		}

		[Button(90)]
		private void CustomSizedButton()
		{
			Toggle = !Toggle;
		}

		[Button(Icon = SdfIconType.Dice1Fill, IconAlignment = IconAlignment.LeftOfText)]
		private void IconButton01()
		{
			Toggle = !Toggle;
		}

		[Button(Icon = SdfIconType.Dice2Fill, IconAlignment = IconAlignment.LeftOfText)]
		private void IconButton02()
		{
			Toggle = !Toggle;
		}
	}
}

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ResponsiveButtonGroupAttribute), Order = 10f)]
	[AttributeExample(typeof(TabGroupAttribute), Order = 10f)]
	internal class BigTabGroupExample
	{
		[HorizontalGroup("Tabs/Split", 0f, 0, 0, 0f, Width = 0.5f)]
		[TabGroup("Tabs/Split/Parameters", "A", false, 0f)]
		[TitleGroup("Tabs", null, TitleAlignments.Left, true, true, false, 0f)]
		public string NameA;

		[HorizontalGroup("Tabs/Split", 0f, 0, 0, 0f, Width = 0.5f)]
		[TitleGroup("Tabs", null, TitleAlignments.Left, true, true, false, 0f)]
		[TabGroup("Tabs/Split/Parameters", "A", false, 0f)]
		public string NameB;

		[TabGroup("Tabs/Split/Parameters", "A", false, 0f)]
		[TitleGroup("Tabs", null, TitleAlignments.Left, true, true, false, 0f)]
		[HorizontalGroup("Tabs/Split", 0f, 0, 0, 0f, Width = 0.5f)]
		public string NameC;

		[TabGroup("Tabs/Split/Parameters", "B", false, 0f)]
		public int ValueA;

		[TabGroup("Tabs/Split/Parameters", "B", false, 0f)]
		public int ValueB;

		[TabGroup("Tabs/Split/Parameters", "B", false, 0f)]
		public int ValueC;

		[ResponsiveButtonGroup("Tabs/Split/Buttons/Responsive/ResponsiveButtons")]
		[TabGroup("Tabs/Split/Buttons", "Responsive", false, 0f)]
		public void Hello()
		{
		}

		[ResponsiveButtonGroup("Tabs/Split/Buttons/Responsive/ResponsiveButtons")]
		public void World()
		{
		}

		[ResponsiveButtonGroup("Tabs/Split/Buttons/Responsive/ResponsiveButtons")]
		public void And()
		{
		}

		[ResponsiveButtonGroup("Tabs/Split/Buttons/Responsive/ResponsiveButtons")]
		public void Such()
		{
		}

		[TabGroup("Tabs/Split/Buttons/More Tabs/SubTabGroup", "A", false, 0f)]
		[TabGroup("Tabs/Split/Buttons", "More Tabs", false, 0f)]
		[Button]
		public void SubButtonA()
		{
		}

		[TabGroup("Tabs/Split/Buttons/More Tabs/SubTabGroup", "A", false, 0f)]
		[Button]
		public void SubButtonB()
		{
		}

		[Button(ButtonSizes.Gigantic)]
		[TabGroup("Tabs/Split/Buttons/More Tabs/SubTabGroup", "B", false, 0f)]
		public void SubButtonC()
		{
		}
	}
}

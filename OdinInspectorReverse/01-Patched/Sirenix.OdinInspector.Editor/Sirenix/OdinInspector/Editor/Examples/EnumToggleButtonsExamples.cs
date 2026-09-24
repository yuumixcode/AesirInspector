using System;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(EnumToggleButtonsAttribute))]
	internal class EnumToggleButtonsExamples
	{
		public enum SomeEnum
		{
			First,
			Second,
			Third,
			Fourth,
			AndSoOn
		}

		public enum SomeEnumWithIcons
		{
			[LabelText(SdfIconType.TextLeft)]
			TextLeft,
			[LabelText(SdfIconType.TextCenter)]
			TextCenter,
			[LabelText(SdfIconType.TextRight)]
			TextRight
		}

		public enum SomeEnumWithIconsAndNames
		{
			[LabelText("Align Left", SdfIconType.TextLeft)]
			TextLeft,
			[LabelText("Align Center", SdfIconType.TextCenter)]
			TextCenter,
			[LabelText("Align Right", SdfIconType.TextRight)]
			TextRight
		}

		[Flags]
		public enum SomeBitmaskEnum
		{
			A = 2,
			B = 4,
			C = 8,
			All = 0xE
		}

		[Title("Default", null, TitleAlignments.Left, true, true)]
		public SomeBitmaskEnum DefaultEnumBitmask;

		[EnumToggleButtons]
		[Title("Standard Enum", null, TitleAlignments.Left, true, true)]
		public SomeEnum SomeEnumField;

		[EnumToggleButtons]
		[HideLabel]
		public SomeEnum WideEnumField;

		[Title("Bitmask Enum", null, TitleAlignments.Left, true, true)]
		[EnumToggleButtons]
		public SomeBitmaskEnum BitmaskEnumField;

		[HideLabel]
		[EnumToggleButtons]
		public SomeBitmaskEnum EnumFieldWide;

		[HideLabel]
		[EnumToggleButtons]
		[Title("Icon Enum", null, TitleAlignments.Left, true, true)]
		public SomeEnumWithIcons EnumWithIcons;

		[EnumToggleButtons]
		[HideLabel]
		public SomeEnumWithIconsAndNames EnumWithIconsAndNames;
	}
}

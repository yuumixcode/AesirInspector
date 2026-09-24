using System;
using System.Runtime.InteropServices;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ButtonGroupAttribute))]
	[AttributeExample(typeof(ButtonAttribute), Order = 10f)]
	internal class ButtonGroupExamples
	{
		[Serializable]
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		[HideLabel]
		public struct IconButtonGroupExamples
		{
			[ButtonGroup("_DefaultGroup", 0f, ButtonHeight = 25)]
			[Button(SdfIconType.ArrowsMove, "")]
			private void ArrowsMove()
			{
			}

			[ButtonGroup("_DefaultGroup", 0f)]
			[Button(SdfIconType.Crop, "")]
			private void Crop()
			{
			}

			[ButtonGroup("_DefaultGroup", 0f)]
			[Button(SdfIconType.TextLeft, "")]
			private void TextLeft()
			{
			}

			[ButtonGroup("_DefaultGroup", 0f)]
			[Button(SdfIconType.TextRight, "")]
			private void TextRight()
			{
			}

			[ButtonGroup("_DefaultGroup", 0f)]
			[Button(SdfIconType.TextParagraph, "")]
			private void TextParagraph()
			{
			}

			[ButtonGroup("_DefaultGroup", 0f)]
			[Button(SdfIconType.Textarea, "")]
			private void Textarea()
			{
			}
		}

		public IconButtonGroupExamples iconButtonGroupExamples;

		[ButtonGroup("_DefaultGroup", 0f)]
		private void A()
		{
		}

		[ButtonGroup("_DefaultGroup", 0f)]
		private void B()
		{
		}

		[ButtonGroup("_DefaultGroup", 0f)]
		private void C()
		{
		}

		[ButtonGroup("_DefaultGroup", 0f)]
		private void D()
		{
		}

		[Button(ButtonSizes.Large)]
		[ButtonGroup("My Button Group", 0f)]
		private void E()
		{
		}

		[ButtonGroup("My Button Group", 0f)]
		[GUIColor(0f, 1f, 0f, 1f)]
		private void F()
		{
		}
	}
}

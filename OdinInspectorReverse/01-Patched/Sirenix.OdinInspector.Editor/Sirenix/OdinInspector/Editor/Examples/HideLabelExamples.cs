using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(HideLabelAttribute))]
	internal class HideLabelExamples
	{
		[Title("Wide Colors", null, TitleAlignments.Left, true, true)]
		[HideLabel]
		[ColorPalette("Fall")]
		public Color WideColor1;

		[HideLabel]
		[ColorPalette("Fall")]
		public Color WideColor2;

		[HideLabel]
		[Title("Wide Vector", null, TitleAlignments.Left, true, true)]
		public Vector3 WideVector1;

		[HideLabel]
		public Vector4 WideVector2;

		[HideLabel]
		[Title("Wide String", null, TitleAlignments.Left, true, true)]
		public string WideString;

		[Title("Wide Multiline Text Field", null, TitleAlignments.Left, true, true)]
		[HideLabel]
		[MultiLineProperty(3)]
		public string WideMultilineTextField = "";
	}
}

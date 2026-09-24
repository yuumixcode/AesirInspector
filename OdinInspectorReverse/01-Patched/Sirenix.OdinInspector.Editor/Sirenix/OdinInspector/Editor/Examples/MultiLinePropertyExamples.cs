using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(TextAreaAttribute))]
	[AttributeExample(typeof(MultilineAttribute))]
	[AttributeExample(typeof(MultiLinePropertyAttribute))]
	internal class MultiLinePropertyExamples
	{
		[TextArea(4, 10)]
		public string UnityTextAreaField = "";

		[Multiline(10)]
		public string UnityMultilineField = "";

		[HideLabel]
		[MultiLineProperty(10)]
		[Title("Wide Multiline Text Field", null, TitleAlignments.Left, true, false)]
		public string WideMultilineTextField = "";

		[MultiLineProperty(10)]
		[ShowInInspector]
		[InfoBox("Odin supports properties, but Unity's own Multiline attribute only works on fields.", InfoMessageType.Info, null)]
		public string OdinMultilineProperty { get; set; }
	}
}

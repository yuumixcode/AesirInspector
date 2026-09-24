using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(DisableInEditorModeAttribute))]
	internal class DisableInEditorModeExamples
	{
		[Title("Disabled in edit mode", null, TitleAlignments.Left, true, true)]
		[DisableInEditorMode]
		public GameObject A;

		[DisableInEditorMode]
		public Material B;
	}
}

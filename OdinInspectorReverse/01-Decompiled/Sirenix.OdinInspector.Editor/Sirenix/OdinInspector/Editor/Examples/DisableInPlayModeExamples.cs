using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(DisableInPlayModeAttribute))]
	internal class DisableInPlayModeExamples
	{
		[Title("Disabled in play mode", null, TitleAlignments.Left, true, true)]
		[DisableInPlayMode]
		public int A;

		[DisableInPlayMode]
		public Material B;
	}
}

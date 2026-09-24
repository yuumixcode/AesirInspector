using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(InfoBoxAttribute), "This example demonstrates the use of the InfoBox attribute.\nAny info box with a warning or error drawn in the inspector will also be found by the Scene Validation tool.")]
	internal class InfoBoxExamples
	{
		[InfoBox("Default info box.", InfoMessageType.Info, null)]
		[Title("InfoBox message types", null, TitleAlignments.Left, true, true)]
		public int A;

		[InfoBox("Warning info box.", InfoMessageType.Warning, null)]
		public int B;

		[InfoBox("Error info box.", InfoMessageType.Error, null)]
		public int C;

		[InfoBox("Info box without an icon.", InfoMessageType.None, null)]
		public int D;

		[Title("Conditional info boxes", null, TitleAlignments.Left, true, true)]
		public bool ToggleInfoBoxes;

		[InfoBox("This info box is only shown while in editor mode.", InfoMessageType.Error, "IsInEditMode")]
		public float G;

		[InfoBox("This info box is hideable by a static field.", "ToggleInfoBoxes")]
		public float E;

		[InfoBox("This info box is hideable by a static field.", "ToggleInfoBoxes")]
		public float F;

		[Title("Info box member reference and attribute expressions", null, TitleAlignments.Left, true, true)]
		[InfoBox("$InfoBoxMessage", InfoMessageType.Info, null)]
		[InfoBox("@\"Time: \" + DateTime.Now.ToString(\"HH:mm:ss\")", InfoMessageType.Info, null)]
		public string InfoBoxMessage = "My dynamic info box message";

		private static bool IsInEditMode()
		{
			return !Application.isPlaying;
		}
	}
}

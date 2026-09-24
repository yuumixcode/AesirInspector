using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(DrawWithUnityAttribute))]
	internal class DrawWithUnityExamples
	{
		[InfoBox("If you ever experience trouble with one of Odin's attributes, there is a good chance that DrawWithUnity will come in handy; it will make Odin draw the value as Unity normally would.", InfoMessageType.Info, null)]
		public GameObject ObjectDrawnWithOdin;

		[DrawWithUnity]
		public GameObject ObjectDrawnWithUnity;
	}
}

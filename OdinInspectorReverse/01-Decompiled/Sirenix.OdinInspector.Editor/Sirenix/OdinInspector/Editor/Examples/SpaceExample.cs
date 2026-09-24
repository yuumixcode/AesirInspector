using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(PropertySpaceAttribute))]
	[AttributeExample(typeof(SpaceAttribute))]
	internal class SpaceExample
	{
		[BoxGroup("Space", true, false, 0f, ShowLabel = false)]
		[Space]
		public int Space;

		[PropertySpace(SpaceBefore = 30f, SpaceAfter = 60f)]
		[BoxGroup("BeforeAndAfter", true, false, 0f, ShowLabel = false)]
		public int BeforeAndAfter;

		[BoxGroup("Property", true, false, 0f, ShowLabel = false)]
		[ShowInInspector]
		[PropertySpace]
		public string Property { get; set; }
	}
}

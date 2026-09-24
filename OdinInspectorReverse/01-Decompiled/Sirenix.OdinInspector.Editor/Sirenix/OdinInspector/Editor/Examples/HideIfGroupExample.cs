using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(HideIfGroupAttribute))]
	internal class HideIfGroupExample
	{
		public bool Toggle = true;

		[BoxGroup("Toggle/Shown Box", true, false, 0f)]
		[HideIfGroup("Toggle", true)]
		public int A;

		[BoxGroup("Toggle/Shown Box", true, false, 0f)]
		[HideIfGroup("Toggle", true)]
		public int B;

		[BoxGroup("Box", true, false, 0f)]
		public InfoMessageType EnumField = InfoMessageType.Info;

		[BoxGroup("Box", true, false, 0f)]
		[HideIfGroup("Box/Toggle", true)]
		public Vector3 X;

		[BoxGroup("Box", true, false, 0f)]
		[HideIfGroup("Box/Toggle", true)]
		public Vector3 Y;

		[HideIfGroup("Box/Toggle/EnumField", true, Value = InfoMessageType.Info)]
		[BoxGroup("Box/Toggle/EnumField/Border", true, false, 0f, ShowLabel = false)]
		public string Name;

		[BoxGroup("Box/Toggle/EnumField/Border", true, false, 0f)]
		public Vector3 Vector;

		[HideIfGroup("RectGroup", true, Condition = "Toggle")]
		public Rect Rect;
	}
}

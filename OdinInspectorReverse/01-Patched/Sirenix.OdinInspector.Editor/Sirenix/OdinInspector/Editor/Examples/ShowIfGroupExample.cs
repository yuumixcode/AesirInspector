using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ShowIfGroupAttribute))]
	internal class ShowIfGroupExample
	{
		public bool Toggle = true;

		[ShowIfGroup("Toggle", true)]
		[BoxGroup("Toggle/Shown Box", true, false, 0f)]
		public int A;

		[BoxGroup("Toggle/Shown Box", true, false, 0f)]
		[ShowIfGroup("Toggle", true)]
		public int B;

		[BoxGroup("Box", true, false, 0f)]
		public InfoMessageType EnumField = InfoMessageType.Info;

		[ShowIfGroup("Box/Toggle", true)]
		[BoxGroup("Box", true, false, 0f)]
		public Vector3 X;

		[ShowIfGroup("Box/Toggle", true)]
		[BoxGroup("Box", true, false, 0f)]
		public Vector3 Y;

		[BoxGroup("Box/Toggle/EnumField/Border", true, false, 0f, ShowLabel = false)]
		[ShowIfGroup("Box/Toggle/EnumField", true, Value = InfoMessageType.Info)]
		public string Name;

		[BoxGroup("Box/Toggle/EnumField/Border", true, false, 0f)]
		public Vector3 Vector;

		[ShowIfGroup("RectGroup", true, Condition = "Toggle")]
		public Rect Rect;
	}
}

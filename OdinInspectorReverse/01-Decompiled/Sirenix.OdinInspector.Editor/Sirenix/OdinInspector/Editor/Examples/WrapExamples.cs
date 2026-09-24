using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(WrapAttribute))]
	internal class WrapExamples
	{
		[Wrap(0.0, 100.0)]
		public int IntWrapFrom0To100;

		[Wrap(0.0, 100.0)]
		public float FloatWrapFrom0To100;

		[Wrap(0.0, 100.0)]
		public Vector3 Vector3WrapFrom0To100;

		[Wrap(0.0, 360.0)]
		public float AngleWrap;

		[Wrap(0.0, 6.2831854820251465)]
		public float RadianWrap;
	}
}

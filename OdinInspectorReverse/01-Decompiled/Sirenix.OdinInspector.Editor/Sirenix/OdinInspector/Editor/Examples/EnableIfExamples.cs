using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(EnableIfAttribute))]
	internal class EnableIfExamples
	{
		public Object SomeObject;

		[EnumToggleButtons]
		public InfoMessageType SomeEnum;

		public bool IsToggled;

		[EnableIf("SomeEnum", InfoMessageType.Info)]
		public Vector2 Info;

		[EnableIf("SomeEnum", InfoMessageType.Error)]
		public Vector2 Error;

		[EnableIf("SomeEnum", InfoMessageType.Warning)]
		public Vector2 Warning;

		[EnableIf("IsToggled")]
		public int EnableIfToggled;

		[EnableIf("SomeObject")]
		public Vector3 EnabledWhenHasReference;

		[EnableIf("@this.IsToggled && this.SomeObject != null || this.SomeEnum == InfoMessageType.Error")]
		public int EnableWithExpression;
	}
}

using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(HideIfAttribute))]
	[AttributeExample(typeof(ShowIfAttribute))]
	internal class ShowAndHideIfExamples
	{
		public Object SomeObject;

		[EnumToggleButtons]
		public InfoMessageType SomeEnum;

		public bool IsToggled;

		[ShowIf("SomeEnum", InfoMessageType.Info, true)]
		public Vector3 Info;

		[ShowIf("SomeEnum", InfoMessageType.Warning, true)]
		public Vector2 Warning;

		[ShowIf("SomeEnum", InfoMessageType.Error, true)]
		public Vector3 Error;

		[ShowIf("IsToggled", true)]
		public Vector2 VisibleWhenToggled;

		[HideIf("IsToggled", true)]
		public Vector3 HiddenWhenToggled;

		[HideIf("SomeObject", true)]
		public Vector3 ShowWhenNull;

		[ShowIf("SomeObject", true)]
		public Vector3 HideWhenNull;

		[EnableIf("@this.IsToggled && this.SomeObject != null || this.SomeEnum == InfoMessageType.Error")]
		public int ShowWithExpression;
	}
}

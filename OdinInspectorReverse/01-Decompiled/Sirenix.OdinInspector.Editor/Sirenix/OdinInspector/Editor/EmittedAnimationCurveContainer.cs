using System.Reflection;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public class EmittedAnimationCurveContainer : EmittedScriptableObject<AnimationCurve>
	{
		public AnimationCurve value;

		public override FieldInfo BackingFieldInfo => typeof(EmittedAnimationCurveContainer).GetField("value");

		public override AnimationCurve GetValue()
		{
			return value;
		}

		public override void SetValue(AnimationCurve value)
		{
			this.value = value;
		}
	}
}

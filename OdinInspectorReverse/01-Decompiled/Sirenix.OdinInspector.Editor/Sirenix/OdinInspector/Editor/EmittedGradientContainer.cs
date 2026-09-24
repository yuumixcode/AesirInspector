using System.Reflection;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public class EmittedGradientContainer : EmittedScriptableObject<Gradient>
	{
		public Gradient value;

		public override FieldInfo BackingFieldInfo => typeof(EmittedGradientContainer).GetField("value");

		public override Gradient GetValue()
		{
			return value;
		}

		public override void SetValue(Gradient value)
		{
			this.value = value;
		}
	}
}

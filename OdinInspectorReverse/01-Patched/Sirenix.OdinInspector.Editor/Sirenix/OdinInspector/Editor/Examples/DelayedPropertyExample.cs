using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(DelayedAttribute))]
	[AttributeExample(typeof(DelayedPropertyAttribute))]
	internal class DelayedPropertyExample
	{
		[OnValueChanged("OnValueChanged", false)]
		[Delayed]
		public int DelayedField;

		[ShowInInspector]
		[OnValueChanged("OnValueChanged", false)]
		[DelayedProperty]
		public string DelayedProperty { get; set; }

		private void OnValueChanged()
		{
			Debug.Log("Value changed!");
		}
	}
}

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(OnStateUpdateAttribute), "The following example shows how OnStateUpdate can be used to control the visible state of a property.")]
	internal class VisibleStateExample
	{
		[OnStateUpdate("@$property.State.Visible = ToggleMyInt")]
		public int MyInt;

		public bool ToggleMyInt;
	}
}

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(EnableGUIAttribute))]
	internal class EnableGUIExample
	{
		[ShowInInspector]
		public int GUIDisabledProperty => 10;

		[ShowInInspector]
		[EnableGUI]
		public int GUIEnabledProperty => 10;
	}
}

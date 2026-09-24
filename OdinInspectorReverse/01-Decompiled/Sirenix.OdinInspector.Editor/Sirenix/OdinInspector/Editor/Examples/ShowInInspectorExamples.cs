namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ShowInInspectorAttribute), "ShowInInspector is used to display properties that otherwise wouldn't be shown in the inspector, such as non-serialized fields or properties.")]
	internal class ShowInInspectorExamples
	{
		[ShowInInspector]
		private int myPrivateInt;

		[ShowInInspector]
		public int MyPropertyInt { get; set; }

		[ShowInInspector]
		public int ReadOnlyProperty => myPrivateInt;

		[ShowInInspector]
		public static bool StaticProperty { get; set; }
	}
}

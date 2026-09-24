namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(HideReferenceObjectPickerAttribute), "When the object picker is hidden, you can right click and set the instance to null, in order to set a new value.\n\nIf you don't want this behavior, you can use DisableContextMenu attribute to ensure people can't change the value.")]
	[ShowOdinSerializedPropertiesInInspector]
	internal class HideReferenceObjectPickerExamples
	{
		public class MyCustomReferenceType
		{
			public int A;

			public int B;

			public int C;
		}

		[Title("Hidden Object Pickers", null, TitleAlignments.Left, true, true)]
		[HideReferenceObjectPicker]
		public MyCustomReferenceType OdinSerializedProperty1 = new MyCustomReferenceType();

		[HideReferenceObjectPicker]
		public MyCustomReferenceType OdinSerializedProperty2 = new MyCustomReferenceType();

		[Title("Shown Object Pickers", null, TitleAlignments.Left, true, true)]
		public MyCustomReferenceType OdinSerializedProperty3 = new MyCustomReferenceType();

		public MyCustomReferenceType OdinSerializedProperty4 = new MyCustomReferenceType();
	}
}

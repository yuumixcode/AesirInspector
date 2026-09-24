namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(EnumPagingAttribute))]
	internal class EnumPagingExamples
	{
		public enum SomeEnum
		{
			A,
			B,
			C
		}

		[EnumPaging]
		public SomeEnum SomeEnumField;
	}
}

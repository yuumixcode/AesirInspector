namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(OnInspectorDisposeAttribute), "The following example demonstrates how OnInspectorDispose works.")]
	internal class OnInspectorDisposeExamples
	{
		public abstract class BaseClass
		{
			public override string ToString()
			{
				return GetType().Name;
			}
		}

		public class A : BaseClass
		{
		}

		public class B : BaseClass
		{
		}

		public class C : BaseClass
		{
		}

		[OnInspectorDispose("@UnityEngine.Debug.Log(\"Dispose event invoked!\")")]
		[ShowInInspector]
		[InfoBox("When you change the type of this field, or set it to null, the former property setup is disposed. The property setup will also be disposed when you deselect this example.", InfoMessageType.Info, null)]
		[DisplayAsString]
		public BaseClass PolymorphicField;
	}
}

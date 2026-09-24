using System;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(OnInspectorInitAttribute), "The following example demonstrates how OnInspectorInit works.")]
	[ExampleAsComponentData(Namespaces = new string[] { "System", "Sirenix.Utilities.Editor" })]
	internal class OnInspectorInitExamples
	{
		[OnInspectorInit("@TimeWhenExampleWasOpened = DateTime.Now.ToString()")]
		public string TimeWhenExampleWasOpened;

		[FoldoutGroup("Delayed Initialization", 0f, Expanded = false, HideWhenChildrenAreInvisible = false)]
		[OnInspectorInit("@TimeFoldoutWasOpened = DateTime.Now.ToString()")]
		public string TimeFoldoutWasOpened;

		[ShowInInspector]
		[DisplayAsString]
		[PropertyOrder(-1f)]
		public string CurrentTime
		{
			get
			{
				GUIHelper.RequestRepaint();
				return DateTime.Now.ToString();
			}
		}
	}
}

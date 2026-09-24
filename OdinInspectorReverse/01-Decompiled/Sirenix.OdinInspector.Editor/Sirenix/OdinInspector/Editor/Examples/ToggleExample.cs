using System;
using Sirenix.OdinInspector.Editor.Examples.Internal;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ToggleAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "System" })]
	internal class ToggleExample
	{
		[Serializable]
		public class MyToggleable
		{
			public bool Enabled;

			public int MyValue;
		}

		[Serializable]
		[Toggle("Enabled")]
		public class ToggleableClass
		{
			public bool Enabled;

			public string Text;
		}

		[Toggle("Enabled")]
		public MyToggleable Toggler = new MyToggleable();

		public ToggleableClass Toggleable = new ToggleableClass();
	}
}

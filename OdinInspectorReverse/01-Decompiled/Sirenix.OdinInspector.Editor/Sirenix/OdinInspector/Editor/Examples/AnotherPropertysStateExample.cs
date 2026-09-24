using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Examples.Internal;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(OnStateUpdateAttribute), "The following example shows how OnStateUpdate can be used to control the state of another property.")]
	[ExampleAsComponentData(Namespaces = new string[] { "System.Collections.Generic" })]
	internal class AnotherPropertysStateExample
	{
		public List<string> list;

		[OnStateUpdate("@#(list).State.Expanded = $value")]
		public bool ExpandList;
	}
}

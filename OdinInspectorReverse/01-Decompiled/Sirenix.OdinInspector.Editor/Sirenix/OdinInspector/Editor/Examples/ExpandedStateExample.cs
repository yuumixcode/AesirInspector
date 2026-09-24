using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Examples.Internal;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[ExampleAsComponentData(Namespaces = new string[] { "System.Collections.Generic" })]
	[AttributeExample(typeof(OnStateUpdateAttribute), "The following example shows how OnStateUpdate can be used to control the expanded state of a list.")]
	internal class ExpandedStateExample
	{
		[OnStateUpdate("@$property.State.Expanded = ExpandList")]
		public List<string> list;

		public bool ExpandList;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor.Examples.Internal;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[Searchable]
	[ExampleAsComponentData(Namespaces = new string[] { "System", "System.Linq", "System.Collections.Generic", "Sirenix.OdinInspector.Editor.Examples" }, AttributeDeclarations = new string[] { "Searchable" })]
	[AttributeExample(typeof(SearchableAttribute), "The Searchable attribute can be applied to a root inspected type, like a Component, ScriptableObject or OdinEditorWindow, to make the whole type searchable.")]
	internal class SearchableInspectorExample
	{
		[Serializable]
		public struct ExampleStruct
		{
			public string Name;

			public int Number;

			public ExampleEnum Enum;

			public ExampleStruct(int nr)
			{
				this = default(ExampleStruct);
				Name = "Element " + nr;
				Number = nr;
				Enum = (ExampleEnum)ExampleHelper.RandomInt(0, 5);
			}
		}

		public enum ExampleEnum
		{
			One,
			Two,
			Three,
			Four,
			Five
		}

		public List<string> strings = new List<string>(from i in Enumerable.Range(1, 10)
			select "Str Element " + i);

		public List<ExampleStruct> searchableList = new List<ExampleStruct>(from i in Enumerable.Range(1, 10)
			select new ExampleStruct(i));
	}
}

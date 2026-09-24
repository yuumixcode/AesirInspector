using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Examples.Internal;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(HideInTablesAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "System", "System.Collections.Generic" })]
	internal class HideInTablesExample
	{
		[Serializable]
		public class MyItem
		{
			public string A;

			public int B;

			[HideInTables]
			public int Hidden;
		}

		public MyItem Item = new MyItem();

		[TableList]
		public List<MyItem> Table = new List<MyItem>
		{
			new MyItem(),
			new MyItem(),
			new MyItem()
		};
	}
}

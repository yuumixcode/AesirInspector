using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(TableListAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "System", "System.Collections.Generic", "Sirenix.OdinInspector.Editor.Examples" })]
	internal class TableListExamples
	{
		[Serializable]
		public class SomeCustomClass
		{
			private readonly int iconVariant;

			[PreviewField(Alignment = ObjectFieldAlignment.Center)]
			[TableColumnWidth(57, true, Resizable = false)]
			public Texture Icon;

			[TextArea]
			public string Description;

			[LabelWidth(22f)]
			[VerticalGroup("Combined Column", 0f)]
			public string A;

			[VerticalGroup("Combined Column", 0f)]
			[LabelWidth(22f)]
			public string B;

			[VerticalGroup("Combined Column", 0f)]
			[LabelWidth(22f)]
			public string C;

			public SomeCustomClass()
			{
			}

			public SomeCustomClass(int iconVariant)
			{
				this.iconVariant = iconVariant;
			}

			[TableColumnWidth(60, true)]
			[Button]
			[VerticalGroup("Actions", 0f)]
			public void Test1()
			{
			}

			[Button]
			[VerticalGroup("Actions", 0f)]
			[TableColumnWidth(60, true)]
			public void Test2()
			{
			}

			[OnInspectorInit]
			private void CreateData()
			{
				Description = ExampleHelper.GetString();
				Icon = ExampleHelper.GetTexture(iconVariant);
			}
		}

		[TableList(ShowIndexLabels = true)]
		public List<SomeCustomClass> TableListWithIndexLabels = new List<SomeCustomClass>
		{
			new SomeCustomClass(0),
			new SomeCustomClass(1)
		};

		[TableList(DrawScrollView = true, MaxScrollViewHeight = 200, MinScrollViewHeight = 100)]
		public List<SomeCustomClass> MinMaxScrollViewTable = new List<SomeCustomClass>
		{
			new SomeCustomClass(2),
			new SomeCustomClass(3)
		};

		[TableList(AlwaysExpanded = true, DrawScrollView = false)]
		public List<SomeCustomClass> AlwaysExpandedTable = new List<SomeCustomClass>
		{
			new SomeCustomClass(4),
			new SomeCustomClass(5)
		};

		[TableList(ShowPaging = true)]
		public List<SomeCustomClass> TableWithPaging = new List<SomeCustomClass>
		{
			new SomeCustomClass(6),
			new SomeCustomClass(7)
		};
	}
}

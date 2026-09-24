using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor.Examples.Internal;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[ExampleAsComponentData(Namespaces = new string[] { "System", "System.Linq", "System.Collections.Generic", "Sirenix.OdinInspector.Editor.Examples" })]
	[AttributeExample(typeof(SearchableAttribute), "The Searchable attribute can be applied to individual members in a type, to make only that member searchable.")]
	internal class SearchableMembersExample
	{
		[Serializable]
		public class ExampleClass
		{
			public string SomeString = "Saehrimnir is a tasty delicacy";

			public int SomeInt = 13579;

			public DataContainer DataContainerOne = new DataContainer
			{
				Name = "Example Data Set One"
			};

			public DataContainer DataContainerTwo = new DataContainer
			{
				Name = "Example Data Set Two"
			};
		}

		[Serializable]
		[Searchable]
		public class DataContainer
		{
			public string Name;

			public List<ExampleStruct> Data = new List<ExampleStruct>(from i in Enumerable.Range(1, 10)
				select new ExampleStruct(i));
		}

		[Serializable]
		public struct FilterableBySquareStruct : ISearchFilterable
		{
			public int Number;

			[EnableGUI]
			[DisplayAsString]
			[ShowInInspector]
			public int Square => Number * Number;

			public FilterableBySquareStruct(int nr)
			{
				Number = nr;
			}

			public bool IsMatch(string searchString)
			{
				return searchString.Contains(Square.ToString());
			}
		}

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

		[Searchable]
		public ExampleClass searchableClass = new ExampleClass();

		[Searchable]
		public List<ExampleStruct> searchableList = new List<ExampleStruct>(from i in Enumerable.Range(1, 10)
			select new ExampleStruct(i));

		[Searchable(FilterOptions = SearchFilterOptions.ISearchFilterableInterface)]
		public List<FilterableBySquareStruct> customFiltering = new List<FilterableBySquareStruct>(from i in Enumerable.Range(1, 10)
			select new FilterableBySquareStruct(i));
	}
}

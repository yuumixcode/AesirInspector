using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor
{
	public class SearchResult
	{
		public InspectorProperty MatchedProperty;

		public List<SearchResult> ChildResults = new List<SearchResult>();
	}
}

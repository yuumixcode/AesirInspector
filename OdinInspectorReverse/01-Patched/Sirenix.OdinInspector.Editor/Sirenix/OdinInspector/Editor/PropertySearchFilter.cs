using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public class PropertySearchFilter
	{
		protected static readonly Func<string, string, bool> FuzzyStringMatcher = FuzzySearch.Contains;

		protected static readonly Func<string, string, bool> ExactStringMatcher = (string search, string str) => str.Contains(search);

		public bool Recursive = true;

		public bool UseFuzzySearch = true;

		public SearchFilterOptions FilterOptions = SearchFilterOptions.All;

		public Func<InspectorProperty, string, bool> MatchFunctionOverride;

		public InspectorProperty Property;

		public string SearchTerm;

		public List<SearchResult> SearchResults;

		protected string searchFieldControlName = "PropertySearchFilter_" + Guid.NewGuid();

		public bool HasSearchResults => SearchResults != null;

		public PropertySearchFilter()
		{
		}

		public PropertySearchFilter(InspectorProperty property)
		{
			Property = property;
		}

		public PropertySearchFilter(InspectorProperty property, SearchableAttribute config)
		{
			Property = property;
			UseFuzzySearch = config.FuzzySearch;
			Recursive = config.Recursive;
			FilterOptions = config.FilterOptions;
		}

		public virtual void UpdateSearch(string searchFilter)
		{
			SearchTerm = searchFilter;
			UpdateSearch();
		}

		public virtual void UpdateSearch()
		{
			SearchResults = Search(SearchTerm);
		}

		public virtual List<SearchResult> Search(string searchTerm)
		{
			if (string.IsNullOrEmpty(SearchTerm))
			{
				return null;
			}
			List<SearchResult> results = new List<SearchResult>();
			if (Recursive)
			{
				foreach (InspectorProperty child in Property.Children)
				{
					results.AddRange(Search(searchTerm, child));
				}
			}
			else
			{
				foreach (InspectorProperty child2 in Property.Children)
				{
					if (IsMatch(child2, searchTerm))
					{
						results.Add(new SearchResult
						{
							MatchedProperty = child2
						});
					}
				}
			}
			return results;
		}

		protected virtual IEnumerable<SearchResult> Search(string searchTerm, InspectorProperty property)
		{
			if (IsMatch(property, searchTerm))
			{
				SearchResult result = new SearchResult();
				result.MatchedProperty = property;
				foreach (InspectorProperty child in property.Children)
				{
					result.ChildResults.AddRange(Search(searchTerm, child));
				}
				yield return result;
				yield break;
			}
			foreach (InspectorProperty child2 in property.Children)
			{
				foreach (SearchResult item in Search(searchTerm, child2))
				{
					yield return item;
				}
			}
		}

		public virtual bool IsMatch(InspectorProperty property, string searchTerm)
		{
			if (MatchFunctionOverride != null)
			{
				return MatchFunctionOverride(property, searchTerm);
			}
			if (property.Name == "InternalOnInspectorGUI")
			{
				return false;
			}
			if (property.Info.PropertyType == PropertyType.Group && (property.Name == "#_DefaultTabGroup" || property.Name == "#_DefaultBoxGroup"))
			{
				return false;
			}
			Func<string, string, bool> stringMatcher = (UseFuzzySearch ? FuzzyStringMatcher : ExactStringMatcher);
			if (!property.ParentType.IsGenericType || !(property.ParentType.GetGenericTypeDefinition() == typeof(EditableKeyValuePair<, >)))
			{
				if (HasSearchFlag(SearchFilterOptions.PropertyName) && stringMatcher(searchTerm, property.Name))
				{
					return true;
				}
				if (HasSearchFlag(SearchFilterOptions.PropertyNiceName) && stringMatcher(searchTerm, property.NiceName))
				{
					return true;
				}
			}
			if (property.ValueEntry != null)
			{
				if (HasSearchFlag(SearchFilterOptions.TypeOfValue))
				{
					if (TypeRegistry.TryGetCustomName(property.ValueEntry.TypeOfValue, out var customName) && stringMatcher(searchTerm, customName))
					{
						return true;
					}
					if (stringMatcher(searchTerm, property.ValueEntry.TypeOfValue.GetNiceFullName()))
					{
						return true;
					}
				}
				object value = property.ValueEntry.WeakSmartValue;
				if (HasSearchFlag(SearchFilterOptions.ISearchFilterableInterface) && value is ISearchFilterable)
				{
					return (value as ISearchFilterable).IsMatch(searchTerm);
				}
				if (HasSearchFlag(SearchFilterOptions.ValueToString))
				{
					string valueString = ((value == null) ? "null" : value.ToString());
					if (stringMatcher(searchTerm, valueString))
					{
						return true;
					}
				}
			}
			return false;
		}

		public virtual bool HasSearchFlag(SearchFilterOptions flag)
		{
			return (FilterOptions & flag) == flag;
		}

		public virtual void DrawSearchResults()
		{
			if (SearchResults == null)
			{
				return;
			}
			InspectorProperty lastParent = Property;
			for (int i = 0; i < SearchResults.Count; i++)
			{
				SearchResult result = SearchResults[i];
				result.MatchedProperty.Update();
				bool indented = false;
				if (result.MatchedProperty.Parent != null && result.MatchedProperty.DrawCount == 0)
				{
					EditorGUI.indentLevel++;
					indented = true;
					if (result.MatchedProperty.Parent != lastParent)
					{
						InspectorProperty current = result.MatchedProperty.Parent;
						string deltaPath = BuildNiceRelativePath("", current);
						while (true)
						{
							current = current.Parent;
							if (current == null || current == Property)
							{
								break;
							}
							deltaPath = BuildNiceRelativePath(deltaPath, current);
						}
						Rect labelRect;
						if (!string.IsNullOrEmpty(deltaPath))
						{
							labelRect = EditorGUILayout.GetControlRect();
							GUI.Label(labelRect, GUIHelper.TempContent(deltaPath), EditorStyles.miniBoldLabel);
						}
						else
						{
							labelRect = EditorGUILayout.GetControlRect(true, 2f);
						}
						SirenixEditorGUI.DrawHorizontalLineSeperator(labelRect.xMin, labelRect.yMax - 0.5f, labelRect.width);
						GUILayout.Space(3f);
					}
				}
				DrawSearchResult(result);
				if (indented)
				{
					EditorGUI.indentLevel--;
				}
				lastParent = result.MatchedProperty.Parent;
			}
		}

		private static string BuildNiceRelativePath(string path, InspectorProperty addProperty)
		{
			if (addProperty.IsTreeRoot)
			{
				return path;
			}
			if (addProperty.Info.PropertyType == PropertyType.Group && (addProperty.Name == "#_DefaultTabGroup" || addProperty.Name == "#_DefaultBoxGroup"))
			{
				return path;
			}
			string niceName = addProperty.NiceName;
			if (addProperty.Info.PropertyType == PropertyType.Group)
			{
				niceName = niceName.TrimStart(new char[1] { '#' });
			}
			if (string.IsNullOrEmpty(path))
			{
				return niceName;
			}
			return niceName + " > " + path;
		}

		public virtual void DrawSearchResult(SearchResult result)
		{
			result.MatchedProperty.Update();
			if (result.MatchedProperty.DrawCount == 0)
			{
				result.MatchedProperty.Draw();
			}
			InspectorProperty lastDeltaParent = null;
			for (int i = 0; i < result.ChildResults.Count; i++)
			{
				SearchResult childResult = result.ChildResults[i];
				EditorGUI.indentLevel++;
				if (childResult.MatchedProperty.Parent != result.MatchedProperty && childResult.MatchedProperty.Parent != lastDeltaParent)
				{
					InspectorProperty current = childResult.MatchedProperty.Parent;
					string deltaPath = BuildNiceRelativePath("", current);
					while (true)
					{
						current = current.Parent;
						if (current == null || current == Property)
						{
							break;
						}
						deltaPath = BuildNiceRelativePath(deltaPath, current);
					}
					Rect labelRect = EditorGUILayout.GetControlRect().AddXMin(GUIHelper.CurrentIndentAmount);
					GUI.Label(labelRect, GUIHelper.TempContent(deltaPath), SirenixGUIStyles.LeftAlignedGreyMiniLabel);
					SirenixEditorGUI.DrawHorizontalLineSeperator(labelRect.xMin, labelRect.yMax - 0.5f, labelRect.width);
					lastDeltaParent = childResult.MatchedProperty.Parent;
				}
				DrawSearchResult(childResult);
				EditorGUI.indentLevel--;
			}
		}

		public virtual void DrawDefaultSearchFieldLayout(GUIContent label)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null);
			rect = ((label == null) ? rect.AddXMin(GUIHelper.CurrentIndentAmount) : EditorGUI.PrefixLabel(rect, label));
			string newTerm = SirenixEditorGUI.SearchField(rect, SearchTerm, forceFocus: false, searchFieldControlName);
			if (newTerm != SearchTerm)
			{
				SearchTerm = newTerm;
				Property.Tree.DelayActionUntilRepaint(delegate
				{
					UpdateSearch();
					GUIHelper.RequestRepaint();
				});
			}
			Rect separatorRect = EditorGUILayout.GetControlRect(true, 3f);
			SirenixEditorGUI.DrawThickHorizontalSeperator(separatorRect.AddXMin(GUIHelper.CurrentIndentAmount));
			GUILayout.Space(2f);
		}
	}
}

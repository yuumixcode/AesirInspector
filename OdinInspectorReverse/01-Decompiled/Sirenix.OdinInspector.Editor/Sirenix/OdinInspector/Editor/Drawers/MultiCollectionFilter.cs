using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public class MultiCollectionFilter<TResolver> : IDisposable where TResolver : ICollectionResolver
	{
		public class IndexNotFoundException : Exception
		{
			public IndexNotFoundException(InspectorProperty expectedProperty, int index)
				: base($"Couldn't find the index of a filtered property: {expectedProperty} @ {index}")
			{
			}
		}

		public readonly bool IsUsed;

		public PropertySearchFilter Filter;

		internal SearchField SearchField;

		internal string ControlName;

		internal InspectorProperty Property;

		internal TResolver Resolver;

		internal PropertyChildren Children;

		internal List<InspectorProperty> FilteredChildren;

		internal int LastChildrenCount;

		internal readonly Action<CollectionChangeInfo> OnChange;

		internal readonly Action ScheduledUpdateAction;

		public bool IsFiltered
		{
			get
			{
				if (IsUsed)
				{
					return FilteredChildren.Count > 0;
				}
				return false;
			}
		}

		public InspectorProperty this[int index]
		{
			get
			{
				if (!IsFiltered)
				{
					return Children[index];
				}
				return FilteredChildren[index];
			}
		}

		public MultiCollectionFilter(InspectorProperty property, TResolver resolver)
		{
			ControlName = $"PropertyTreeSearchField_{Guid.NewGuid()}";
			Property = property;
			Children = property.Children;
			Resolver = resolver;
			LastChildrenCount = Children.Count;
			SearchableAttribute searchable = property.GetAttribute<SearchableAttribute>();
			if (searchable == null)
			{
				IsUsed = false;
				return;
			}
			SearchField = new SearchField();
			Filter = new PropertySearchFilter(null, searchable);
			FilteredChildren = new List<InspectorProperty>(Children.Count);
			ScheduledUpdateAction = delegate
			{
				Update();
				GUIHelper.RequestRepaint();
			};
			IsUsed = true;
		}

		public int GetCount()
		{
			if (Children.Count != LastChildrenCount)
			{
				Update();
				LastChildrenCount = Children.Count;
			}
			if (!IsFiltered)
			{
				return Children.Count;
			}
			return FilteredChildren.Count;
		}

		public void Draw()
		{
			if (IsUsed)
			{
				Rect rect = EditorGUILayout.GetControlRect(false).AddYMin(2f);
				if (UnityVersion.IsVersionOrGreater(2019, 3))
				{
					rect = rect.AddY(-2f);
				}
				Draw(rect);
			}
		}

		public void Draw(Rect rect)
		{
			EditorGUI.BeginChangeCheck();
			string searchTerm = SearchField.Draw(rect, Filter.SearchTerm, "Find element...");
			if (EditorGUI.EndChangeCheck())
			{
				if (!string.IsNullOrEmpty(searchTerm))
				{
					Property.State.Expanded = true;
				}
				Filter.SearchTerm = searchTerm;
				ScheduleUpdate();
			}
		}

		public void Update()
		{
			if (!IsUsed)
			{
				return;
			}
			FilteredChildren.Clear();
			if (string.IsNullOrEmpty(Filter.SearchTerm))
			{
				return;
			}
			for (int i = 0; i < Children.Count; i++)
			{
				InspectorProperty parentProperty = Children[i];
				for (int j = 0; j < parentProperty.Children.Count; j++)
				{
					InspectorProperty property = parentProperty.Children[j];
					if (Filter.IsMatch(property, Filter.SearchTerm))
					{
						FilteredChildren.Add(parentProperty);
						break;
					}
					if (!Filter.Recursive)
					{
						continue;
					}
					bool foundMatch = false;
					foreach (InspectorProperty recursiveProperty in property.Children.Recurse())
					{
						if (Filter.IsMatch(recursiveProperty, Filter.SearchTerm))
						{
							FilteredChildren.Add(parentProperty);
							foundMatch = true;
							break;
						}
					}
					if (foundMatch)
					{
						break;
					}
				}
			}
		}

		public void ScheduleUpdate()
		{
			Property.Tree.DelayActionUntilRepaint(ScheduledUpdateAction);
		}

		public void Dispose()
		{
			if (OnChange != null)
			{
				ref TResolver resolver = ref Resolver;
				Action<CollectionChangeInfo> onChange = OnChange;
				resolver.OnAfterChange -= onChange;
			}
		}

		/// <summary>
		/// Retrieves the index of a filtered item; if the collection is not filtered, it just returns the passed index.
		/// </summary>
		/// <param name="index">The index to find.</param>
		///
		/// <returns>
		/// The index in the collection of the filtered item,
		/// or the passed index if the collection is not <see cref="P:Sirenix.OdinInspector.Editor.Drawers.MultiCollectionFilter`1.IsFiltered">filtered</see>.
		/// </returns>
		///
		/// <exception cref="T:Sirenix.OdinInspector.Editor.Drawers.MultiCollectionFilter`1.IndexNotFoundException">
		/// This is thrown if it's unable to find the index in the original collection,
		/// this indicates a discrepancy between the filtered collection and the original collection.
		/// </exception>
		public int GetCollectionIndex(int index)
		{
			if (!IsFiltered)
			{
				return index;
			}
			InspectorProperty expected = FilteredChildren[index];
			for (int i = 0; i < Children.Count; i++)
			{
				if (Children[i] == expected)
				{
					return i;
				}
			}
			throw new IndexNotFoundException(expected, index);
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Handles the selection of a Odin Menu Tree with support for multi selection.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuItem" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuStyle" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTreeExtensions" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuEditorWindow" />
	public class OdinMenuTreeSelection : IList<OdinMenuItem>, ICollection<OdinMenuItem>, IEnumerable<OdinMenuItem>, IEnumerable
	{
		private readonly List<OdinMenuItem> selection;

		private bool supportsMultiSelect;

		/// <summary>
		/// Gets the count.
		/// </summary>
		public int Count => selection.Count;

		/// <summary>
		/// Gets the first selected value, returns null if non is selected.
		/// </summary>
		public object SelectedValue
		{
			get
			{
				if (selection.Count > 0)
				{
					return selection[0].Value;
				}
				return null;
			}
		}

		/// <summary>
		/// Gets all selected values.
		/// </summary>
		public IEnumerable<object> SelectedValues
		{
			get
			{
				foreach (OdinMenuItem item in selection)
				{
					yield return item.Value;
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether multi selection is supported.
		/// </summary>
		public bool SupportsMultiSelect
		{
			get
			{
				return supportsMultiSelect;
			}
			set
			{
				supportsMultiSelect = value;
			}
		}

		/// <summary>
		/// Gets the <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuItem" /> at the specified index.
		/// </summary>
		public OdinMenuItem this[int index] => selection[index];

		bool ICollection<OdinMenuItem>.IsReadOnly => false;

		OdinMenuItem IList<OdinMenuItem>.this[int index]
		{
			get
			{
				return selection[index];
			}
			set
			{
				Add(value);
			}
		}

		/// <summary>
		/// Occurs whenever the selection has changed.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use SelectionChanged which also provides a SelectionChangedType argument")]
		public event Action OnSelectionChanged;

		/// <summary>
		/// Occurs whenever the selection has changed.
		/// </summary>
		public event Action<SelectionChangedType> SelectionChanged;

		/// <summary>
		/// Usually occurs whenever the user hits return, or double click a menu item.
		/// </summary>
		public event Action<OdinMenuTreeSelection> SelectionConfirmed;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuTreeSelection" /> class.
		/// </summary>
		/// <param name="supportsMultiSelect">if set to <c>true</c> [supports multi select].</param>
		public OdinMenuTreeSelection(bool supportsMultiSelect)
		{
			this.supportsMultiSelect = supportsMultiSelect;
			selection = new List<OdinMenuItem>();
		}

		/// <summary>
		/// Adds a menu item to the selection. If the menu item is already selected, then the item is pushed to the bottom of the selection list.
		/// If multi selection is off, then the previous selected menu item is removed first.
		/// Adding a item to the selection triggers <see cref="E:Sirenix.OdinInspector.Editor.OdinMenuTreeSelection.SelectionChanged" />.
		/// </summary>
		/// <param name="item">The item.</param>
		public void Add(OdinMenuItem item)
		{
			if (!supportsMultiSelect)
			{
				selection.Clear();
			}
			Remove(item);
			selection.Add(item);
			ApplyChanges(SelectionChangedType.ItemAdded);
		}

		/// <summary>
		/// Clears the selection and triggers <see cref="E:Sirenix.OdinInspector.Editor.OdinMenuTreeSelection.OnSelectionChanged" />.
		/// </summary>
		public void Clear()
		{
			selection.Clear();
			ApplyChanges(SelectionChangedType.SelectionCleared);
		}

		/// <summary>
		/// Determines whether an OdinMenuItem is selected.
		/// </summary>
		public bool Contains(OdinMenuItem item)
		{
			return selection.Contains(item);
		}

		/// <summary>
		/// Copies all the elements of the current array to the specified array starting at the specified destination array index.
		/// </summary>
		public void CopyTo(OdinMenuItem[] array, int arrayIndex)
		{
			selection.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		public IEnumerator<OdinMenuItem> GetEnumerator()
		{
			return selection.GetEnumerator();
		}

		/// <summary>
		/// Searches for the specified menu item and returns the index location.
		/// </summary>
		public int IndexOf(OdinMenuItem item)
		{
			return selection.IndexOf(item);
		}

		/// <summary>
		/// Removes the specified menu item and triggers <see cref="E:Sirenix.OdinInspector.Editor.OdinMenuTreeSelection.SelectionChanged" />.
		/// </summary>
		public bool Remove(OdinMenuItem item)
		{
			bool result = selection.Remove(item);
			if (result)
			{
				ApplyChanges(SelectionChangedType.ItemRemoved);
			}
			return result;
		}

		/// <summary>
		/// Removes the menu item at the specified index and triggers <see cref="E:Sirenix.OdinInspector.Editor.OdinMenuTreeSelection.SelectionChanged" />.
		/// </summary>
		public void RemoveAt(int index)
		{
			selection.RemoveAt(index);
			ApplyChanges(SelectionChangedType.ItemRemoved);
		}

		/// <summary>
		/// Triggers OnSelectionConfirmed.
		/// </summary>
		public void ConfirmSelection()
		{
			if (this.SelectionConfirmed != null)
			{
				this.SelectionConfirmed(this);
			}
		}

		private void ApplyChanges(SelectionChangedType type)
		{
			try
			{
				if (this.OnSelectionChanged != null)
				{
					this.OnSelectionChanged();
				}
				if (this.SelectionChanged != null)
				{
					this.SelectionChanged(type);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}

		void IList<OdinMenuItem>.Insert(int index, OdinMenuItem item)
		{
			throw new NotSupportedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return selection.GetEnumerator();
		}
	}
}

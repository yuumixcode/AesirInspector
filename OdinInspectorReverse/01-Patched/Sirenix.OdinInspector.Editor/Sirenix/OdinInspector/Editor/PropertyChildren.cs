using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Represents the children of an <see cref="T:Sirenix.OdinInspector.Editor.InspectorProperty" />.
	/// </summary>
	public sealed class PropertyChildren : IEnumerable<InspectorProperty>, IEnumerable
	{
		internal struct ExistingChildEnumerator
		{
			private PropertyChildren children;

			private InspectorProperty current;

			private int index;

			private int count;

			private GarbageFreeIterators.DictionaryIterator<int, InspectorProperty> dictionaryIterator;

			private bool isEmpty;

			public static readonly ExistingChildEnumerator Empty;

			public InspectorProperty Current
			{
				get
				{
					if (isEmpty)
					{
						return null;
					}
					if (children != null)
					{
						return current;
					}
					return dictionaryIterator.Current.Value;
				}
			}

			static ExistingChildEnumerator()
			{
				Empty = new ExistingChildEnumerator
				{
					isEmpty = true
				};
			}

			public ExistingChildEnumerator(PropertyChildren children)
			{
				this.children = children;
				current = null;
				index = -1;
				count = children.Count;
				dictionaryIterator = default(GarbageFreeIterators.DictionaryIterator<int, InspectorProperty>);
				isEmpty = false;
			}

			public ExistingChildEnumerator(GarbageFreeIterators.DictionaryIterator<int, InspectorProperty> dictionaryIterator)
			{
				this = default(ExistingChildEnumerator);
				this.dictionaryIterator = dictionaryIterator;
			}

			public void Dispose()
			{
				if (!isEmpty && children == null)
				{
					dictionaryIterator.Dispose();
				}
			}

			public ExistingChildEnumerator GetEnumerator()
			{
				return this;
			}

			public bool MoveNext()
			{
				if (isEmpty)
				{
					return false;
				}
				if (children == null)
				{
					return dictionaryIterator.MoveNext();
				}
				if (index >= count)
				{
					return false;
				}
				do
				{
					index++;
					if (index >= count)
					{
						current = null;
						return false;
					}
				}
				while (!children.childrenByIndex.TryGetValue(index, out current));
				return true;
			}

			public void Reset()
			{
				index = -1;
				current = null;
			}
		}

		private Dictionary<int, InspectorPropertyInfo> infosByIndex = new Dictionary<int, InspectorPropertyInfo>();

		private Dictionary<int, InspectorProperty> childrenByIndex = new Dictionary<int, InspectorProperty>();

		private Dictionary<int, string> pathsByIndex = new Dictionary<int, string>();

		private bool allowChildren;

		private OdinPropertyResolver resolver;

		private IRefreshableResolver refreshableResolver;

		private IPathRedirector pathRedirector;

		private IHasSpecialPropertyPaths hasSpecialPropertyPaths;

		/// <summary>
		/// The <see cref="T:Sirenix.OdinInspector.Editor.InspectorProperty" /> that this instance handles children for.
		/// </summary>
		private InspectorProperty property;

		/// <summary>
		/// Gets a child by index. This is an alias for <see cref="M:Sirenix.OdinInspector.Editor.PropertyChildren.Get(System.Int32)" />.
		/// </summary>
		/// <param name="index">The index of the child to get.</param>
		/// <returns>The child at the given index.</returns>
		public InspectorProperty this[int index] => Get(index);

		/// <summary>
		/// Gets a child by name. This is an alias for <see cref="M:Sirenix.OdinInspector.Editor.PropertyChildren.Get(System.String)" />.
		/// </summary>
		/// <param name="name">The name of the child to get.</param>
		/// <returns>The child, if a child was found; otherwise, null.</returns>
		public InspectorProperty this[string name] => Get(name);

		/// <summary>
		/// Gets a child by name. This is an alias for <see cref="!:Get(StringSlice)" />.
		/// </summary>
		/// <param name="name">The name of the child to get.</param>
		/// <returns>The child, if a child was found; otherwise, null.</returns>
		public InspectorProperty this[StringSlice name] => Get(ref name);

		/// <summary>
		/// The number of children on the property.
		/// </summary>
		public int Count
		{
			get
			{
				if (!allowChildren)
				{
					return 0;
				}
				return property.ChildResolver.ChildCount;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.PropertyChildren" /> class.
		/// </summary>
		/// <param name="property">The property to handle children for.</param>
		/// <exception cref="T:System.ArgumentNullException">property is null</exception>
		internal PropertyChildren(InspectorProperty property)
		{
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}
			this.property = property;
			resolver = this.property.ChildResolver;
			refreshableResolver = resolver as IRefreshableResolver;
			pathRedirector = resolver as IPathRedirector;
			hasSpecialPropertyPaths = resolver as IHasSpecialPropertyPaths;
		}

		internal void ClearAndDisposeChildrenFromIndex(int index)
		{
			List<int> toRemoveIndices = new List<int>();
			foreach (InspectorProperty child in childrenByIndex.Values)
			{
				int childIndex = child.Index;
				if (childIndex >= index)
				{
					toRemoveIndices.Add(childIndex);
					try
					{
						child.Dispose();
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
			}
			foreach (int childIndex2 in toRemoveIndices)
			{
				infosByIndex.Remove(childIndex2);
				childrenByIndex.Remove(childIndex2);
				pathsByIndex.Remove(childIndex2);
			}
		}

		internal void ClearAndDisposeChildren()
		{
			foreach (InspectorProperty child in childrenByIndex.Values)
			{
				try
				{
					child.Dispose();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			infosByIndex.Clear();
			childrenByIndex.Clear();
			pathsByIndex.Clear();
		}

		/// <summary>
		/// Updates this instance of <see cref="T:Sirenix.OdinInspector.Editor.PropertyChildren" />.
		/// </summary>
		public void Update()
		{
			bool wasAllowingChildren = allowChildren;
			allowChildren = true;
			if (property != property.Tree.RootProperty && property.ValueEntry != null && (property.ValueEntry.ValueState == PropertyValueState.Reference || property.ValueEntry.ValueState == PropertyValueState.NullReference || property.ValueEntry.ValueState == PropertyValueState.ReferencePathConflict || property.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict))
			{
				allowChildren = false;
				if (wasAllowingChildren)
				{
					ClearAndDisposeChildren();
				}
			}
			if (allowChildren)
			{
				int childCountBefore = property.ChildResolver.ChildCount;
				property.ChildResolver.ForceUpdateChildCount();
				int childCountAfter = property.ChildResolver.ChildCount;
				if (childCountAfter < childCountBefore)
				{
					ClearAndDisposeChildrenFromIndex(childCountAfter);
				}
			}
		}

		/// <summary>
		/// Gets a child by name.
		/// </summary>
		/// <param name="name">The name of the child to get.</param>
		/// <returns>The child, if a child was found; otherwise, null.</returns>
		/// <exception cref="T:System.ArgumentNullException">name</exception>
		public InspectorProperty Get(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (!allowChildren || Count == 0)
			{
				return null;
			}
			int index = resolver.ChildNameToIndex(name);
			if (index >= 0 && index < Count)
			{
				return Get(index);
			}
			if (pathRedirector != null && pathRedirector.TryGetRedirectedProperty(name, out var result))
			{
				result.Update();
				return result;
			}
			return null;
		}

		/// <summary>
		/// Gets a child by name.
		/// </summary>
		/// <param name="name">The name of the child to get.</param>
		/// <returns>The child, if a child was found; otherwise, null.</returns>
		/// <exception cref="T:System.ArgumentNullException">name</exception>
		public InspectorProperty Get(ref StringSlice name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (!allowChildren || Count == 0)
			{
				return null;
			}
			int index = resolver.ChildNameToIndex(ref name);
			if (index >= 0 && index < Count)
			{
				return Get(index);
			}
			if (pathRedirector != null && pathRedirector.TryGetRedirectedProperty(name.ToString(), out var result))
			{
				result.Update();
				return result;
			}
			return null;
		}

		/// <summary>
		/// Gets a child by index.
		/// </summary>
		/// <param name="index">The index of the child to get.</param>
		/// <returns>
		/// The child at the given index.
		/// </returns>
		/// <exception cref="T:System.IndexOutOfRangeException">The given index was out of range.</exception>
		public InspectorProperty Get(int index)
		{
			if (index < 0 || index >= Count)
			{
				throw new IndexOutOfRangeException();
			}
			if (!childrenByIndex.TryGetValue(index, out var result) || NeedsRefresh(index))
			{
				if (result != null)
				{
					result.Dispose();
					childrenByIndex.Remove(index);
				}
				result = InspectorProperty.Create(property.Tree, property, GetInfo(index), index, isRoot: false);
				childrenByIndex[index] = result;
			}
			result.Update();
			return result;
		}

		/// <summary>
		/// Gets the path of the child at a given index.
		/// </summary>
		/// <param name="index">The index to get the path of.</param>
		/// <returns>The path of the child at the given index.</returns>
		/// <exception cref="T:System.IndexOutOfRangeException">The given index was out of range.</exception>
		public string GetPath(int index)
		{
			if (index < 0 || index >= Count)
			{
				throw new IndexOutOfRangeException();
			}
			if (!pathsByIndex.TryGetValue(index, out var result) || NeedsRefresh(index))
			{
				result = ((hasSpecialPropertyPaths != null) ? hasSpecialPropertyPaths.GetSpecialChildPath(index) : ((!property.IsTreeRoot) ? (property.Path + "." + GetInfo(index).PropertyName) : GetInfo(index).PropertyName));
				pathsByIndex[index] = result;
			}
			return result;
		}

		/// <summary>
		/// Returns an IEnumerable that recursively yields all children of the property, depth first.
		/// </summary>
		public IEnumerable<InspectorProperty> Recurse()
		{
			for (int i = 0; i < Count; i++)
			{
				InspectorProperty child = this[i];
				yield return child;
				foreach (InspectorProperty item in child.Children.Recurse())
				{
					yield return item;
				}
			}
		}

		/// <summary>
		/// Gets the property's already created children. If the child count is less than or equal to 10000, children are returned in order. If the count is larger than 10000, they are returned in no particular order.
		/// </summary>
		internal ExistingChildEnumerator GetExistingChildren()
		{
			if (childrenByIndex == null || childrenByIndex.Count == 0)
			{
				return ExistingChildEnumerator.Empty;
			}
			if (Count <= 10000)
			{
				return new ExistingChildEnumerator(this);
			}
			return new ExistingChildEnumerator(childrenByIndex.GFIterator());
		}

		private InspectorPropertyInfo GetInfo(int index)
		{
			if (!infosByIndex.TryGetValue(index, out var info) || (refreshableResolver != null && refreshableResolver.ChildPropertyRequiresRefresh(index, info)))
			{
				info = resolver.GetChildInfo(index);
				infosByIndex[index] = info;
			}
			return info;
		}

		private bool NeedsRefresh(int index)
		{
			if (infosByIndex.TryGetValue(index, out var info))
			{
				if (refreshableResolver != null)
				{
					return refreshableResolver.ChildPropertyRequiresRefresh(index, info);
				}
				return false;
			}
			return true;
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		public IEnumerator<InspectorProperty> GetEnumerator()
		{
			for (int i = 0; i < Count; i++)
			{
				yield return this[i];
			}
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			for (int i = 0; i < Count; i++)
			{
				yield return this[i];
			}
		}
	}
}

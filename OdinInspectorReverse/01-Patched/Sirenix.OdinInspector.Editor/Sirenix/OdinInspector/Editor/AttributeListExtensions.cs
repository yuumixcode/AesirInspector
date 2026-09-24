using System;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Extension method for List&lt;Attribute&gt;
	/// </summary>
	public static class AttributeListExtensions
	{
		/// <summary>
		/// Determines whether the list contains a specific attribute type.
		/// </summary>
		/// <typeparam name="T">The type of attribute.</typeparam>
		/// <param name="attributeList">The attribute list.</param>
		/// <returns>
		///   <c>true</c> if the specified attribute list has attribute; otherwise, <c>false</c>.
		/// </returns>
		public static bool HasAttribute<T>(this IList<Attribute> attributeList) where T : Attribute
		{
			int count = attributeList.Count;
			for (int i = 0; i < count; i++)
			{
				if (attributeList[i] is T)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Adds a new instance of the given type of attribute if it's not in the list.
		/// </summary>
		/// <typeparam name="T">The type of attribute.</typeparam>
		/// <param name="attributeList">The attribute list.</param>
		/// <returns></returns>
		public static T GetOrAddAttribute<T>(this List<Attribute> attributeList) where T : Attribute, new()
		{
			int count = attributeList.Count;
			for (int i = 0; i < count; i++)
			{
				if (attributeList[i] is T attr)
				{
					return attr;
				}
			}
			T attr2 = new T();
			attributeList.Add(attr2);
			return attr2;
		}

		/// <summary>
		/// Gets the first instance of an attribute of the given type in the list.
		/// </summary>
		/// <typeparam name="T">The type of attribute.</typeparam>
		/// <param name="attributeList">The attribute list.</param>
		/// <returns></returns>
		public static T GetAttribute<T>(this IList<Attribute> attributeList) where T : Attribute
		{
			for (int i = 0; i < attributeList.Count; i++)
			{
				if (attributeList[i] is T attr)
				{
					return attr;
				}
			}
			return null;
		}

		/// <summary>
		/// Adds a new instance of the attribute to the list.
		/// </summary>
		/// <typeparam name="T">The type of attribute.</typeparam>
		/// <param name="attributeList">The attribute list.</param>
		/// <returns></returns>
		public static T Add<T>(this List<Attribute> attributeList) where T : Attribute, new()
		{
			T attr = new T();
			attributeList.Add(attr);
			return attr;
		}

		/// <summary>
		/// Removes all instances of the given type in the list.
		/// </summary>
		/// <typeparam name="T">The type of attribute.</typeparam>
		/// <param name="attributeList">The attribute list.</param>
		/// <returns></returns>
		public static bool RemoveAttributeOfType<T>(this List<Attribute> attributeList) where T : Attribute
		{
			int count = attributeList.Count;
			bool removed = false;
			for (int i = 0; i < count; i++)
			{
				if (attributeList[i] is T)
				{
					attributeList.RemoveAt(i);
					i--;
					count--;
					removed = true;
				}
			}
			return removed;
		}
	}
}

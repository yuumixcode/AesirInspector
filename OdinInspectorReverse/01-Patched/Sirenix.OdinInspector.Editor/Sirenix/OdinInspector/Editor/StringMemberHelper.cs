using System;
using System.ComponentModel;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// This class has been made fully obsolete, and has been replaced by <see cref="T:Sirenix.OdinInspector.Editor.ValueResolvers.ValueResolver" />. 
	/// It was a helper class to handle strings for labels and other similar purposes.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("StringMemberHelper is obsolete. Use the ValueResolver system instead.", false)]
	public class StringMemberHelper
	{
		private string buffer;

		private ValueResolver<string> resolver;

		/// <summary>
		/// If any error occurred while looking for members, it will be stored here.
		/// </summary>
		public string ErrorMessage => resolver.ErrorMessage;

		/// <summary>
		/// Gets a value indicating whether or not the string is retrived from a from a member. 
		/// </summary>
		public bool IsDynamicString => true;

		/// <summary>
		/// Gets the type of the object.
		/// </summary>
		public Type ObjectType => resolver.Context.Property.ParentType;

		/// <summary>
		/// Obsolete. Use other constructor.
		/// </summary>
		[Obsolete("Use a contructor with an InspectorProperty argument instead.", true)]
		public StringMemberHelper(Type objectType, string path, bool allowInstanceMember = true, bool allowStaticMember = true)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Obsolete. Use other constructor.
		/// </summary>
		[Obsolete("Use a contructor with an InspectorProperty argument instead.", true)]
		public StringMemberHelper(Type objectType, string path, ref string errorMessage, bool allowInstanceMember = true, bool allowStaticMember = true)
		{
			throw new NotSupportedException();
		}

		[Obsolete("Use a contructor with an InspectorProperty argument instead.", true)]
		public StringMemberHelper(Type objectType, bool isStatic, string text)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Creates a StringMemberHelper to get a display string.
		/// </summary>
		/// <param name="property">Inspector property to get string from.</param>
		/// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method, and will try to parse it as an expression if it starts with '@'.</param>
		public StringMemberHelper(InspectorProperty property, string text)
		{
			resolver = ValueResolver.GetForString(property, text);
		}

		/// <summary>
		/// Creates a StringMemberHelper to get a display string.
		/// </summary>
		/// <param name="property">Inspector property to get string from.</param>
		/// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method, and will try to parse it as an expression if it starts with '@'.</param>/// <param name="text">The input string. If the first character is a '$', then StringMemberHelper will look for a member string field, property or method.</param>
		public StringMemberHelper(InspectorProperty property, string text, ref string errorMessage)
			: this(property, text)
		{
			if (errorMessage == null)
			{
				errorMessage = ErrorMessage;
			}
		}

		/// <summary>
		/// Gets the string from the StringMemberHelper.
		/// Only updates the string buffer in Layout events.
		/// </summary>
		/// <param name="entry">The property entry, to get the instance reference from.</param>
		/// <returns>The current display string.</returns>
		public string GetString(IPropertyValueEntry entry)
		{
			return GetString(entry.Property.ParentValues[0]);
		}

		/// <summary>
		/// Gets the string from the StringMemberHelper.
		/// Only updates the string buffer in Layout events.
		/// </summary>
		/// <param name="property">The property, to get the instance reference from.</param>
		/// <returns>The current string.</returns>
		public string GetString(InspectorProperty property)
		{
			return GetString(property.ParentValues[0]);
		}

		/// <summary>
		/// Gets the string from the StringMemberHelper.
		/// Only updates the string buffer in Layout events.
		/// </summary>
		/// <param name="instance">The instance, for evt. member references.</param>
		/// <returns>The current string.</returns>
		public string GetString(object instance)
		{
			if (buffer == null || Event.current == null || Event.current.type == EventType.Layout)
			{
				buffer = ForceGetString(instance);
			}
			return buffer;
		}

		/// <summary>
		/// Gets the string from the StringMemberHelper.
		/// </summary>
		/// <param name="entry">The property entry, to get the instance reference from.</param>
		/// <returns>The current string.</returns>
		public string ForceGetString(IPropertyValueEntry entry)
		{
			if (entry.Property != resolver.Context.Property)
			{
				throw new ArgumentException("You *must* provide the entry for the property to this call that you originally provided to the constructor. Yes, this is silly. That's why this class is obsolete!");
			}
			return resolver.GetValue();
		}

		/// <summary>
		/// Gets the string from the StringMemberHelper.
		/// </summary>
		/// <param name="property">The property, to get the instance reference from.</param>
		/// <returns>The current string.</returns>
		public string ForceGetString(InspectorProperty property)
		{
			if (property != resolver.Context.Property)
			{
				throw new ArgumentException("You *must* provide the property to this call that you originally provided to the constructor. Yes, this is silly. That's why this class is obsolete!");
			}
			return resolver.GetValue();
		}

		/// <summary>
		/// Gets the string from the StringMemberHelper.
		/// </summary>
		/// <param name="instance">The instance, for evt. member references.</param>
		/// <returns>The current string.</returns>
		public string ForceGetString(object instance)
		{
			return resolver.GetValue();
		}
	}
}

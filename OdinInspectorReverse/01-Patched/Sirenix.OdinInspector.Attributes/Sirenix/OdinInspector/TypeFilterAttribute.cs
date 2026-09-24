using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public class TypeFilterAttribute : Attribute
	{
		/// <summary>
		/// A resolved string that should evaluate to a value that is assignable to IList; e.g, arrays and lists are compatible.
		/// </summary>
		public string FilterGetter;

		/// <summary>
		/// Gets or sets the title for the dropdown. Null by default.
		/// </summary>
		public string DropdownTitle;

		/// <summary>
		/// If true, the value will be drawn normally after the type selector dropdown has been drawn. False by default.
		/// </summary>
		public bool DrawValueNormally;

		/// <summary>
		/// Name of any field, property or method member that implements IList. E.g. arrays or Lists. Obsolete; use the FilterGetter member instead.
		/// </summary>
		[Obsolete("Use the FilterGetter member instead.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public string MemberName
		{
			get
			{
				return FilterGetter;
			}
			set
			{
				FilterGetter = value;
			}
		}

		/// <summary>
		/// Creates a dropdown menu for a property.
		/// </summary>
		/// <param name="filterGetter">A resolved string that should evaluate to a value that is assignable to IList; e.g, arrays and lists are compatible.</param>
		public TypeFilterAttribute(string filterGetter)
		{
			FilterGetter = filterGetter;
		}
	}
}

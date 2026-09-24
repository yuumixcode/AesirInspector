using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Customize the behavior for dictionaries in the inspector.
	/// </summary>
	[Conditional("UNITY_EDITOR")]
	public sealed class DictionaryDrawerSettings : Attribute
	{
		/// <summary>
		/// Specify an alternative key label for the dictionary drawer.
		/// </summary>
		public string KeyLabel = "Key";

		/// <summary>
		/// Specify an alternative value label for the dictionary drawer.
		/// </summary>
		public string ValueLabel = "Value";

		/// <summary>
		/// Specify how the dictionary should draw its items.
		/// </summary>
		public DictionaryDisplayOptions DisplayMode;

		/// <summary>
		/// Gets or sets a value indicating whether this instance is read only.
		/// </summary>
		public bool IsReadOnly;

		/// <summary>
		/// Gets or sets a value indicating the default key column width of the dictionary.
		/// </summary>
		public float KeyColumnWidth = 130f;
	}
}

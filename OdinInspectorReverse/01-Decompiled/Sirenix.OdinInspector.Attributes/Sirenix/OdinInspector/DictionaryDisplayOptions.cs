namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Various display modes for the dictionary to draw its items.
	/// </summary>
	public enum DictionaryDisplayOptions
	{
		/// <summary>
		/// Draws all dictionary items in two columns. The left column contains all key values, the right column displays all values.
		/// </summary>
		OneLine,
		/// <summary>
		/// Draws each dictionary item in a box with the key in the header and the value inside the box.
		/// Whether or not the box is expanded or collapsed by default, is determined by the
		/// "Expand Foldout By Default" setting found in the preferences window "Tools &gt; Odin &gt; Inspector &gt; Preferences &gt; Drawers &gt; Settings".
		/// </summary>
		Foldout,
		/// <summary>
		/// Draws each dictionary item in a collapsed foldout with the key in the header and the value inside the box.
		/// </summary>
		CollapsedFoldout,
		/// <summary>
		/// Draws each dictionary item in an expanded foldout with the key in the header and the value inside the box.
		/// </summary>
		ExpandedFoldout
	}
}

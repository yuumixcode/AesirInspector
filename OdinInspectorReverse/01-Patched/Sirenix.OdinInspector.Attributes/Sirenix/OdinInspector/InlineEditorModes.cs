namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Editor modes for <see cref="T:Sirenix.OdinInspector.InlineEditorAttribute" />
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.InlineEditorAttribute" />
	public enum InlineEditorModes
	{
		/// <summary>
		/// Draws only the editor GUI
		/// </summary>
		GUIOnly,
		/// <summary>
		/// Draws the editor GUI and the editor header.
		/// </summary>
		GUIAndHeader,
		/// <summary>
		/// Draws the editor GUI to the left, and a small editor preview to the right.
		/// </summary>
		GUIAndPreview,
		/// <summary>
		/// Draws a small editor preview without any GUI.
		/// </summary>
		SmallPreview,
		/// <summary>
		/// Draws a large editor preview without any GUI.
		/// </summary>
		LargePreview,
		/// <summary>
		/// Draws the editor header and GUI to the left, and a small editor preview to the right.
		/// </summary>
		FullEditor
	}
}

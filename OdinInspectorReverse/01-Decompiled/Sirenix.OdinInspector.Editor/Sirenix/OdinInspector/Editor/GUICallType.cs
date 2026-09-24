using System;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Specifies hows any given drawer should drawer the property.
	/// Changing this behavior, also changes which methods should be overridden in the drawer.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinValueDrawer`1" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinAttributeDrawer`2" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinAttributeDrawer`1" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinGroupDrawer`1" />
	[Obsolete("Removed support GUICallType.Rect and DrawPropertyRect as it didn't really do much. You can get the same behaviour by overriding DrawPropertyLayout and calling GUILayoutUtility.GetRect or EditorGUILayout.GetControlRect.", true)]
	public enum GUICallType
	{
		/// <summary>
		/// GUILayout enabled the use of GUILayout, EditorGUILayout and <see cref="T:Sirenix.Utilities.Editor.SirenixEditorGUI" />
		/// </summary>
		GUILayout,
		/// <summary>
		/// Draws the property using Unity's GUI, and EditorGUI.
		/// </summary>
		Rect
	}
}

using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// The GUIStyleState Drawer
	/// </summary>
	/// <seealso cref="!:Sirenix.OdinInspector.Editor.OdinValueDrawer&lt;UnityEngine.GUIStyleState&gt;" />
	public class GUIStyleStateDrawer : OdinValueDrawer<GUIStyleState>
	{
		private bool isVisible;

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			isVisible = SirenixEditorGUI.ExpandFoldoutByDefault;
		}

		/// <summary>
		/// Draws the property with GUILayout support.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<GUIStyleState> entry = base.ValueEntry;
			InspectorProperty property = entry.Property;
			if (label != null)
			{
				isVisible = SirenixEditorGUI.Foldout(isVisible, label);
				if (SirenixEditorGUI.BeginFadeGroup(isVisible, isVisible))
				{
					EditorGUI.indentLevel++;
					entry.SmartValue.background = (Texture2D)SirenixEditorFields.UnityObjectField(label, entry.SmartValue.background, typeof(Texture2D), true);
					entry.SmartValue.textColor = EditorGUILayout.ColorField(label ?? GUIContent.none, entry.SmartValue.textColor);
					EditorGUI.indentLevel--;
				}
				SirenixEditorGUI.EndFadeGroup();
			}
			else
			{
				entry.SmartValue.background = (Texture2D)SirenixEditorFields.UnityObjectField(label, entry.SmartValue.background, typeof(Texture2D), true);
				entry.SmartValue.textColor = EditorGUILayout.ColorField(label ?? GUIContent.none, entry.SmartValue.textColor);
			}
		}
	}
}

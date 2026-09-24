using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws char properties marked with <see cref="T:UnityEngine.DelayedAttribute" />.
	/// </summary>
	public sealed class DelayedAttributeCharDrawer : OdinAttributeDrawer<DelayedAttribute, char>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			EditorGUI.BeginChangeCheck();
			string s = new string(base.ValueEntry.SmartValue, 1);
			s = SirenixEditorFields.DelayedTextField(label, s, GUILayoutOptions.MinWidth(0f));
			if (EditorGUI.EndChangeCheck() && s.Length > 0)
			{
				base.ValueEntry.SmartValue = s[0];
			}
		}
	}
}

using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws ulong properties marked with <see cref="T:UnityEngine.DelayedAttribute" />.
	/// </summary>
	public sealed class DelayedAttributeUInt64Drawer : OdinAttributeDrawer<DelayedAttribute, ulong>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			ulong value = base.ValueEntry.SmartValue;
			string str = value.ToString();
			str = ((label == null) ? EditorGUILayout.DelayedTextField(str, GUILayoutOptions.MinWidth(0f)) : EditorGUILayout.DelayedTextField(label, str, GUILayoutOptions.MinWidth(0f)));
			if (GUI.changed && ulong.TryParse(str, out value))
			{
				base.ValueEntry.SmartValue = value;
			}
		}
	}
}

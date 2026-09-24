using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws decimal properties marked with <see cref="T:UnityEngine.DelayedAttribute" />.
	/// </summary>
	public sealed class DelayedAttributeDecimalDrawer : OdinAttributeDrawer<DelayedAttribute, decimal>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			decimal value = base.ValueEntry.SmartValue;
			string str = value.ToString();
			str = SirenixEditorFields.DelayedTextField(label, str, GUILayoutOptions.MinWidth(0f));
			if (GUI.changed && decimal.TryParse(str, out value))
			{
				base.ValueEntry.SmartValue = value;
			}
		}
	}
}

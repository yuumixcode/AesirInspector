using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws sbyte properties marked with <see cref="T:UnityEngine.DelayedAttribute" />.
	/// </summary>
	public sealed class DelayedAttributeSByteDrawer : OdinAttributeDrawer<DelayedAttribute, sbyte>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			int value = SirenixEditorFields.DelayedIntField(label, base.ValueEntry.SmartValue, GUILayoutOptions.MinWidth(0f));
			if (value < -128)
			{
				value = -128;
			}
			else if (value > 127)
			{
				value = 127;
			}
			base.ValueEntry.SmartValue = (sbyte)value;
		}
	}
}

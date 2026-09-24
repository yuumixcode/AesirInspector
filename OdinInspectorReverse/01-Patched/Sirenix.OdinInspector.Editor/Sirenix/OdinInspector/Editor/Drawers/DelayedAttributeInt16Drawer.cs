using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws short properties marked with <see cref="T:UnityEngine.DelayedAttribute" />.
	/// </summary>
	public sealed class DelayedAttributeInt16Drawer : OdinAttributeDrawer<DelayedAttribute, short>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			int value = SirenixEditorFields.DelayedIntField(label, base.ValueEntry.SmartValue, GUILayoutOptions.MinWidth(0f));
			if (value < -32768)
			{
				value = -32768;
			}
			else if (value > 32767)
			{
				value = 32767;
			}
			base.ValueEntry.SmartValue = (short)value;
		}
	}
}

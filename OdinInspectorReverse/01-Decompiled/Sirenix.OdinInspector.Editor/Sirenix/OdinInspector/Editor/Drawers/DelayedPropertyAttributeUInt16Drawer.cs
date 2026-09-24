using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws ushort properties marked with <see cref="T:Sirenix.OdinInspector.DelayedPropertyAttribute" />.
	/// </summary>
	public sealed class DelayedPropertyAttributeUInt16Drawer : OdinAttributeDrawer<DelayedPropertyAttribute, ushort>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			int value = SirenixEditorFields.DelayedIntField(label, base.ValueEntry.SmartValue, GUILayoutOptions.MinWidth(0f));
			if (value < 0)
			{
				value = 0;
			}
			else if (value > 65535)
			{
				value = 65535;
			}
			base.ValueEntry.SmartValue = (ushort)value;
		}
	}
}

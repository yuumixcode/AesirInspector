using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws uint properties marked with <see cref="T:Sirenix.OdinInspector.DelayedPropertyAttribute" />.
	/// </summary>
	public sealed class DelayedPropertyAttributeUInt32Drawer : OdinAttributeDrawer<DelayedPropertyAttribute, uint>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			long value = SirenixEditorFields.DelayedLongField(label, base.ValueEntry.SmartValue, GUILayoutOptions.MinWidth(0f));
			if (value < 0)
			{
				value = 0L;
			}
			else if (value > uint.MaxValue)
			{
				value = 4294967295L;
			}
			base.ValueEntry.SmartValue = (uint)value;
		}
	}
}

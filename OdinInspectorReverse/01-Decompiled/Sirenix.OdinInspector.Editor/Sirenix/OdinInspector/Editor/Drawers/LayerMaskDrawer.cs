using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// LayerMask property drawer.
	/// </summary>
	public class LayerMaskDrawer : OdinValueDrawer<LayerMask>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<LayerMask> entry = base.ValueEntry;
			entry.SmartValue = SirenixEditorFields.LayerMaskField(label, entry.SmartValue);
		}
	}
}

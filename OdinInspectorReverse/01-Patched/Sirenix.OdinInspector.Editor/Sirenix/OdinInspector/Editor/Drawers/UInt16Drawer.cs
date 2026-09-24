using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Ushort property drawer.
	/// </summary>
	public sealed class UInt16Drawer : OdinValueDrawer<ushort>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			int value = ((!GlobalConfig<GeneralDrawerConfig>.Instance.EnableSmartNumberFields) ? SirenixEditorFields.IntField(label, base.ValueEntry.SmartValue) : SirenixEditorFields.SmartIntField(base.Property.ToFieldExpressionContext(), label, base.ValueEntry.SmartValue));
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

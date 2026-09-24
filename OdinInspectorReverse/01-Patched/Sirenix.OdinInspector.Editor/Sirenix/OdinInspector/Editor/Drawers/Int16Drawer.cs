using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Short property drawer.
	/// </summary>
	public sealed class Int16Drawer : OdinValueDrawer<short>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			int value = ((!GlobalConfig<GeneralDrawerConfig>.Instance.EnableSmartNumberFields) ? SirenixEditorFields.IntField(label, base.ValueEntry.SmartValue) : SirenixEditorFields.SmartIntField(base.Property.ToFieldExpressionContext(), label, base.ValueEntry.SmartValue));
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

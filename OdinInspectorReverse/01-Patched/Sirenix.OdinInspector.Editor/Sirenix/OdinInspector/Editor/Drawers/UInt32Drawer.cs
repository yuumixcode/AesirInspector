using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Uint property drawer.
	/// </summary>
	public sealed class UInt32Drawer : OdinValueDrawer<uint>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			long value = ((!GlobalConfig<GeneralDrawerConfig>.Instance.EnableSmartNumberFields) ? SirenixEditorFields.LongField(label, base.ValueEntry.SmartValue) : SirenixEditorFields.SmartLongField(base.Property.ToFieldExpressionContext(), label, base.ValueEntry.SmartValue));
			if (value > uint.MaxValue)
			{
				value = 4294967295L;
			}
			else if (value < 0)
			{
				value = 0L;
			}
			base.ValueEntry.SmartValue = (uint)value;
		}
	}
}

using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Ulong property drawer.
	/// </summary>
	public sealed class UInt64Drawer : OdinValueDrawer<ulong>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			long value = ((!GlobalConfig<GeneralDrawerConfig>.Instance.EnableSmartNumberFields) ? SirenixEditorFields.LongField(label, (long)base.ValueEntry.SmartValue) : SirenixEditorFields.SmartLongField(base.Property.ToFieldExpressionContext(), label, (long)base.ValueEntry.SmartValue));
			if (value < 0)
			{
				value = 0L;
			}
			base.ValueEntry.SmartValue = (ulong)value;
		}
	}
}

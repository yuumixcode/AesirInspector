using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Long property drawer.
	/// </summary>
	public sealed class Int64Drawer : OdinValueDrawer<long>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (GlobalConfig<GeneralDrawerConfig>.Instance.EnableSmartNumberFields)
			{
				base.ValueEntry.SmartValue = SirenixEditorFields.SmartLongField(base.Property.ToFieldExpressionContext(), label, base.ValueEntry.SmartValue);
			}
			else
			{
				base.ValueEntry.SmartValue = SirenixEditorFields.LongField(label, base.ValueEntry.SmartValue);
			}
		}
	}
}

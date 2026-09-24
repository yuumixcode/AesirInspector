using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// SByte property drawer.
	/// </summary>
	public sealed class SByteDrawer : OdinValueDrawer<sbyte>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			int value = ((!GlobalConfig<GeneralDrawerConfig>.Instance.EnableSmartNumberFields) ? SirenixEditorFields.IntField(label, base.ValueEntry.SmartValue) : SirenixEditorFields.SmartIntField(base.Property.ToFieldExpressionContext(), label, base.ValueEntry.SmartValue));
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

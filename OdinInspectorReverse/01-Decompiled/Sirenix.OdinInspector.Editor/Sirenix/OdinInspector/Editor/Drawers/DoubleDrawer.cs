using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Double property drawer.
	/// </summary>
	public sealed class DoubleDrawer : OdinValueDrawer<double>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (GlobalConfig<GeneralDrawerConfig>.Instance.EnableSmartNumberFields)
			{
				base.ValueEntry.SmartValue = SirenixEditorFields.SmartDoubleField(base.Property.ToFieldExpressionContext(), label, base.ValueEntry.SmartValue);
			}
			else
			{
				base.ValueEntry.SmartValue = SirenixEditorFields.DoubleField(label, base.ValueEntry.SmartValue);
			}
		}
	}
}

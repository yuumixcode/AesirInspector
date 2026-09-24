using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Float property drawer.
	/// </summary>
	public sealed class SingleDrawer : OdinValueDrawer<float>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (GlobalConfig<GeneralDrawerConfig>.Instance.EnableSmartNumberFields)
			{
				base.ValueEntry.SmartValue = SirenixEditorFields.SmartFloatField(base.Property.ToFieldExpressionContext(), label, base.ValueEntry.SmartValue);
			}
			else
			{
				base.ValueEntry.SmartValue = SirenixEditorFields.FloatField(label, base.ValueEntry.SmartValue);
			}
		}
	}
}

using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Int property drawer.
	/// </summary>
	public sealed class Int32Drawer : OdinValueDrawer<int>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (GlobalConfig<GeneralDrawerConfig>.Instance.EnableSmartNumberFields)
			{
				base.ValueEntry.SmartValue = SirenixEditorFields.SmartIntField(base.Property.ToFieldExpressionContext(), label, base.ValueEntry.SmartValue);
			}
			else
			{
				base.ValueEntry.SmartValue = SirenixEditorFields.IntField(label, base.ValueEntry.SmartValue);
			}
		}
	}
}

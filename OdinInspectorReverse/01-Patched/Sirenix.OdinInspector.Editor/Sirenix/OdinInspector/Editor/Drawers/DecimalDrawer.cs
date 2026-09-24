using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Decimal property drawer.
	/// </summary>
	public sealed class DecimalDrawer : OdinValueDrawer<decimal>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			base.ValueEntry.SmartValue = SirenixEditorFields.SmartDecimalField(base.Property.ToFieldExpressionContext(), label, base.ValueEntry.SmartValue);
		}
	}
}

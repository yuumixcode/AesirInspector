using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws decimal properties marked with <see cref="T:UnityEngine.RangeAttribute" />.
	/// </summary>
	public sealed class RangeAttributeDecimalDrawer : OdinAttributeDrawer<RangeAttribute, decimal>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<decimal> entry = base.ValueEntry;
			RangeAttribute attribute = base.Attribute;
			EditorGUI.BeginChangeCheck();
			float value = SirenixEditorFields.RangeFloatField(label, (float)entry.SmartValue, attribute.min, attribute.max);
			if (EditorGUI.EndChangeCheck())
			{
				entry.SmartValue = (decimal)value;
			}
		}
	}
}

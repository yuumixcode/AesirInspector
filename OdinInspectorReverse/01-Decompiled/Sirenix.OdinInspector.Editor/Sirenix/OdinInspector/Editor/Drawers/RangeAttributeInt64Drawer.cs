using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws long properties marked with <see cref="T:UnityEngine.RangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:UnityEngine.RangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class RangeAttributeInt64Drawer : OdinAttributeDrawer<RangeAttribute, long>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<long> entry = base.ValueEntry;
			RangeAttribute attribute = base.Attribute;
			long uValue = entry.SmartValue;
			if (uValue < int.MinValue)
			{
				uValue = -2147483648L;
			}
			else if (uValue > int.MaxValue)
			{
				uValue = 2147483647L;
			}
			EditorGUI.BeginChangeCheck();
			int value = SirenixEditorFields.RangeIntField(label, (int)uValue, (int)attribute.min, (int)attribute.max);
			if (EditorGUI.EndChangeCheck())
			{
				entry.SmartValue = value;
			}
		}
	}
}

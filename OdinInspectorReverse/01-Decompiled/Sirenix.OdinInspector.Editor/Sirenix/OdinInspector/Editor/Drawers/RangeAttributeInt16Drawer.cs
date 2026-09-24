using System;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws short properties marked with <see cref="T:UnityEngine.RangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:UnityEngine.RangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class RangeAttributeInt16Drawer : OdinAttributeDrawer<RangeAttribute, short>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<short> entry = base.ValueEntry;
			RangeAttribute attribute = base.Attribute;
			EditorGUI.BeginChangeCheck();
			int value = SirenixEditorFields.RangeIntField(label, entry.SmartValue, Math.Max(-32768, (int)attribute.min), Math.Min(32767, (int)attribute.max));
			if (EditorGUI.EndChangeCheck())
			{
				if (value < -32768)
				{
					value = -32768;
				}
				else if (value > 32767)
				{
					value = 32767;
				}
				entry.SmartValue = (short)value;
			}
		}
	}
}

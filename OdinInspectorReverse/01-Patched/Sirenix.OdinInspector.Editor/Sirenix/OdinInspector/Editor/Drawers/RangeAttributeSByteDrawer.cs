using System;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws sbyte properties marked with <see cref="T:UnityEngine.RangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:UnityEngine.RangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class RangeAttributeSByteDrawer : OdinAttributeDrawer<RangeAttribute, sbyte>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<sbyte> entry = base.ValueEntry;
			RangeAttribute attribute = base.Attribute;
			EditorGUI.BeginChangeCheck();
			int value = SirenixEditorFields.RangeIntField(label, entry.SmartValue, Math.Max(-128, (int)attribute.min), Math.Min(127, (int)attribute.max));
			if (EditorGUI.EndChangeCheck())
			{
				if (value < -128)
				{
					value = -128;
				}
				else if (value > 127)
				{
					value = 127;
				}
				entry.SmartValue = (sbyte)value;
			}
		}
	}
}

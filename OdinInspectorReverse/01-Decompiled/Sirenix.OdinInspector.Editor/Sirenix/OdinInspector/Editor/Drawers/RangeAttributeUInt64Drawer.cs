using System;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws ulong properties marked with <see cref="T:UnityEngine.RangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:UnityEngine.RangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class RangeAttributeUInt64Drawer : OdinAttributeDrawer<RangeAttribute, ulong>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<ulong> entry = base.ValueEntry;
			RangeAttribute attribute = base.Attribute;
			ulong uValue = entry.SmartValue;
			if (uValue > int.MaxValue)
			{
				uValue = 2147483647uL;
			}
			EditorGUI.BeginChangeCheck();
			int value = SirenixEditorFields.RangeIntField(label, (int)uValue, Math.Max(0, (int)attribute.min), (int)attribute.max);
			if (EditorGUI.EndChangeCheck())
			{
				if (value < 0)
				{
					value = 0;
				}
				entry.SmartValue = (ulong)value;
			}
		}
	}
}

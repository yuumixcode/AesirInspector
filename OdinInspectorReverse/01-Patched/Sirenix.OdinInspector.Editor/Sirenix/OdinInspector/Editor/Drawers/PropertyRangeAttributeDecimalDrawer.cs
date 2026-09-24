using System;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws decimal properties marked with <see cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />.
	/// </summary>
	public sealed class PropertyRangeAttributeDecimalDrawer : OdinAttributeDrawer<PropertyRangeAttribute, decimal>
	{
		private ValueResolver<decimal> getterMinValue;

		private ValueResolver<decimal> getterMaxValue;

		/// <summary>
		/// Initialized the drawer.
		/// </summary>
		protected override void Initialize()
		{
			if (base.Attribute.MinGetter != null)
			{
				getterMinValue = ValueResolver.Get<decimal>(base.Property, base.Attribute.MinGetter);
			}
			if (base.Attribute.MaxGetter != null)
			{
				getterMaxValue = ValueResolver.Get<decimal>(base.Property, base.Attribute.MaxGetter);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			decimal min = ((getterMinValue != null) ? getterMinValue.GetValue() : ((decimal)base.Attribute.Min));
			decimal max = ((getterMaxValue != null) ? getterMaxValue.GetValue() : ((decimal)base.Attribute.Max));
			if (getterMinValue != null && getterMinValue.ErrorMessage != null)
			{
				SirenixEditorGUI.MessageBox(getterMinValue.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			}
			if (getterMaxValue != null && getterMaxValue.ErrorMessage != null)
			{
				SirenixEditorGUI.MessageBox(getterMaxValue.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			}
			EditorGUI.BeginChangeCheck();
			float value = SirenixEditorFields.RangeFloatField(label, (float)base.ValueEntry.SmartValue, (float)Math.Min(min, max), (float)Math.Max(min, max));
			if (EditorGUI.EndChangeCheck())
			{
				base.ValueEntry.SmartValue = (decimal)value;
			}
		}
	}
}

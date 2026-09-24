using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws ushort properties marked with <see cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class PropertyRangeAttributeUInt16Drawer : OdinAttributeDrawer<PropertyRangeAttribute, ushort>
	{
		private ValueResolver<ushort> getterMinValue;

		private ValueResolver<ushort> getterMaxValue;

		/// <summary>
		/// Initialized the drawer.
		/// </summary>
		protected override void Initialize()
		{
			if (base.Attribute.MinGetter != null)
			{
				getterMinValue = ValueResolver.Get<ushort>(base.Property, base.Attribute.MinGetter);
			}
			if (base.Attribute.MaxGetter != null)
			{
				getterMaxValue = ValueResolver.Get<ushort>(base.Property, base.Attribute.MaxGetter);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			ushort min = ((getterMinValue != null) ? getterMinValue.GetValue() : ((ushort)base.Attribute.Min));
			ushort max = ((getterMaxValue != null) ? getterMaxValue.GetValue() : ((ushort)base.Attribute.Max));
			if (getterMinValue != null && getterMinValue.ErrorMessage != null)
			{
				SirenixEditorGUI.MessageBox(getterMinValue.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			}
			if (getterMaxValue != null && getterMaxValue.ErrorMessage != null)
			{
				SirenixEditorGUI.MessageBox(getterMaxValue.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			}
			EditorGUI.BeginChangeCheck();
			int value = SirenixEditorFields.RangeIntField(label, base.ValueEntry.SmartValue, Mathf.Min(min, max), Mathf.Max(min, max));
			if (EditorGUI.EndChangeCheck())
			{
				if (value < 0)
				{
					value = 0;
				}
				else if (value > 65535)
				{
					value = 65535;
				}
				base.ValueEntry.SmartValue = (ushort)value;
			}
		}
	}
}

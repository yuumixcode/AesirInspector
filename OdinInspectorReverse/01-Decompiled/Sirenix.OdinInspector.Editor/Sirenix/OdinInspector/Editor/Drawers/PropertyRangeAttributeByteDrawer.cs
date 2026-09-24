using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws byte properties marked with <see cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyRangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class PropertyRangeAttributeByteDrawer : OdinAttributeDrawer<PropertyRangeAttribute, byte>
	{
		private ValueResolver<byte> getterMinValue;

		private ValueResolver<byte> getterMaxValue;

		/// <summary>
		/// Initialized the drawer.
		/// </summary>
		protected override void Initialize()
		{
			if (base.Attribute.MinGetter != null)
			{
				getterMinValue = ValueResolver.Get<byte>(base.Property, base.Attribute.MinGetter);
			}
			if (base.Attribute.MaxGetter != null)
			{
				getterMaxValue = ValueResolver.Get<byte>(base.Property, base.Attribute.MaxGetter);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			byte min = ((getterMinValue != null) ? getterMinValue.GetValue() : ((byte)base.Attribute.Min));
			byte max = ((getterMaxValue != null) ? getterMaxValue.GetValue() : ((byte)base.Attribute.Max));
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
				else if (value > 255)
				{
					value = 255;
				}
				base.ValueEntry.SmartValue = (byte)value;
			}
		}
	}
}

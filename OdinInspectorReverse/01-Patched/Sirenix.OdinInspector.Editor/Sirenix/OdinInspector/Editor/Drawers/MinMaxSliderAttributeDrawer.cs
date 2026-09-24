using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws Vector2 properties marked with <see cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:UnityEngine.RangeAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class MinMaxSliderAttributeDrawer : OdinAttributeDrawer<MinMaxSliderAttribute, Vector2>
	{
		private ValueResolver<double> minGetter;

		private ValueResolver<double> maxGetter;

		private ValueResolver<Vector2> rangeGetter;

		protected override void Initialize()
		{
			if (base.Attribute.MinMaxValueGetter != null)
			{
				rangeGetter = ValueResolver.Get(base.Property, base.Attribute.MinMaxValueGetter, new Vector2(base.Attribute.MinValue, base.Attribute.MaxValue));
				return;
			}
			minGetter = ValueResolver.Get(base.Property, base.Attribute.MinValueGetter, (double)base.Attribute.MinValue);
			maxGetter = ValueResolver.Get(base.Property, base.Attribute.MaxValueGetter, (double)base.Attribute.MaxValue);
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			Vector2 range = ((rangeGetter == null) ? new Vector2((float)minGetter.GetValue(), (float)maxGetter.GetValue()) : rangeGetter.GetValue());
			EditorGUI.BeginChangeCheck();
			Vector2 value = SirenixEditorFields.MinMaxSlider(label, base.ValueEntry.SmartValue, range, base.Attribute.ShowFields);
			if (EditorGUI.EndChangeCheck())
			{
				base.ValueEntry.SmartValue = value;
			}
		}
	}
}

using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws Vector2Int properties marked with <see cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />.
	/// </summary>
	public class Vector2IntMinMaxAttributeDrawer : OdinAttributeDrawer<MinMaxSliderAttribute, Vector2Int>
	{
		private ValueResolver<float> minGetter;

		private ValueResolver<float> maxGetter;

		private ValueResolver<Vector2Int> vector2IntMinMaxGetter;

		/// <summary>
		/// Initializes the drawer by resolving any optional references to members for min/max value.
		/// </summary>
		protected override void Initialize()
		{
			minGetter = ValueResolver.Get(base.Property, base.Attribute.MinValueGetter, base.Attribute.MinValue);
			maxGetter = ValueResolver.Get(base.Property, base.Attribute.MaxValueGetter, base.Attribute.MaxValue);
			if (base.Attribute.MinMaxValueGetter != null)
			{
				vector2IntMinMaxGetter = ValueResolver.Get<Vector2Int>(base.Property, base.Attribute.MinMaxValueGetter);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			ValueResolver.DrawErrors(minGetter, maxGetter, vector2IntMinMaxGetter);
			Vector2 range = default(Vector2);
			if (vector2IntMinMaxGetter != null && !vector2IntMinMaxGetter.HasError)
			{
				range = UnityShims.Vector2Int.op_Implicit(vector2IntMinMaxGetter.GetValue());
			}
			else
			{
				range.x = minGetter.GetValue();
				range.y = maxGetter.GetValue();
			}
			EditorGUI.BeginChangeCheck();
			Vector2 value = SirenixEditorFields.MinMaxSlider(label, UnityShims.Vector2Int.op_Implicit(base.ValueEntry.SmartValue), range, base.Attribute.ShowFields);
			if (EditorGUI.EndChangeCheck())
			{
				base.ValueEntry.SmartValue = new Vector2Int((int)value.x, (int)value.y);
			}
		}
	}
}

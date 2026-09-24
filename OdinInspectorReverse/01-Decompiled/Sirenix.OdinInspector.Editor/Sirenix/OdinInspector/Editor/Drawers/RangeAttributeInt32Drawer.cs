using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws int properties marked with <see cref="T:UnityEngine.RangeAttribute" />.
	/// </summary>
	/// <seealso cref="T:UnityEngine.RangeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MaxValueAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MinMaxSliderAttribute" />
	/// <seealso cref="T:UnityEngine.DelayedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	public sealed class RangeAttributeInt32Drawer : OdinAttributeDrawer<RangeAttribute, int>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<int> entry = base.ValueEntry;
			RangeAttribute attribute = base.Attribute;
			EditorGUI.BeginChangeCheck();
			int value = SirenixEditorFields.RangeIntField(label, entry.SmartValue, (int)attribute.min, (int)attribute.max);
			if (EditorGUI.EndChangeCheck())
			{
				entry.SmartValue = value;
			}
		}
	}
}

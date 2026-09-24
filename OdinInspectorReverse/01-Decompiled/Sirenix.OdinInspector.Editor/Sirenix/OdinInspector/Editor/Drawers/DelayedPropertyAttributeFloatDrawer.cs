using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws float properties marked with <see cref="T:Sirenix.OdinInspector.DelayedPropertyAttribute" />.
	/// </summary>
	public sealed class DelayedPropertyAttributeFloatDrawer : OdinAttributeDrawer<DelayedPropertyAttribute, float>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			base.ValueEntry.SmartValue = SirenixEditorFields.DelayedFloatField(label, base.ValueEntry.SmartValue, GUILayoutOptions.MinWidth(0f));
		}
	}
}

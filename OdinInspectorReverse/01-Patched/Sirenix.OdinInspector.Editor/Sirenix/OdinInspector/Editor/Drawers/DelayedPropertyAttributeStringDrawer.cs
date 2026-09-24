using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws string properties marked with <see cref="T:Sirenix.OdinInspector.DelayedPropertyAttribute" />.
	/// </summary>
	public sealed class DelayedPropertyAttributeStringDrawer : OdinAttributeDrawer<DelayedPropertyAttribute, string>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			base.ValueEntry.SmartValue = SirenixEditorFields.DelayedTextField(label, base.ValueEntry.SmartValue, GUILayoutOptions.MinWidth(0f));
		}
	}
}

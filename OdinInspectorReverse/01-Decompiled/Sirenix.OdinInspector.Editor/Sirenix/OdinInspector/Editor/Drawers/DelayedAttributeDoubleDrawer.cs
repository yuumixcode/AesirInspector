using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws double properties marked with <see cref="T:UnityEngine.DelayedAttribute" />.
	/// </summary>
	public sealed class DelayedAttributeDoubleDrawer : OdinAttributeDrawer<DelayedAttribute, double>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			base.ValueEntry.SmartValue = SirenixEditorFields.DelayedDoubleField(label, base.ValueEntry.SmartValue, GUILayoutOptions.MinWidth(0f));
		}
	}
}

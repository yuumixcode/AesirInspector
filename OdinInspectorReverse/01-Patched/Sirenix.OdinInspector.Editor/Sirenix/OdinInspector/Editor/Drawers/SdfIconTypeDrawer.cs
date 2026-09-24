using Sirenix.OdinInspector.Editor.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	[DrawerPriority(0.0, 0.0, 1.0)]
	public class SdfIconTypeDrawer : OdinValueDrawer<SdfIconType>
	{
		protected override void DrawPropertyLayout(GUIContent label)
		{
			base.ValueEntry.SmartValue = SdfIconSelector.DrawIconSelectorDropdownField(label, base.ValueEntry.SmartValue);
		}
	}
}

using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	internal class OdinMenuTreeDrawer : OdinValueDrawer<OdinMenuTree>
	{
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<OdinMenuTree> entry = base.ValueEntry;
			OdinMenuTree tree = entry.SmartValue;
			if (tree != null)
			{
				tree.DrawMenuTree();
				tree.HandleKeyboardMenuNavigation();
			}
		}
	}
}

using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public class HideIfGroupAttributeDrawer : OdinGroupDrawer<HideIfGroupAttribute>
	{
		protected override void DrawPropertyLayout(GUIContent label)
		{
			for (int i = 0; i < base.Property.Children.Count; i++)
			{
				base.Property.Children[i].Draw();
			}
		}
	}
}

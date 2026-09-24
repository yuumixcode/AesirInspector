using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Drawer for the <see cref="T:Sirenix.OdinInspector.VerticalGroupAttribute" />
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.VerticalGroupAttribute" />
	public class VerticalGroupAttributeDrawer : OdinGroupDrawer<VerticalGroupAttribute>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			InspectorProperty property = base.Property;
			VerticalGroupAttribute attribute = base.Attribute;
			GUILayout.BeginVertical();
			if (attribute.PaddingTop != 0f)
			{
				GUILayout.Space(attribute.PaddingTop);
			}
			for (int i = 0; i < property.Children.Count; i++)
			{
				InspectorProperty child = property.Children[i];
				child.Draw(child.Label);
			}
			if (attribute.PaddingBottom != 0f)
			{
				GUILayout.Space(attribute.PaddingBottom);
			}
			GUILayout.EndVertical();
		}
	}
}

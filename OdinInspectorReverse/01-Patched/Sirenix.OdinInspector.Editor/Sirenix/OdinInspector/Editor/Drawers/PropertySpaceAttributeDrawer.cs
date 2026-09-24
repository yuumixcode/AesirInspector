using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws a space for properties marked with the PropertySpace attribute.
	/// </summary>
	[DrawerPriority(2.0, 0.0, 0.0)]
	public sealed class PropertySpaceAttributeDrawer : OdinAttributeDrawer<PropertySpaceAttribute>
	{
		private bool drawSpace;

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			if (base.Property.Parent == null)
			{
				drawSpace = true;
			}
			else if (base.Property.Parent.ChildResolver is ICollectionResolver)
			{
				drawSpace = false;
			}
			else
			{
				drawSpace = true;
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (drawSpace && base.Attribute.SpaceBefore != 0f)
			{
				GUILayout.Space(base.Attribute.SpaceBefore);
			}
			CallNextDrawer(label);
			if (base.Attribute.SpaceAfter != 0f)
			{
				GUILayout.Space(base.Attribute.SpaceAfter);
			}
		}
	}
}

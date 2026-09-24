using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:UnityEngine.SpaceAttribute" />.
	/// </summary>
	/// <seealso cref="T:UnityEngine.SpaceAttribute" />
	[DrawerPriority(2.0, 0.0, 0.0)]
	public sealed class SpaceAttributeDrawer : OdinAttributeDrawer<SpaceAttribute>
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
			if (drawSpace)
			{
				SpaceAttribute attribute = base.Attribute;
				if (attribute.height == 0f)
				{
					EditorGUILayout.Space();
				}
				else
				{
					GUILayout.Space(attribute.height);
				}
			}
			CallNextDrawer(label);
		}
	}
}

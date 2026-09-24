using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Draws Vector2 properties marked with <see cref="T:Sirenix.OdinInspector.WrapAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	[DrawerPriority(0.3, 0.0, 0.0)]
	public class WrapAttributeVector2Drawer : OdinAttributeDrawer<WrapAttribute, Vector2>
	{
		/// <summary>
		/// Not yet documented.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			CallNextDrawer(label);
			base.ValueEntry.SmartValue = new Vector2(MathUtilities.Wrap(base.ValueEntry.SmartValue.x, (float)base.Attribute.Min, (float)base.Attribute.Max), MathUtilities.Wrap(base.ValueEntry.SmartValue.y, (float)base.Attribute.Min, (float)base.Attribute.Max));
		}
	}
}

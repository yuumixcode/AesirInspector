using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Draws int properties marked with <see cref="T:Sirenix.OdinInspector.WrapAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	[DrawerPriority(0.3, 0.0, 0.0)]
	public class WrapAttributeInt32Drawer : OdinAttributeDrawer<WrapAttribute, int>
	{
		/// <summary>
		/// Not yet documented.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<int> entry = base.ValueEntry;
			WrapAttribute attribute = base.Attribute;
			CallNextDrawer(label);
			base.ValueEntry.SmartValue = (int)MathUtilities.Wrap(base.ValueEntry.SmartValue, base.Attribute.Min, base.Attribute.Max);
		}
	}
}

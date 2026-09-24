using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Draws long properties marked with <see cref="T:Sirenix.OdinInspector.WrapAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	[DrawerPriority(0.3, 0.0, 0.0)]
	public class WrapAttributeInt64Drawer : OdinAttributeDrawer<WrapAttribute, long>
	{
		/// <summary>
		/// Not yet documented.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<long> entry = base.ValueEntry;
			WrapAttribute attribute = base.Attribute;
			CallNextDrawer(label);
			base.ValueEntry.SmartValue = (long)MathUtilities.Wrap(base.ValueEntry.SmartValue, base.Attribute.Min, base.Attribute.Max);
		}
	}
}

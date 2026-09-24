using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Draws float properties marked with <see cref="T:Sirenix.OdinInspector.WrapAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	[DrawerPriority(0.3, 0.0, 0.0)]
	public class WrapAttributeFloatDrawer : OdinAttributeDrawer<WrapAttribute, float>
	{
		/// <summary>
		/// Not yet documented.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			CallNextDrawer(label);
			base.ValueEntry.SmartValue = (float)MathUtilities.Wrap(base.ValueEntry.SmartValue, base.Attribute.Min, base.Attribute.Max);
		}
	}
}

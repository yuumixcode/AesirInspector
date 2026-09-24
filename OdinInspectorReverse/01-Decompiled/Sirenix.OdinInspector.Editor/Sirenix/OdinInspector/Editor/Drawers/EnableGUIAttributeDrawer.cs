using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws members marked with <see cref="T:Sirenix.OdinInspector.EnableGUIAttribute" />.
	/// </summary>
	[DrawerPriority(2.0, 0.0, 0.0)]
	public sealed class EnableGUIAttributeDrawer : OdinAttributeDrawer<EnableGUIAttribute>
	{
		protected override void DrawPropertyLayout(GUIContent label)
		{
			GUIHelper.PushGUIEnabled(enabled: true);
			CallNextDrawer(label);
			GUIHelper.PopGUIEnabled();
		}
	}
}

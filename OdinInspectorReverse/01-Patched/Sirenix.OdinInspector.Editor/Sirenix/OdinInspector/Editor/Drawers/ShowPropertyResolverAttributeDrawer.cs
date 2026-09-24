using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Drawer for the ShowPropertyResolver attribute.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.ShowPropertyResolverAttribute" />
	[DrawerPriority(10000.0, 0.0, 0.0)]
	public class ShowPropertyResolverAttributeDrawer : OdinAttributeDrawer<ShowPropertyResolverAttribute>
	{
		protected override void DrawPropertyLayout(GUIContent label)
		{
			InspectorProperty property = base.Property;
			string name = ((property.ChildResolver != null) ? property.ChildResolver.GetType().GetNiceName() : "None");
			SirenixEditorGUI.BeginToolbarBox(name, false);
			CallNextDrawer(label);
			SirenixEditorGUI.EndToolbarBox();
		}
	}
}

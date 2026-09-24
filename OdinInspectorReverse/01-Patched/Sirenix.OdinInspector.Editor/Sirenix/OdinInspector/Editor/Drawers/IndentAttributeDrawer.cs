using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.IndentAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.IndentAttribute" />
	[DrawerPriority(0.5, 0.0, 0.0)]
	public sealed class IndentAttributeDrawer : OdinAttributeDrawer<IndentAttribute>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			GUIHelper.PushIndentLevel(EditorGUI.indentLevel + base.Attribute.IndentLevel);
			CallNextDrawer(label);
			GUIHelper.PopIndentLevel();
		}
	}
}

using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Show drawer chain attribute drawer.
	/// </summary>
	[DrawerPriority(10000.0, 0.0, 0.0)]
	public class ShowDrawerChainAttributeDrawer : OdinAttributeDrawer<ShowDrawerChainAttribute>
	{
		private int drawnDepth;

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			InspectorProperty property = base.Property;
			BakedDrawerChain chain = property.GetActiveDrawerChain();
			OdinDrawer[] drawers = chain.BakedDrawerArray;
			SirenixEditorGUI.BeginToolbarBox("Drawers for property '" + base.Property.Path + "'", false);
			for (int i = 0; i < drawers.Length; i++)
			{
				bool highlight = drawers[i].GetType().Assembly != typeof(ShowDrawerChainAttributeDrawer).Assembly;
				if (highlight)
				{
					GUIHelper.PushColor(Color.green);
				}
				if (i > drawnDepth)
				{
					GUIHelper.PushColor(new Color(1f, 1f, 1f, 0.5f));
				}
				if (drawers[i] != this)
				{
					string tooltip = "You can toggle drawers on and off for debugging purposes. The state will not be saved anywhere, and is only for the current property.";
					string labelText = i + ": " + drawers[i].GetType().GetNiceName() + (drawers[i].SkipWhenDrawing ? " (skipped)" : "");
					GUIContent labelContent = GUIHelper.TempContent(labelText, tooltip);
					drawers[i].SkipWhenDrawing = !EditorGUILayout.ToggleLeft(labelContent, !drawers[i].SkipWhenDrawing);
				}
				else
				{
					EditorGUILayout.LabelField("     " + i + ": " + drawers[i].GetType().GetNiceName() + (drawers[i].SkipWhenDrawing ? " (skipped)" : ""));
				}
				Rect rect = GUILayoutUtility.GetLastRect();
				if (i > drawnDepth)
				{
					GUIHelper.PopColor();
				}
				GUI.Label(rect, DrawerUtilities.GetDrawerPriority(drawers[i].GetType()).ToString(), SirenixGUIStyles.RightAlignedGreyMiniLabel);
				if (highlight)
				{
					GUIHelper.PopColor();
				}
			}
			SirenixEditorGUI.EndToolbarBox();
			CallNextDrawer(label);
			drawnDepth = chain.CurrentIndex;
		}
	}
}

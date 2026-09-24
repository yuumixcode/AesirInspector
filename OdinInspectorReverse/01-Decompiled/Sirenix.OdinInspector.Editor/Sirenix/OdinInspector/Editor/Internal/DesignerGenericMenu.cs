using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class DesignerGenericMenu<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems
	{
		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			InspectorProperty root = property.Tree.RootProperty;
			if (!(root.ValueEntry.WeakSmartValue is DesignerSelection selection))
			{
				return;
			}
			DesignerEditor editor = selection.owningEditor;
			DesignerEditorContext context = editor?.Context;
			if (context?.SelectedNode == null)
			{
				return;
			}
			bool isChanged = context.Selection.IsPropertyChanged(property);
			genericMenu.AddSeparator(string.Empty);
			GUIContent revertButtonContent = new GUIContent("Revert To Original");
			if (isChanged)
			{
				genericMenu.AddItem(revertButtonContent, on: false, delegate
				{
					context.RevertAttributeMemberChangeOnSelection(property);
					editor.SyncIfNeeded();
				});
			}
			else
			{
				genericMenu.AddDisabledItem(revertButtonContent, on: false);
			}
		}

		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			EditorWindow focusedWindow = EditorWindow.focusedWindow;
			if (focusedWindow == null)
			{
				return false;
			}
			if (!(focusedWindow is DesignerEditorWindow))
			{
				return focusedWindow is DesignerAttributePopup;
			}
			return true;
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			CallNextDrawer(label);
		}
	}
}

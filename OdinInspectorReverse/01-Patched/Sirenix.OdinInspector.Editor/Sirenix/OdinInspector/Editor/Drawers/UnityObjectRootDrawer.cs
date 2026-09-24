using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	[DrawerPriority(0.0, 100.0, 0.0)]
	public sealed class UnityObjectRootDrawer<T> : OdinValueDrawer<T> where T : Object
	{
		public static readonly bool IsGameObject = typeof(T) == typeof(GameObject);

		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			return property.IsTreeRoot;
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (IsGameObject)
			{
				SirenixEditorGUI.MessageBox("Odin does not currently have a full GameObject inspector window substitute implemented, so a GameObject cannot be directly inspected inline in the editor.");
				SirenixEditorFields.UnityObjectField(base.ValueEntry.SmartValue, typeof(GameObject), true);
				GUILayout.BeginHorizontal();
				GUIHelper.PushGUIEnabled(base.ValueEntry.SmartValue != null);
				string text = ((base.ValueEntry.SmartValue != null) ? ("Open Inspector window for " + base.ValueEntry.SmartValue.name) : "Open Inspector window (null)");
				if (GUILayout.Button(GUIHelper.TempContent(text)))
				{
					GUIHelper.OpenInspectorWindow(base.ValueEntry.SmartValue);
					GUIHelper.ExitGUI(removeFocusControl: true);
				}
				text = ((base.ValueEntry.SmartValue != null) ? ("Select " + base.ValueEntry.SmartValue.name) : "Select GO (null)");
				if (GUILayout.Button(GUIHelper.TempContent(text)))
				{
					Selection.activeObject = base.ValueEntry.SmartValue;
					GUIHelper.ExitGUI(removeFocusControl: true);
				}
				GUIHelper.PopGUIEnabled();
				GUILayout.EndHorizontal();
			}
			else
			{
				int count = base.Property.Children.Count;
				for (int i = 0; i < count; i++)
				{
					base.Property.Children[i].Draw();
				}
			}
		}
	}
}

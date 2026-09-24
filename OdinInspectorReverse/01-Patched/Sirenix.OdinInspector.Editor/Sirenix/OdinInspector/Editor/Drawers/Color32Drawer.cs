using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Color32 property drawer.
	/// </summary>
	public sealed class Color32Drawer : PrimitiveCompositeDrawer<Color32>, IDefinesGenericMenuItems
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyField(IPropertyValueEntry<Color32> entry, GUIContent label)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null);
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, label);
			}
			bool disableContext = false;
			if (Event.current.OnMouseDown(rect, 1, useEvent: false))
			{
				GUIHelper.PushEventType(EventType.Used);
				disableContext = true;
			}
			entry.SmartValue = UnityShims.Color32.op_Implicit(EditorGUI.ColorField(rect, UnityShims.Color32.op_Implicit(entry.SmartValue)));
			if (disableContext)
			{
				GUIHelper.PopEventType();
			}
		}

		void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			ColorDrawer.PopulateGenericMenu((IPropertyValueEntry<Color32>)property.ValueEntry, genericMenu);
		}
	}
}

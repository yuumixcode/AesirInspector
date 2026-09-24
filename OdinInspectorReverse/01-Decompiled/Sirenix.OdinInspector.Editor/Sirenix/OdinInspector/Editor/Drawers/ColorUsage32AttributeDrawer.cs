using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws Color properties marked with <see cref="T:UnityEngine.ColorUsageAttribute" />.
	/// </summary>
	public sealed class ColorUsage32AttributeDrawer : OdinAttributeDrawer<ColorUsageAttribute, Color32>, IDefinesGenericMenuItems
	{
		private ColorPickerHDRConfig pickerConfig;

		protected override void Initialize()
		{
			pickerConfig = new ColorPickerHDRConfig(base.Attribute.minBrightness, base.Attribute.maxBrightness, base.Attribute.minExposureValue, base.Attribute.maxExposureValue);
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null);
			bool disableContext = false;
			if (Event.current.OnMouseDown(rect, 1, useEvent: false))
			{
				GUIHelper.PushEventType(EventType.Used);
				disableContext = true;
			}
			base.ValueEntry.SmartValue = UnityShims.Color32.op_Implicit(EditorGUI.ColorField(rect, label ?? GUIContent.none, UnityShims.Color32.op_Implicit(base.ValueEntry.SmartValue), showEyedropper: true, base.Attribute.showAlpha, base.Attribute.hdr, pickerConfig));
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

using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class ButtonHeightSelectorAttributeDrawer : OdinAttributeDrawer<ButtonHeightSelectorAttribute, int>
	{
		private static GUIStyle _style;

		private static GUIStyle Style
		{
			get
			{
				GUIStyle obj = _style ?? new GUIStyle(EditorStyles.numberField)
				{
					padding = new RectOffset(3, 18, 2, 1)
				};
				_style = obj;
				return obj;
			}
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight);
			int slideRectId = GUIUtility.GetControlID(FocusType.Passive, rect);
			if (label != null)
			{
				base.ValueEntry.SmartValue = SirenixEditorGUI.SlideRectInt(rect.AlignLeft(EditorGUIUtility.labelWidth), slideRectId, base.ValueEntry.SmartValue);
				rect = EditorGUI.PrefixLabel(rect, label);
			}
			Rect dropdownRect = rect.AlignRight(EditorGUIUtility.singleLineHeight);
			EditorGUIUtility.AddCursorRect(dropdownRect, MouseCursor.Arrow);
			if (GUI.Button(dropdownRect, "", GUIStyle.none))
			{
				int[] values = (from ButtonSizes e in Enum.GetValues(typeof(ButtonSizes))
					select (int)e).ToArray();
				GenericSelector<int> selector = new GenericSelector<int>("", supportsMultiSelect: false, (int i) => Enum.GetName(typeof(ButtonSizes), i) ?? i.ToString(), values);
				selector.EnableSingleClickToSelect();
				selector.SelectionConfirmed += delegate(IEnumerable<int> selections)
				{
					int smartValue = selections.First();
					base.ValueEntry.SmartValue = smartValue;
				};
				selector.ShowInPopup();
			}
			base.ValueEntry.SmartValue = EditorGUI.IntField(rect, base.ValueEntry.SmartValue, Style);
			SdfIcons.DrawIcon(dropdownRect.Padding(4f), SdfIconType.CaretDownFill);
		}
	}
}

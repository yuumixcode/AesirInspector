using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class ColorPaletteNameSelectorAttributeDrawer : OdinAttributeDrawer<ColorPaletteNameSelectorAttribute, string>
	{
		private static GUIStyle _style;

		private static GUIStyle Style
		{
			get
			{
				GUIStyle obj = _style ?? new GUIStyle(EditorStyles.textField)
				{
					padding = new RectOffset(3, 18, 2, 1),
					alignment = TextAnchor.MiddleLeft
				};
				_style = obj;
				return obj;
			}
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight);
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, label);
			}
			Rect dropdownRect = rect.AlignRight(EditorGUIUtility.singleLineHeight);
			EditorGUIUtility.AddCursorRect(dropdownRect, MouseCursor.Arrow);
			if (GUI.Button(dropdownRect, "", GUIStyle.none))
			{
				IEnumerable<string> names = GlobalConfig<ColorPaletteManager>.Instance.ColorPalettes.Select((ColorPalette p) => p.Name);
				GenericSelector<string> selector = new GenericSelector<string>(names);
				selector.EnableSingleClickToSelect();
				selector.SelectionConfirmed += delegate(IEnumerable<string> selections)
				{
					string text = selections.FirstOrDefault();
					if (text != null)
					{
						base.ValueEntry.SmartValue = text;
					}
				};
				selector.ShowInPopup();
			}
			base.ValueEntry.SmartValue = EditorGUI.TextField(rect, base.ValueEntry.SmartValue, Style);
			SdfIcons.DrawIcon(dropdownRect.Padding(4f), SdfIconType.CaretDownFill);
		}
	}
}

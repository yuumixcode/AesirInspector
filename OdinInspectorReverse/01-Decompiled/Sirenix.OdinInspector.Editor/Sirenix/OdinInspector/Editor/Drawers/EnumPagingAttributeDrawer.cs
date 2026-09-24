using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Odin drawer for the <see cref="T:Sirenix.OdinInspector.EnumPagingAttribute" />.
	/// </summary>
	public class EnumPagingAttributeDrawer<T> : OdinAttributeDrawer<EnumPagingAttribute, T>
	{
		/// <summary>
		/// Returns <c>true</c> if the drawer can draw the type.
		/// </summary>
		public override bool CanDrawTypeFilter(Type type)
		{
			return type.IsEnum;
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> entry = base.ValueEntry;
			Rect rect = EditorGUILayout.GetControlRect(label != null);
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, label);
			}
			Rect btnRightRect = rect.AlignRight(20f);
			Rect btnLeftRect = btnRightRect;
			btnLeftRect.x -= btnLeftRect.width;
			btnLeftRect.height -= 1f;
			btnRightRect.height -= 1f;
			if (GUI.Button(btnLeftRect, GUIContent.none))
			{
				string[] names = Enum.GetNames(typeof(T));
				string name = Enum.GetName(typeof(T), entry.SmartValue);
				int currNameIndex = ((IList<string>)names).IndexOf(name);
				currNameIndex = MathUtilities.Wrap(currNameIndex - 1, 0, names.Length);
				entry.SmartValue = (T)Enum.Parse(typeof(T), names[currNameIndex]);
			}
			if (GUI.Button(btnRightRect, GUIContent.none))
			{
				string[] names2 = Enum.GetNames(typeof(T));
				string name2 = Enum.GetName(typeof(T), entry.SmartValue);
				int currNameIndex2 = ((IList<string>)names2).IndexOf(name2);
				currNameIndex2 = MathUtilities.Wrap(currNameIndex2 + 1, 0, names2.Length);
				entry.SmartValue = (T)Enum.Parse(typeof(T), names2[currNameIndex2]);
			}
			EditorIcons.TriangleLeft.Draw(btnLeftRect.AlignCenter(16f, 16f));
			EditorIcons.TriangleRight.Draw(btnRightRect.AlignCenter(16f, 16f));
			rect.xMax -= btnRightRect.width * 2f;
			entry.WeakSmartValue = SirenixEditorFields.EnumDropdown(rect, (Enum)entry.WeakSmartValue);
		}
	}
}

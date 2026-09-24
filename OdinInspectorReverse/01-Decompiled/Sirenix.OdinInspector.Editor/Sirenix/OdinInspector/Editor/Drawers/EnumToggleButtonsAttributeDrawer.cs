using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws an enum in a horizontal button group instead of a dropdown.
	/// </summary>
	public class EnumToggleButtonsAttributeDrawer<T> : OdinAttributeDrawer<EnumToggleButtonsAttribute, T>
	{
		private static Color ActiveColor = (EditorGUIUtility.isProSkin ? Color.white : new Color(0.802f, 0.802f, 0.802f, 1f));

		private static Color InactiveColor = (EditorGUIUtility.isProSkin ? new Color(0.75f, 0.75f, 0.75f, 1f) : Color.white);

		private (GUIContent name, ulong value, SdfIconType icon, string tooltip)[] Members;

		private float[] NameSizes;

		private bool IsFlagsEnum;

		private List<int> ColumnCounts;

		private float PreviousControlRectWidth;

		/// <summary>
		/// Returns <c>true</c> if the drawer can draw the type.
		/// </summary>
		public override bool CanDrawTypeFilter(Type type)
		{
			return type.IsEnum;
		}

		protected override void Initialize()
		{
			Type enumType = base.ValueEntry.TypeOfValue;
			string[] enumNames = Enum.GetNames(enumType);
			Members = EnumTypeUtilities<T>.VisibleEnumMemberInfos.Select((EnumTypeUtilities<T>.EnumMember x) => (new GUIContent(x.NiceName), TypeExtensions.GetEnumBitmask(Enum.Parse(enumType, x.Name), enumType), Icon: x.Icon, Tooltip: x.Tooltip)).ToArray();
			IsFlagsEnum = enumType.IsDefined<FlagsAttribute>();
			NameSizes = Members.Select(((GUIContent name, ulong value, SdfIconType icon, string tooltip) x) => SirenixGUIStyles.MiniButtonMid.CalcSize(x.name).x).ToArray();
			ColumnCounts = new List<int> { NameSizes.Length };
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> entry = base.ValueEntry;
			Type t = entry.WeakValues[0].GetType();
			int i;
			for (i = 1; i < entry.WeakValues.Count; i++)
			{
				if (t != entry.WeakValues[i].GetType())
				{
					SirenixEditorGUI.MessageBox("ToggleEnum does not support multiple different enum types.", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
					return;
				}
			}
			ulong value = TypeExtensions.GetEnumBitmask(entry.SmartValue, typeof(T));
			Rect controlRect = default(Rect);
			i = 0;
			for (int j = 0; j < ColumnCounts.Count; j++)
			{
				SirenixEditorGUI.GetFeatureRichControlRect((j == 0) ? label : GUIContent.none, out var _, out var _, out var rect);
				if (j == 0)
				{
					controlRect = rect;
				}
				else
				{
					rect.xMin = controlRect.xMin;
				}
				float xMax = rect.xMax;
				rect.width /= ColumnCounts[j];
				rect.width = (int)rect.width;
				int from = i;
				for (int to = i + ColumnCounts[j]; i < to; i++)
				{
					(GUIContent, ulong, SdfIconType, string) member = Members[i];
					bool selected;
					if (IsFlagsEnum)
					{
						ulong mask = TypeExtensions.GetEnumBitmask(member.Item2, typeof(T));
						selected = ((value == 0L) ? (mask == 0) : (mask != 0L && (mask & value) == mask));
					}
					else
					{
						selected = member.Item2 == value;
					}
					Rect btnRect = rect;
					GUIStyle style;
					if (i == from && i == to - 1)
					{
						style = SirenixGUIStyles.MiniButton;
						btnRect.x -= 1f;
						btnRect.xMax = xMax + 1f;
					}
					else if (i == from)
					{
						style = SirenixGUIStyles.MiniButtonLeft;
					}
					else if (i == to - 1)
					{
						style = SirenixGUIStyles.MiniButtonRight;
						btnRect.xMax = xMax;
					}
					else
					{
						style = SirenixGUIStyles.MiniButtonMid;
					}
					member.Item1.tooltip = member.Item4;
					if (SirenixEditorGUI.SDFIconButton(btnRect, member.Item1, member.Item3, IconAlignment.LeftOfText, style, selected))
					{
						GUIHelper.RemoveFocusControl();
						if (!IsFlagsEnum || Event.current.button == 1 || UnityShims.Misc.GetEventModifiers(Event.current) == 2)
						{
							entry.WeakSmartValue = Enum.ToObject(typeof(T), member.Item2);
						}
						else
						{
							value = ((member.Item2 == 0L) ? 0 : ((!selected) ? (value | member.Item2) : (value & ~member.Item2)));
							entry.WeakSmartValue = Enum.ToObject(typeof(T), value);
						}
						GUIHelper.RequestRepaint();
					}
					rect.x += rect.width;
				}
			}
			if (Event.current.type != EventType.Repaint || PreviousControlRectWidth == controlRect.width)
			{
				return;
			}
			PreviousControlRectWidth = controlRect.width;
			float maxBtnWidth = 0f;
			int row = 0;
			ColumnCounts.Clear();
			ColumnCounts.Add(0);
			for (i = 0; i < NameSizes.Length; i++)
			{
				float btnWidth = NameSizes[i] + 3f;
				int columnCount = ++ColumnCounts[row];
				float columnWidth = controlRect.width / (float)columnCount;
				maxBtnWidth = Mathf.Max(btnWidth, maxBtnWidth);
				if (maxBtnWidth > columnWidth && columnCount > 1)
				{
					ColumnCounts[row]--;
					ColumnCounts.Add(1);
					row++;
					maxBtnWidth = btnWidth;
				}
			}
		}
	}
}

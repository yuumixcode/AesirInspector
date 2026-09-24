using System;
using System.Text.RegularExpressions;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.DisplayAsStringAttribute" />.
	/// Calls the properties ToString method to get the string to draw.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.HideLabelAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.LabelTextAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.InfoBoxAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DetailedInfoBoxAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.MultiLinePropertyAttribute" />
	/// <seealso cref="T:UnityEngine.MultilineAttribute" />
	[DrawerPriority(0.0, 0.0, 999.9995)]
	public sealed class DisplayAsStringAttributeDrawer<T> : OdinAttributeDrawer<DisplayAsStringAttribute, T>, IDefinesGenericMenuItems
	{
		private GUIStyle labelStyle;

		private readonly Regex richTextTags = new Regex("<\\/?\\s*(b|i|size|color|material|quad)(\\s*[^>]*)?>");

		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			genericMenu.AddItem(new GUIContent("Copy to clipboard"), on: false, delegate
			{
				string input = ((base.ValueEntry.SmartValue == null) ? "Null" : base.ValueEntry.SmartValue.ToString());
				input = richTextTags.Replace(input, string.Empty);
				GUIUtility.systemCopyBuffer = input;
			});
			genericMenu.AddItem(new GUIContent("Copy to clipboard (include rich text tags)"), on: false, delegate
			{
				string systemCopyBuffer = ((base.ValueEntry.SmartValue == null) ? "Null" : base.ValueEntry.SmartValue.ToString());
				GUIUtility.systemCopyBuffer = systemCopyBuffer;
			});
		}

		protected override void Initialize()
		{
			TextAnchor alignment = base.Attribute.Alignment switch
			{
				TextAlignment.Right => TextAnchor.MiddleRight, 
				TextAlignment.Center => TextAnchor.MiddleCenter, 
				_ => TextAnchor.MiddleLeft, 
			};
			labelStyle = new GUIStyle(EditorStyles.label)
			{
				alignment = alignment,
				richText = base.Attribute.EnableRichText,
				stretchWidth = !base.Attribute.Overflow,
				wordWrap = !base.Attribute.Overflow,
				fontSize = base.Attribute.FontSize
			};
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> entry = base.ValueEntry;
			DisplayAsStringAttribute attribute = base.Attribute;
			if (entry.Property.ChildResolver is ICollectionResolver)
			{
				CallNextDrawer(label);
				return;
			}
			string str = ((string.IsNullOrEmpty(base.Attribute.Format) || !(entry.SmartValue is IFormattable f)) ? ((entry.SmartValue == null) ? "Null" : entry.SmartValue.ToString()) : f.ToString(base.Attribute.Format, null));
			if (label == null)
			{
				EditorGUILayout.LabelField(str, labelStyle, GUILayoutOptions.MinWidth(0f));
			}
			else if (!attribute.Overflow)
			{
				GUIContent stringLabel = GUIHelper.TempContent(str);
				Rect position = EditorGUILayout.GetControlRect(hasLabel: false, labelStyle.CalcHeight(stringLabel, entry.Property.LastDrawnValueRect.width - GUIHelper.BetterLabelWidth), GUILayoutOptions.MinWidth(0f));
				Rect rect = EditorGUI.PrefixLabel(position, label);
				GUI.Label(rect, stringLabel, labelStyle);
			}
			else
			{
				SirenixEditorGUI.GetFeatureRichControlRect(label, out var _, out var _, out var rect2);
				GUI.Label(rect2, str, labelStyle);
			}
		}
	}
}

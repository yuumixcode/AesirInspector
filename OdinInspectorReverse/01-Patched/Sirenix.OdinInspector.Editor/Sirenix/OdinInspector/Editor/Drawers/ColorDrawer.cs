using System;
using Clipboard = Sirenix.Utilities.Editor.Clipboard;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Color property drawer.
	/// </summary>
	public sealed class ColorDrawer : PrimitiveCompositeDrawer<Color>, IDefinesGenericMenuItems
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyField(IPropertyValueEntry<Color> entry, GUIContent label)
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
			entry.SmartValue = EditorGUI.ColorField(rect, entry.SmartValue);
			if (disableContext)
			{
				GUIHelper.PopEventType();
			}
		}

		internal static void PopulateGenericMenu<T>(IPropertyValueEntry<T> entry, GenericMenu genericMenu)
		{
			Color color;
			if (entry.TypeOfValue == typeof(Color))
			{
				color = (Color)(object)entry.SmartValue;
			}
			else
			{
				color = UnityShims.Color32.op_Implicit((Color32)(object)entry.SmartValue);
			}
			Color colorInClipboard;
			bool hasColorInClipboard = ColorExtensions.TryParseString(EditorGUIUtility.systemCopyBuffer, out colorInClipboard);
			if (genericMenu.GetItemCount() > 0)
			{
				genericMenu.AddSeparator("");
			}
			genericMenu.AddItem(new GUIContent("Copy RGBA"), on: false, delegate
			{
				EditorGUIUtility.systemCopyBuffer = entry.SmartValue.ToString();
			});
			genericMenu.AddItem(new GUIContent("Copy HEX"), on: false, delegate
			{
				EditorGUIUtility.systemCopyBuffer = "#" + ColorUtility.ToHtmlStringRGBA(color);
			});
			genericMenu.AddItem(new GUIContent("Copy Color Code Declaration"), on: false, delegate
			{
				EditorGUIUtility.systemCopyBuffer = color.ToCSharpColor();
			});
			if (hasColorInClipboard)
			{
				genericMenu.ReplaceOrAdd("Paste", on: false, delegate
				{
					entry.Property.Tree.DelayActionUntilRepaint(delegate
					{
						SetEntryValue(entry, colorInClipboard);
					});
					GUIHelper.RequestRepaint();
				});
			}
			else if (Clipboard.CanPaste(typeof(Color)) || Clipboard.CanPaste(typeof(Color32)))
			{
				genericMenu.ReplaceOrAdd("Paste", on: false, delegate
				{
					entry.Property.Tree.DelayActionUntilRepaint(delegate
					{
						SetEntryValue(entry, Clipboard.Paste());
					});
					GUIHelper.RequestRepaint();
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Paste"));
			}
		}

		private static void SetEntryValue<T>(IPropertyValueEntry<T> entry, object value)
		{
			Type type = value.GetType();
			T tValue = ((typeof(T) == typeof(Color)) ? ((!(type == typeof(Color))) ? ((T)(object)UnityShims.Color32.op_Implicit((Color32)value)) : ((T)value)) : ((!(type == typeof(Color))) ? ((T)value) : ((T)(object)UnityShims.Color32.op_Implicit((Color)value))));
			for (int i = 0; i < entry.ValueCount; i++)
			{
				entry.Values[i] = tValue;
			}
		}

		void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			PopulateGenericMenu((IPropertyValueEntry<Color>)property.ValueEntry, genericMenu);
		}
	}
}

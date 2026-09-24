using System;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	/// <summary> Temporary. </summary>
	/// <warning>This implementation <b>will</b> get refactored.</warning>
	internal static class OdinInternalDragAndDropUtils
	{
		public static object ObjectSelectorZone(Rect position, Rect popupPosition, object value, Type type, bool allowSceneObjects, int id, InspectorProperty property, object selectorKey, int selectorId)
		{
			Rect selectRect = position.AlignBottom(15f).AlignCenter(45f);
			UnityEngine.Object uObj = value as UnityEngine.Object;
			selectRect.xMin = Mathf.Max(selectRect.xMin, position.xMin);
			bool isMouseOver = Event.current.IsMouseOver(position);
			bool hasKeyboardFocus = GUIUtility.keyboardControl == id;
			bool hide = DragAndDropUtilities.IsDragging || (Event.current.type == EventType.Repaint && !isMouseOver);
			int selectButtonID = GUIUtility.GetControlID(FocusType.Passive, selectRect);
			if (!hide)
			{
				if ((bool)uObj)
				{
					Rect inspectBtn = position.AlignRight(14f);
					inspectBtn.height = 14f;
					SirenixEditorGUI.BeginDrawOpenInspector(inspectBtn, uObj, position);
					SirenixEditorGUI.EndDrawOpenInspector(inspectBtn, uObj);
				}
				bool isPressed = false;
				if (SirenixEditorGUI.DoButton(selectRect, selectButtonID, out var isHover, out var isActive))
				{
					isPressed = true;
					GUIHelper.RemoveFocusControl();
					Event.current.Use();
				}
				if (Event.current.type == EventType.Repaint)
				{
					SirenixGUIStyles.TagButton.Draw(selectRect, "Select", isHover, isActive, on: false, hasKeyboardFocus: false);
				}
				if (hasKeyboardFocus && Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown)
				{
					isPressed = true;
					Event.current.Use();
				}
				if (isPressed)
				{
					if (property != null)
					{
						OdinObjectSelector.Show(popupPosition, selectorKey, selectorId, property, allowSceneObjects);
					}
					else
					{
						OdinObjectSelector.Show(popupPosition, selectorKey, selectorId, value, type, type, allowSceneObjects, disallowNullValues: false, property);
					}
				}
			}
			if (OdinObjectSelector.IsReadyToClaim(selectorKey, selectorId))
			{
				if (property != null)
				{
					value = OdinObjectSelector.ClaimAndAssign(property);
					GUI.changed = true;
					return value;
				}
				GUI.changed = true;
				return OdinObjectSelector.Claim();
			}
			if (hasKeyboardFocus && Event.current.keyCode == KeyCode.Delete && Event.current.type == EventType.KeyDown)
			{
				Event.current.Use();
				GUI.changed = true;
				return null;
			}
			if ((bool)uObj && Event.current.rawType == EventType.MouseUp && isMouseOver && Event.current.button == 0)
			{
				UnityEngine.Object pingObj = uObj;
				if (pingObj is Component component)
				{
					pingObj = component.gameObject;
				}
				EditorGUIUtility.PingObject(pingObj);
			}
			EditorGUI.BeginChangeCheck();
			value = OdinObjectSelector.GetChangedObject(value, selectorKey, selectorId);
			if (EditorGUI.EndChangeCheck())
			{
				if (property != null)
				{
					object capturedValue = value;
					property.Tree.DelayActionUntilRepaint(delegate
					{
						property.ValueEntry.WeakSmartValue = capturedValue;
					});
				}
				GUIHelper.RequestRepaint();
			}
			return value;
		}

		public static object ObjectPickerZone(Rect rect, object value, Type type, bool allowSceneObjects, int id)
		{
			GUIUtility.GetControlID(FocusType.Passive);
			ObjectPicker objectPicker = ObjectPicker.GetObjectPicker(type.FullName + "+" + GUIHelper.CurrentWindowEntityId.ToString() + "+" + id, type);
			Rect selectRect = rect.AlignBottom(15f).AlignCenter(45f);
			UnityEngine.Object uObj = value as UnityEngine.Object;
			selectRect.xMin = Mathf.Max(selectRect.xMin, rect.xMin);
			bool hide = DragAndDropUtilities.IsDragging || (Event.current.type == EventType.Repaint && !rect.Contains(Event.current.mousePosition));
			if (hide)
			{
				GUIHelper.PushColor(new Color(0f, 0f, 0f, 0f));
				GUIHelper.PushGUIEnabled(enabled: false);
			}
			bool hideInspectorBtn = !hide && !uObj;
			if (hideInspectorBtn)
			{
				GUIHelper.PushGUIEnabled(enabled: false);
				GUIHelper.PushColor(new Color(0f, 0f, 0f, 0f));
			}
			Rect inspectBtn = rect.AlignRight(14f);
			inspectBtn.height = 14f;
			SirenixEditorGUI.BeginDrawOpenInspector(inspectBtn, uObj, rect);
			SirenixEditorGUI.EndDrawOpenInspector(inspectBtn, uObj);
			if (hideInspectorBtn)
			{
				GUIHelper.PopColor();
				GUIHelper.PopGUIEnabled();
			}
			if (GUI.Button(selectRect, "select", SirenixGUIStyles.TagButton))
			{
				GUIHelper.RemoveFocusControl();
				objectPicker.ShowObjectPicker(value, allowSceneObjects, rect);
				Event.current.Use();
			}
			if (Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown && GUIUtility.keyboardControl == id)
			{
				objectPicker.ShowObjectPicker(value, allowSceneObjects, rect);
				Event.current.Use();
			}
			if (hide)
			{
				GUIHelper.PopColor();
				GUIHelper.PopGUIEnabled();
			}
			if (objectPicker.IsReadyToClaim)
			{
				GUIHelper.RequestRepaint();
				GUI.changed = true;
				object newValue = objectPicker.ClaimObject();
				Event.current.Use();
				return newValue;
			}
			if (objectPicker.IsPickerOpen && typeof(UnityEngine.Object).IsAssignableFrom(type))
			{
				return objectPicker.CurrentSelectedObject;
			}
			if (Event.current.keyCode == KeyCode.Delete && Event.current.type == EventType.KeyDown && GUIUtility.keyboardControl == id)
			{
				Event.current.Use();
				GUI.changed = true;
				return null;
			}
			if ((bool)uObj && Event.current.rawType == EventType.MouseUp && rect.Contains(Event.current.mousePosition) && Event.current.button == 0)
			{
				UnityEngine.Object pingObj = uObj;
				if (pingObj is Component)
				{
					pingObj = (pingObj as Component).gameObject;
				}
				EditorGUIUtility.PingObject(pingObj);
			}
			return value;
		}

		public static object DoObjectPickerZoneWrapper(Rect rect, Rect popupRect, object value, Type type, bool allowSceneObjects, int id)
		{
			if (GlobalConfig<GeneralDrawerConfig>.Instance.useOldTypeSelector)
			{
				return ObjectPickerZone(rect, value, type, allowSceneObjects, id);
			}
			return ObjectSelectorZone(rect, popupRect, value, type, allowSceneObjects, id, null, null, id);
		}
	}
}

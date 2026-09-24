using System;
using Sirenix.Config;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	/// <summary> Temporary. </summary>
	/// <warning>This implementation <b>will</b> get refactored.</warning>
	internal static class OdinInternalEditorFields
	{
		public struct UnityObjectFieldArgs
		{
			public InspectorProperty Property;

			public Rect Rect;

			public GUIContent Label;

			public Type BaseType;

			public UnityEngine.Object Value;

			public bool AllowSceneObjects;

			public bool ReadOnly;

			public bool ReadOnlyDontDisableGUI;

			public static UnityObjectFieldArgs CreateForProperty(InspectorProperty property, Rect rect, bool allowSceneObjects, GUIContent label = null)
			{
				UnityObjectFieldArgs result = new UnityObjectFieldArgs
				{
					Property = property,
					Rect = rect,
					Label = label
				};
				IPropertyValueEntry valueEntry = property.ValueEntry;
				if (valueEntry != null)
				{
					object weakValue = valueEntry.WeakSmartValue;
					result.BaseType = valueEntry.BaseValueType;
					result.Value = weakValue as UnityEngine.Object;
				}
				else
				{
					result.BaseType = property.Info.TypeOfValue;
					result.Value = null;
				}
				result.AllowSceneObjects = allowSceneObjects;
				result.ReadOnly = false;
				result.ReadOnlyDontDisableGUI = false;
				return result;
			}

			public static UnityObjectFieldArgs CreateForWrapper(Rect position, GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects, bool readOnly)
			{
				return new UnityObjectFieldArgs
				{
					Property = null,
					Rect = position,
					Label = label,
					BaseType = objectType,
					Value = value,
					AllowSceneObjects = allowSceneObjects,
					ReadOnly = readOnly,
					ReadOnlyDontDisableGUI = false
				};
			}
		}

		public struct PolymorphicFieldArgs
		{
			public InspectorProperty Property;

			public Rect Rect;

			public int Id;

			public bool HasKeyboardFocus;

			public GUIContent Label;

			public object Value;

			public Type ValueType;

			public Type BaseType;

			public bool AllowSceneObjects;

			public bool DisallowNullValues;

			public bool ReadOnly;

			public bool ShowBaseType;

			public string Title;

			public static PolymorphicFieldArgs CreateForProperty(InspectorProperty property, Rect rect, int id, GUIContent label = null)
			{
				PolymorphicFieldArgs result = new PolymorphicFieldArgs
				{
					Property = property,
					Rect = rect
				};
				if (id == 0)
				{
					id = GUIUtility.GetControlID(PolymorphicFieldHash, FocusType.Keyboard, rect);
				}
				result.Id = id;
				result.HasKeyboardFocus = GUIUtility.keyboardControl == id;
				result.Label = label;
				IPropertyValueEntry valueEntry = property.ValueEntry;
				if (valueEntry != null)
				{
					object weakValue = valueEntry.WeakSmartValue;
					result.Value = weakValue;
					result.ValueType = valueEntry.TypeOfValue;
					result.BaseType = valueEntry.BaseValueType;
					result.DisallowNullValues = !property.ValueEntry.SerializationBackend.SupportsPolymorphism;
				}
				else
				{
					result.Value = null;
					result.ValueType = property.Info.TypeOfValue;
					result.BaseType = property.Info.TypeOfValue;
					result.DisallowNullValues = false;
				}
				result.AllowSceneObjects = property.GetAttribute<AssetsOnlyAttribute>() == null;
				result.ReadOnly = false;
				result.ShowBaseType = true;
				result.Title = null;
				return result;
			}

			public static PolymorphicFieldArgs CreateForWrapper(Rect position, object value, Type baseType, bool allowSceneObjects, bool hasKeyboardFocus, int id, bool disallowNullValues = false, bool readOnly = false, bool showBaseType = true, string title = null)
			{
				Type valueType = ((value == null) ? baseType : value.GetType());
				return new PolymorphicFieldArgs
				{
					Property = null,
					Rect = position,
					Id = id,
					HasKeyboardFocus = hasKeyboardFocus,
					Label = null,
					Value = value,
					ValueType = valueType,
					BaseType = baseType,
					AllowSceneObjects = allowSceneObjects,
					DisallowNullValues = disallowNullValues,
					ReadOnly = readOnly,
					ShowBaseType = showBaseType,
					Title = title
				};
			}
		}

		public struct UnityPreviewFieldArgs
		{
			public InspectorProperty Property;

			public Rect Rect;

			public int Id;

			public GUIContent Label;

			public UnityEngine.Object Value;

			public Type Type;

			public Texture Preview;

			public Sirenix.Utilities.Editor.ObjectFieldAlignment Alignment;

			public bool DragOnly;

			public bool AllowMove;

			public bool AllowSwap;

			public bool AllowSceneObjects;

			public static Rect GetDefaultRect(GUIContent label, float height = 30f)
			{
				return EditorGUILayout.GetControlRect(label != null, height);
			}

			public static UnityPreviewFieldArgs CreateForProperty(InspectorProperty property, Rect rect, Sirenix.Utilities.Editor.ObjectFieldAlignment alignment, int id = 0, GUIContent label = null)
			{
				UnityPreviewFieldArgs result = default(UnityPreviewFieldArgs);
				if (id == 0)
				{
					id = DragAndDropUtilities.GetDragAndDropId(rect);
				}
				result.Property = property;
				result.Rect = rect;
				result.Id = id;
				result.Label = label;
				if (property.ValueEntry != null)
				{
					result.Value = property.ValueEntry.WeakSmartValue as UnityEngine.Object;
					result.Type = property.ValueEntry.BaseValueType;
				}
				else
				{
					result.Value = null;
					result.Type = property.Info.TypeOfValue;
				}
				result.Preview = null;
				result.Alignment = alignment;
				result.DragOnly = false;
				result.AllowMove = true;
				result.AllowSwap = true;
				result.AllowSceneObjects = true;
				return result;
			}
		}

		private const int INVALID_SELECTOR_ID = 0;

		public static int PolymorphicFieldHash = "PolymorphicFieldHash".GetHashCode();

		public static object TryGetSelectorKeyFromProperty(InspectorProperty property)
		{
			if (property == null || property.Path == null || property.Tree.RootProperty == null)
			{
				return property;
			}
			object owner = property.Tree.RootProperty.ValueEntry.WeakSmartValue;
			if (owner is UnityEngine.Object unityObj && unityObj != null)
			{
				return $"{OdinEntityId.FromObject(unityObj)}:{property.Path}";
			}
			return property;
		}

		public static UnityEngine.Object UnityObjectField(in UnityObjectFieldArgs args)
		{
			UnityEngine.Object originalValue = args.Value;
			UnityEngine.Object value = args.Value;
			InspectorProperty property = args.Property;
			Rect position = args.Rect;
			GUIContent label = args.Label;
			Type objectType = args.BaseType;
			bool allowSceneObjects = args.AllowSceneObjects;
			bool readOnly = args.ReadOnly || args.ReadOnlyDontDisableGUI;
			bool disableGui = args.ReadOnly;
			Rect penRect;
			if (GlobalConfig<GeneralDrawerConfig>.Instance.useOldUnityObjectField)
			{
				bool originalValueWasFakeNull = value == null && (object)value != null;
				string cachedLabel = null;
				if (label != null)
				{
					cachedLabel = label.text;
				}
				penRect = position;
				penRect.x += penRect.width - 38f;
				penRect.width = 20f;
				SirenixEditorGUI.BeginDrawOpenInspector(penRect, value, SirenixEditorGUI.IndentLabelRect(position, label != null));
				value = ((cachedLabel == null) ? EditorGUI.ObjectField(position, value, objectType, allowSceneObjects) : EditorGUI.ObjectField(position, cachedLabel, value, objectType, allowSceneObjects));
				SirenixEditorGUI.EndDrawOpenInspector(penRect, value);
				if (originalValueWasFakeNull && (object)value == null)
				{
					value = originalValue;
				}
				return value;
			}
			int id = GUIUtility.GetControlID(EditorGUI_Internals.ObjectFieldHash, FocusType.Keyboard, position);
			object selectorKey = TryGetSelectorKeyFromProperty(args.Property);
			int selectorId = ((selectorKey == null) ? id : (-2147475450));
			if (Event.current.rawType == EventType.MouseDown && Event.current.button == 0 && Event.current.IsMouseOver(position))
			{
				GUIUtility.keyboardControl = id;
			}
			position = ((label == null) ? EditorGUI.IndentedRect(position) : EditorGUI.PrefixLabel(position, id, label));
			selectorId = ((selectorId == 0) ? id : selectorId);
			bool hasProperty = property != null;
			bool wasFakeNull = value == null && (object)value != null;
			penRect = position;
			penRect.x += penRect.width - 20f;
			penRect.width = 20f;
			Rect fieldButtonPosition;
			bool isFieldButtonHover;
			bool isFieldButtonActive;
			if (!args.ReadOnlyDontDisableGUI)
			{
				fieldButtonPosition = position.AlignRight(position.height);
				if (SirenixEditorGUI.DoButton(fieldButtonPosition, id, out isFieldButtonHover, out isFieldButtonActive) && !readOnly)
				{
					OdinObjectSelector.Show(position, selectorKey, selectorId, value, objectType, allowSceneObjects, disallowNullValues: false, property);
				}
			}
			else
			{
				fieldButtonPosition = Rect.zero;
				isFieldButtonHover = false;
				isFieldButtonActive = false;
			}
			penRect.x -= fieldButtonPosition.width;
			Rect dragDropRect = position;
			dragDropRect.width -= fieldButtonPosition.width + penRect.width;
			if (!readOnly)
			{
				EditorGUI.BeginChangeCheck();
				value = DragAndDropUtilities.DragAndDropZone(dragDropRect, value, objectType, allowMove: true, allowSwap: true, allowSceneObjects) as UnityEngine.Object;
				if (EditorGUI.EndChangeCheck() && hasProperty)
				{
					UnityEngine.Object capturedValue = value;
					property.Tree.DelayActionUntilRepaint(delegate
					{
						property.ValueEntry.WeakSmartValue = capturedValue;
					});
					GUIHelper.RequestRepaint();
				}
			}
			else
			{
				DragAndDropUtilities.DragZone(dragDropRect, value, objectType, allowMove: false, allowSwap: false);
			}
			int dragId = DragAndDropUtilities.PrevDragAndDropId;
			bool isDragging = DragAndDropUtilities.IsDragging && DragAndDropUtilities.CurrentDropId == dragId;
			SirenixEditorGUI.BeginDrawOpenInspector(penRect, value, position);
			bool isHover = Event.current.IsMouseOver(position);
			bool hasKeyboardFocus = GUIUtility.keyboardControl == id;
			if (Event.current.type == EventType.MouseDown && isHover)
			{
				GUIUtility.keyboardControl = id;
			}
			if (!readOnly)
			{
				EditorGUI.BeginChangeCheck();
				value = OdinObjectSelector.GetChangedObject(value, selectorKey, selectorId);
				if (EditorGUI.EndChangeCheck())
				{
					if (hasProperty)
					{
						UnityEngine.Object capturedValue2 = value;
						property.Tree.DelayActionUntilRepaint(delegate
						{
							property.ValueEntry.WeakSmartValue = capturedValue2;
						});
					}
					GUIHelper.RequestRepaint();
				}
			}
			switch (Event.current.type)
			{
			case EventType.Repaint:
			{
				bool lastGUIEnabled = GUI.enabled;
				if (disableGui)
				{
					GUI.enabled = false;
				}
				Vector2 previousIconSize = EditorGUIUtility.GetIconSize();
				EditorGUIUtility.SetIconSize(new Vector2(12f, 12f));
				EditorStyles.objectField.Draw(position, GUIContent.none, isHover, isActive: false, isDragging, hasKeyboardFocus);
				Rect contentRect = position;
				contentRect.width -= fieldButtonPosition.width;
				contentRect.width -= 20f;
				contentRect = contentRect.Padding(0f, 1.5f);
				contentRect.position += new Vector2(0.5f, 0.5f);
				Texture unityIcon = GetUnityIcon(value, objectType);
				string unityLabel = GetUnityLabel(value, objectType, useNiceName: false);
				if (unityIcon == null)
				{
					contentRect.position += new Vector2(2f, 0f);
				}
				float labelWidth = SirenixGUIStyles.Label.CalcWidth(unityLabel);
				contentRect = contentRect.AddXMin(2f);
				if (labelWidth > contentRect.width)
				{
					int prevLeft = SirenixGUIStyles.Label.padding.left;
					int prevRight = SirenixGUIStyles.Label.padding.right;
					SirenixGUIStyles.Label.padding.left = 2;
					SirenixGUIStyles.Label.padding.right = 0;
					GUI.Label(contentRect.TakeFromRight(12f), GUIHelper.TempContent("...", unityLabel), SirenixGUIStyles.Label);
					SirenixGUIStyles.Label.padding.left = prevLeft;
					GUI.Label(contentRect, GUIHelper.TempContent(unityLabel, unityIcon), SirenixGUIStyles.Label);
					SirenixGUIStyles.Label.padding.right = prevRight;
				}
				else
				{
					GUI.Label(contentRect, GUIHelper.TempContent(unityLabel, unityIcon), SirenixGUIStyles.Label);
				}
				EditorStyles_Internal.ObjectFieldButton.Draw(fieldButtonPosition.Padding(-1f, 1f, 1f, 1f), Event.current.IsMouseOver(fieldButtonPosition), isFieldButtonHover, isFieldButtonActive, hasKeyboardFocus: false);
				if (disableGui)
				{
					GUI.enabled = lastGUIEnabled;
				}
				EditorGUIUtility.SetIconSize(previousIconSize);
				break;
			}
			case EventType.KeyDown:
				if (!hasKeyboardFocus || readOnly)
				{
					break;
				}
				switch (Event.current.keyCode)
				{
				case KeyCode.Backspace:
					if (readOnly)
					{
						break;
					}
					value = null;
					if (hasProperty)
					{
						property.Tree.DelayActionUntilRepaint(delegate
						{
							property.ValueEntry.WeakSmartValue = null;
						});
						GUIHelper.RequestRepaint();
					}
					GUI.changed = true;
					Event.current.Use();
					break;
				case KeyCode.Delete:
					if (readOnly || (UnityShims.Misc.GetEventModifiers(Event.current) & 1) != 0)
					{
						break;
					}
					value = null;
					if (hasProperty)
					{
						property.Tree.DelayActionUntilRepaint(delegate
						{
							property.ValueEntry.WeakSmartValue = null;
						});
						GUIHelper.RequestRepaint();
					}
					GUI.changed = true;
					Event.current.Use();
					break;
				case KeyCode.V:
					if (!readOnly && UnityShims.Misc.GetEventModifiers(Event.current) == 2)
					{
						value = HandlePasteEvent(value, objectType, property) as UnityEngine.Object;
					}
					break;
				}
				break;
			}
			switch (Event.current.rawType)
			{
			case EventType.MouseDown:
			{
				Rect area = position;
				area.width -= fieldButtonPosition.width;
				bool isMouseOver = Event.current.IsMouseOver(area);
				if (Event.current.button == 0 && isMouseOver && value != null)
				{
					switch (Event.current.clickCount)
					{
					case 1:
						GUIUtility.keyboardControl = id;
						EditorGUIUtility.PingObject(value);
						Event.current.Use();
						break;
					case 2:
						GUIUtility.keyboardControl = id;
						EditorGUIUtility.PingObject(value);
						AssetDatabase.OpenAsset(value);
						GUIHelper.ExitGUI(removeFocusControl: false);
						Event.current.Use();
						break;
					}
				}
				break;
			}
			case EventType.KeyDown:
				if (hasKeyboardFocus)
				{
					KeyCode keyCode = Event.current.keyCode;
					if (keyCode == KeyCode.C && UnityShims.Misc.GetEventModifiers(Event.current) == 2)
					{
						HandleCopyEvent(value);
					}
				}
				break;
			}
			SirenixEditorGUI.EndDrawOpenInspector(penRect, value);
			if (!readOnly && OdinObjectSelector.IsReadyToClaim(selectorKey, selectorId))
			{
				value = ((!hasProperty) ? ((UnityEngine.Object)OdinObjectSelector.Claim()) : ((UnityEngine.Object)OdinObjectSelector.ClaimAndAssign(property)));
				GUI.changed = true;
			}
			if (!readOnly && wasFakeNull && (object)value == null)
			{
				value = originalValue;
				if (hasProperty)
				{
					UnityEngine.Object capturedValue3 = value;
					property.Tree.DelayActionUntilRepaint(delegate
					{
						property.ValueEntry.WeakSmartValue = capturedValue3;
					});
					GUIHelper.RequestRepaint();
				}
			}
			return value;
		}

		public static object PolymorphicObjectField(in PolymorphicFieldArgs args)
		{
			Rect position = args.Rect;
			int id = args.Id;
			bool hasKeyboardFocus = args.HasKeyboardFocus;
			object value = args.Value;
			Type valueType = args.ValueType;
			Type baseType = args.BaseType;
			bool allowSceneObjects = args.AllowSceneObjects;
			bool disallowNullValues = args.DisallowNullValues;
			bool readOnly = args.ReadOnly;
			bool showBaseType = args.ShowBaseType;
			string title = args.Title;
			if (GlobalConfig<GeneralDrawerConfig>.Instance.useOldPolymorphicField)
			{
				if (title != null)
				{
					return OldPolymorphicObjectField(position, value, baseType, title, allowSceneObjects, hasKeyboardFocus, id);
				}
				return OldPolymorphicObjectField(position, value, baseType, allowSceneObjects, hasKeyboardFocus, id);
			}
			GUIContent label = args.Label;
			InspectorProperty property = args.Property;
			object selectorKey = TryGetSelectorKeyFromProperty(args.Property);
			int selectorId = ((selectorKey == null) ? args.Id : (-2147475450));
			position = ((label == null) ? EditorGUI.IndentedRect(position) : EditorGUI.PrefixLabel(position, id, label));
			bool hasProperty = property != null;
			object originalValue = value;
			UnityEngine.Object valueUnity = value as UnityEngine.Object;
			bool wasFakeNull = (bool)valueUnity && valueUnity == null && (object)valueUnity != null;
			float buttonWidth = position.height;
			Rect dragDropRect = position;
			if (valueUnity != null)
			{
				dragDropRect.width -= buttonWidth;
			}
			dragDropRect.width -= buttonWidth;
			dragDropRect.width -= 2f;
			bool isDragging;
			if (!readOnly)
			{
				EditorGUI.BeginChangeCheck();
				value = DragAndDropUtilities.DragAndDropZone(dragDropRect, value, baseType, allowMove: true, allowSwap: true, allowSceneObjects);
				if (EditorGUI.EndChangeCheck() && hasProperty)
				{
					object capturedValue = value;
					property.Tree.DelayActionUntilRepaint(delegate
					{
						property.ValueEntry.WeakSmartValue = capturedValue;
					});
					GUIHelper.RequestRepaint();
				}
				int dragId = DragAndDropUtilities.PrevDragAndDropId;
				isDragging = DragAndDropUtilities.IsDragging && DragAndDropUtilities.CurrentDropId == dragId;
				EditorGUI.BeginChangeCheck();
				value = OdinObjectSelector.GetChangedObject(value, selectorKey, selectorId);
				if (EditorGUI.EndChangeCheck())
				{
					if (hasProperty)
					{
						object capturedValue2 = value;
						property.Tree.DelayActionUntilRepaint(delegate
						{
							property.ValueEntry.WeakSmartValue = capturedValue2;
						});
					}
					GUIHelper.RequestRepaint();
				}
			}
			else
			{
				isDragging = false;
			}
			bool isNonUnityBase = !typeof(UnityEngine.Object).IsAssignableFrom(baseType);
			bool isHover = Event.current.IsMouseOver(position);
			if (Event.current.type == EventType.MouseDown && isHover)
			{
				GUIUtility.keyboardControl = id;
			}
			object visualValue = value;
			if (OdinObjectSelector.IsCurrentSelector(selectorKey, selectorId))
			{
				visualValue = OdinObjectSelector.SelectorObject;
			}
			bool isIllegal = value != null && GlobalConfig<TypeRegistryUserConfig>.Instance.IsIllegal(valueType);
			if (isIllegal)
			{
				GUIHelper.PushColor(Color.yellow);
			}
			if (typeof(UnityEngine.Object).IsAssignableFrom((visualValue == null) ? baseType : visualValue.GetType()))
			{
				Rect penRect = position;
				penRect.x += penRect.width - 38f;
				if (isNonUnityBase)
				{
					penRect.x -= position.height;
				}
				penRect.width = 20f;
				SirenixEditorGUI.BeginDrawOpenInspector(penRect, valueUnity, SirenixEditorGUI.IndentLabelRect(position, label != null));
				bool isDropdownHover = false;
				Rect dropdownButtonPosition;
				if (isNonUnityBase)
				{
					dropdownButtonPosition = position.AlignRight(position.height);
					if (SirenixEditorGUI.DoButton(dropdownButtonPosition, out isDropdownHover) && !readOnly)
					{
						OdinObjectSelector.Show(position, selectorKey, selectorId, value, valueType, baseType, allowSceneObjects, disallowNullValues, property);
					}
				}
				else
				{
					dropdownButtonPosition = Rect.zero;
				}
				Rect fieldButtonPosition = position.AlignRight(position.height).SubX(dropdownButtonPosition.width);
				if (SirenixEditorGUI.DoButton(fieldButtonPosition, id, out var isButtonHover, out var isButtonActive) && !readOnly)
				{
					OdinObjectSelector.Show(position, selectorKey, selectorId, value, valueType, baseType, allowSceneObjects, disallowNullValues, property, useUnitySelector: true);
				}
				EventType type = Event.current.type;
				if (type == EventType.Repaint)
				{
					UnityEngine.Object visualValueUnity = (UnityEngine.Object)visualValue;
					bool lastGUIEnabled = GUI.enabled;
					if (readOnly)
					{
						GUI.enabled = false;
					}
					EditorStyles.objectField.Draw(position, GUIContent.none, isHover, isActive: false, isDragging, hasKeyboardFocus);
					Rect contentRect = position;
					contentRect.width -= dropdownButtonPosition.width;
					contentRect.width -= fieldButtonPosition.width;
					contentRect = contentRect.Padding(0f, 1.5f);
					contentRect.position += new Vector2(0.5f, 0.5f);
					Type iconType = ((value == null) ? baseType : valueType);
					Rect labelPosition;
					SdfIconType icon;
					Color? iconColor;
					if (isIllegal)
					{
						GetContentPositions(contentRect, out var imagePosition, out labelPosition);
						imagePosition = imagePosition.Padding(2f).AddX(1f);
						SdfIcons.DrawIcon(imagePosition, SdfIconType.ExclamationTriangleFill, Color.yellow);
					}
					else if (TypeRegistry.TryGetIcon(iconType, out icon, out iconColor))
					{
						GetContentPositions(contentRect, out var imagePosition2, out labelPosition);
						imagePosition2 = imagePosition2.Padding(2f).AddX(1f);
						if (iconColor.HasValue)
						{
							SdfIcons.DrawIcon(imagePosition2, icon, iconColor.Value);
						}
						else
						{
							SdfIcons.DrawIcon(imagePosition2, icon);
						}
					}
					else if (visualValueUnity == null)
					{
						GetContentPositions(contentRect, out labelPosition);
					}
					else
					{
						Texture image = GetUnityIcon(visualValueUnity, valueType);
						if (image != null)
						{
							GetContentPositions(contentRect, out var imagePosition3, out labelPosition);
							GUI.DrawTexture(imagePosition3, image, ScaleMode.ScaleToFit);
						}
						else
						{
							GetContentPositions(contentRect, out labelPosition);
						}
					}
					GUI.Label(labelPosition, GetUnityLabel(visualValueUnity, baseType, useNiceName: true));
					if (isNonUnityBase)
					{
						Rect dropdownRect = dropdownButtonPosition.AlignCenter(Math.Min(10f, dropdownButtonPosition.width), Math.Min(10f, dropdownButtonPosition.height));
						if (isDropdownHover)
						{
							SdfIcons.DrawIcon(dropdownRect, SdfIconType.CaretDownFill);
						}
						else if (EditorGUIUtility.isProSkin)
						{
							SdfIcons.DrawIcon(dropdownRect, SdfIconType.CaretDownFill, new Color(1f, 1f, 1f, 0.5f));
						}
						else
						{
							SdfIcons.DrawIcon(dropdownRect, SdfIconType.CaretDownFill, new Color(0f, 0f, 0f, 0.5f));
						}
					}
					EditorStyles_Internal.ObjectFieldButton.Draw(fieldButtonPosition.Padding(-1f, 1f, 1f, 1f), Event.current.IsMouseOver(fieldButtonPosition), isButtonHover, isButtonActive, hasKeyboardFocus: false);
					if (readOnly)
					{
						GUI.enabled = lastGUIEnabled;
					}
				}
				if (Event.current.rawType == EventType.MouseDown)
				{
					Rect area = position;
					area.width -= dropdownButtonPosition.width;
					area.width -= fieldButtonPosition.width;
					bool isMouseOver = Event.current.IsMouseOver(area);
					if (Event.current.button == 0 && isMouseOver && valueUnity != null)
					{
						switch (Event.current.clickCount)
						{
						case 1:
							GUIUtility.keyboardControl = id;
							EditorGUIUtility.PingObject(valueUnity);
							Event.current.Use();
							break;
						case 2:
							GUIUtility.keyboardControl = id;
							EditorGUIUtility.PingObject(valueUnity);
							AssetDatabase.OpenAsset(valueUnity);
							GUIHelper.ExitGUI(removeFocusControl: false);
							Event.current.Use();
							break;
						}
					}
				}
				SirenixEditorGUI.EndDrawOpenInspector(penRect, valueUnity);
			}
			else
			{
				GUIUtility.GetControlID(GUI_Internals.ButtonHash, FocusType.Passive, position);
				Rect dropdownButtonPosition2 = position.AlignRight(position.height);
				if (SirenixEditorGUI.DoButton(dropdownButtonPosition2, out var isDropdownHover2) && !readOnly)
				{
					OdinObjectSelector.Show(position, selectorKey, selectorId, value, valueType, baseType, allowSceneObjects, disallowNullValues, property);
				}
				Rect focusPosition = position;
				focusPosition.width -= dropdownButtonPosition2.width;
				if (SirenixEditorGUI.DoButton(focusPosition, id, out isHover, out var _) && !readOnly)
				{
					GUIUtility.keyboardControl = id;
				}
				EventType type2 = Event.current.type;
				if (type2 == EventType.Repaint)
				{
					bool lastGUIEnabled2 = GUI.enabled;
					if (readOnly)
					{
						GUI.enabled = false;
					}
					string baseTypeName = (showBaseType ? (" (" + TypeRegistry.GetNiceName(baseType) + ")") : string.Empty);
					string valueLabelText = (EditorGUI.showMixedValue ? ("— Conflict" + baseTypeName) : ((title == null) ? ((visualValue == null) ? (" None" + baseTypeName) : (TypeRegistry.GetNiceName(valueType) + baseTypeName)) : (" " + title)));
					EditorStyles.objectField.Draw(position, GUIContent.none, isHover, isActive: false, isDragging, hasKeyboardFocus);
					Rect dropdownRect2 = dropdownButtonPosition2.AlignCenter(Math.Min(10f, dropdownButtonPosition2.width), Math.Min(10f, dropdownButtonPosition2.height));
					if (isDropdownHover2)
					{
						SdfIcons.DrawIcon(dropdownRect2, SdfIconType.CaretDownFill);
					}
					else if (EditorGUIUtility.isProSkin)
					{
						SdfIcons.DrawIcon(dropdownRect2, SdfIconType.CaretDownFill, new Color(1f, 1f, 1f, 0.5f));
					}
					else
					{
						SdfIcons.DrawIcon(dropdownRect2, SdfIconType.CaretDownFill, new Color(0f, 0f, 0f, 0.5f));
					}
					position.width -= dropdownButtonPosition2.width;
					Type iconType2 = ((value == null) ? baseType : valueType);
					Rect labelPosition2;
					SdfIconType icon2;
					Color? iconColor2;
					if (isIllegal)
					{
						GetContentPositions(position, out var iconPosition, out labelPosition2);
						iconPosition = iconPosition.Padding(2f).AddX(1f);
						SdfIcons.DrawIcon(iconPosition, SdfIconType.ExclamationTriangleFill, Color.yellow);
					}
					else if (TypeRegistry.TryGetIcon(iconType2, out icon2, out iconColor2))
					{
						GetContentPositions(position, out var iconPosition2, out labelPosition2);
						iconPosition2 = iconPosition2.Padding(2f).AddX(1f);
						if (iconColor2.HasValue)
						{
							SdfIcons.DrawIcon(iconPosition2, icon2, iconColor2.Value);
						}
						else
						{
							SdfIcons.DrawIcon(iconPosition2, icon2);
						}
					}
					else if (value == null)
					{
						GetContentPositions(position, out labelPosition2);
					}
					else
					{
						GetContentPositions(position, out var iconPosition3, out labelPosition2);
						iconPosition3 = iconPosition3.Padding(2f).AddX(1f);
						SdfIcons.DrawIcon(iconPosition3, SdfIconType.PuzzleFill);
					}
					GUI.Label(labelPosition2, valueLabelText);
					if (readOnly)
					{
						GUI.enabled = lastGUIEnabled2;
					}
				}
			}
			if (hasKeyboardFocus)
			{
				if (Event.current.type == EventType.KeyDown)
				{
					switch (Event.current.keyCode)
					{
					case KeyCode.Backspace:
						if (readOnly)
						{
							break;
						}
						value = null;
						if (hasProperty)
						{
							property.Tree.DelayActionUntilRepaint(delegate
							{
								property.ValueEntry.WeakSmartValue = null;
							});
							GUIHelper.RequestRepaint();
						}
						GUI.changed = true;
						Event.current.Use();
						break;
					case KeyCode.Delete:
						if (readOnly || (UnityShims.Misc.GetEventModifiers(Event.current) & 1) != 0)
						{
							break;
						}
						if (hasProperty)
						{
							property.Tree.DelayActionUntilRepaint(delegate
							{
								property.ValueEntry.WeakSmartValue = null;
							});
							GUIHelper.RequestRepaint();
						}
						value = null;
						GUI.changed = true;
						Event.current.Use();
						break;
					case KeyCode.V:
						if (!readOnly && UnityShims.Misc.GetEventModifiers(Event.current) == 2)
						{
							value = HandlePasteEvent(value, baseType, property);
						}
						break;
					}
				}
				if (Event.current.rawType == EventType.KeyDown)
				{
					KeyCode keyCode = Event.current.keyCode;
					if (keyCode == KeyCode.C && UnityShims.Misc.GetEventModifiers(Event.current) == 2)
					{
						HandleCopyEvent(value);
					}
				}
			}
			if (!readOnly)
			{
				if (OdinObjectSelector.IsReadyToClaim(selectorKey, selectorId))
				{
					value = ((!hasProperty) ? OdinObjectSelector.Claim() : OdinObjectSelector.ClaimAndAssign(property));
					GUI.changed = true;
				}
				if (wasFakeNull && (object)valueUnity == null)
				{
					value = originalValue;
				}
			}
			if (isIllegal)
			{
				GUIHelper.PopColor();
			}
			return value;
		}

		public static object UnityPreviewObjectField(in UnityPreviewFieldArgs args)
		{
			InspectorProperty property = args.Property;
			int id = args.Id;
			Rect rect = args.Rect;
			GUIContent label = args.Label;
			UnityEngine.Object value = args.Value;
			Type type = args.Type;
			Sirenix.Utilities.Editor.ObjectFieldAlignment alignment = args.Alignment;
			bool dragOnly = args.DragOnly;
			bool allowMove = args.AllowMove;
			bool allowSwap = args.AllowSwap;
			bool allowSceneObjects = args.AllowSceneObjects;
			object selectorKey = TryGetSelectorKeyFromProperty(args.Property);
			int selectorId = ((selectorKey == null) ? id : (-2147475450));
			UnityEngine.Object originalValue = value;
			rect = ((label == null) ? EditorGUI.IndentedRect(rect) : EditorGUI.PrefixLabel(rect, id, label));
			Rect popupPosition = rect;
			rect = alignment switch
			{
				Sirenix.Utilities.Editor.ObjectFieldAlignment.Left => rect.AlignLeft(rect.height), 
				Sirenix.Utilities.Editor.ObjectFieldAlignment.Center => rect.AlignCenter(rect.height), 
				_ => rect.AlignRight(rect.height), 
			};
			if (args.Preview != null)
			{
				DragAndDropUtilities.DrawDropZone(rect, args.Preview, null, id);
			}
			else
			{
				DragAndDropUtilities.DrawDropZone(rect, args.Value, null, id);
			}
			if (!dragOnly)
			{
				value = DragAndDropUtilities.DropZone(rect, value, type, id) as UnityEngine.Object;
				value = ((!GlobalConfig<GeneralDrawerConfig>.Instance.useOldUnityPreviewField) ? (OdinInternalDragAndDropUtils.ObjectSelectorZone(rect, popupPosition, value, type, allowSceneObjects, id, property, selectorKey, selectorId) as UnityEngine.Object) : (OdinInternalDragAndDropUtils.ObjectPickerZone(rect, value, type, allowSceneObjects, id) as UnityEngine.Object));
				if (GUIUtility.keyboardControl == id)
				{
					if (Event.current.type == EventType.KeyDown)
					{
						KeyCode keyCode = Event.current.keyCode;
						if (keyCode == KeyCode.V && UnityShims.Misc.GetEventModifiers(Event.current) == 2)
						{
							value = HandlePasteEvent(value, type, property) as UnityEngine.Object;
						}
					}
					if (Event.current.rawType == EventType.KeyDown)
					{
						KeyCode keyCode2 = Event.current.keyCode;
						if (keyCode2 == KeyCode.C && UnityShims.Misc.GetEventModifiers(Event.current) == 2)
						{
							HandleCopyEvent(value);
						}
					}
				}
			}
			value = DragAndDropUtilities.DragZone(rect, value, type, allowMove, allowSwap, id) as UnityEngine.Object;
			if (property != null && originalValue != value)
			{
				UnityEngine.Object capturedValue = value;
				property.Tree.DelayActionUntilRepaint(delegate
				{
					property.ValueEntry.WeakSmartValue = capturedValue;
					GUIHelper.RequestRepaint();
				});
				GUIHelper.RequestRepaint();
			}
			if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
			{
				GUIUtility.keyboardControl = id;
				GUIUtility.hotControl = id;
			}
			return value;
		}

		public static UnityEngine.Object UnityObjectFieldWrapper(Rect position, GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects, bool readOnly = false)
		{
			return UnityObjectField(UnityObjectFieldArgs.CreateForWrapper(position, label, value, objectType, allowSceneObjects, readOnly));
		}

		public static object PolymorphicObjectFieldWrapper(Rect position, object value, Type baseType, bool allowSceneObjects, bool hasKeyboardFocus, int id, bool disallowNullValues = false, bool readOnly = false, bool showBaseType = true, string title = null)
		{
			return PolymorphicObjectField(PolymorphicFieldArgs.CreateForWrapper(position, value, baseType, allowSceneObjects, hasKeyboardFocus, id, disallowNullValues, readOnly, showBaseType, title));
		}

		private static Texture GetUnityIcon(UnityEngine.Object obj, Type type)
		{
			return EditorGUIUtility.ObjectContent(obj, type).image;
		}

		private static string GetUnityLabel(UnityEngine.Object obj, Type type, bool useNiceName)
		{
			bool isNull = obj == null;
			string typeName = (useNiceName ? (" (" + type.GetNiceName() + ")") : (" (" + ObjectNames.NicifyVariableName(type.Name) + ")"));
			if (EditorGUI.showMixedValue)
			{
				return "— Conflict" + typeName;
			}
			if (isNull)
			{
				if ((object)obj != null && OdinEntityId.FromObject(obj).IsValid)
				{
					return "Missing" + typeName;
				}
				return "None" + typeName;
			}
			GUIContent content = EditorGUIUtility.ObjectContent(obj, type);
			return content.text;
		}

		private static void GetContentPositions(Rect position, out Rect textPosition)
		{
			textPosition = position;
		}

		private static void GetContentPositions(Rect position, out Rect iconPosition, out Rect textPosition)
		{
			iconPosition = position.TakeFromLeft(position.height - 2.5f).AddX(1f);
			textPosition = position;
		}

		private static void HandleCopyEvent(object value)
		{
			UnityEngine.Object unityObj = value as UnityEngine.Object;
			if (value == null)
			{
				return;
			}
			if ((bool)unityObj)
			{
				if (unityObj == null)
				{
					return;
				}
				Clipboard.Copy(unityObj, CopyModes.CopyReference);
			}
			else
			{
				Clipboard.Copy(value, CopyModes.DeepCopy);
			}
			Event.current.Use();
		}

		private static object HandlePasteEvent(object value, Type baseType, InspectorProperty property)
		{
			if (!Clipboard.CanPaste(baseType))
			{
				return value;
			}
			if (property != null)
			{
				if (!property.ValueEntry.IsEditable)
				{
					return value;
				}
				int valueCount = property.ValueEntry.ValueCount;
				if (valueCount <= 0)
				{
					return value;
				}
				value = Clipboard.Paste();
				object capturedValue = value;
				property.Tree.DelayActionUntilRepaint(delegate
				{
					property.ValueEntry.WeakValues[0] = capturedValue;
					for (int i = 1; i < property.ValueEntry.ValueCount; i++)
					{
						property.ValueEntry.WeakValues[i] = Clipboard.Paste();
					}
					GUIHelper.RequestRepaint();
				});
				GUIHelper.RequestRepaint();
			}
			else
			{
				value = Clipboard.Paste();
			}
			Event.current.Use();
			return value;
		}

		public static object OldPolymorphicObjectField(Rect rect, object value, Type type, bool allowSceneObjects, bool hasKeyboardFocus, int id)
		{
			EventType e = Event.current.type;
			int dropId = DragAndDropUtilities.GetDragAndDropId(rect);
			Rect penRect = rect;
			UnityEngine.Object uObj = value as UnityEngine.Object;
			if ((bool)uObj)
			{
				penRect.x += penRect.width - 38f;
				penRect.width = 20f;
				SirenixEditorGUI.BeginDrawOpenInspector(penRect, uObj, rect);
			}
			if (e == EventType.Repaint)
			{
				GUIContent title;
				if (EditorGUI.showMixedValue)
				{
					title = new GUIContent("   — Conflict (" + type.GetNiceName() + ")");
				}
				else if (value == null)
				{
					title = new GUIContent("   Null (" + type.GetNiceName() + ")");
				}
				else if ((bool)uObj)
				{
					string baseType = ((value.GetType() == type) ? "" : (" : " + type.GetNiceName()));
					title = new GUIContent("   " + uObj.name + " (" + value.GetType().GetNiceName() + baseType + ")");
				}
				else
				{
					string baseType2 = ((value.GetType() == type) ? "" : (" : " + type.GetNiceName()));
					title = new GUIContent("   " + value.GetType().GetNiceName() + baseType2);
				}
				EditorStyles.objectField.Draw(rect, title, id, DragAndDropUtilities.HoveringAcceptedDropZone == dropId);
				if ((bool)uObj)
				{
					Texture2D thumbnail = GUIHelper.GetAssetThumbnail(uObj, value.GetType(), preferObjectPreviewOverFileIcon: true);
					if (thumbnail != null)
					{
						GUI.DrawTexture(rect.AlignLeft(rect.height * 0.75f).SetHeight(rect.height * 0.75f).AddX(3f)
							.AddY(1.5f), thumbnail);
					}
				}
				else if (UnityVersion.IsVersionOrGreater(2019, 3))
				{
					EditorIcons.StarPointer.Draw(rect.AlignLeft(rect.height - 3f).AlignCenterY(rect.height - 3f).AddY(1f));
				}
				else
				{
					EditorIcons.StarPointer.Draw(rect.AlignLeft(rect.height));
				}
			}
			if ((bool)uObj)
			{
				SirenixEditorGUI.EndDrawOpenInspector(penRect, uObj);
			}
			ObjectPicker objectPicker = ObjectPicker.GetObjectPicker(type.FullName + "+" + GUIUtility.GetControlID(FocusType.Passive), type);
			value = DragAndDropUtilities.DropZone(rect, value, type, allowSceneObjects: true, dropId);
			if ((Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition)) || (hasKeyboardFocus && Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown))
			{
				if (!rect.AlignRight(16f).Contains(Event.current.mousePosition) && (bool)uObj)
				{
					if (Event.current.clickCount == 1)
					{
						EditorGUIUtility.PingObject(uObj);
					}
					else if (Event.current.clickCount == 2)
					{
						AssetDatabase.OpenAsset(uObj);
					}
				}
				else
				{
					objectPicker.ShowObjectPicker(value, allowSceneObjects, rect);
				}
				Event.current.Use();
			}
			if (objectPicker.IsReadyToClaim && Event.current.type == EventType.Repaint)
			{
				GUI.changed = true;
				return objectPicker.ClaimObject();
			}
			return value;
		}

		public static object OldPolymorphicObjectField(Rect rect, object value, Type type, string title, bool allowSceneObjects, bool hasKeyboardFocus, int id)
		{
			EventType e = Event.current.type;
			int dropId = DragAndDropUtilities.GetDragAndDropId(rect);
			Rect penRect = rect;
			UnityEngine.Object uObj = value as UnityEngine.Object;
			if ((bool)uObj)
			{
				penRect.x += penRect.width - 38f;
				penRect.width = 20f;
				SirenixEditorGUI.BeginDrawOpenInspector(penRect, uObj, rect);
			}
			if (e == EventType.Repaint)
			{
				EditorStyles.objectField.Draw(rect, GUIHelper.TempContent("   " + title), id, DragAndDropUtilities.HoveringAcceptedDropZone == dropId);
				if ((bool)uObj)
				{
					Texture2D thumbnail = GUIHelper.GetAssetThumbnail(uObj, value.GetType(), preferObjectPreviewOverFileIcon: true);
					if (thumbnail != null)
					{
						GUI.DrawTexture(rect.AlignLeft(rect.height * 0.75f).SetHeight(rect.height * 0.75f).AddX(3f)
							.AddY(1.5f), thumbnail);
					}
				}
				else if (UnityVersion.IsVersionOrGreater(2019, 3))
				{
					EditorIcons.StarPointer.Draw(rect.AlignLeft(rect.height - 3f).AlignCenterY(rect.height - 3f).AddY(1f));
				}
				else
				{
					EditorIcons.StarPointer.Draw(rect.AlignLeft(rect.height));
				}
			}
			if ((bool)uObj)
			{
				SirenixEditorGUI.EndDrawOpenInspector(penRect, uObj);
			}
			ObjectPicker objectPicker = ObjectPicker.GetObjectPicker(type.FullName + "+" + GUIUtility.GetControlID(FocusType.Passive), type);
			value = DragAndDropUtilities.DropZone(rect, value, type, allowSceneObjects: true, dropId);
			if ((Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition)) || (hasKeyboardFocus && Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown))
			{
				if (!rect.AlignRight(16f).Contains(Event.current.mousePosition) && (bool)uObj)
				{
					if (Event.current.clickCount == 1)
					{
						EditorGUIUtility.PingObject(uObj);
					}
					else if (Event.current.clickCount == 2)
					{
						AssetDatabase.OpenAsset(uObj);
					}
				}
				else
				{
					objectPicker.ShowObjectPicker(value, allowSceneObjects, rect);
				}
				Event.current.Use();
			}
			if (objectPicker.IsReadyToClaim && Event.current.type == EventType.Repaint)
			{
				GUI.changed = true;
				return objectPicker.ClaimObject();
			}
			return value;
		}
	}
}

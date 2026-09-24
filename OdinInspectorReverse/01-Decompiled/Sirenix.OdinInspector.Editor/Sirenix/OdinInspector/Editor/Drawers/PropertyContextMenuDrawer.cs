using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public static class PropertyContextMenuDrawer
	{
		public static class GenericMenuUtility
		{
			private static FieldInfo GenericMenu_menuItems_Field;

			public static readonly bool Available;

			static GenericMenuUtility()
			{
				GenericMenu_menuItems_Field = typeof(GenericMenu).GetField("menuItems", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}

			public static ArrayList GetMenuItems(GenericMenu genericMenu)
			{
				throw new NotImplementedException();
			}
		}

		private class IndexPopupWindow
		{
			[HideInInspector]
			public string IntLabel;

			[HideInInspector]
			public int MaxCount;

			[LabelText("$IntLabel")]
			[MinValue(0.0)]
			[MaxValue("$MaxCount")]
			public int Value;

			[HideInInspector]
			public Action MoveAction;

			[HideInInspector]
			public Action CloseWindowAction;

			[Button]
			[HorizontalGroup("Buttons", 0f, 0, 0, 0f)]
			public void Move()
			{
				MoveAction();
				CloseWindowAction();
			}

			[Button]
			[HorizontalGroup("Buttons", 0f, 0, 0, 0f)]
			public void Cancel()
			{
				CloseWindowAction();
			}

			[PropertyOrder(-1f)]
			[OnInspectorGUI]
			private void DetectEnter()
			{
				if (Event.current.OnKeyUp(KeyCode.Return))
				{
					Move();
					GUIHelper.ExitGUI(removeFocusControl: true);
				}
			}
		}

		private static MethodInfo EditorGUI_FillPropertyContextMenu = typeof(EditorGUI).GetMethod("FillPropertyContextMenu", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[3]
		{
			typeof(SerializedProperty),
			typeof(SerializedProperty),
			typeof(GenericMenu)
		}, null);

		/// <summary>
		/// Adds the right click area.
		/// </summary>
		public static void AddRightClickArea(InspectorProperty property, Rect rect)
		{
			int id = GUIUtility.GetControlID(FocusType.Passive);
			AddRightClickArea(property, rect, id);
		}

		/// <summary>
		/// Adds the right click area.
		/// </summary>
		public static void AddRightClickArea(InspectorProperty property, Rect rect, int id)
		{
			if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && rect.Contains(Event.current.mousePosition))
			{
				GUIUtility.hotControl = id;
				Event.current.Use();
				GUIHelper.RequestRepaint();
			}
			if (Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition) && id == GUIUtility.hotControl)
			{
				GUIHelper.RemoveFocusControl();
				Event.current.Use();
				GenericMenu menu = new GenericMenu();
				GUIHelper.RemoveFocusControl();
				PopulateGenericMenu(property, menu);
				property.PopulateGenericMenu(menu);
				if (menu.GetItemCount() == 0)
				{
					menu = null;
				}
				else
				{
					menu.ShowAsContext();
				}
			}
			if (GUIUtility.hotControl == id && Event.current.type == EventType.Repaint)
			{
				rect.width = 3f;
				rect.x -= 4f;
				SirenixEditorGUI.DrawSolidRect(rect, SirenixGUIStyles.HighlightedTextColor);
			}
		}

		public static GenericMenu FillUnityContextMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			SerializedProperty unityProperty = property.Tree.GetUnityPropertyForPath(property.UnityPropertyPath);
			if (unityProperty == null)
			{
				return genericMenu ?? new GenericMenu();
			}
			return FillUnityContextMenu(unityProperty, genericMenu);
		}

		public static GenericMenu FillUnityContextMenu(SerializedProperty property, GenericMenu genericMenu = null)
		{
			if (EditorGUI_FillPropertyContextMenu != null)
			{
				return (GenericMenu)EditorGUI_FillPropertyContextMenu.Invoke(null, new object[3] { property, null, genericMenu });
			}
			object handler = UnityPropertyHandlerUtility.ScriptAttributeUtility_GetHandler(property);
			if (handler != null)
			{
				genericMenu = genericMenu ?? new GenericMenu();
				UnityPropertyHandlerUtility.PropertyHandler_AddMenuItems(handler, property, genericMenu);
			}
			return genericMenu ?? new GenericMenu();
		}

		private static void PopulateChangedFromPrefabContext(InspectorProperty property, GenericMenu genericMenu)
		{
			if (!property.Tree.PrefabModificationHandler.HasPrefabs)
			{
				return;
			}
			IPropertyValueEntry entry = property.ValueEntry;
			if (entry == null)
			{
				return;
			}
			InspectorProperty prefabProperty = null;
			if (property.Tree.PrefabModificationHandler.PrefabPropertyTree != null)
			{
				prefabProperty = property.Tree.PrefabModificationHandler.PrefabPropertyTree.GetPropertyAtPath(property.Path);
			}
			bool active = prefabProperty != null;
			int moddedChildren = property.Children.Recurse().Count((InspectorProperty c) => c.ValueEntry != null && c.ValueEntry.ValueChangedFromPrefab);
			bool showApplyToPrefab = false;
			if (entry.ValueChangedFromPrefab || moddedChildren > 0)
			{
				if (active)
				{
					genericMenu.AddItem(new GUIContent("Revert to prefab value" + ((moddedChildren > 0) ? (" (" + moddedChildren + " child modifications to revert)") : "")), on: false, delegate
					{
						property.RecordForUndo("Revert to prefab value");
						for (int i = 0; i < entry.ValueCount; i++)
						{
							property.Tree.PrefabModificationHandler.RemovePrefabModification(property, i, PrefabModificationType.Value);
						}
						if (property.Tree.UnitySerializedObject != null)
						{
							property.Tree.UnitySerializedObject.Update();
						}
					});
					showApplyToPrefab = true;
				}
				else
				{
					genericMenu.AddDisabledItem(new GUIContent("Revert to prefab value (Does not exist on prefab)"));
				}
			}
			if (entry.ListLengthChangedFromPrefab)
			{
				if (active)
				{
					genericMenu.AddItem(new GUIContent("Revert to prefab list length"), on: false, delegate
					{
						property.RecordForUndo("Revert to prefab list length");
						for (int i = 0; i < entry.ValueCount; i++)
						{
							property.Tree.PrefabModificationHandler.RemovePrefabModification(property, i, PrefabModificationType.ListLength);
						}
						property.Children.Update();
						if (property.Tree.UnitySerializedObject != null)
						{
							property.Tree.UnitySerializedObject.Update();
						}
					});
					showApplyToPrefab = true;
				}
				else
				{
					genericMenu.AddDisabledItem(new GUIContent("Revert to prefab list length (Does not exist on prefab)"));
				}
			}
			if (entry.DictionaryChangedFromPrefab)
			{
				if (active)
				{
					genericMenu.AddItem(new GUIContent("Revert dictionary changes to prefab value"), on: false, delegate
					{
						property.RecordForUndo("Revert to prefab dictionary");
						for (int i = 0; i < entry.ValueCount; i++)
						{
							property.Tree.PrefabModificationHandler.RemovePrefabModification(property, i, PrefabModificationType.Dictionary);
						}
						property.Children.Update();
						if (property.Tree.UnitySerializedObject != null)
						{
							property.Tree.UnitySerializedObject.Update();
						}
					});
					showApplyToPrefab = true;
				}
				else
				{
					genericMenu.AddDisabledItem(new GUIContent("Revert dictionary changes to prefab value (Does not exist on prefab)"));
				}
			}
			if (!showApplyToPrefab)
			{
				return;
			}
			string applyText = "Apply value to prefab '" + (prefabProperty.Tree.WeakTargets[0] as UnityEngine.Object).name + "'";
			genericMenu.AddItem(new GUIContent(applyText), on: false, delegate
			{
				bool flag = false;
				UnityEngine.Object[] objectsToUndo = prefabProperty.SerializationRoot.ValueEntry.WeakValues.Cast<UnityEngine.Object>().AppendWith(property.SerializationRoot.ValueEntry.WeakValues.Cast<UnityEngine.Object>()).ToArray();
				Undo.RecordObjects(objectsToUndo, applyText);
				if (OdinPrefabSerializationEditorUtility.HasApplyPropertyOverride && property.ValueEntry.SerializationBackend.IsUnity && property.Tree.UnitySerializedObject != null)
				{
					SerializedProperty serializedProperty = property.Tree.GetUnitySerializedObjectNoUpdate().FindProperty(property.UnityPropertyPath);
					if (serializedProperty != null)
					{
						PrefabModificationHandler prefabModificationHandler = property.Tree.PrefabModificationHandler;
						for (int i = 0; i < prefabModificationHandler.TargetPrefabs.Count; i++)
						{
							string assetPath = AssetDatabase.GetAssetPath(prefabModificationHandler.TargetPrefabs[i]);
							OdinPrefabSerializationEditorUtility.ApplyPropertyOverride(serializedProperty, assetPath);
						}
						flag = true;
					}
				}
				if (!flag)
				{
					IPropertyValueEntry valueEntry = prefabProperty.ValueEntry;
					for (int j = 0; j < entry.ValueCount; j++)
					{
						object obj = entry.WeakValues[j];
						object value = Sirenix.Serialization.SerializationUtility.CreateCopy(obj);
						valueEntry.WeakValues[j] = value;
					}
					valueEntry.ApplyChanges();
					for (int k = 0; k < entry.ValueCount; k++)
					{
						if (entry.ValueChangedFromPrefab)
						{
							property.Tree.PrefabModificationHandler.RemovePrefabModification(property, k, PrefabModificationType.Value);
						}
						if (entry.ListLengthChangedFromPrefab)
						{
							property.Tree.PrefabModificationHandler.RemovePrefabModification(property, k, PrefabModificationType.ListLength);
						}
						if (entry.DictionaryChangedFromPrefab)
						{
							property.Tree.PrefabModificationHandler.RemovePrefabModification(property, k, PrefabModificationType.Dictionary);
						}
					}
				}
				if (property.Tree.UnitySerializedObject != null)
				{
					property.Tree.UnitySerializedObject.Update();
				}
				Undo.FlushUndoRecordObjects();
			});
		}

		private static void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			bool fillUnityContextMenuForPrefab = GlobalConfig<GeneralDrawerConfig>.Instance.UseUnityContextMenuForModifications && property.Tree.PrefabModificationHandler.HasPrefabs && property.ValueEntry != null && property.ValueEntry.SerializationBackend.IsUnity && (property.ValueEntry.ValueChangedFromPrefab || property.ValueEntry.ChildValueChangedFromPrefab);
			if (fillUnityContextMenuForPrefab || Event.current.shift)
			{
				FillUnityContextMenu(property, genericMenu);
			}
			if (!fillUnityContextMenuForPrefab)
			{
				PopulateChangedFromPrefabContext(property, genericMenu);
			}
			if (genericMenu.GetItemCount() > 0)
			{
				genericMenu.AddSeparator("");
			}
			if (property.Parent != null && property.Parent.ChildResolver is IOrderedCollectionResolver)
			{
				IOrderedCollectionResolver parentResolver = property.Parent.ChildResolver as IOrderedCollectionResolver;
				ListDrawerSettingsAttribute parentListDrawerSettings = property.Parent.GetAttribute<ListDrawerSettingsAttribute>();
				if ((parentListDrawerSettings != null && parentListDrawerSettings.IsReadOnly) || parentResolver.IsReadOnly)
				{
					genericMenu.AddDisabledItem(new GUIContent("Move element to top"));
					genericMenu.AddDisabledItem(new GUIContent("Move element to bottom"));
					genericMenu.AddDisabledItem(new GUIContent("Move element to index"));
					genericMenu.AddDisabledItem(new GUIContent("Duplicate element"));
					genericMenu.AddDisabledItem(new GUIContent("Insert pasted element"));
					genericMenu.AddDisabledItem(new GUIContent("Insert new element"));
					genericMenu.AddDisabledItem(new GUIContent("Delete element"));
				}
				else
				{
					genericMenu.AddItem(new GUIContent("Move element to top"), on: false, delegate
					{
						object[] array = new object[property.ValueEntry.WeakValues.Count];
						for (int i = 0; i < array.Length; i++)
						{
							array[i] = property.ValueEntry.WeakValues[i];
						}
						parentResolver.QueueRemoveAt(property.Index);
						parentResolver.QueueInsertAt(0, array);
					});
					genericMenu.AddItem(new GUIContent("Move element to bottom"), on: false, delegate
					{
						object[] array = new object[property.ValueEntry.WeakValues.Count];
						for (int i = 0; i < array.Length; i++)
						{
							array[i] = property.ValueEntry.WeakValues[i];
						}
						parentResolver.QueueRemoveAt(property.Index);
						parentResolver.QueueAdd(array);
					});
					genericMenu.AddItem(new GUIContent("Move element to index"), on: false, delegate
					{
						IndexPopupWindow popup = new IndexPopupWindow();
						popup.IntLabel = "Index";
						popup.Value = property.Index;
						popup.MaxCount = property.Parent.Children.Count - 1;
						popup.MoveAction = delegate
						{
							int num2 = popup.Value;
							int index = property.Index;
							bool flag = false;
							if (num2 < 0)
							{
								num2 = 0;
							}
							if (num2 > property.Parent.Children.Count)
							{
								flag = true;
							}
							object[] array = new object[property.ValueEntry.WeakValues.Count];
							for (int i = 0; i < array.Length; i++)
							{
								array[i] = property.ValueEntry.WeakValues[i];
							}
							parentResolver.QueueRemoveAt(property.Index);
							if (flag)
							{
								parentResolver.QueueAdd(array);
							}
							else
							{
								parentResolver.QueueInsertAt(num2, array);
							}
						};
						property.Tree.DelayActionUntilRepaint(delegate
						{
							OdinEditorWindow window = OdinEditorWindow.InspectObjectInDropDown(popup, 120f);
							popup.CloseWindowAction = delegate
							{
								EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, new EditorApplication.CallbackFunction(window.Close));
							};
						});
					});
					genericMenu.AddItem(new GUIContent("Duplicate element"), on: false, delegate
					{
						object[] array = new object[property.ValueEntry.WeakValues.Count];
						for (int i = 0; i < array.Length; i++)
						{
							array[i] = Sirenix.Serialization.SerializationUtility.CreateCopy(property.ValueEntry.WeakValues[i]);
						}
						if (property.Index + 1 >= property.Parent.Children.Count)
						{
							parentResolver.QueueAdd(array);
						}
						else
						{
							parentResolver.QueueInsertAt(property.Index + 1, array);
						}
					});
					if (Clipboard.CanPaste(parentResolver.ElementType))
					{
						genericMenu.AddItem(new GUIContent("Insert pasted element"), on: false, delegate
						{
							object obj = Clipboard.Paste();
							object[] array = new object[property.ValueEntry.WeakValues.Count];
							array[0] = obj;
							for (int i = 1; i < array.Length; i++)
							{
								array[i] = Sirenix.Serialization.SerializationUtility.CreateCopy(property.ValueEntry.WeakValues[i]);
							}
							parentResolver.QueueInsertAt(property.Index, array);
						});
					}
					else
					{
						genericMenu.AddDisabledItem(new GUIContent("Insert pasted element"));
					}
					IHackyListDrawerInteractions parentListDrawer = null;
					OdinDrawer[] bakedDrawerArray = property.Parent.GetActiveDrawerChain().BakedDrawerArray;
					foreach (OdinDrawer drawer in bakedDrawerArray)
					{
						parentListDrawer = drawer as IHackyListDrawerInteractions;
						if (parentListDrawer != null)
						{
							break;
						}
					}
					if (parentListDrawer == null || !parentListDrawer.CanCreateValuesToAdd)
					{
						genericMenu.AddDisabledItem(new GUIContent("Insert new element"));
					}
					else
					{
						genericMenu.AddItem(new GUIContent("Insert new element"), on: false, delegate
						{
							property.Tree.DelayActionUntilRepaint(delegate
							{
								parentListDrawer.CreateValuesToAdd(delegate(object[] values)
								{
									parentResolver.QueueInsertAt(property.Index, values);
								}, UnityShims.Rect.Ctor(Event.current.mousePosition, Vector2.one));
							});
						});
					}
					genericMenu.AddItem(new GUIContent("Delete element"), on: false, delegate
					{
						property.Tree.DelayActionUntilRepaint(delegate
						{
							parentResolver.QueueRemoveAt(property.Index);
						});
					});
				}
			}
			object[] objs = (from x in property.ValueEntry.WeakValues.FilterCast<object>()
				where x != null
				select x).ToArray();
			object valueToCopy = ((objs == null || objs.Length == 0) ? null : ((objs.Length == 1) ? objs[0] : objs));
			bool isUnityObject = property.ValueEntry.BaseValueType.InheritsFrom(typeof(UnityEngine.Object));
			bool hasValue = valueToCopy != null;
			bool canPaste = Clipboard.CanPaste(property.ValueEntry.BaseValueType);
			bool isEditable = property.ValueEntry.IsEditable;
			bool isNullable = (property.ValueEntry.BaseValueType.IsClass || property.ValueEntry.BaseValueType.IsInterface) && !property.Info.TypeOfValue.IsValueType && (property.ValueEntry.SerializationBackend.SupportsPolymorphism || isUnityObject);
			if (canPaste && isEditable)
			{
				genericMenu.AddItem(new GUIContent("Paste"), on: false, delegate
				{
					property.Tree.DelayActionUntilRepaint(delegate
					{
						for (int i = 0; i < property.ValueEntry.ValueCount; i++)
						{
							property.ValueEntry.WeakValues[i] = Clipboard.Paste();
						}
						GUIHelper.RequestRepaint();
					});
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Paste"));
			}
			if (hasValue)
			{
				if (isUnityObject)
				{
					genericMenu.AddItem(new GUIContent("Copy"), on: false, delegate
					{
						Clipboard.Copy(valueToCopy, CopyModes.CopyReference);
					});
				}
				else if (!property.ValueEntry.SerializationBackend.SupportsCyclicReferences)
				{
					genericMenu.AddItem(new GUIContent("Copy"), on: false, delegate
					{
						Clipboard.Copy(valueToCopy, CopyModes.DeepCopy);
					});
				}
				else
				{
					genericMenu.AddItem(new GUIContent("Copy"), on: false, delegate
					{
						Clipboard.Copy(valueToCopy, CopyModes.DeepCopy);
					});
					genericMenu.AddItem(new GUIContent("Copy Special/Deep Copy (default)"), on: false, delegate
					{
						Clipboard.Copy(valueToCopy, CopyModes.DeepCopy);
					});
					genericMenu.AddItem(new GUIContent("Copy Special/Shallow Copy"), on: false, delegate
					{
						Clipboard.Copy(valueToCopy, CopyModes.ShallowCopy);
					});
					genericMenu.AddItem(new GUIContent("Copy Special/Copy Reference"), on: false, delegate
					{
						Clipboard.Copy(valueToCopy, CopyModes.CopyReference);
					});
				}
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Copy"));
			}
			if (!isNullable)
			{
				return;
			}
			genericMenu.AddSeparator("");
			if (hasValue && isEditable)
			{
				genericMenu.AddItem(new GUIContent("Set To Null"), on: false, delegate
				{
					property.Tree.DelayActionUntilRepaint(delegate
					{
						for (int i = 0; i < property.ValueEntry.ValueCount; i++)
						{
							property.ValueEntry.WeakValues[i] = null;
						}
						GUIHelper.RequestRepaint();
					});
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Set To Null"));
			}
		}
	}
	/// <summary>
	/// Opens a context menu for any given property on right click. The context menu is populated by all relevant drawers that implements <see cref="T:Sirenix.OdinInspector.Editor.IDefinesGenericMenuItems" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.IDefinesGenericMenuItems" />
	[DrawerPriority(95.0, 0.0, 0.0)]
	public sealed class PropertyContextMenuDrawer<T> : OdinValueDrawer<T>
	{
		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			return !property.IsTreeRoot;
		}

		/// <summary>
		/// Initializes the drawer.
		/// </summary>
		protected override void Initialize()
		{
			DisableContextMenuAttribute disableAttr = base.Property.GetAttribute<DisableContextMenuAttribute>();
			if (disableAttr != null && disableAttr.DisableForMember)
			{
				base.SkipWhenDrawing = true;
			}
			else if (base.Property.Parent != null && base.Property.Parent.ChildResolver is ICollectionResolver)
			{
				base.SkipWhenDrawing = base.Property.Parent.GetAttribute<DisableContextMenuAttribute>()?.DisableForCollectionElements ?? false;
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			CallNextDrawer(label);
			int id = GUIUtility.GetControlID(FocusType.Passive);
			if (Event.current.type != EventType.Layout)
			{
				Rect rect = ((base.Property.Parent == null || !(base.Property.Parent.ChildResolver is ICollectionResolver)) ? base.Property.LastDrawnValueRect : GUIHelper.GetCurrentLayoutRect());
				GUIHelper.PushGUIEnabled(enabled: true);
				PropertyContextMenuDrawer.AddRightClickArea(base.Property, rect, id);
				GUIHelper.PopGUIEnabled();
			}
		}
	}
}

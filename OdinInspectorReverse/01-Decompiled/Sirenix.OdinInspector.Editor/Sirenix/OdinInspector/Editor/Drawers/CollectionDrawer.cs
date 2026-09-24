using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor.ActionResolvers;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities.Editor.Expressions;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Property drawer for anything that has a <see cref="T:Sirenix.OdinInspector.Editor.ICollectionResolver" />.
	/// </summary>
	[AllowGUIEnabledForReadonly]
	[DrawerPriority(0.0, 0.0, 0.9)]
	public class CollectionDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems, IHackyListDrawerInteractions
	{
		private class FilteredPropertyChildren
		{
			public InspectorProperty Property;

			public PropertyChildren Children;

			public PropertySearchFilter SearchFilter;

			private List<InspectorProperty> FilteredChildren;

			private int lastUpdatedChildrenCount = -1;

			public bool IsCurrentlyFiltered => FilteredChildren != null;

			public int Count
			{
				get
				{
					if (FilteredChildren == null)
					{
						return Children.Count;
					}
					if (Children.Count != lastUpdatedChildrenCount)
					{
						Update();
						if (FilteredChildren == null)
						{
							return Children.Count;
						}
					}
					return FilteredChildren.Count;
				}
			}

			public InspectorProperty this[int index]
			{
				get
				{
					if (FilteredChildren == null)
					{
						return Children[index];
					}
					return FilteredChildren[index];
				}
			}

			public FilteredPropertyChildren(InspectorProperty property, PropertySearchFilter searchFilter)
			{
				Property = property;
				Children = property.Children;
				SearchFilter = searchFilter;
			}

			public void Update()
			{
				if (SearchFilter == null || string.IsNullOrEmpty(SearchFilter.SearchTerm))
				{
					FilteredChildren = null;
					return;
				}
				if (FilteredChildren != null)
				{
					FilteredChildren.Clear();
				}
				else
				{
					FilteredChildren = new List<InspectorProperty>();
				}
				for (int i = 0; i < Children.Count; i++)
				{
					InspectorProperty child = Children[i];
					if (SearchFilter.IsMatch(child, SearchFilter.SearchTerm))
					{
						FilteredChildren.Add(child);
					}
					else
					{
						if (!SearchFilter.Recursive)
						{
							continue;
						}
						foreach (InspectorProperty recursiveChild in child.Children.Recurse())
						{
							if (SearchFilter.IsMatch(recursiveChild, SearchFilter.SearchTerm))
							{
								FilteredChildren.Add(child);
								break;
							}
						}
					}
				}
				lastUpdatedChildrenCount = Children.Count;
			}

			public void ScheduleUpdate()
			{
				Property.Tree.DelayActionUntilRepaint(delegate
				{
					Update();
					GUIHelper.RequestRepaint();
				});
			}
		}

		private struct ListItemInfo
		{
			public float Width;

			public Rect RemoveBtnRect;

			public Rect DragHandleRect;
		}

		private class ListDrawerConfigInfo
		{
			public ICollectionResolver CollectionResolver;

			public IOrderedCollectionResolver OrderedCollectionResolver;

			public bool IsEmpty;

			public ListDrawerSettingsAttribute CustomListDrawerOptions;

			public int Count;

			public int StartIndex;

			public int EndIndex;

			public DropZoneHandle DropZone;

			public Vector2 DraggingMousePosition;

			public Vector2 DropZoneTopLeft;

			public int InsertAt;

			public int RemoveAt;

			public object[] RemoveValues;

			public bool ShowAllWhilePaging;

			public bool JumpToNextPageOnAdd;

			public GeneralDrawerConfig ListConfig;

			public InspectorProperty Property;

			public GUIContent Label;

			public bool IsAboutToDroppingUnityObjects;

			public bool IsDroppingUnityObjects;

			public bool HideAddButton;

			public bool HideRemoveButton;

			public FilteredPropertyChildren FilteredChildren;

			public bool BaseDraggable;

			public bool BaseIsReadOnly;

			public string SearchFieldControlName = "CollectionSearchFilter_" + Guid.NewGuid();

			public ActionResolver OnTitleBarGUI;

			public ActionResolver GetCustomAddFunctionVoid;

			public ValueResolver GetCustomAddFunction;

			public ActionResolver CustomRemoveIndexFunction;

			public ActionResolver CustomRemoveElementFunction;

			public ActionResolver OnBeginListElementGUI;

			public ActionResolver OnEndListElementGUI;

			public ValueResolver<Color> ElementColor;

			public Func<object, InspectorProperty, object> GetListElementLabelText;

			public GUIStyle ListItemStyle = new GUIStyle(GUIStyle.none)
			{
				padding = new RectOffset(25, 20, 3, 3)
			};

			public bool IsReadOnly
			{
				get
				{
					if (!BaseIsReadOnly)
					{
						return FilteredChildren.IsCurrentlyFiltered;
					}
					return true;
				}
			}

			public bool Draggable
			{
				get
				{
					if (BaseDraggable)
					{
						return !FilteredChildren.IsCurrentlyFiltered;
					}
					return false;
				}
			}

			public int NumberOfItemsPerPage
			{
				get
				{
					if (!CustomListDrawerOptions.NumberOfItemsPerPageHasValue)
					{
						return ListConfig.NumberOfItemsPrPage;
					}
					return CustomListDrawerOptions.NumberOfItemsPerPage;
				}
			}
		}

		private static readonly int CollectionDrawerId = "id_CollectionDrawer".GetHashCode();

		private static GUILayoutOption[] listItemOptions = GUILayoutOptions.MinHeight(25f).ExpandWidth();

		private ListDrawerConfigInfo info;

		private string errorMessage;

		private Action<object[]> onValuesCreated;

		private Action superHackyAddFunctionWeSeriouslyNeedANewListDrawer;

		private TypeSelectorSettingsAttribute selectorSettings;

		private PolymorphicDrawerSettingsAttribute polymorphicSettings;

		bool IHackyListDrawerInteractions.CanCreateValuesToAdd => info.GetCustomAddFunctionVoid == null;

		void IHackyListDrawerInteractions.CreateValuesToAdd(Action<object[]> onCreated, Rect potentialPopupPosition)
		{
			onValuesCreated = onCreated;
			StartCreatingValues(potentialPopupPosition);
		}

		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			return property.ChildResolver is ICollectionResolver;
		}

		void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			if (property.ValueEntry.WeakSmartValue == null)
			{
				return;
			}
			ICollectionResolver resolver = property.ChildResolver as ICollectionResolver;
			bool isReadOnly = resolver.IsReadOnly;
			ListDrawerSettingsAttribute config = property.GetAttribute<ListDrawerSettingsAttribute>();
			bool isEditable = !isReadOnly && property.ValueEntry.IsEditable && (config == null || !config.IsReadOnlyHasValue || (config.IsReadOnlyHasValue && !config.IsReadOnly));
			bool pasteElement = isEditable && Clipboard.CanPaste(resolver.ElementType);
			bool clearList = isEditable && property.Children.Count > 0;
			bool setCollectionLength = isEditable && property.ChildResolver is IOrderedCollectionResolver && typeof(IList).IsAssignableFrom(typeof(T));
			int windowWidth = 300;
			Rect rect = property.LastDrawnValueRect.AlignTop(1f);
			rect.y += 14f;
			rect.xMin += rect.width * 0.5f - (float)windowWidth * 0.5f;
			rect.position = GUIUtility.GUIToScreenPoint(rect.position);
			rect.width = 1f;
			if (setCollectionLength)
			{
				if (info.GetCustomAddFunctionVoid != null)
				{
					genericMenu.AddDisabledItem(new GUIContent("Set Collection Size - disabled by 'void " + info.CustomListDrawerOptions.CustomAddFunction + "'"));
				}
				else
				{
					genericMenu.AddItem(new GUIContent("Set Collection Size"), on: false, delegate
					{
						EditorWindow window = null;
						Action cancel = delegate
						{
							UnityEditorEventUtility.EditorApplication_delayCall += window.Close;
						};
						Action<int> confirm = delegate(int size)
						{
							UnityEditorEventUtility.EditorApplication_delayCall += window.Close;
							SetCollectionSize(property, size);
						};
						CollectionSizeDialogue obj = new CollectionSizeDialogue(confirm, cancel, property.ChildResolver.MaxChildCountSeen);
						window = OdinEditorWindow.InspectObjectInDropDown(obj, rect, windowWidth);
						GUIHelper.RequestRepaint();
					});
				}
			}
			if (pasteElement)
			{
				genericMenu.AddItem(new GUIContent("Paste Element"), on: false, delegate
				{
					(property.ChildResolver as ICollectionResolver).QueueAdd(new object[1] { Clipboard.Paste() });
					GUIHelper.RequestRepaint();
				});
			}
			if (clearList)
			{
				genericMenu.AddSeparator("");
				genericMenu.AddItem(new GUIContent("Clear Collection"), on: false, delegate
				{
					(property.ChildResolver as ICollectionResolver).QueueClear();
					GUIHelper.RequestRepaint();
				});
			}
			else
			{
				genericMenu.AddSeparator("");
				genericMenu.AddDisabledItem(new GUIContent("Clear Collection"));
			}
		}

		private void SetCollectionSize(InspectorProperty p, int targetSize)
		{
			IOrderedCollectionResolver resolver = p.ChildResolver as IOrderedCollectionResolver;
			for (int i = 0; i < p.ParentValues.Count; i++)
			{
				IList collection = p.ValueEntry.WeakValues[i] as IList;
				int size = collection.Count;
				int delta = Math.Abs(targetSize - size);
				if (targetSize > size)
				{
					for (int j = 0; j < delta; j++)
					{
						object value = GetValueToAdd(i);
						resolver.QueueAdd(value, i);
					}
				}
				else
				{
					for (int k = 0; k < delta; k++)
					{
						resolver.QueueRemoveAt(size - (1 + k), i);
					}
				}
			}
		}

		private object GetValueToAdd(int selectionIndex)
		{
			bool wasFallback;
			return GetValueToAdd(selectionIndex, out wasFallback);
		}

		private object GetValueToAdd(int selectionIndex, out bool wasFallback)
		{
			wasFallback = false;
			if (info.GetCustomAddFunction != null)
			{
				return info.GetCustomAddFunction.GetWeakValue(selectionIndex);
			}
			if (info.CustomListDrawerOptions.AlwaysAddDefaultValue)
			{
				if (!info.Property.ValueEntry.SerializationBackend.SupportsPolymorphism)
				{
					return UnitySerializationUtility.CreateDefaultUnityInitializedObject(info.CollectionResolver.ElementType);
				}
				if (info.CollectionResolver.ElementType.IsValueType)
				{
					return Activator.CreateInstance(info.CollectionResolver.ElementType);
				}
				return null;
			}
			if (info.CustomListDrawerOptions.AddCopiesLastElement && info.Count > 0)
			{
				object lastObject = null;
				IPropertyValueEntry lastElementProperty = info.FilteredChildren[info.Count - 1].ValueEntry;
				if (info.Property.ValueEntry.WeakValues[selectionIndex] is IEnumerable collection)
				{
					foreach (object item in collection)
					{
						lastObject = item;
					}
				}
				else
				{
					lastObject = lastElementProperty.WeakValues[selectionIndex];
				}
				return Sirenix.Serialization.SerializationUtility.CreateCopy(lastObject);
			}
			if (info.CollectionResolver.ElementType.InheritsFrom<UnityEngine.Object>() && UnityShims.Misc.GetEventModifiers(Event.current) == 2)
			{
				return null;
			}
			wasFallback = true;
			Type elementType = (base.Property.ChildResolver as ICollectionResolver).ElementType;
			if (!base.ValueEntry.SerializationBackend.SupportsPolymorphism)
			{
				return UnitySerializationUtility.CreateDefaultUnityInitializedObject(elementType);
			}
			if (!elementType.IsValueType)
			{
				return null;
			}
			return Activator.CreateInstance(elementType);
		}

		/// <summary>
		/// Initializes the drawer.
		/// </summary>
		protected override void Initialize()
		{
			selectorSettings = base.Property.GetAttribute<TypeSelectorSettingsAttribute>();
			polymorphicSettings = base.Property.GetAttribute<PolymorphicDrawerSettingsAttribute>();
			ICollectionResolver resolver = base.Property.ChildResolver as ICollectionResolver;
			bool isReadOnly = resolver.IsReadOnly;
			ListDrawerSettingsAttribute customListDrawerOptions = base.Property.GetAttribute<ListDrawerSettingsAttribute>() ?? new ListDrawerSettingsAttribute();
			isReadOnly = !base.ValueEntry.IsEditable || isReadOnly || (customListDrawerOptions.IsReadOnlyHasValue && customListDrawerOptions.IsReadOnly);
			PropertySearchFilter searchFilter = null;
			SearchableAttribute searchAttr = base.Property.GetAttribute<SearchableAttribute>();
			if (searchAttr != null)
			{
				searchFilter = new PropertySearchFilter(null, searchAttr);
				resolver.OnAfterChange += delegate
				{
					base.Property.Children.Update();
					info.FilteredChildren.Update();
				};
			}
			if (customListDrawerOptions.DefaultExpandedStateHasValue)
			{
				base.Property.State.Expanded = customListDrawerOptions.DefaultExpandedState;
			}
			info = new ListDrawerConfigInfo
			{
				StartIndex = 0,
				RemoveAt = -1,
				ShowAllWhilePaging = false,
				EndIndex = 0,
				CustomListDrawerOptions = customListDrawerOptions,
				BaseIsReadOnly = isReadOnly,
				BaseDraggable = !isReadOnly,
				HideAddButton = (isReadOnly || customListDrawerOptions.HideAddButton),
				HideRemoveButton = (isReadOnly || customListDrawerOptions.HideRemoveButton),
				FilteredChildren = new FilteredPropertyChildren(base.Property, searchFilter)
			};
			info.ListConfig = GlobalConfig<GeneralDrawerConfig>.Instance;
			info.Property = base.Property;
			if (customListDrawerOptions.DraggableHasValue && !customListDrawerOptions.DraggableItems)
			{
				info.BaseDraggable = false;
			}
			if (!(base.Property.ChildResolver is IOrderedCollectionResolver))
			{
				info.BaseDraggable = false;
			}
			if (info.CustomListDrawerOptions.OnBeginListElementGUI != null)
			{
				info.OnBeginListElementGUI = ActionResolver.Get(base.Property, info.CustomListDrawerOptions.OnBeginListElementGUI, new Sirenix.OdinInspector.Editor.ActionResolvers.NamedValue("index", typeof(int)));
			}
			if (info.CustomListDrawerOptions.OnEndListElementGUI != null)
			{
				info.OnEndListElementGUI = ActionResolver.Get(base.Property, info.CustomListDrawerOptions.OnEndListElementGUI, new Sirenix.OdinInspector.Editor.ActionResolvers.NamedValue("index", typeof(int)));
			}
			if (info.CustomListDrawerOptions.OnTitleBarGUI != null)
			{
				info.OnTitleBarGUI = ActionResolver.Get(base.Property, info.CustomListDrawerOptions.OnTitleBarGUI);
			}
			if (info.CustomListDrawerOptions.ElementColor != null)
			{
				info.ElementColor = ValueResolver.Get<Color>(base.Property, info.CustomListDrawerOptions.ElementColor, new Sirenix.OdinInspector.Editor.ValueResolvers.NamedValue[2]
				{
					new Sirenix.OdinInspector.Editor.ValueResolvers.NamedValue("index", typeof(int)),
					new Sirenix.OdinInspector.Editor.ValueResolvers.NamedValue("defaultColor", typeof(Color))
				});
			}
			if (info.CustomListDrawerOptions.ListElementLabelName != null)
			{
				info.GetListElementLabelText = CreateListElementLabelNameGetter(info.CustomListDrawerOptions.ListElementLabelName, resolver.ElementType, ref errorMessage);
			}
			if (info.CustomListDrawerOptions.CustomAddFunction != null)
			{
				info.GetCustomAddFunction = ValueResolver.Get(resolver.ElementType, base.Property, info.CustomListDrawerOptions.CustomAddFunction);
				if (info.GetCustomAddFunction.HasError)
				{
					info.GetCustomAddFunctionVoid = ActionResolver.Get(base.Property, info.CustomListDrawerOptions.CustomAddFunction);
					if (!info.GetCustomAddFunctionVoid.HasError)
					{
						info.GetCustomAddFunction = null;
					}
				}
			}
			if (info.CustomListDrawerOptions.CustomRemoveIndexFunction != null)
			{
				if (!(base.Property.ChildResolver is IOrderedCollectionResolver))
				{
					if (errorMessage != null)
					{
						errorMessage += "\n\n";
					}
					errorMessage += "ListDrawerSettings.CustomRemoveIndexFunction is invalid on unordered collections. Use ListDrawerSetings.CustomRemoveElementFunction instead.";
				}
				else
				{
					info.CustomRemoveIndexFunction = ActionResolver.Get(base.Property, info.CustomListDrawerOptions.CustomRemoveIndexFunction, new Sirenix.OdinInspector.Editor.ActionResolvers.NamedValue("index", typeof(int)));
				}
			}
			else if (info.CustomListDrawerOptions.CustomRemoveElementFunction != null)
			{
				info.CustomRemoveElementFunction = ActionResolver.Get(base.Property, info.CustomListDrawerOptions.CustomRemoveElementFunction, new Sirenix.OdinInspector.Editor.ActionResolvers.NamedValue("removeElement", resolver.ElementType));
			}
		}

		private static Func<object, InspectorProperty, object> CreateListElementLabelNameGetter(string resolvedString, Type elementType, ref string errorMessage)
		{
			if (resolvedString.Length > 1 && resolvedString[0] == '@')
			{
				string expression = resolvedString.Substring(1);
				Type[] parameters = new Type[1] { typeof(InspectorProperty) };
				string[] parameterNames = new string[1] { "property" };
				string exprError;
				Delegate exprDelegate = ExpressionUtility.ParseExpression(expression, isStatic: false, elementType, parameters, parameterNames, out exprError);
				if (exprError != null)
				{
					if (errorMessage != null)
					{
						errorMessage += "\n\n";
					}
					errorMessage += exprError;
					return null;
				}
				Type exprType = exprDelegate.Method.ReturnType;
				if (exprType == typeof(void) || exprType == null)
				{
					if (errorMessage != null)
					{
						errorMessage += "\n\n";
					}
					errorMessage = errorMessage + "ListElementLabelName expression '" + expression + "' is not allowed to evaluate to 'void'.";
					return null;
				}
				object[] exprParameters = new object[2];
				return delegate(object instance, InspectorProperty property)
				{
					exprParameters[0] = instance;
					exprParameters[1] = property;
					return exprDelegate.DynamicInvoke(exprParameters);
				};
			}
			FieldInfo fieldInfo = elementType.GetField(resolvedString, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			if (fieldInfo != null)
			{
				if (fieldInfo.IsStatic)
				{
					return (object instance, InspectorProperty property) => fieldInfo.GetValue(null);
				}
				return (object instance, InspectorProperty property) => fieldInfo.GetValue(instance);
			}
			PropertyInfo propertyInfo = elementType.GetProperty(resolvedString, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			if (propertyInfo != null)
			{
				if (propertyInfo.IsStatic())
				{
					return (object instance, InspectorProperty property) => propertyInfo.GetValue(null, null);
				}
				return (object instance, InspectorProperty property) => propertyInfo.GetValue(instance, null);
			}
			MethodInfo methodInfo = elementType.GetMethod(resolvedString, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, null, Type.EmptyTypes, null);
			if (methodInfo != null)
			{
				if (methodInfo.ReturnType == typeof(void) || methodInfo.ReturnType == null)
				{
					if (errorMessage != null)
					{
						errorMessage += "\n\n";
					}
					errorMessage = errorMessage + "ListElementLabelName method '" + resolvedString + "' on element type '" + elementType.GetNiceName() + "' is not allowed to return void.";
					return null;
				}
				if (methodInfo.IsStatic)
				{
					return (object instance, InspectorProperty prop) => methodInfo.Invoke(null, null);
				}
				return (object instance, InspectorProperty prop) => methodInfo.Invoke(instance, null);
			}
			if (errorMessage != null)
			{
				errorMessage += "\n\n";
			}
			errorMessage = errorMessage + "Couldn't find any field, property or parameterless method named '" + resolvedString + "' on element type '" + elementType.GetNiceName() + "' to use for ListElementLabelName.";
			return null;
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			ICollectionResolver resolver = base.Property.ChildResolver as ICollectionResolver;
			bool isReadOnly = resolver.IsReadOnly;
			if (errorMessage != null)
			{
				SirenixEditorGUI.MessageBox(errorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			}
			ActionResolver.DrawErrors(info.OnTitleBarGUI, info.GetCustomAddFunctionVoid, info.CustomRemoveIndexFunction, info.CustomRemoveElementFunction, info.OnBeginListElementGUI, info.OnEndListElementGUI);
			ValueResolver.DrawErrors(info.GetCustomAddFunction, info.ElementColor);
			if (info.Label == null || (label != null && label.text != info.Label.text))
			{
				info.Label = new GUIContent((label == null || string.IsNullOrEmpty(label.text)) ? base.Property.ValueEntry.TypeOfValue.GetNiceName() : label.text, (label == null) ? string.Empty : label.tooltip);
			}
			info.Label.image = label?.image;
			info.BaseIsReadOnly = resolver.IsReadOnly;
			info.ListItemStyle.padding.left = (info.Draggable ? 25 : 7);
			info.ListItemStyle.padding.right = ((info.BaseIsReadOnly || info.HideRemoveButton) ? 4 : 20);
			if (Event.current.type == EventType.Repaint)
			{
				info.DropZoneTopLeft = GUIUtility.GUIToScreenPoint(new Vector2(0f, 0f));
			}
			info.CollectionResolver = base.Property.ChildResolver as ICollectionResolver;
			info.OrderedCollectionResolver = base.Property.ChildResolver as IOrderedCollectionResolver;
			info.Count = info.FilteredChildren.Count;
			info.IsEmpty = info.FilteredChildren.Count == 0;
			Rect rect = SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyMargin);
			BeginDropZone();
			DrawToolbar();
			if (SirenixEditorGUI.BeginFadeGroup(UniqueDrawerKey.Create(base.Property, this), base.Property.State.Expanded))
			{
				GUIHelper.PushLabelWidth(GUIHelper.BetterLabelWidth - (float)info.ListItemStyle.padding.left);
				DrawItems();
				GUIHelper.PopLabelWidth();
			}
			SirenixEditorGUI.EndFadeGroup();
			EndDropZone();
			SirenixEditorGUI.EndIndentedVertical();
			Rect prefabOverrideRect = rect;
			prefabOverrideRect.height -= 1f;
			base.Property.PrefabModificationBarSourceRectOverride = prefabOverrideRect;
			if (info.OrderedCollectionResolver != null)
			{
				if (info.RemoveAt < 0 || Event.current.type != EventType.Repaint)
				{
					return;
				}
				try
				{
					if (info.CustomRemoveIndexFunction != null && !info.CustomRemoveIndexFunction.HasError)
					{
						base.Property.RecordForUndo("Custom List Remove (Index '" + info.RemoveAt + "')");
						info.CustomRemoveIndexFunction.Context.NamedValues.Set("index", info.RemoveAt);
						info.CustomRemoveIndexFunction.DoActionForAllSelectionIndices();
						base.Property.MarkSerializationRootDirty();
					}
					else if (info.CustomRemoveElementFunction != null && !info.CustomRemoveElementFunction.HasError)
					{
						base.Property.RecordForUndo("Custom List Remove (Element)");
						for (int i = 0; i < base.Property.ParentValues.Count; i++)
						{
							info.CustomRemoveElementFunction.Context.NamedValues.Set("removeElement", base.Property.Children[info.RemoveAt].ValueEntry.WeakValues[i]);
							info.CustomRemoveElementFunction.DoAction(i);
						}
						base.Property.MarkSerializationRootDirty();
					}
					else
					{
						info.OrderedCollectionResolver.QueueRemoveAt(info.RemoveAt);
					}
				}
				finally
				{
					info.RemoveAt = -1;
					info.FilteredChildren.ScheduleUpdate();
				}
				GUIHelper.RequestRepaint();
			}
			else
			{
				if (info.RemoveValues == null || Event.current.type != EventType.Repaint)
				{
					return;
				}
				try
				{
					if (info.CustomRemoveElementFunction != null && !info.CustomRemoveElementFunction.HasError)
					{
						for (int j = 0; j < base.Property.ParentValues.Count; j++)
						{
							info.CustomRemoveElementFunction.Context.NamedValues.Set("removeElement", info.RemoveValues[j]);
							info.CustomRemoveElementFunction.DoAction(j);
						}
					}
					else
					{
						info.CollectionResolver.QueueRemove(info.RemoveValues);
					}
				}
				finally
				{
					info.RemoveValues = null;
					info.FilteredChildren.ScheduleUpdate();
				}
				GUIHelper.RequestRepaint();
			}
		}

		private DropZoneHandle BeginDropZone()
		{
			if (info.OrderedCollectionResolver == null)
			{
				return null;
			}
			DropZoneHandle dropZone = DragAndDropManager.BeginDropZone(GetKey(base.Property), info.CollectionResolver.ElementType, canAcceptMove: true);
			dropZone.Enabled = !info.IsReadOnly;
			info.DropZone = dropZone;
			return dropZone;
		}

		private static UnityEngine.Object[] HandleUnityObjectsDrop(ListDrawerConfigInfo info)
		{
			if (info.IsReadOnly)
			{
				return null;
			}
			EventType eventType = Event.current.type;
			if (eventType == EventType.Layout)
			{
				info.IsAboutToDroppingUnityObjects = false;
			}
			if ((eventType == EventType.DragUpdated || eventType == EventType.DragPerform) && info.DropZone.Rect.Contains(Event.current.mousePosition))
			{
				UnityEngine.Object[] objReferences = null;
				if (DragAndDrop.objectReferences.Any((UnityEngine.Object n) => n != null && info.CollectionResolver.ElementType.IsAssignableFrom(n.GetType())))
				{
					objReferences = DragAndDrop.objectReferences.Where((UnityEngine.Object x) => x != null && info.CollectionResolver.ElementType.IsAssignableFrom(x.GetType())).Reverse().ToArray();
				}
				else if (info.CollectionResolver.ElementType.InheritsFrom(typeof(Component)))
				{
					UnityEngine.Object[] array = (from x in DragAndDrop.objectReferences.OfType<GameObject>()
						select x.GetComponent(info.CollectionResolver.ElementType) into x
						where x != null
						select x).Reverse().ToArray();
					objReferences = array;
				}
				else if (info.CollectionResolver.ElementType.InheritsFrom(typeof(Sprite)) && DragAndDrop.objectReferences.Any((UnityEngine.Object n) => n is Texture2D && AssetDatabase.Contains(n)))
				{
					UnityEngine.Object[] array = (from x in DragAndDrop.objectReferences.OfType<Texture2D>().Select(delegate(Texture2D x)
						{
							string assetPath = AssetDatabase.GetAssetPath(x);
							return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
						})
						where x != null
						select x).Reverse().ToArray();
					objReferences = array;
				}
				if (objReferences != null && objReferences.Length != 0)
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					Event.current.Use();
					info.IsAboutToDroppingUnityObjects = true;
					info.IsDroppingUnityObjects = info.IsAboutToDroppingUnityObjects;
					if (eventType == EventType.DragPerform)
					{
						DragAndDrop.AcceptDrag();
						return objReferences;
					}
				}
			}
			if (eventType == EventType.Repaint)
			{
				info.IsDroppingUnityObjects = info.IsAboutToDroppingUnityObjects;
			}
			return null;
		}

		private void EndDropZone()
		{
			if (info.OrderedCollectionResolver == null)
			{
				return;
			}
			if (info.DropZone.IsReadyToClaim)
			{
				if (info.InsertAt == -1)
				{
					info.InsertAt = info.FilteredChildren.Count;
				}
				CollectionDrawerStaticInfo.CurrentDraggingPropertyInfo = null;
				CollectionDrawerStaticInfo.CurrentDroppingPropertyInfo = info.Property;
				object droppedObject = info.DropZone.ClaimObject();
				object[] values = new object[info.Property.Tree.WeakTargets.Count];
				for (int i = 0; i < values.Length; i++)
				{
					values[i] = droppedObject;
				}
				if (info.DropZone.IsCrossWindowDrag)
				{
					GUIHelper.RequestRepaint();
					UnityEditorEventUtility.EditorApplication_delayCall += delegate
					{
						info.OrderedCollectionResolver.QueueInsertAt(Mathf.Clamp(info.InsertAt, 0, info.FilteredChildren.Count), values);
					};
				}
				else
				{
					info.OrderedCollectionResolver.QueueInsertAt(Mathf.Clamp(info.InsertAt, 0, info.FilteredChildren.Count), values);
				}
			}
			else if (!info.IsReadOnly)
			{
				UnityEngine.Object[] droppedObjects = HandleUnityObjectsDrop(info);
				if (droppedObjects != null)
				{
					if (info.InsertAt == -1)
					{
						info.InsertAt = info.FilteredChildren.Count;
					}
					UnityEngine.Object[] array = droppedObjects;
					foreach (UnityEngine.Object obj in array)
					{
						object[] values2 = new object[info.Property.Tree.WeakTargets.Count];
						for (int i2 = 0; i2 < values2.Length; i2++)
						{
							values2[i2] = obj;
						}
						info.OrderedCollectionResolver.QueueInsertAt(Mathf.Clamp(info.InsertAt, 0, info.FilteredChildren.Count), values2);
					}
				}
			}
			DragAndDropManager.EndDropZone();
		}

		private void DrawToolbar()
		{
			SirenixEditorGUI.BeginHorizontalToolbar();
			if (info.DropZone != null && DragAndDropManager.IsDragInProgress && !info.DropZone.IsAccepted)
			{
				GUIHelper.PushGUIEnabled(enabled: false);
			}
			if (info.Property.ValueEntry.ListLengthChangedFromPrefab)
			{
				GUIHelper.PushIsBoldLabel(isBold: true);
			}
			bool drawFoldout = info.CustomListDrawerOptions.ShowFoldout;
			if (info.IsEmpty)
			{
				drawFoldout = false;
			}
			Rect foldoutRect = default(Rect);
			if (!drawFoldout)
			{
				GUILayout.Label(info.Label, GUILayoutOptions.ExpandWidth(expand: false));
			}
			else
			{
				float tmp = EditorGUIUtility.fieldWidth;
				EditorGUIUtility.fieldWidth = 10f;
				foldoutRect = EditorGUILayout.GetControlRect(false);
				EditorGUIUtility.fieldWidth = tmp;
			}
			if (info.Property.ValueEntry.ListLengthChangedFromPrefab)
			{
				GUIHelper.PopIsBoldLabel();
			}
			if (info.DropZone != null && DragAndDropManager.IsDragInProgress && !info.DropZone.IsAccepted)
			{
				GUIHelper.PopGUIEnabled();
			}
			GUILayout.FlexibleSpace();
			if (info.FilteredChildren.SearchFilter != null)
			{
				Rect rect = EditorGUILayout.GetControlRect(false).AddYMin(2f);
				if (UnityVersion.IsVersionOrGreater(2019, 3))
				{
					rect = rect.AddY(-2f);
				}
				string newTerm = SirenixEditorGUI.SearchField(rect, info.FilteredChildren.SearchFilter.SearchTerm, forceFocus: false, info.SearchFieldControlName);
				if (newTerm != info.FilteredChildren.SearchFilter.SearchTerm)
				{
					if (!string.IsNullOrEmpty(newTerm))
					{
						base.Property.State.Expanded = true;
					}
					info.FilteredChildren.SearchFilter.SearchTerm = newTerm;
					info.FilteredChildren.ScheduleUpdate();
				}
			}
			if (drawFoldout)
			{
				base.Property.State.Expanded = SirenixEditorGUI.Foldout(foldoutRect, base.Property.State.Expanded, info.Label ?? GUIContent.none);
			}
			else
			{
				base.Property.State.Expanded = true;
			}
			if (info.CustomListDrawerOptions.ShowItemCountHasValue ? info.CustomListDrawerOptions.ShowItemCount : info.ListConfig.ShowItemCount)
			{
				if (info.Property.ValueEntry.ValueState == PropertyValueState.CollectionLengthConflict)
				{
					GUILayout.Label(info.Count + " / " + info.CollectionResolver.MaxCollectionLength + " items", SirenixGUIStyles.CenteredGreyMiniLabel);
				}
				else if (info.FilteredChildren.IsCurrentlyFiltered)
				{
					GUILayout.Label(info.Count + " / " + info.Property.Children.Count + " items", EditorStyles.centeredGreyMiniLabel);
				}
				else
				{
					GUILayout.Label(info.IsEmpty ? "Empty" : (info.Count + " items"), SirenixGUIStyles.CenteredGreyMiniLabel);
				}
			}
			bool paging = !info.CustomListDrawerOptions.PagingHasValue || info.CustomListDrawerOptions.ShowPaging;
			bool hidePaging = (info.ListConfig.HidePagingWhileCollapsed && !base.Property.State.Expanded) || (info.ListConfig.HidePagingWhileOnlyOnePage && info.Count <= info.NumberOfItemsPerPage);
			int numberOfItemsPrPage = Math.Max(1, info.NumberOfItemsPerPage);
			int numberOfPages = Mathf.CeilToInt((float)info.Count / (float)numberOfItemsPrPage);
			int pageIndex = ((info.Count != 0) ? (info.StartIndex / numberOfItemsPrPage % info.Count) : 0);
			if (paging)
			{
				bool disablePaging = paging && !hidePaging && (DragAndDropManager.IsDragInProgress || info.ShowAllWhilePaging || !base.Property.State.Expanded);
				if (disablePaging)
				{
					GUIHelper.PushGUIEnabled(enabled: false);
				}
				if (!hidePaging)
				{
					if (pageIndex == 0)
					{
						GUIHelper.PushGUIEnabled(enabled: false);
					}
					if (SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleLeft, ignoreGUIEnabled: true))
					{
						if (Event.current.button == 0)
						{
							info.StartIndex -= numberOfItemsPrPage;
						}
						else
						{
							info.StartIndex = 0;
						}
					}
					if (pageIndex == 0)
					{
						GUIHelper.PopGUIEnabled();
					}
					int userPageIndex = EditorGUILayout.IntField((numberOfPages != 0) ? (pageIndex + 1) : 0, GUILayoutOptions.Width(10 + numberOfPages.ToString(CultureInfo.InvariantCulture).Length * 10)) - 1;
					if (pageIndex != userPageIndex)
					{
						info.StartIndex = userPageIndex * numberOfItemsPrPage;
					}
					GUILayout.Label("/ " + numberOfPages);
					if (pageIndex == numberOfPages - 1)
					{
						GUIHelper.PushGUIEnabled(enabled: false);
					}
					if (SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleRight, ignoreGUIEnabled: true))
					{
						if (Event.current.button == 0)
						{
							info.StartIndex += numberOfItemsPrPage;
						}
						else
						{
							info.StartIndex = numberOfItemsPrPage * numberOfPages;
						}
					}
					if (pageIndex == numberOfPages - 1)
					{
						GUIHelper.PopGUIEnabled();
					}
				}
				pageIndex = ((info.Count != 0) ? (info.StartIndex / numberOfItemsPrPage % info.Count) : 0);
				int newStartIndex = Mathf.Clamp(pageIndex * numberOfItemsPrPage, 0, Mathf.Max(0, info.Count - 1));
				if (newStartIndex != info.StartIndex)
				{
					info.StartIndex = newStartIndex;
					int newPageIndex = ((info.Count != 0) ? (info.StartIndex / numberOfItemsPrPage % info.Count) : 0);
					if (pageIndex != newPageIndex)
					{
						pageIndex = newPageIndex;
						info.StartIndex = Mathf.Clamp(pageIndex * numberOfItemsPrPage, 0, Mathf.Max(0, info.Count - 1));
					}
				}
				info.EndIndex = Mathf.Min(info.StartIndex + numberOfItemsPrPage, info.Count);
				if (disablePaging)
				{
					GUIHelper.PopGUIEnabled();
				}
			}
			else
			{
				info.StartIndex = 0;
				info.EndIndex = info.Count;
			}
			if (paging && !hidePaging && info.ListConfig.ShowExpandButton)
			{
				if (info.Count < 300)
				{
					if (SirenixEditorGUI.ToolbarButton(info.ShowAllWhilePaging ? EditorIcons.TriangleUp : EditorIcons.TriangleDown, ignoreGUIEnabled: true))
					{
						info.ShowAllWhilePaging = !info.ShowAllWhilePaging;
					}
				}
				else
				{
					info.ShowAllWhilePaging = false;
				}
			}
			if (!info.IsReadOnly && !info.HideAddButton)
			{
				if (OdinObjectSelector.IsReadyToClaim(info, CollectionDrawerId))
				{
					object[] claimedObjects = OdinObjectSelector.ClaimMultiple(info.Property.Tree.WeakTargets.Count);
					if (onValuesCreated != null)
					{
						onValuesCreated(claimedObjects);
						onValuesCreated = null;
					}
					else
					{
						info.CollectionResolver.QueueAdd(claimedObjects);
					}
				}
				superHackyAddFunctionWeSeriouslyNeedANewListDrawer = CollectionDrawerStaticInfo.NextCustomAddFunction;
				CollectionDrawerStaticInfo.NextCustomAddFunction = null;
				if (SirenixEditorGUI.ToolbarButton(SdfIconType.Plus))
				{
					StartCreatingValues(GUIHelper.GetCurrentLayoutRect());
				}
				info.JumpToNextPageOnAdd = paging && info.Count % numberOfItemsPrPage == 0 && pageIndex + 1 == numberOfPages;
			}
			if (info.OnTitleBarGUI != null && !info.OnTitleBarGUI.HasError)
			{
				info.OnTitleBarGUI.DoAction();
			}
			SirenixEditorGUI.EndHorizontalToolbar();
		}

		private void StartCreatingValues(Rect potentialPopupPosition)
		{
			if (superHackyAddFunctionWeSeriouslyNeedANewListDrawer != null)
			{
				superHackyAddFunctionWeSeriouslyNeedANewListDrawer();
				return;
			}
			if (info.GetCustomAddFunctionVoid != null && !info.GetCustomAddFunctionVoid.HasError)
			{
				info.GetCustomAddFunctionVoid.DoAction();
				UnityEngine.Object root = base.Property.SerializationRoot.ValueEntry.WeakValues[0] as UnityEngine.Object;
				if (root != null)
				{
					InspectorUtilities.RegisterUnityObjectDirty(root);
				}
				return;
			}
			object[] objs = new object[info.Property.ValueEntry.ValueCount];
			objs[0] = GetValueToAdd(0, out var wasFallback);
			if (wasFallback)
			{
				Type elementType = info.CollectionResolver.ElementType;
				if (elementType == typeof(Type))
				{
					TypeDrawerSettingsAttribute typeSelectorSettings = base.Property.GetAttribute<TypeDrawerSettingsAttribute>();
					List<Type> validTypes;
					if (typeSelectorSettings != null)
					{
						validTypes = ((!(typeSelectorSettings.BaseType != null)) ? TypeRegistry.GetValidTypesInCategory(AssemblyCategory.All) : TypeRegistry.GetInheritors(typeSelectorSettings.BaseType));
						for (int i = validTypes.Count - 1; i >= 0; i--)
						{
							if (!typeSelectorSettings.Filter.IsValidType(validTypes[i]))
							{
								validTypes.RemoveAt(i);
							}
						}
					}
					else
					{
						validTypes = TypeRegistry.GetValidTypesInCategory(AssemblyCategory.All);
					}
					TypeSelectorV2 selector = new TypeSelectorV2(validTypes);
					selector.SelectionConfirmed += delegate(IEnumerable<Type> types)
					{
						Type type = types.FirstOrDefault();
						if (type == typeof(TypeSelectorV2.TypeSelectorNoneValue))
						{
							type = null;
						}
						int count = info.Property.Tree.WeakTargets.Count;
						Type[] array = new Type[count];
						for (int j = 0; j < count; j++)
						{
							array[j] = type;
						}
						if (onValuesCreated != null)
						{
							Action<object[]> action = onValuesCreated;
							object[] obj = array;
							action(obj);
							onValuesCreated = null;
						}
						else
						{
							ICollectionResolver collectionResolver = info.CollectionResolver;
							object[] obj = array;
							collectionResolver.QueueAdd(obj);
						}
					};
					selector.ShowInPopup(potentialPopupPosition);
				}
				else
				{
					bool allowSceneObjects = InspectorPropertyInfoUtility.InspectorPropertySupportsAssigningSceneReferences(base.Property);
					ListDrawerConfigInfo key = info;
					int collectionDrawerId = CollectionDrawerId;
					bool disallowNullValues = !base.Property.ValueEntry.SerializationBackend.SupportsPolymorphism;
					OdinObjectSelector.Show(potentialPopupPosition, key, collectionDrawerId, null, elementType, allowSceneObjects, disallowNullValues, info.Property);
				}
			}
			else
			{
				for (int i2 = 1; i2 < objs.Length; i2++)
				{
					objs[i2] = GetValueToAdd(i2);
				}
				if (onValuesCreated != null)
				{
					onValuesCreated(objs);
					onValuesCreated = null;
				}
				else
				{
					info.CollectionResolver.QueueAdd(objs);
				}
			}
		}

		private void DrawItems()
		{
			if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
			{
				info.DraggingMousePosition = Event.current.mousePosition;
			}
			info.InsertAt = -1;
			int from = 0;
			int to = info.Count;
			if ((!info.CustomListDrawerOptions.PagingHasValue || info.CustomListDrawerOptions.ShowPaging) && !info.ShowAllWhilePaging)
			{
				from = Mathf.Clamp(info.StartIndex, 0, info.Count);
				to = Mathf.Clamp(info.EndIndex, 0, info.Count);
			}
			Color evenColor = SirenixGUIStyles.ListItemColorEven;
			Color oddColor = SirenixGUIStyles.ListItemColorOdd;
			bool drawEmptySpace = (info.DropZone != null && info.DropZone.IsBeingHovered) || info.IsDroppingUnityObjects;
			float height = ((!drawEmptySpace) ? 0f : (info.IsDroppingUnityObjects ? 16f : DragAndDropManager.CurrentDraggingHandle.Rect.height));
			Rect rect = SirenixEditorGUI.BeginVerticalList(true, true);
			int i = 0;
			int j = from;
			int k = from;
			for (; j < to; j++)
			{
				DragHandle dragHandle = BeginDragHandle(j);
				if (drawEmptySpace)
				{
					Rect topHalf = dragHandle.Rect;
					topHalf.height /= 2f;
					if (topHalf.Contains(info.DraggingMousePosition) || (topHalf.y > info.DraggingMousePosition.y && i == 0))
					{
						GUILayout.Space(height);
						drawEmptySpace = false;
						info.InsertAt = k;
					}
				}
				if (!dragHandle.IsDragging)
				{
					k++;
					DrawItem(info.FilteredChildren[j], dragHandle, evenColor, oddColor);
				}
				else
				{
					if (Event.current.type == EventType.Repaint && info.InsertAt != j)
					{
						int localJ = j;
						ListDrawerConfigInfo localInfo = info;
						Vector2 p = GUIUtility.GUIToScreenPoint(new Vector2(dragHandle.Rect.x, dragHandle.Rect.y));
						Rect r = dragHandle.Rect;
						base.Property.Tree.DelayAction(delegate
						{
							p = GUIUtility.ScreenToGUIPoint(p);
							r.x = p.x;
							r.y = p.y;
							_ = localInfo.InsertAt;
							_ = localJ;
						});
					}
					CollectionDrawerStaticInfo.DelayedGUIDrawer.Begin(dragHandle.Rect.width, dragHandle.Rect.height);
					DragAndDropManager.AllowDrop = false;
					DrawItem(info.FilteredChildren[j], dragHandle, evenColor, oddColor);
					DragAndDropManager.AllowDrop = true;
					CollectionDrawerStaticInfo.DelayedGUIDrawer.End();
				}
				if (drawEmptySpace)
				{
					Rect bottomHalf = dragHandle.Rect;
					bottomHalf.height /= 2f;
					bottomHalf.y += bottomHalf.height;
					if (bottomHalf.Contains(info.DraggingMousePosition) || (bottomHalf.yMax < info.DraggingMousePosition.y && j + 1 == to))
					{
						Rect targetRect = GUILayoutUtility.GetRect(0f, height);
						drawEmptySpace = false;
						info.InsertAt = Mathf.Min(k, to);
					}
				}
				EndDragHandle();
				i++;
			}
			if (drawEmptySpace)
			{
				Rect targetRect2 = GUILayoutUtility.GetRect(0f, height);
				info.InsertAt = ((info.DraggingMousePosition.y > rect.center.y) ? to : from);
			}
			if (to == info.FilteredChildren.Count && info.Property.ValueEntry.ValueState == PropertyValueState.CollectionLengthConflict)
			{
				SirenixEditorGUI.BeginListItem(false, null);
				GUILayout.Label(GUIHelper.TempContent("------"), EditorStyles.centeredGreyMiniLabel);
				SirenixEditorGUI.EndListItem();
			}
			SirenixEditorGUI.EndVerticalList();
		}

		private void EndDragHandle()
		{
			DragHandle handle = DragAndDropManager.EndDragHandle();
			if (!handle.IsDragging)
			{
				return;
			}
			info.Property.Tree.DelayAction(delegate
			{
				if (DragAndDropManager.CurrentDraggingHandle != null)
				{
					CollectionDrawerStaticInfo.DelayedGUIDrawer.Draw(info.DraggingMousePosition - DragAndDropManager.CurrentDraggingHandle.MouseDownPostionOffset);
				}
			});
		}

		private object GetKey(InspectorProperty prop)
		{
			return prop;
		}

		private DragHandle BeginDragHandle(int j)
		{
			InspectorProperty child = info.FilteredChildren[j];
			DragHandle dragHandle = DragAndDropManager.BeginDragHandle(GetKey(child), child.ValueEntry.WeakSmartValue, info.IsReadOnly ? DragAndDropMethods.Reference : DragAndDropMethods.Move);
			dragHandle.Enabled = info.Draggable;
			if (dragHandle.OnDragStarted)
			{
				CollectionDrawerStaticInfo.CurrentDroppingPropertyInfo = null;
				CollectionDrawerStaticInfo.CurrentDraggingPropertyInfo = info.FilteredChildren[j];
				dragHandle.OnDragFinnished = delegate(DropEvents dropEvent)
				{
					if (dropEvent == DropEvents.Moved)
					{
						if (dragHandle.IsCrossWindowDrag || (CollectionDrawerStaticInfo.CurrentDroppingPropertyInfo != null && CollectionDrawerStaticInfo.CurrentDroppingPropertyInfo.Tree != info.Property.Tree))
						{
							GUIHelper.RequestRepaint();
							UnityEditorEventUtility.EditorApplication_delayCall += delegate
							{
								info.OrderedCollectionResolver.QueueRemoveAt(j);
							};
						}
						else
						{
							info.OrderedCollectionResolver.QueueRemoveAt(j);
						}
					}
					CollectionDrawerStaticInfo.CurrentDraggingPropertyInfo = null;
				};
			}
			return dragHandle;
		}

		private Rect DrawItem(InspectorProperty itemProperty, DragHandle dragHandle, Color evenColor, Color oddColor)
		{
			int index = itemProperty.Index;
			PropertyContext<ListItemInfo> listItemInfo = itemProperty.Context.GetGlobal<ListItemInfo>("listItemInfo");
			Color color = ((index % 2 == 0) ? evenColor : oddColor);
			if (info.ElementColor != null && !info.ElementColor.HasError)
			{
				info.ElementColor.Context.NamedValues.Set("index", index);
				info.ElementColor.Context.NamedValues.Set("defaultColor", color);
				color = info.ElementColor.GetValue();
			}
			Rect rect = SirenixEditorGUI.BeginListItem(allowHover: false, info.ListItemStyle, color, color, color, color, listItemOptions);
			if (Event.current.type == EventType.Repaint && !info.BaseIsReadOnly)
			{
				listItemInfo.Value.Width = rect.width;
				dragHandle.DragHandleRect = new Rect(rect.x + 4f, rect.y, 20f, rect.height);
				listItemInfo.Value.DragHandleRect = new Rect(rect.x + 4f, rect.y + 2f + (float)(((int)rect.height - 23) / 2), 20f, 20f);
				listItemInfo.Value.RemoveBtnRect = new Rect(listItemInfo.Value.DragHandleRect.x + rect.width - 22f, listItemInfo.Value.DragHandleRect.y + 1f, 14f, 14f);
				if (info.Draggable)
				{
					Color tmp = GUI.color;
					GUI.color *= new Color(1f, 1f, 1f, 0.4f);
					GUI.DrawTexture(listItemInfo.Value.DragHandleRect, EditorIcons.List.Inactive);
					GUI.color = tmp;
				}
			}
			GUIHelper.PushHierarchyMode(hierarchyMode: false);
			GUIContent label = null;
			if (info.CustomListDrawerOptions.ShowIndexLabelsHasValue)
			{
				if (info.CustomListDrawerOptions.ShowIndexLabels)
				{
					label = new GUIContent(index.ToString());
				}
			}
			else if (info.ListConfig.ShowIndexLabels)
			{
				label = new GUIContent(index.ToString());
			}
			if (info.GetListElementLabelText != null)
			{
				object value = itemProperty.ValueEntry.WeakSmartValue;
				if (value == null)
				{
					if (label == null)
					{
						label = new GUIContent("Null");
					}
					else
					{
						label.text += " : Null";
					}
				}
				else
				{
					label = label ?? new GUIContent("");
					object text = info.GetListElementLabelText(value, itemProperty);
					if (text != null)
					{
						if (!string.IsNullOrEmpty(label.text))
						{
							label.text += " : ";
						}
						label.text += text.ToString();
					}
				}
			}
			itemProperty.Update();
			if (info.OnBeginListElementGUI != null && !info.OnBeginListElementGUI.HasError)
			{
				info.OnBeginListElementGUI.Context.NamedValues.Set("index", index);
				info.OnBeginListElementGUI.DoAction();
			}
			if (itemProperty.PrefabModificationBarSourceRectOverride == default(Rect))
			{
				Rect copy = rect;
				copy.x += 2f;
				copy.height -= 1f;
				itemProperty.PrefabModificationBarSourceRectOverride = copy;
			}
			itemProperty.Draw(label);
			if (info.OnEndListElementGUI != null && !info.OnEndListElementGUI.HasError)
			{
				info.OnEndListElementGUI.Context.NamedValues.Set("index", index);
				info.OnEndListElementGUI.DoAction();
			}
			GUIHelper.PopHierarchyMode();
			if (!info.BaseIsReadOnly && !info.HideRemoveButton && SirenixEditorGUI.SDFIconButton(listItemInfo.Value.RemoveBtnRect, null, SdfIconType.X, IconAlignment.LeftOfText, SirenixGUIStyles.IconButton, selected: false))
			{
				if (info.OrderedCollectionResolver != null)
				{
					if (index >= 0)
					{
						info.RemoveAt = index;
					}
				}
				else
				{
					object[] values = new object[itemProperty.ValueEntry.ValueCount];
					for (int i = 0; i < values.Length; i++)
					{
						values[i] = itemProperty.ValueEntry.WeakValues[i];
					}
					info.RemoveValues = values;
				}
			}
			SirenixEditorGUI.EndListItem();
			return rect;
		}
	}
}

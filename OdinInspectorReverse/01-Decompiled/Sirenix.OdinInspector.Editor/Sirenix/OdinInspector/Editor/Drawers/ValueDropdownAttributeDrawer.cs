using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.ValueDropdownAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.ValueDropdownAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ValueDropdownItem`1" />
	/// <summary>
	/// Draws the property.
	/// </summary>
	[DrawerPriority(0.0, 0.0, 2002.0)]
	public sealed class ValueDropdownAttributeDrawer : OdinAttributeDrawer<ValueDropdownAttribute>
	{
		private string error;

		private GUIContent label;

		private bool isList;

		private bool isListElement;

		private Func<IEnumerable<ValueDropdownItem>> getValues;

		private Func<IEnumerable<object>> getSelection;

		private IEnumerable<object> result;

		private bool enableMultiSelect;

		private Dictionary<object, string> nameLookup;

		private ValueResolver<object> rawGetter;

		private LocalPersistentContext<bool> isToggled;

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			rawGetter = ValueResolver.Get<object>(base.Property, base.Attribute.ValuesGetter);
			isToggled = this.GetPersistentValue("Toggled", SirenixEditorGUI.ExpandFoldoutByDefault);
			error = rawGetter.ErrorMessage;
			isList = base.Property.ChildResolver is ICollectionResolver;
			isListElement = base.Property.Parent != null && base.Property.Parent.ChildResolver is ICollectionResolver;
			getSelection = () => base.Property.ValueEntry.WeakValues.Cast<object>();
			getValues = delegate
			{
				object value = rawGetter.GetValue();
				return (value != null) ? (from object x in value as IEnumerable
					where x != null
					select x).Select(delegate(object x)
				{
					if (x is ValueDropdownItem)
					{
						return (ValueDropdownItem)x;
					}
					if (x is IValueDropdownItem)
					{
						IValueDropdownItem valueDropdownItem = x as IValueDropdownItem;
						return new ValueDropdownItem(valueDropdownItem.GetText(), valueDropdownItem.GetValue());
					}
					return new ValueDropdownItem(null, x);
				}) : null;
			};
			ReloadDropdownCollections();
		}

		private void ReloadDropdownCollections()
		{
			if (error != null)
			{
				return;
			}
			object first = null;
			object value = rawGetter.GetValue();
			if (value != null)
			{
				first = (value as IEnumerable).Cast<object>().FirstOrDefault();
			}
			if (first is IValueDropdownItem)
			{
				IEnumerable<ValueDropdownItem> vals = getValues();
				nameLookup = new Dictionary<object, string>(new IValueDropdownEqualityComparer(isTypeLookup: false));
				{
					foreach (ValueDropdownItem item in vals)
					{
						nameLookup[item] = item.Text;
					}
					return;
				}
			}
			nameLookup = null;
		}

		private static IEnumerable<ValueDropdownItem> ToValueDropdowns(IEnumerable<object> query)
		{
			return query.Select(delegate(object x)
			{
				if (x is ValueDropdownItem)
				{
					return (ValueDropdownItem)x;
				}
				if (x is IValueDropdownItem)
				{
					IValueDropdownItem valueDropdownItem = x as IValueDropdownItem;
					return new ValueDropdownItem(valueDropdownItem.GetText(), valueDropdownItem.GetValue());
				}
				return new ValueDropdownItem(null, x);
			});
		}

		/// <summary>
		/// Draws the property with GUILayout support. This method is called by DrawPropertyImplementation if the GUICallType is set to GUILayout, which is the default.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			this.label = label;
			if (base.Property.ValueEntry == null)
			{
				CallNextDrawer(label);
			}
			else if (error != null)
			{
				SirenixEditorGUI.MessageBox(error, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				CallNextDrawer(label);
			}
			else if (isList)
			{
				if (base.Attribute.DisableListAddButtonBehaviour)
				{
					CallNextDrawer(label);
					return;
				}
				Action oldSelector = CollectionDrawerStaticInfo.NextCustomAddFunction;
				CollectionDrawerStaticInfo.NextCustomAddFunction = OpenSelector;
				CallNextDrawer(label);
				if (result != null)
				{
					AddResult(result);
					result = null;
				}
				CollectionDrawerStaticInfo.NextCustomAddFunction = oldSelector;
			}
			else if (base.Attribute.DrawDropdownForListElements || !isListElement)
			{
				DrawDropdown();
			}
			else
			{
				CallNextDrawer(label);
			}
		}

		private void AddResult(IEnumerable<object> query)
		{
			if (isList)
			{
				ICollectionResolver changer = base.Property.ChildResolver as ICollectionResolver;
				if (enableMultiSelect)
				{
					changer.QueueClear();
				}
				{
					foreach (object item in query)
					{
						object[] arr = new object[base.Property.ParentValues.Count];
						for (int i = 0; i < arr.Length; i++)
						{
							if (base.Attribute.CopyValues)
							{
								arr[i] = Sirenix.Serialization.SerializationUtility.CreateCopy(item);
							}
							else
							{
								arr[i] = item;
							}
						}
						changer.QueueAdd(arr);
					}
					return;
				}
			}
			object first = query.FirstOrDefault();
			for (int j = 0; j < base.Property.ValueEntry.WeakValues.Count; j++)
			{
				if (base.Attribute.CopyValues)
				{
					base.Property.ValueEntry.WeakValues[j] = Sirenix.Serialization.SerializationUtility.CreateCopy(first);
				}
				else
				{
					base.Property.ValueEntry.WeakValues[j] = first;
				}
			}
		}

		private void DrawDropdown()
		{
			IEnumerable<object> newResult = null;
			if (base.Attribute.AppendNextDrawer && !isList)
			{
				GUILayout.BeginHorizontal();
				float width = 15f;
				if (label != null)
				{
					width += GUIHelper.BetterLabelWidth;
				}
				GUIContent t = GUIHelper.TempContent("");
				if (base.Property.Info.TypeOfValue == typeof(Type))
				{
					t.image = GUIHelper.GetAssetThumbnail(null, base.Property.ValueEntry.WeakSmartValue as Type, preferObjectPreviewOverFileIcon: false);
				}
				newResult = OdinSelector<object>.DrawSelectorDropdown(label, t, ShowSelector, !base.Attribute.OnlyChangeValueOnConfirm, GUIStyle.none, GUILayoutOptions.Width(width));
				if (Event.current.type == EventType.Repaint)
				{
					Rect btnRect = GUILayoutUtility.GetLastRect().AlignRight(15f);
					btnRect.y += 4f;
					SirenixGUIStyles.PaneOptions.Draw(btnRect, GUIContent.none, 0);
				}
				GUILayout.BeginVertical();
				bool disable = base.Attribute.DisableGUIInAppendedDrawer;
				if (disable)
				{
					GUIHelper.PushGUIEnabled(enabled: false);
				}
				CallNextDrawer(null);
				if (disable)
				{
					GUIHelper.PopGUIEnabled();
				}
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
			else
			{
				GUIContent valueName = GUIHelper.TempContent(GetCurrentValueName());
				if (base.Property.Info.TypeOfValue == typeof(Type))
				{
					valueName.image = GUIHelper.GetAssetThumbnail(null, base.Property.ValueEntry.WeakSmartValue as Type, preferObjectPreviewOverFileIcon: false);
				}
				if (!base.Attribute.HideChildProperties && base.Property.Children.Count > 0)
				{
					isToggled.Value = SirenixEditorGUI.Foldout(isToggled.Value, label, out var valRect);
					newResult = OdinSelector<object>.DrawSelectorDropdown(valRect, valueName, ShowSelector, !base.Attribute.OnlyChangeValueOnConfirm);
					if (SirenixEditorGUI.BeginFadeGroup(this, isToggled.Value))
					{
						EditorGUI.indentLevel++;
						for (int i = 0; i < base.Property.Children.Count; i++)
						{
							InspectorProperty child = base.Property.Children[i];
							child.Draw(child.Label);
						}
						EditorGUI.indentLevel--;
					}
					SirenixEditorGUI.EndFadeGroup();
				}
				else
				{
					newResult = OdinSelector<object>.DrawSelectorDropdown(label, valueName, ShowSelector, !base.Attribute.OnlyChangeValueOnConfirm, null);
				}
			}
			if (newResult != null && newResult.Any())
			{
				AddResult(newResult);
			}
		}

		private void OpenSelector()
		{
			ReloadDropdownCollections();
			UnityShims.Rect.Ctor(out var rect, Event.current.mousePosition, Vector2.zero);
			OdinSelector<object> selector = ShowSelector(rect);
			selector.SelectionConfirmed += delegate(IEnumerable<object> x)
			{
				result = x;
			};
		}

		private OdinSelector<object> ShowSelector(Rect rect)
		{
			GenericSelector<object> selector = CreateSelector();
			rect.x = (int)rect.x;
			rect.y = (int)rect.y;
			rect.width = (int)rect.width;
			rect.height = (int)rect.height;
			if (base.Attribute.AppendNextDrawer && !isList)
			{
				rect.xMax = GUIHelper.GetCurrentLayoutRect().xMax;
			}
			selector.ShowInPopup(rect, new Vector2(base.Attribute.DropdownWidth, base.Attribute.DropdownHeight));
			return selector;
		}

		private GenericSelector<object> CreateSelector()
		{
			bool isUniqueList = base.Attribute.IsUniqueList;
			IEnumerable<ValueDropdownItem> query = getValues() ?? Enumerable.Empty<ValueDropdownItem>();
			if (query.Any())
			{
				if ((isList && base.Attribute.ExcludeExistingValuesInList) || (isListElement && isUniqueList))
				{
					List<ValueDropdownItem> list = query.ToList();
					InspectorProperty listProperty = base.Property.FindParent((InspectorProperty x) => x.ChildResolver is ICollectionResolver, includeSelf: true);
					IValueDropdownEqualityComparer comparer = new IValueDropdownEqualityComparer(isTypeLookup: false);
					listProperty.ValueEntry.WeakValues.Cast<IEnumerable>().SelectMany((IEnumerable x) => x.Cast<object>()).ForEach(delegate(object x)
					{
						list.RemoveAll((ValueDropdownItem c) => comparer.Equals(c, x));
					});
					query = list;
				}
				if (nameLookup != null)
				{
					foreach (ValueDropdownItem item in query)
					{
						if (item.Value != null)
						{
							nameLookup[item.Value] = item.Text;
						}
					}
				}
			}
			bool enableSearch = base.Attribute.NumberOfItemsBeforeEnablingSearch == 0 || (query != null && query.Take(base.Attribute.NumberOfItemsBeforeEnablingSearch).Count() == base.Attribute.NumberOfItemsBeforeEnablingSearch);
			GenericSelector<object> selector = new GenericSelector<object>(base.Attribute.DropdownTitle, supportsMultiSelect: false, query.Select((ValueDropdownItem x) => new GenericSelectorItem<object>(x.Text, x.Value)));
			enableMultiSelect = isList && isUniqueList && !base.Attribute.ExcludeExistingValuesInList;
			if (base.Attribute.FlattenTreeView)
			{
				selector.FlattenedTree = true;
			}
			if (isList && !base.Attribute.ExcludeExistingValuesInList && isUniqueList)
			{
				selector.CheckboxToggle = true;
			}
			else if (!base.Attribute.DoubleClickToConfirm && !enableMultiSelect)
			{
				selector.EnableSingleClickToSelect();
			}
			if (isList && enableMultiSelect)
			{
				selector.SelectionTree.Selection.SupportsMultiSelect = true;
				selector.DrawConfirmSelectionButton = true;
			}
			selector.SelectionTree.Config.DrawSearchToolbar = enableSearch;
			IEnumerable<object> selection = Enumerable.Empty<object>();
			if (!isList)
			{
				selection = getSelection();
			}
			else if (enableMultiSelect)
			{
				selection = getSelection().SelectMany((object x) => (x as IEnumerable).Cast<object>());
			}
			selector.SetSelection(selection);
			selector.SelectionTree.EnumerateTree().AddThumbnailIcons(preferAssetPreviewAsIcon: true);
			if (base.Attribute.ExpandAllMenuItems)
			{
				selector.SelectionTree.EnumerateTree(delegate(OdinMenuItem x)
				{
					x.Toggled = true;
				});
			}
			if (base.Attribute.SortDropdownItems)
			{
				selector.SelectionTree.SortMenuItemsByName();
			}
			return selector;
		}

		private string GetCurrentValueName()
		{
			if (!EditorGUI.showMixedValue)
			{
				object weakValue = base.Property.ValueEntry.WeakSmartValue;
				string name = null;
				if (nameLookup != null && weakValue != null)
				{
					nameLookup.TryGetValue(weakValue, out name);
				}
				return new GenericSelectorItem<object>(name, weakValue).GetNiceName();
			}
			return "—";
		}
	}
}

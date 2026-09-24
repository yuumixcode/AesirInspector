using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	[DrawerPriority(0.0, 0.0, 2002.0)]
	public sealed class AssetSelectorAttributeDrawer<T> : OdinAttributeDrawer<AssetSelectorAttribute, T>
	{
		private GUIContent label;

		private bool isList;

		private bool isListElement;

		private Func<IEnumerable<ValueDropdownItem>> getValues;

		private Func<IEnumerable<object>> getSelection;

		private Type elementOrBaseType;

		private bool isString;

		private IEnumerable<object> result;

		private bool enableMultiSelect;

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			isList = base.Property.ChildResolver is IOrderedCollectionResolver;
			isListElement = base.Property.Parent != null && base.Property.Parent.ChildResolver is IOrderedCollectionResolver;
			getSelection = () => base.Property.ValueEntry.WeakValues.Cast<object>();
			elementOrBaseType = (isList ? (base.Property.ChildResolver as IOrderedCollectionResolver).ElementType : base.Property.ValueEntry.BaseValueType);
			isString = elementOrBaseType == typeof(string);
			getValues = delegate
			{
				string text = base.Attribute.Filter ?? "";
				if (string.IsNullOrEmpty(text) && !typeof(Component).IsAssignableFrom(elementOrBaseType) && !elementOrBaseType.IsInterface)
				{
					text = "t:" + elementOrBaseType.Name;
				}
				string[] source = AssetDatabase.FindAssets(text, base.Attribute.SearchInFolders ?? new string[0]);
				return from x in source.Select((string x) => AssetDatabase.GUIDToAssetPath(x)).Distinct().SelectMany(delegate(string x)
					{
						IEnumerable<UnityEngine.Object> source2 = ((!x.EndsWith(".unity", StringComparison.InvariantCultureIgnoreCase) && !x.EndsWith(".scene", StringComparison.InvariantCultureIgnoreCase)) ? AssetDatabase.LoadAllAssetsAtPath(x) : Enumerable.Repeat(AssetDatabase.LoadAssetAtPath(x, typeof(UnityEngine.Object)), 1));
						return from obj in source2
							where obj != null && elementOrBaseType.IsAssignableFrom(obj.GetType())
							select new
							{
								o = obj,
								p = x
							};
					})
					select new ValueDropdownItem
					{
						Text = x.p + (AssetDatabase.IsMainAsset(x.o) ? "" : ("/" + x.o.name)),
						Value = (isString ? ((object)x.p) : ((object)x.o))
					};
			};
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
			else if (isList)
			{
				if (base.Attribute.DisableListAddButtonBehaviour)
				{
					CallNextDrawer(label);
					return;
				}
				CollectionDrawerStaticInfo.NextCustomAddFunction = OpenSelector;
				CallNextDrawer(label);
				if (result != null)
				{
					AddResult(result);
					result = null;
				}
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
				IOrderedCollectionResolver changer = base.Property.ChildResolver as IOrderedCollectionResolver;
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
							arr[i] = item;
						}
						changer.QueueAdd(arr);
					}
					return;
				}
			}
			object first = query.FirstOrDefault();
			for (int j = 0; j < base.Property.ValueEntry.WeakValues.Count; j++)
			{
				base.Property.ValueEntry.WeakValues[j] = first;
			}
		}

		private void DrawDropdown()
		{
			IEnumerable<object> newResult = null;
			if (!isList)
			{
				GUILayout.BeginHorizontal();
				float width = 15f;
				if (label != null)
				{
					width += GUIHelper.BetterLabelWidth;
				}
				newResult = OdinSelector<object>.DrawSelectorDropdown(label, GUIContent.none, ShowSelector, GUIStyle.none, GUILayoutOptions.Width(width));
				if (Event.current.type == EventType.Repaint)
				{
					Rect btnRect = GUILayoutUtility.GetLastRect().AlignRight(15f);
					btnRect.y += 4f;
					SirenixGUIStyles.PaneOptions.Draw(btnRect, GUIContent.none, 0);
				}
				GUILayout.BeginVertical();
				CallNextDrawer(null);
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
			else
			{
				string valueName = GetCurrentValueName();
				newResult = OdinSelector<object>.DrawSelectorDropdown(label, valueName, ShowSelector, null);
			}
			if (newResult != null && newResult.Any())
			{
				AddResult(newResult);
			}
		}

		private void OpenSelector()
		{
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
			if (!isList)
			{
				rect.xMax = GUIHelper.GetCurrentLayoutRect().xMax;
			}
			selector.ShowInPopup(rect, new Vector2(base.Attribute.DropdownWidth, base.Attribute.DropdownHeight));
			return selector;
		}

		private GenericSelector<object> CreateSelector()
		{
			base.Attribute.IsUniqueList = base.Attribute.IsUniqueList || base.Attribute.ExcludeExistingValuesInList;
			IEnumerable<ValueDropdownItem> query = getValues() ?? Enumerable.Empty<ValueDropdownItem>();
			if (query.Any() && ((isList && base.Attribute.ExcludeExistingValuesInList) || (isListElement && base.Attribute.IsUniqueList)))
			{
				List<ValueDropdownItem> list = query.ToList();
				InspectorProperty listProperty = base.Property.FindParent((InspectorProperty x) => x.ChildResolver is IOrderedCollectionResolver, includeSelf: true);
				IValueDropdownEqualityComparer comparer = new IValueDropdownEqualityComparer(isTypeLookup: false);
				listProperty.ValueEntry.WeakValues.Cast<IEnumerable>().SelectMany((IEnumerable x) => x.Cast<object>()).ForEach(delegate(object x)
				{
					list.RemoveAll((ValueDropdownItem c) => comparer.Equals(c, x));
				});
				query = list;
			}
			GenericSelector<object> selector = new GenericSelector<object>(base.Attribute.DropdownTitle, supportsMultiSelect: false, query.Select((ValueDropdownItem x) => new GenericSelectorItem<object>(x.Text, x.Value)));
			enableMultiSelect = isList && base.Attribute.IsUniqueList && !base.Attribute.ExcludeExistingValuesInList;
			if (base.Attribute.FlattenTreeView)
			{
				selector.FlattenedTree = true;
			}
			if (isList && !base.Attribute.ExcludeExistingValuesInList && base.Attribute.IsUniqueList)
			{
				selector.CheckboxToggle = true;
			}
			else if (!enableMultiSelect)
			{
				selector.EnableSingleClickToSelect();
			}
			if (isList && enableMultiSelect)
			{
				selector.SelectionTree.Selection.SupportsMultiSelect = true;
				selector.DrawConfirmSelectionButton = true;
			}
			selector.SelectionTree.Config.DrawSearchToolbar = true;
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
			return selector;
		}

		private string GetCurrentValueName()
		{
			if (!EditorGUI.showMixedValue)
			{
				return base.Property.ValueEntry.WeakSmartValue?.ToString() ?? "";
			}
			return "—";
		}
	}
}

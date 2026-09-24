using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.TypeFilterAttribute" />.
	/// </summary>
	[DrawerPriority(0.0, 0.0, 2002.0)]
	public sealed class TypeFilterAttributeDrawer : OdinAttributeDrawer<TypeFilterAttribute>
	{
		private string error;

		private bool useSpecialListBehaviour;

		private Func<IEnumerable<ValueDropdownItem>> getValues;

		private Func<IEnumerable<object>> getSelection;

		private IEnumerable<object> result;

		private Dictionary<object, string> nameLookup;

		private ValueResolver<object> rawGetter;

		protected override bool CanDrawAttributeProperty(InspectorProperty property)
		{
			return property.ValueEntry != null;
		}

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			rawGetter = ValueResolver.Get<object>(base.Property, base.Attribute.FilterGetter);
			error = rawGetter.ErrorMessage;
			useSpecialListBehaviour = base.Property.ChildResolver is ICollectionResolver && !base.Attribute.DrawValueNormally;
			getSelection = () => base.Property.ValueEntry.WeakValues.Cast<object>();
			getValues = delegate
			{
				object value = rawGetter.GetValue();
				return (value != null) ? (from object x in rawGetter.GetValue() as IEnumerable
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
				first = (rawGetter.GetValue() as IEnumerable).Cast<object>().FirstOrDefault();
			}
			if (first is IValueDropdownItem)
			{
				IEnumerable<ValueDropdownItem> vals = getValues();
				nameLookup = new Dictionary<object, string>(new IValueDropdownEqualityComparer(isTypeLookup: true));
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
			if (base.Property.ValueEntry == null)
			{
				CallNextDrawer(label);
			}
			else if (error != null)
			{
				SirenixEditorGUI.MessageBox(error, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				CallNextDrawer(label);
			}
			else if (useSpecialListBehaviour)
			{
				CollectionDrawerStaticInfo.NextCustomAddFunction = OpenSelector;
				CallNextDrawer(label);
				if (result != null)
				{
					AddResult(result);
					result = null;
				}
				CollectionDrawerStaticInfo.NextCustomAddFunction = null;
			}
			else
			{
				DrawDropdown(label);
			}
		}

		private void AddResult(IEnumerable<object> query)
		{
			if (!query.Any())
			{
				return;
			}
			if (useSpecialListBehaviour)
			{
				ICollectionResolver changer = base.Property.ChildResolver as ICollectionResolver;
				{
					foreach (object item in query)
					{
						object[] arr = new object[base.Property.ParentValues.Count];
						for (int i = 0; i < arr.Length; i++)
						{
							Type type = item as Type;
							if (type != null)
							{
								arr[i] = CreateInstance(type);
							}
						}
						changer.QueueAdd(arr);
					}
					return;
				}
			}
			object first = query.FirstOrDefault();
			Type type2 = first as Type;
			for (int j = 0; j < base.Property.ValueEntry.WeakValues.Count; j++)
			{
				if (type2 != null)
				{
					base.Property.ValueEntry.WeakValues[j] = CreateInstance(type2);
				}
			}
		}

		private object CreateInstance(Type type)
		{
			if (base.Property.ValueEntry.SerializationBackend == SerializationBackend.Unity)
			{
				object value = UnitySerializationUtility.CreateDefaultUnityInitializedObject(type);
				if (value != null)
				{
					return value;
				}
			}
			if (type == typeof(string))
			{
				return "";
			}
			if (type.IsAbstract || type.IsInterface)
			{
				Debug.LogError("TypeFilter was asked to instantiate a value of type '" + type.GetNiceFullName() + "', but it is abstract or an interface and cannot be instantiated.");
				return null;
			}
			if (type.IsValueType || type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) != null)
			{
				return Activator.CreateInstance(type);
			}
			return FormatterServices.GetUninitializedObject(type);
		}

		private void DrawDropdown(GUIContent label)
		{
			EditorGUI.BeginChangeCheck();
			IEnumerable<object> newResult = null;
			string valueName = GetCurrentValueName();
			if (base.Attribute.DrawValueNormally)
			{
				newResult = OdinSelector<object>.DrawSelectorDropdown(label, valueName, ShowSelector, null);
				CallNextDrawer(label);
			}
			else if (base.Property.Children.Count > 0)
			{
				base.Property.State.Expanded = SirenixEditorGUI.Foldout(base.Property.State.Expanded, label, out var valRect);
				newResult = OdinSelector<object>.DrawSelectorDropdown(valRect, valueName, ShowSelector);
				if (SirenixEditorGUI.BeginFadeGroup(this, base.Property.State.Expanded))
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
				newResult = OdinSelector<object>.DrawSelectorDropdown(label, valueName, ShowSelector, null);
			}
			if (EditorGUI.EndChangeCheck() && newResult != null)
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
			selector.ShowInPopup(rect, new Vector2(0f, 0f));
			return selector;
		}

		private GenericSelector<object> CreateSelector()
		{
			IEnumerable<ValueDropdownItem> query = getValues();
			if (query == null)
			{
				query = Enumerable.Empty<ValueDropdownItem>();
			}
			bool enableSearch = query.Take(10).Count() == 10;
			GenericSelector<object> selector = new GenericSelector<object>(base.Attribute.DropdownTitle, supportsMultiSelect: false, query.Select((ValueDropdownItem x) => new GenericSelectorItem<object>(x.Text, x.Value)));
			selector.CheckboxToggle = false;
			selector.EnableSingleClickToSelect();
			selector.SelectionTree.Config.DrawSearchToolbar = enableSearch;
			IEnumerable<object> selection = Enumerable.Empty<object>();
			if (!useSpecialListBehaviour)
			{
				selection = getSelection();
			}
			selection = selection.Select((Func<object, object>)((object x) => x?.GetType()));
			selector.SetSelection(selection);
			selector.SelectionTree.EnumerateTree().AddThumbnailIcons(preferAssetPreviewAsIcon: true);
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
				if (weakValue != null)
				{
					weakValue = weakValue.GetType();
				}
				return new GenericSelectorItem<object>(name, weakValue).GetNiceName();
			}
			return "—";
		}
	}
}

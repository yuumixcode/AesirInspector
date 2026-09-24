using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities.Editor.Expressions;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Type property drawer
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[DrawerPriority(0.0, 0.0, 2001.0)]
	public class TypeDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems where T : Type
	{
		private TypeDrawerSettingsAttribute settings;

		private static readonly TwoWaySerializationBinder Binder = new DefaultSerializationBinder();

		public string TypeNameTemp;

		public bool IsValid = true;

		public string UniqueControlName;

		public bool WasFocusedControl;

		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			IPropertyValueEntry<T> entry = property.ValueEntry as IPropertyValueEntry<T>;
			if (entry.IsEditable)
			{
				Rect rect = entry.Property.LastDrawnValueRect;
				genericMenu.AddItem(new GUIContent("Change Type"), on: false, delegate
				{
					base.Property.Tree.DelayActionUntilRepaint(delegate
					{
						ShowSelectorInPopup(rect);
					});
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Change Type"));
			}
		}

		protected override void Initialize()
		{
			UniqueControlName = Guid.NewGuid().ToString();
			settings = base.Property.GetAttribute<TypeDrawerSettingsAttribute>();
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> entry = base.ValueEntry;
			if (!IsValid)
			{
				GUIHelper.PushColor(Color.red);
			}
			GUI.SetNextControlName(UniqueControlName);
			Rect rect = EditorGUILayout.GetControlRect();
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, label);
			}
			Rect fieldRect = rect;
			Rect dropdownRect = rect.AlignRight(18f);
			EditorGUIUtility.AddCursorRect(dropdownRect, MouseCursor.Arrow);
			if (GUI.Button(dropdownRect, GUIContent.none, GUIStyle.none))
			{
				ShowSelectorInPopup(rect);
			}
			if (Event.current.type == EventType.Layout)
			{
				TypeNameTemp = ((entry.SmartValue != null) ? Binder.BindToName(entry.SmartValue) : null);
			}
			EditorGUI.BeginChangeCheck();
			TypeNameTemp = SirenixEditorFields.DelayedTextField(fieldRect, TypeNameTemp);
			EditorIcons.TriangleDown.Draw(dropdownRect);
			if (!IsValid)
			{
				GUIHelper.PopColor();
			}
			bool isFocused = GUI.GetNameOfFocusedControl() == UniqueControlName;
			bool defocused = false;
			if (isFocused != WasFocusedControl)
			{
				defocused = !isFocused;
				WasFocusedControl = isFocused;
			}
			if (EditorGUI.EndChangeCheck())
			{
				if (TypeNameTemp == null || string.IsNullOrEmpty(TypeNameTemp.Trim()))
				{
					entry.SmartValue = null;
					IsValid = true;
				}
				else
				{
					Type type = Binder.BindToType(TypeNameTemp);
					if (type == null)
					{
						type = AssemblyUtilities.GetTypeByCachedFullName(TypeNameTemp);
					}
					if (type == null)
					{
						ExpressionUtility.TryParseTypeNameAsCSharpIdentifier(TypeNameTemp, out type);
					}
					if (type == null)
					{
						IsValid = false;
					}
					else
					{
						entry.WeakSmartValue = type;
						IsValid = true;
					}
				}
			}
			if (defocused)
			{
				TypeNameTemp = ((entry.SmartValue == null) ? "" : Binder.BindToName(entry.SmartValue));
				IsValid = true;
			}
		}

		private void ShowSelectorInPopup(Rect position)
		{
			IPropertyValueEntry<T> entry = base.Property.ValueEntry as IPropertyValueEntry<T>;
			List<Type> types;
			if (GlobalConfig<GeneralDrawerConfig>.Instance.useOldTypeSelector || settings == null)
			{
				types = TypeRegistry.GetValidTypesInCategory(AssemblyCategory.All);
			}
			else
			{
				types = ((!(settings.BaseType != null)) ? TypeRegistry.GetValidTypesInCategory(AssemblyCategory.All) : TypeRegistry.GetInheritors(settings.BaseType));
				for (int i = types.Count - 1; i >= 0; i--)
				{
					if (!settings.Filter.IsValidType(types[i]))
					{
						types.RemoveAt(i);
					}
				}
			}
			List<Type> types2 = types;
			bool? showNoneItem = true;
			InspectorProperty inspectorProperty = base.Property;
			OdinSelector<Type> selector = TypeSelectorHandler_WILL_BE_DEPRECATED.InstantiateSelector(types2, supportsMultiSelect: false, null, null, showHidden: false, null, showNoneItem, inspectorProperty);
			selector.SelectionConfirmed += delegate(IEnumerable<Type> t)
			{
				Type type = t.FirstOrDefault();
				if (type == typeof(TypeSelectorV2.TypeSelectorNoneValue))
				{
					type = null;
				}
				entry.Property.Tree.DelayAction(delegate
				{
					entry.WeakSmartValue = type;
					IsValid = true;
					entry.ApplyChanges();
				});
			};
			selector.SetSelection(entry.SmartValue);
			selector.ShowInPopup(position);
		}
	}
}

using System;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Evaluates all strings, enums and primitive types and ensures EditorGUI.showMixedValue is true if there are any value conflicts in the current selection.
	/// </summary>
	[DrawerPriority(0.5, 0.0, 0.0)]
	[AllowGUIEnabledForReadonly]
	public sealed class PrimitiveValueConflictDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems
	{
		/// <summary>
		/// Sets the drawer to only be evaluated on primitive types, strings and enums.
		/// </summary>
		public override bool CanDrawTypeFilter(Type type)
		{
			if (!type.IsPrimitive && !(type == typeof(string)))
			{
				return type.IsEnum;
			}
			return true;
		}

		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			return property.Tree.WeakTargets.Count > 1;
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> entry = base.ValueEntry;
			if (entry.ValueState == PropertyValueState.PrimitiveValueConflict)
			{
				GUI.changed = false;
				EditorGUI.showMixedValue = true;
				CallNextDrawer(label);
				if (GUI.changed)
				{
					for (int i = 0; i < entry.ValueCount; i++)
					{
						entry.Values[i] = entry.SmartValue;
					}
				}
				EditorGUI.showMixedValue = false;
			}
			else
			{
				EditorGUI.showMixedValue = false;
				CallNextDrawer(label);
			}
		}

		void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			if (property.ValueEntry.ValueState != PropertyValueState.PrimitiveValueConflict)
			{
				return;
			}
			PropertyTree tree = property.Tree;
			if (!typeof(UnityEngine.Object).IsAssignableFrom(tree.TargetType))
			{
				return;
			}
			for (int i = 0; i < tree.WeakTargets.Count; i++)
			{
				object value = property.ValueEntry.WeakValues[i];
				string valueString = ((value == null) ? "null" : value.ToString());
				string contentString = "Resolve value conflict with.../" + ((UnityEngine.Object)tree.WeakTargets[i]).name + " (" + valueString + ")";
				genericMenu.AddItem(new GUIContent(contentString), on: false, delegate
				{
					property.Tree.DelayActionUntilRepaint(delegate
					{
						for (int j = 0; j < property.ValueEntry.WeakValues.Count; j++)
						{
							property.ValueEntry.WeakValues[j] = value;
						}
					});
				});
			}
		}
	}
}

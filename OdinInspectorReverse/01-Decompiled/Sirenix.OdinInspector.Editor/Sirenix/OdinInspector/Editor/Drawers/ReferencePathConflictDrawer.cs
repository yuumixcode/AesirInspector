using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties with a <see cref="F:Sirenix.OdinInspector.Editor.PropertyValueState.ReferencePathConflict" /> set.
	/// </summary>
	[AllowGUIEnabledForReadonly]
	[DrawerPriority(0.5, 0.0, 0.0)]
	public sealed class ReferencePathConflictDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems where T : class
	{
		private static readonly bool IsUnityObject = typeof(Object).IsAssignableFrom(typeof(T));

		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			if (!property.IsTreeRoot)
			{
				return property.Tree.WeakTargets.Count > 1;
			}
			return false;
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> entry = base.ValueEntry;
			if (entry.ValueState == PropertyValueState.ReferencePathConflict)
			{
				if (IsUnityObject)
				{
					bool prev = EditorGUI.showMixedValue;
					EditorGUI.showMixedValue = true;
					CallNextDrawer(label);
					EditorGUI.showMixedValue = prev;
				}
				else
				{
					EditorGUILayout.LabelField(label, new GUIContent("Reference path conflict... (right-click to resolve)"));
				}
			}
			else
			{
				CallNextDrawer(label);
			}
		}

		void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			if (property.ValueEntry.ValueState != PropertyValueState.ReferencePathConflict)
			{
				return;
			}
			PropertyTree tree = property.Tree;
			if (!typeof(Object).IsAssignableFrom(tree.TargetType))
			{
				return;
			}
			for (int i = 0; i < tree.WeakTargets.Count; i++)
			{
				object value = property.ValueEntry.WeakValues[i];
				string valueString = ((value == null) ? "null" : value.GetType().GetNiceName());
				tree.ObjectIsReferenced(value, out var path);
				string contentString = "Resolve reference path conflict with.../" + ((Object)tree.WeakTargets[i]).name + " -> " + path + " (" + valueString + ")";
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

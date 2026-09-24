using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// <para>
	/// When multiple objects are selected and inspected, this his drawer ensures UnityEditor.EditorGUI.showMixedValue
	/// gets set to true if there are any conflicts in the selection for any given property.
	/// Otherwise the next drawer is called.
	/// </para>
	/// <para>This drawer also implements <see cref="T:Sirenix.OdinInspector.Editor.IDefinesGenericMenuItems" /> and provides a right-click context menu item for resolving conflicts if any.</para>
	/// </summary>
	[DrawerPriority(0.5, 0.0, 0.0)]
	[AllowGUIEnabledForReadonly]
	public sealed class ReferenceValueConflictDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems where T : class
	{
		private bool allowSceneObjects;

		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			if (!property.IsTreeRoot)
			{
				return property.Tree.WeakTargets.Count > 1;
			}
			return false;
		}

		protected override void Initialize()
		{
			allowSceneObjects = InspectorPropertyInfoUtility.InspectorPropertySupportsAssigningSceneReferences(base.Property);
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> entry = base.ValueEntry;
			if (entry.ValueState == PropertyValueState.ReferenceValueConflict)
			{
				GUIHelper.PushGUIEnabled(GUI.enabled && entry.IsEditable);
				if (typeof(Object).IsAssignableFrom(entry.TypeOfValue))
				{
					bool prev = EditorGUI.showMixedValue;
					EditorGUI.showMixedValue = true;
					CallNextDrawer(label);
					EditorGUI.showMixedValue = prev;
				}
				else
				{
					Rect position = EditorGUILayout.GetControlRect();
					bool prev2 = EditorGUI.showMixedValue;
					EditorGUI.showMixedValue = true;
					OdinInternalEditorFields.PolymorphicFieldArgs polymorphicArgs = OdinInternalEditorFields.PolymorphicFieldArgs.CreateForProperty(base.Property, position, 0);
					polymorphicArgs.AllowSceneObjects = allowSceneObjects;
					if (GlobalConfig<GeneralDrawerConfig>.Instance.useOldPolymorphicField)
					{
						if (label != null)
						{
							position = EditorGUI.PrefixLabel(position, label);
						}
						polymorphicArgs.Rect = position;
						EditorGUI.BeginChangeCheck();
						object newValue = OdinInternalEditorFields.PolymorphicObjectField(in polymorphicArgs);
						if (EditorGUI.EndChangeCheck())
						{
							base.ValueEntry.Property.Tree.DelayActionUntilRepaint(delegate
							{
								base.ValueEntry.WeakValues[0] = newValue;
								for (int i = 1; i < base.ValueEntry.ValueCount; i++)
								{
									base.ValueEntry.WeakValues[i] = Sirenix.Serialization.SerializationUtility.CreateCopy(newValue);
								}
							});
						}
					}
					else
					{
						polymorphicArgs.Label = label;
						OdinInternalEditorFields.PolymorphicObjectField(in polymorphicArgs);
					}
					EditorGUI.showMixedValue = prev2;
				}
				GUIHelper.PopGUIEnabled();
			}
			else
			{
				CallNextDrawer(label);
			}
		}

		void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			if (property.ValueEntry.ValueState != PropertyValueState.ReferenceValueConflict)
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
				string contentString = "Resolve type conflict with.../" + ((Object)tree.WeakTargets[i]).name + " (" + valueString + ")";
				genericMenu.AddItem(new GUIContent(contentString), on: false, delegate
				{
					property.Tree.DelayActionUntilRepaint(delegate
					{
						property.ValueEntry.WeakValues[0] = value;
						for (int j = 1; j < property.ValueEntry.WeakValues.Count; j++)
						{
							property.ValueEntry.WeakValues[j] = Sirenix.Serialization.SerializationUtility.CreateCopy(value);
						}
					});
				});
			}
		}
	}
}

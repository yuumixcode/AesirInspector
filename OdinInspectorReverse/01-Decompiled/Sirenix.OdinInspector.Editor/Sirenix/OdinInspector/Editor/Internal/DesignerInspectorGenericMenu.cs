using System;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	[DrawerPriority(double.MinValue, 0.0, 0.0)]
	public class DesignerInspectorGenericMenu<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems
	{
		internal const string SUB_MENU_NAME = "Visual Designer";

		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			if ((property.Parent == null || !property.Parent.ChildResolver.IsCollection) && DesignerUtils.CanTypeBeDesigned(property.Info.TypeOfOwner))
			{
				return !DesignerUtils.IsExcludedFromDesigner(property);
			}
			return false;
		}

		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			if (!DesignerUtils.CanTypeBeDesigned(property.Info.TypeOfOwner) || EditorWindow.focusedWindow?.GetType() == typeof(DesignerAttributePopup))
			{
				return;
			}
			genericMenu.AddSeparator(string.Empty);
			genericMenu.AddItem(new GUIContent("Visual Designer/Edit Attributes"), on: false, delegate
			{
				InspectorProperty localProperty2 = property;
				property.Tree.DelayActionUntilRepaint(delegate
				{
					DesignerEditor forOwner = DesignerEditors.GetForOwner(localProperty2);
					if (forOwner != null)
					{
						forOwner.SyncIfNeeded();
						DesignerEditorNode designerEditorNode = forOwner.RootNode.FindNodeByProperty(localProperty2);
						if (designerEditorNode != null)
						{
							forOwner.OpenPopup(Rect.zero, designerEditorNode);
						}
					}
				});
			});
			InspectorProperty parent = base.Property.ParentValueProperty;
			if (parent != null)
			{
				_003C_003Ec__DisplayClass2_0 CS_0024_003C_003E8__locals0;
				genericMenu.AddItem(new GUIContent("Visual Designer/Customize '" + parent.ValueEntry.TypeOfValue.GetNiceName() + "'"), on: false, delegate
				{
					base.Property.Tree.DelayActionUntilRepaint(delegate
					{
						DesignerEditor designerEditor = DesignerEditors.Get((InspectorProperty)(object)CS_0024_003C_003E8__locals0);
						if (designerEditor != null)
						{
							designerEditor.SyncIfNeeded();
							designerEditor.OpenWindow();
						}
					});
				});
			}
			Type typeOfValue = ((property.ValueEntry != null) ? property.ValueEntry.TypeOfValue : property.Info?.TypeOfValue);
			if (typeOfValue != null && DesignerUtils.CanTypeBeDesigned(typeOfValue))
			{
				InspectorProperty localProperty = base.Property;
				genericMenu.AddItem(new GUIContent("Visual Designer/Customize '" + typeOfValue.GetNiceName() + "'"), on: false, delegate
				{
					OdinVisualDesigner.OpenForProperty(localProperty);
				});
			}
			if (!property.ChildResolver.IsCollection || !(property.ChildResolver is ICollectionResolver collectionResolver))
			{
				return;
			}
			bool showEditElementItem = true;
			Type elementType = collectionResolver.ElementType;
			if (elementType.IsGenericType)
			{
				Type genericTypeDef = elementType.GetGenericTypeDefinition();
				if (genericTypeDef == typeof(EditableKeyValuePair<, >))
				{
					Type[] genericArgs = elementType.GetGenericArguments();
					if (genericArgs.Length == 2)
					{
						DesignerUtils.AddGenericMeuItemEditType(genericMenu, "Visual Designer/Customize Key Type", genericArgs[0]);
						DesignerUtils.AddGenericMeuItemEditType(genericMenu, "Visual Designer/Customize Value Type", genericArgs[1]);
					}
					showEditElementItem = false;
				}
			}
			if (showEditElementItem)
			{
				DesignerUtils.AddGenericMeuItemEditType(genericMenu, "Visual Designer/Customize Element Type", elementType);
			}
		}

		protected override void Initialize()
		{
			base.SkipWhenDrawing = true;
		}
	}
}

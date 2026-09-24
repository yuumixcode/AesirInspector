using System;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Drawer for the <see cref="T:Sirenix.OdinInspector.InlinePropertyAttribute" /> attribute.
	/// </summary>
	[DrawerPriority(0.0, 0.0, 0.11)]
	public class InlinePropertyAttributeDrawer : OdinAttributeDrawer<InlinePropertyAttribute>
	{
		private PropertySearchFilter searchFilter;

		private string searchFieldControlName;

		protected override void Initialize()
		{
			SearchableAttribute searchAttr = base.Property.GetAttribute<SearchableAttribute>();
			if (searchAttr != null)
			{
				searchFilter = new PropertySearchFilter(base.Property, searchAttr);
				searchFieldControlName = "PropertyTreeSearchField_" + Guid.NewGuid();
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			bool pushLabelWidth = base.Attribute.LabelWidth > 0;
			if (label == null)
			{
				if (pushLabelWidth)
				{
					GUIHelper.PushLabelWidth(base.Attribute.LabelWidth);
					HorizontalGroupAttributeDrawer.PushLabelWidthDefault(base.Attribute.LabelWidth);
				}
				CallNextDrawer(label);
				if (pushLabelWidth)
				{
					GUIHelper.PopLabelWidth();
					HorizontalGroupAttributeDrawer.PopLabelWidthDefault();
				}
				return;
			}
			SirenixEditorGUI.BeginVerticalPropertyLayout(label);
			if (pushLabelWidth)
			{
				GUIHelper.PushLabelWidth(base.Attribute.LabelWidth);
				HorizontalGroupAttributeDrawer.PushLabelWidthDefault(base.Attribute.LabelWidth);
			}
			if (searchFilter != null)
			{
				Rect rect = EditorGUILayout.GetControlRect();
				string newTerm = SirenixEditorGUI.SearchField(rect, searchFilter.SearchTerm, forceFocus: false, searchFieldControlName);
				if (newTerm != searchFilter.SearchTerm)
				{
					searchFilter.SearchTerm = newTerm;
					base.Property.Tree.DelayActionUntilRepaint(delegate
					{
						searchFilter.UpdateSearch();
						GUIHelper.RequestRepaint();
					});
				}
			}
			if (searchFilter != null && searchFilter.HasSearchResults)
			{
				searchFilter.DrawSearchResults();
			}
			else
			{
				for (int i = 0; i < base.Property.Children.Count; i++)
				{
					InspectorProperty child = base.Property.Children[i];
					child.Draw(child.Label);
				}
			}
			if (pushLabelWidth)
			{
				GUIHelper.PopLabelWidth();
				HorizontalGroupAttributeDrawer.PopLabelWidthDefault();
			}
			GUILayout.Space(2f);
			SirenixEditorGUI.EndVerticalPropertyLayout();
		}
	}
}

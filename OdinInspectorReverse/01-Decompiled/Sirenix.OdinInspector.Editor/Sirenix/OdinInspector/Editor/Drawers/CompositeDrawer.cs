using System;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Drawer for composite properties.
	/// </summary>
	[DrawerPriority(0.0, 0.0, 0.1)]
	public class CompositeDrawer : OdinDrawer
	{
		private PropertySearchFilter searchFilter;

		private string searchFieldControlName;

		protected override void Initialize()
		{
			SearchableAttribute searchAttr = base.Property.GetAttribute<SearchableAttribute>();
			if (searchAttr != null && !base.Property.IsTreeRoot)
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
			InspectorProperty property = base.Property;
			if (property.IsTreeRoot)
			{
				label = null;
			}
			if (property.Children.Count == 0)
			{
				if (property.ValueEntry != null)
				{
					if (label != null)
					{
						Rect rect = EditorGUILayout.GetControlRect();
						GUI.Label(rect, label);
					}
					return;
				}
				GUILayout.BeginHorizontal();
				if (label != null)
				{
					EditorGUILayout.PrefixLabel(label);
				}
				SirenixEditorGUI.WarningMessageBox("There is no drawer defined for property " + property.NiceName + " of type " + property.Info.PropertyType.ToString() + ".");
				GUILayout.EndHorizontal();
				return;
			}
			if (label == null)
			{
				if (searchFilter != null)
				{
					searchFilter.DrawDefaultSearchFieldLayout(null);
				}
				if (searchFilter != null && searchFilter.HasSearchResults)
				{
					searchFilter.DrawSearchResults();
					return;
				}
				for (int i = 0; i < property.Children.Count; i++)
				{
					InspectorProperty child = property.Children[i];
					child.Draw(child.Label);
				}
				return;
			}
			float tmp = EditorGUIUtility.fieldWidth;
			EditorGUIUtility.fieldWidth = 10f;
			Rect foldoutRect = EditorGUILayout.GetControlRect(false);
			EditorGUIUtility.fieldWidth = tmp;
			if (searchFilter != null)
			{
				Rect rect2 = GUILayoutUtility.GetLastRect().AddXMin(GUIHelper.BetterLabelWidth).AddY(1f);
				string newTerm = SirenixEditorGUI.SearchField(rect2, searchFilter.SearchTerm, forceFocus: false, searchFieldControlName);
				if (newTerm != searchFilter.SearchTerm)
				{
					searchFilter.SearchTerm = newTerm;
					base.Property.Tree.DelayActionUntilRepaint(delegate
					{
						if (!string.IsNullOrEmpty(newTerm))
						{
							base.Property.State.Expanded = true;
						}
						searchFilter.UpdateSearch();
						GUIHelper.RequestRepaint();
					});
				}
			}
			if (base.Property.PrefabModificationBarSourceRectOverride == default(Rect))
			{
				base.Property.PrefabModificationBarSourceRectOverride = foldoutRect;
			}
			base.Property.State.Expanded = SirenixEditorGUI.Foldout(foldoutRect, base.Property.State.Expanded, label);
			if (SirenixEditorGUI.BeginFadeGroup(this, base.Property.State.Expanded))
			{
				EditorGUI.indentLevel++;
				if (searchFilter != null && searchFilter.HasSearchResults)
				{
					searchFilter.DrawSearchResults();
				}
				else
				{
					for (int i2 = 0; i2 < property.Children.Count; i2++)
					{
						InspectorProperty child2 = property.Children[i2];
						child2.Draw(child2.Label);
					}
				}
				EditorGUI.indentLevel--;
			}
			SirenixEditorGUI.EndFadeGroup();
		}
	}
}

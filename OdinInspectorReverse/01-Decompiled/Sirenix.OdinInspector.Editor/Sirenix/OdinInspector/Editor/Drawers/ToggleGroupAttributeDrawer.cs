using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws all properties grouped together with the <see cref="T:Sirenix.OdinInspector.ToggleGroupAttribute" />
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.ToggleGroupAttribute" />
	public class ToggleGroupAttributeDrawer : OdinGroupDrawer<ToggleGroupAttribute>
	{
		private ValueResolver<string> titleHelper;

		private string errorMessage;

		private InspectorProperty toggleProperty;

		private PropertyContext<string> openToggleGlobalContext;

		protected override void Initialize()
		{
			toggleProperty = base.Property.Children.Get(base.Attribute.ToggleMemberName);
			toggleProperty.GetActiveDrawerChain();
			if (toggleProperty == null)
			{
				errorMessage = "No property or field named " + base.Attribute.ToggleMemberName + " found. Make sure the property is part of the inspector and the group.";
			}
			else
			{
				titleHelper = ValueResolver.GetForString(base.Property, base.Attribute.ToggleGroupTitle ?? base.Attribute.GroupName);
				if (titleHelper.HasError)
				{
					errorMessage = titleHelper.ErrorMessage;
				}
			}
			if (base.Attribute.CollapseOthersOnExpand)
			{
				InspectorProperty parent = base.Property.ParentValueProperty;
				while (parent != null && !parent.Info.HasBackingMembers)
				{
					parent = parent.ParentValueProperty;
				}
				if (parent == null)
				{
					parent = base.Property.Tree.RootProperty;
				}
				openToggleGlobalContext = parent.Context.GetGlobal("OpenFoldoutToggleGroup", (string)null);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (errorMessage != null)
			{
				SirenixEditorGUI.MessageBox(errorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				return;
			}
			if (base.Attribute.CollapseOthersOnExpand && openToggleGlobalContext != null && openToggleGlobalContext.Value != null && openToggleGlobalContext.Value != base.Property.Path)
			{
				base.Property.State.Expanded = false;
			}
			bool isEnabled = (bool)toggleProperty.ValueEntry.WeakSmartValue;
			bool val = isEnabled;
			string title = titleHelper.GetValue();
			bool prev = base.Property.State.Expanded;
			bool visibleBuffer = base.Property.State.Expanded;
			if (SirenixEditorGUI.BeginToggleGroup(UniqueDrawerKey.Create(base.Property, this), ref isEnabled, ref visibleBuffer, title))
			{
				for (int i = 0; i < base.Property.Children.Count; i++)
				{
					InspectorProperty child = base.Property.Children[i];
					if (child != toggleProperty)
					{
						child.Draw(child.Label);
					}
				}
			}
			SirenixEditorGUI.EndToggleGroup();
			base.Property.State.Expanded = visibleBuffer;
			if (openToggleGlobalContext != null && prev != base.Property.State.Expanded && base.Property.State.Expanded)
			{
				openToggleGlobalContext.Value = base.Property.Path;
			}
			toggleProperty.ValueEntry.WeakSmartValue = isEnabled;
		}
	}
}

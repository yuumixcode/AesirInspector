using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.ToggleAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.ToggleAttribute" />
	public class ToggleAttributeDrawer : OdinAttributeDrawer<ToggleAttribute>
	{
		private InspectorProperty toggleProperty;

		private PropertyContext<string> openToggleGlobalContext;

		protected override void Initialize()
		{
			toggleProperty = base.Property.Children.Get(base.Attribute.ToggleMemberName);
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
			if (toggleProperty == null)
			{
				SirenixEditorGUI.MessageBox(base.Attribute.ToggleMemberName + " is not a member of " + base.Property.NiceName + ".", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				return;
			}
			if (toggleProperty.ValueEntry.TypeOfValue != typeof(bool))
			{
				SirenixEditorGUI.MessageBox(base.Attribute.ToggleMemberName + " on " + base.Property.NiceName + "  must be a boolean.", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				return;
			}
			bool isEnabled = (bool)toggleProperty.ValueEntry.WeakSmartValue;
			if (base.Attribute.CollapseOthersOnExpand && openToggleGlobalContext != null && openToggleGlobalContext.Value != null && openToggleGlobalContext.Value != base.Property.Path)
			{
				base.Property.State.Expanded = false;
			}
			bool prev = base.Property.State.Expanded;
			bool visibleBuffer = base.Property.State.Expanded;
			if (SirenixEditorGUI.BeginToggleGroup(UniqueDrawerKey.Create(base.Property, this), ref isEnabled, ref visibleBuffer, (label != null) ? label.text : base.Property.NiceName))
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

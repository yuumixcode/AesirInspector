using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws all properties grouped together with the <see cref="T:Sirenix.OdinInspector.FoldoutGroupAttribute" />
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.FoldoutGroupAttribute" />
	public class FoldoutGroupAttributeDrawer : OdinGroupDrawer<FoldoutGroupAttribute>
	{
		private ValueResolver<string> titleGetter;

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			titleGetter = ValueResolver.GetForString(base.Property, base.Attribute.GroupName);
			if (base.Attribute.HasDefinedExpanded)
			{
				base.Property.State.Expanded = base.Attribute.Expanded;
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			InspectorProperty property = base.Property;
			FoldoutGroupAttribute attribute = base.Attribute;
			if (titleGetter.HasError)
			{
				SirenixEditorGUI.MessageBox(titleGetter.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			}
			SirenixEditorGUI.BeginBox();
			SirenixEditorGUI.BeginBoxHeader();
			GUIContent content = GUIHelper.TempContent(titleGetter.HasError ? property.Label.text : titleGetter.GetValue());
			base.Property.State.Expanded = SirenixEditorGUI.Foldout(base.Property.State.Expanded, content);
			SirenixEditorGUI.EndBoxHeader();
			if (SirenixEditorGUI.BeginFadeGroup(this, base.Property.State.Expanded))
			{
				for (int i = 0; i < property.Children.Count; i++)
				{
					InspectorProperty child = property.Children[i];
					child.Draw(child.Label);
				}
			}
			SirenixEditorGUI.EndFadeGroup();
			SirenixEditorGUI.EndBox();
		}
	}
}

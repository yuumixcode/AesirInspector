using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.SuffixLabelAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.LabelTextAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyTooltipAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.InlineButtonAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.CustomValueDrawerAttribute" />
	[DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
	[AllowGUIEnabledForReadonly]
	public sealed class SuffixLabelAttributeDrawer : OdinAttributeDrawer<SuffixLabelAttribute>
	{
		private ValueResolver<string> labelResolver;

		private ValueResolver<Color> iconColorResolver;

		protected override void Initialize()
		{
			labelResolver = ValueResolver.GetForString(base.Property, base.Attribute.Label);
			iconColorResolver = ValueResolver.Get(base.Property, base.Attribute.IconColor, SirenixGUIStyles.RightAlignedGreyMiniLabel.normal.textColor);
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (labelResolver.HasError)
			{
				SirenixEditorGUI.MessageBox(labelResolver.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			}
			if (iconColorResolver.HasError)
			{
				SirenixEditorGUI.MessageBox(iconColorResolver.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			}
			if (base.Attribute.Overlay)
			{
				CallNextDrawer(label);
				GUIHelper.PushGUIEnabled(enabled: true);
				if (base.Attribute.HasDefinedIcon)
				{
					Rect rect = GUILayoutUtility.GetLastRect().HorizontalPadding(0f, 22f);
					GUI.Label(rect, labelResolver.GetValue(), SirenixGUIStyles.RightAlignedGreyMiniLabel);
					SdfIcons.DrawIcon(rect.AlignRight(12f).AddX(14f), base.Attribute.Icon, iconColorResolver.GetValue());
				}
				else
				{
					Rect rect2 = GUILayoutUtility.GetLastRect().HorizontalPadding(0f, 8f);
					GUI.Label(rect2, labelResolver.GetValue(), SirenixGUIStyles.RightAlignedGreyMiniLabel);
				}
				GUIHelper.PopGUIEnabled();
				return;
			}
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
			CallNextDrawer(label);
			GUILayout.EndVertical();
			GUIHelper.PushGUIEnabled(enabled: true);
			GUILayout.Label(labelResolver.GetValue(), SirenixGUIStyles.RightAlignedGreyMiniLabel, GUILayoutOptions.ExpandWidth(expand: false));
			if (base.Attribute.HasDefinedIcon)
			{
				Rect iconRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUILayout.Width(12f));
				SdfIcons.DrawIcon(iconRect.AlignCenter(12f), base.Attribute.Icon, iconColorResolver.GetValue());
			}
			GUIHelper.PopGUIEnabled();
			GUILayout.EndHorizontal();
		}
	}
}

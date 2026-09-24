using Sirenix.OdinInspector.Editor.ActionResolvers;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.InlineButtonAttribute" />
	/// </summary>
	[DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
	public sealed class InlineButtonAttributeDrawer<T> : OdinAttributeDrawer<InlineButtonAttribute, T>
	{
		private ValueResolver<string> labelGetter;

		private ActionResolver clickAction;

		private ValueResolver<bool> showIfGetter;

		private ValueResolver<Color> buttonColorGetter;

		private ValueResolver<Color> textColorGetter;

		private bool show = true;

		private string tooltip;

		protected override void Initialize()
		{
			if (base.Attribute.Label != null)
			{
				labelGetter = ValueResolver.GetForString(base.Property, base.Attribute.Label);
			}
			else
			{
				labelGetter = ValueResolver.Get(base.Property, null, base.Attribute.Action.SplitPascalCase());
			}
			clickAction = ActionResolver.Get(base.Property, base.Attribute.Action);
			showIfGetter = ValueResolver.Get(base.Property, base.Attribute.ShowIf, fallbackValue: true);
			buttonColorGetter = ValueResolver.Get<Color>(base.Property, base.Attribute.ButtonColor);
			textColorGetter = ValueResolver.Get(base.Property, base.Attribute.TextColor, SirenixGUIStyles.Button.normal.textColor);
			show = showIfGetter.GetValue();
			tooltip = base.Property.GetAttribute<PropertyTooltipAttribute>()?.Tooltip ?? base.Property.GetAttribute<TooltipAttribute>()?.tooltip;
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (labelGetter.HasError || clickAction.HasError || showIfGetter.HasError || buttonColorGetter.HasError || textColorGetter.HasError)
			{
				labelGetter.DrawError();
				clickAction.DrawError();
				buttonColorGetter.DrawError();
				textColorGetter.DrawError();
				CallNextDrawer(label);
				return;
			}
			if (Event.current.type == EventType.Layout)
			{
				show = showIfGetter.GetValue();
			}
			if (show)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.BeginVertical();
				CallNextDrawer(label);
				EditorGUILayout.EndVertical();
				GUIContent buttonLabel = new GUIContent(labelGetter.GetValue(), tooltip);
				SirenixEditorGUI.CalculateMinimumSDFIconButtonWidth(buttonLabel.text, null, base.Attribute.Icon != SdfIconType.None, EditorGUIUtility.singleLineHeight, out var _, out var _, out var _, out var btnWidth);
				Rect btnRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUILayout.MaxWidth(btnWidth));
				Color btnColor = buttonColorGetter.GetValue();
				Color btnTextColor = textColorGetter.GetValue();
				if (SirenixEditorGUI.SDFIconButton(btnRect, buttonLabel, btnColor, btnTextColor, base.Attribute.Icon, base.Attribute.IconAlignment))
				{
					InvokeButton(buttonLabel);
				}
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				CallNextDrawer(label);
			}
		}

		private void InvokeButton(GUIContent buttonLabel)
		{
			base.Property.RecordForUndo("Click " + buttonLabel);
			clickAction.DoActionForAllSelectionIndices();
		}
	}
}

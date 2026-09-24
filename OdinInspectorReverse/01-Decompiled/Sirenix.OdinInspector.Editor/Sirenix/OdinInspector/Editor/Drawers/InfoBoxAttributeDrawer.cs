using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.InfoBoxAttribute" />.
	/// Draws an info box above the property. Error and warning info boxes can be tracked by Odin Scene Validator.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.InfoBoxAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DetailedInfoBoxAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.RequiredAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ValidateInputAttribute" />
	[DrawerPriority(0.0, 10001.0, 0.0)]
	public sealed class InfoBoxAttributeDrawer : OdinAttributeDrawer<InfoBoxAttribute>
	{
		private bool drawMessageBox;

		private ValueResolver<bool> visibleIfResolver;

		private ValueResolver<string> messageResolver;

		private ValueResolver<Color> iconColorResolver;

		private MessageType messageType;

		protected override void Initialize()
		{
			visibleIfResolver = ValueResolver.Get(base.Property, base.Attribute.VisibleIf, fallbackValue: true);
			messageResolver = ValueResolver.GetForString(base.Property, base.Attribute.Message);
			iconColorResolver = ValueResolver.Get(base.Property, base.Attribute.IconColor, EditorStyles.label.normal.textColor);
			drawMessageBox = visibleIfResolver.GetValue();
			switch (base.Attribute.InfoMessageType)
			{
			default:
				messageType = MessageType.None;
				break;
			case InfoMessageType.Info:
				messageType = MessageType.Info;
				break;
			case InfoMessageType.Warning:
				messageType = MessageType.Warning;
				break;
			case InfoMessageType.Error:
				messageType = MessageType.Error;
				break;
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			bool valid = true;
			if (visibleIfResolver.HasError)
			{
				SirenixEditorGUI.MessageBox(visibleIfResolver.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				valid = false;
			}
			if (messageResolver.HasError)
			{
				SirenixEditorGUI.MessageBox(messageResolver.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				valid = false;
			}
			if (iconColorResolver.HasError)
			{
				SirenixEditorGUI.MessageBox(iconColorResolver.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				valid = false;
			}
			if (!valid)
			{
				CallNextDrawer(label);
				return;
			}
			if (base.Attribute.GUIAlwaysEnabled)
			{
				GUIHelper.PushGUIEnabled(enabled: true);
			}
			if (Event.current.type == EventType.Layout)
			{
				drawMessageBox = visibleIfResolver.GetValue();
			}
			if (drawMessageBox)
			{
				string message = messageResolver.GetValue();
				GUIHelper.PushIsBoldLabel(isBold: false);
				if (base.Attribute.HasDefinedIcon)
				{
					SirenixEditorGUI.MessageBox(message, base.Attribute.Icon, iconColorResolver.GetValue(), GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				}
				else
				{
					SirenixEditorGUI.MessageBox(message, messageType, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				}
				GUIHelper.PopIsBoldLabel();
			}
			if (base.Attribute.GUIAlwaysEnabled)
			{
				GUIHelper.PopGUIEnabled();
			}
			CallNextDrawer(label);
		}
	}
}

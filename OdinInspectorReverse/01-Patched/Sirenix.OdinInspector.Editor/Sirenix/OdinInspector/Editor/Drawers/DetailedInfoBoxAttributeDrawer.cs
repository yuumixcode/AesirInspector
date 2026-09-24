using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.DetailedInfoBoxAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.DetailedInfoBoxAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.InfoBoxAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.RequiredAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.OnInspectorGUIAttribute" />
	[DrawerPriority(0.0, 100.0, 0.0)]
	public sealed class DetailedInfoBoxAttributeDrawer : OdinAttributeDrawer<DetailedInfoBoxAttribute>
	{
		private bool drawMessageBox;

		private MessageType messageType;

		private bool valid;

		private ValueResolver<bool> visibleIfGetter;

		private ValueResolver<string> messageGetter;

		private ValueResolver<string> detailsGetter;

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			visibleIfGetter = ValueResolver.Get(base.Property, base.Attribute.VisibleIf, fallbackValue: true);
			messageGetter = ValueResolver.GetForString(base.Property, base.Attribute.Message);
			detailsGetter = ValueResolver.GetForString(base.Property, base.Attribute.Details);
			valid = !visibleIfGetter.HasError && !messageGetter.HasError && !detailsGetter.HasError;
			base.Property.State.Create("ShowDetailedMessage", persistent: false, defaultValue: false);
			switch (base.Attribute.InfoMessageType)
			{
			case InfoMessageType.None:
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
			default:
				Debug.LogError("Unknown InfoBoxType: " + base.Attribute.InfoMessageType);
				break;
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			ValueResolver.DrawErrors(visibleIfGetter, messageGetter, detailsGetter);
			if (valid)
			{
				if (Event.current.type == EventType.Layout)
				{
					drawMessageBox = visibleIfGetter.GetValue();
				}
				if (drawMessageBox)
				{
					base.Property.State.Set("ShowDetailedMessage", !SirenixEditorGUI.DetailedMessageBox(messageGetter.GetValue(), detailsGetter.GetValue(), messageType, !base.Property.State.Get<bool>("ShowDetailedMessage"), GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize));
				}
			}
			CallNextDrawer(label);
		}
	}
}

using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws a warning message for non-serialized properties that sports both the SerializeField and the ShowInInspector attribute.
	/// </summary>
	[DrawerPriority(1.0, 0.0, 0.0)]
	public class NonSerializedShowInInspectorWarningAttributeDrawer : OdinAttributeDrawer<ShowInInspectorAttribute>
	{
		/// <summary>
		/// Determines if the drawer can draw the property.
		/// </summary>
		/// <param name="property">The property to test.</param>
		/// <returns><c>true</c> if the drawer can draw the property; otherwise <c>false</c>.</returns>
		protected override bool CanDrawAttributeProperty(InspectorProperty property)
		{
			return property.Info.PropertyType == PropertyType.Value;
		}

		/// <summary>
		/// Initializes the drawer.
		/// </summary>
		protected override void Initialize()
		{
			if (GlobalConfig<GlobalSerializationConfig>.Instance.HideNonSerializedShowInInspectorWarningMessages || !base.Property.Info.HasSingleBackingMember || base.Property.Info.SerializationBackend != SerializationBackend.None || !base.Property.Info.GetMemberInfo().IsDefined(typeof(SerializeField), inherit: true))
			{
				base.SkipWhenDrawing = true;
			}
		}

		/// <summary>
		/// Draws the warning message and calls the next drawer.
		/// </summary>
		/// <param name="label">The label for the property.</param>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			SirenixEditorGUI.WarningMessageBox("You have used the SerializeField and the ShowInInspector attributes together, but the member is not serialized.\nAre you certain that is correct?\nYou can try using the Serialization Debugger, or you can disable this message from the preferences window.");
			CallNextDrawer(label);
		}
	}
}

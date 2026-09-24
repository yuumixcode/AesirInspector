using System;
using System.Reflection;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// <para>
	/// When first learning to use the Odin Inspector, it is common for people to misunderstand the OdinSerialize attribute,
	/// and use it in places where it does not achive the deceired goal.
	/// </para>
	/// <para>
	/// This drawer will display a warning message if the OdinSerialize attribute is potentially used in such cases.
	/// </para>
	/// </summary>
	/// <seealso cref="!:Sirenix.OdinInspector.Editor.OdinAttributeDrawer&lt;Sirenix.Serialization.OdinSerializeAttribute&gt;" />
	[DrawerPriority(1000.0, 0.0, 0.0)]
	public sealed class OdinSerializeAttributeWarningDrawer : OdinAttributeDrawer<OdinSerializeAttribute>
	{
		private string message;

		protected override bool CanDrawAttributeProperty(InspectorProperty property)
		{
			if (GlobalConfig<GlobalSerializationConfig>.Instance.HideOdinSerializeAttributeWarningMessages)
			{
				return false;
			}
			if (property.Parent != null)
			{
				return property.Parent.IsTreeRoot;
			}
			return false;
		}

		protected override void Initialize()
		{
			InspectorProperty property = base.Property;
			if (property.ValueEntry.SerializationBackend == SerializationBackend.None)
			{
				message = "The following property is marked with the [OdinSerialize] attribute, but the property is not part of any object that uses the Odin Serialization Protocol. \n\nAre you perhaps forgetting to inherit from one of our serialized base classes such as SerializedMonoBehaviour or SerializedScriptableObject? \n\n";
				FieldInfo fieldInfo = property.Info.GetMemberInfo() as FieldInfo;
				if (fieldInfo != null && fieldInfo.IsPublic && property.Info.GetAttribute<NonSerializedAttribute>() == null)
				{
					message += "Odin will also serialize public fields by default, so are you sure you need to mark the field with the [OdinSerialize] attribute?\n\n";
				}
			}
			else if (property.ValueEntry.SerializationBackend == SerializationBackend.Odin && UnitySerializationUtility.GuessIfUnityWillSerialize(property.Info.GetMemberInfo()))
			{
				message = "The following property is marked with the [OdinSerialize] attribute, but Unity is also serializing it. You can either remove the [OdinSerialize] attribute and let Unity serialize it, or you can use the [NonSerialized] attribute together with the [OdinSerialize] attribute if you want Odin to serialize it instead of Unity.\n\n";
				if (property.Info.TypeOfOwner.GetAttribute<SerializableAttribute>() != null && (AssemblyUtilities.GetAssemblyCategory(property.Info.TypeOfOwner.Assembly) & AssemblyCategory.ProjectSpecific) != AssemblyCategory.None)
				{
					message = message + "Odin's default serialization protocol does not require a type to be marked with the [Serializable] attribute in order for it to be serialized, which Unity does. Therefore you could remove the System.Serializable attribute from " + property.Info.TypeOfOwner.GetNiceFullName() + " if you want Unity never to serialize the type.\n\n";
				}
			}
			if (message != null)
			{
				message += "Check out our online manual for more information.\n\n";
				message += "This message can be disabled in the 'Tools > Odin > Serializer > Preferences' window, but it is recommended that you don't.";
			}
		}

		/// <summary>
		/// Draws The Property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (message != null)
			{
				SirenixEditorGUI.WarningMessageBox(message);
			}
			CallNextDrawer(label);
		}
	}
}

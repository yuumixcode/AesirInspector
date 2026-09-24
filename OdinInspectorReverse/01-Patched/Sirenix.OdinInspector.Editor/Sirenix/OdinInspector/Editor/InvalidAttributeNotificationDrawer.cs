using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	[DrawerPriority(-1.0, -1.0, -1.0)]
	[OdinDontRegister]
	public class InvalidAttributeNotificationDrawer<TInvalidAttribute> : OdinDrawer
	{
		public string errorMessage;

		public string validTypeMessage;

		public bool isFolded = true;

		protected override void Initialize()
		{
			StringBuilder sb = new StringBuilder("Attribute '").Append(typeof(TInvalidAttribute).GetNiceName()).Append("' cannot be put on property '").Append(base.Property.Name)
				.Append("'");
			if (base.Property.ValueEntry != null)
			{
				sb.Append(" of base type '").Append(base.Property.ValueEntry.BaseValueType.GetNiceName()).Append("'");
			}
			sb.Append('.');
			errorMessage = sb.ToString();
			sb.Length = 0;
			List<Type> validTypes = DrawerUtilities.InvalidAttributeTargetUtility.GetValidTargets(typeof(TInvalidAttribute));
			sb.AppendLine("The following types are valid:");
			sb.AppendLine();
			for (int i = 0; i < validTypes.Count; i++)
			{
				Type type = validTypes[i];
				sb.Append(type.GetNiceName());
				if (type.IsGenericParameter)
				{
					sb.Append(" ").Append(type.GetGenericParameterConstraintsString(useFullTypeNames: true));
				}
				sb.AppendLine();
			}
			sb.Append("Supported collections where the element type is any of the above types");
			validTypeMessage = sb.ToString();
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			isFolded = SirenixEditorGUI.DetailedMessageBox(errorMessage, validTypeMessage, MessageType.Error, isFolded, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			CallNextDrawer(label);
		}
	}
}

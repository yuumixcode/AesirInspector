using System;
using Sirenix.Utilities;
using UnityEditor;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public static class ValidatorExtensions
	{
		public static ValidationResultType ToValidationResultType(this InfoMessageType messageType)
		{
			return messageType switch
			{
				InfoMessageType.Error => ValidationResultType.Error, 
				InfoMessageType.Warning => ValidationResultType.Warning, 
				_ => ValidationResultType.Valid, 
			};
		}

		public static ValidatorSeverity ToValidatorSeverity(this InfoMessageType messageType)
		{
			return messageType switch
			{
				InfoMessageType.Error => ValidatorSeverity.Error, 
				InfoMessageType.Warning => ValidatorSeverity.Warning, 
				_ => ValidatorSeverity.Ignore, 
			};
		}

		internal static string GetNiceValidatorTypeName(this Type t)
		{
			if (t == null)
			{
				return "-";
			}
			string k = "";
			k = t.GetNiceName();
			int i = k.IndexOf('<');
			if (i >= 0)
			{
				k = k.Substring(0, i);
			}
			for (int j = 0; j < 2; j++)
			{
				if (k.FastEndsWith("Validator"))
				{
					k = k.Substring(0, k.Length - "Validator".Length);
				}
				if (k.FastEndsWith("Attribute"))
				{
					k = k.Substring(0, k.Length - "Attribute".Length);
				}
			}
			return ObjectNames.NicifyVariableName(k);
		}
	}
}

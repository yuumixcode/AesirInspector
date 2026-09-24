using System;
using Sirenix.OdinValidator.Editor;
using Sirenix.Utilities;
using UnityEditor;

namespace Sirenix.OdinInspector.Editor.Validation
{
	/// <summary>
	/// <para>
	/// Apply this to an assembly to register validation rules for the validation system.
	/// This enables locating of all relevant validator types very quickly.
	/// </para>
	/// <para>
	/// Only use this to register types derived from Validator! It is important to understand
	/// that a rule is simply a serialized Validator instance that can be enabled, disabled, 
	/// and have its contained values modified from the rules management GUI in the Odin Validator
	/// window.
	/// </para>
	/// <para>Read our Odin Validator tutorials for more information.</para>
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.Validation.RegisterValidatorAttribute" />
	public class RegisterValidationRuleAttribute : RegisterValidatorAttribute
	{
		public string Name;

		public string Description;

		public bool EnabledByDefault;

		public RegisterValidationRuleAttribute(Type validatorType, string name = null, string description = null, bool enabledByDefault = true)
			: base(validatorType, typeof(RuleConfig))
		{
			if (name == null)
			{
				Name = validatorType.GetNiceName();
				if (Name.FastEndsWith("Validator"))
				{
					Name = Name.Substring(0, Name.Length - "Validator".Length).Trim();
				}
				Name = ObjectNames.NicifyVariableName(Name);
			}
			else
			{
				Name = name;
			}
			Description = description;
			EnabledByDefault = enabledByDefault;
		}
	}
}

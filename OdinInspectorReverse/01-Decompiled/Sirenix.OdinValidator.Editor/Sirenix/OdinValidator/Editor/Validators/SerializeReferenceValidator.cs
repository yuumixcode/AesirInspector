using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor.Validators
{
	public class SerializeReferenceValidator : ValueValidator<object>
	{
		[Tooltip("The severity of the validation result.")]
		public ValidatorSeverity Severity = ValidatorSeverity.Warning;

		private Type lastValidatedType;

		private bool isBaseValid;

		private bool isInstanceValid;

		private string baseErrorMessage;

		private string instanceErrorMessage;

		public override bool CanValidateProperty(InspectorProperty property)
		{
			return property.GetAttribute<SerializeReference>() != null;
		}

		protected override void Validate(ValidationResult result)
		{
			IPropertyValueEntry<object> entry = base.ValueEntry;
			if (lastValidatedType != entry.TypeOfValue)
			{
				lastValidatedType = entry.TypeOfValue;
				SerializeReferenceValidityResult baseValidity = SerializeReferenceUtility.ValidateBaseType(entry.BaseValueType, out baseErrorMessage);
				isBaseValid = baseValidity == SerializeReferenceValidityResult.Valid;
				if (entry.BaseValueType == entry.TypeOfValue)
				{
					isInstanceValid = true;
					instanceErrorMessage = string.Empty;
				}
				else
				{
					SerializeReferenceValidityResult instanceValidity = SerializeReferenceUtility.ValidateInstanceType(entry.BaseValueType, entry.TypeOfValue, out instanceErrorMessage);
					isInstanceValid = instanceValidity == SerializeReferenceValidityResult.Valid;
				}
			}
			if (!isBaseValid)
			{
				result.Add(Severity, baseErrorMessage);
			}
			else if (!isInstanceValid)
			{
				result.Add(Severity, instanceErrorMessage);
			}
		}
	}
}

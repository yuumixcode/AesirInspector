using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Reflection.Editor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor.Validators
{
	public class MissingUnityObjectReferenceValidator : ValueValidator<Object>
	{
		[Tooltip("The severity of the validation result.")]
		public ValidatorSeverity Severity;

		protected override void Validate(ValidationResult result)
		{
			if (Severity == ValidatorSeverity.Ignore || !(base.Value == null) || (object)base.Value == null || !OdinEntityId.FromObject(base.Value).IsValid)
			{
				return;
			}
			result.AddError($"A Unity object reference has gone missing from {base.Property.Tree.RootProperty.ValueEntry.WeakSmartValue}.!").WithMetaData("Property path", base.Property.UnityPropertyPath).WithMetaData("Entity ID", OdinEntityId.FromObject(base.Value))
				.WithFix("Remove Missing Reference", delegate
				{
					for (int i = 0; i < base.Property.ValueEntry.WeakValues.Count; i++)
					{
						base.Property.ValueEntry.WeakValues.ForceSetValue(i, null);
					}
				});
		}
	}
}

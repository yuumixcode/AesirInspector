using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor.Validators
{
	public class ListNullElementValidator : ValueValidator<object>
	{
		[Tooltip("The severity of the validation result.")]
		public ValidatorSeverity ValidatorSeverity = ValidatorSeverity.Warning;

		public override bool CanValidateProperty(InspectorProperty property)
		{
			return property.ChildResolver is ICollectionResolver;
		}

		protected override void Validate(ValidationResult result)
		{
			for (int i = base.Property.Children.Count - 1; i >= 0; i--)
			{
				InspectorProperty child = base.Property.Children[i];
				if (child.ValueEntry.ValueState == PropertyValueState.NullReference)
				{
					int capturedIndex = i;
					result.Add(ValidatorSeverity, $"\"{base.Property.NiceName}\" collection has a missing element at index {capturedIndex}.").WithFix(Fix.Create("Remove Missing Element", delegate
					{
						IOrderedCollectionResolver orderedCollectionResolver = base.Property.ChildResolver as IOrderedCollectionResolver;
						orderedCollectionResolver.QueueRemoveAt(capturedIndex);
						orderedCollectionResolver.ApplyChanges();
					}));
				}
			}
		}
	}
}

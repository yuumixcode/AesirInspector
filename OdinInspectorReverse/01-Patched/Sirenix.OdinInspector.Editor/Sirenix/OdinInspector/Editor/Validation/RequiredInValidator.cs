using Sirenix.OdinInspector.Editor.ValueResolvers;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class RequiredInValidator<T> : AttributeValidator<RequiredInAttribute, T> where T : class
	{
		private ValueResolver<string> errorMessageGetter;

		private bool canValidate;

		protected override void Initialize()
		{
			canValidate = (OdinPrefabUtility.GetPrefabKind(base.Property) & base.Attribute.PrefabKind) != 0;
			if (base.Attribute.ErrorMessage != null)
			{
				errorMessageGetter = ValueResolver.GetForString(base.Property, base.Attribute.ErrorMessage);
			}
		}

		protected override void Validate(ValidationResult result)
		{
			if (canValidate && !IsValid(base.ValueEntry.SmartValue))
			{
				string msg = ((errorMessageGetter != null) ? errorMessageGetter.GetValue() : (base.Property.NiceName + " is required"));
				result.AddError(msg).WithFix(delegate(FixArgs<T> x)
				{
					base.Property.ValueEntry.WeakSmartValue = x.NewValue;
				}, offerInInspector: false);
			}
		}

		private bool IsValid(T memberValue)
		{
			if (memberValue == null)
			{
				return false;
			}
			if (memberValue is string && string.IsNullOrEmpty(memberValue as string))
			{
				return false;
			}
			if (memberValue is Object && memberValue as Object == null)
			{
				return false;
			}
			return true;
		}
	}
}

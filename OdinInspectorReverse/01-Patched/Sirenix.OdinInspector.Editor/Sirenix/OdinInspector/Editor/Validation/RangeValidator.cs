using System;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class RangeValidator<T> : AttributeValidator<RangeAttribute, T> where T : struct
	{
		private class RangeFix
		{
			public T NewValue;
		}

		private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));

		private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

		public override bool CanValidateProperty(InspectorProperty property)
		{
			if (!IsNumber)
			{
				return IsVector;
			}
			return true;
		}

		protected override void Validate(ValidationResult result)
		{
			if (!GenericNumberUtility.NumberIsInRange(base.ValueEntry.SmartValue, Math.Min(base.Attribute.min, base.Attribute.max), Math.Max(base.Attribute.min, base.Attribute.max)))
			{
				result.AddError("Number is not in range.").WithFix(Fix.Create(delegate(RangeFix f)
				{
					base.ValueEntry.SmartValue = f.NewValue;
				}));
			}
		}
	}
}

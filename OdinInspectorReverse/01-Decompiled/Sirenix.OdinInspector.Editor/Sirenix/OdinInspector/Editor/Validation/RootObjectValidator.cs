using System.ComponentModel;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public abstract class RootObjectValidator<TValue> : ValueValidator<TValue> where TValue : Object
	{
		public TValue Object => base.ValueEntry.SmartValue;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public new TValue Value
		{
			get
			{
				return base.Value;
			}
			set
			{
				base.Value = value;
			}
		}

		public sealed override bool CanValidateProperty(InspectorProperty property)
		{
			if (!property.IsTreeRoot || !CanValidateRootProperty(property))
			{
				return false;
			}
			int count = property.ValueEntry.ValueCount;
			for (int i = 0; i < count; i++)
			{
				if (!CanValidateObject((TValue)property.ValueEntry.WeakValues[i]))
				{
					return false;
				}
			}
			return true;
		}

		protected virtual bool CanValidateRootProperty(InspectorProperty rootProperty)
		{
			return true;
		}

		protected virtual bool CanValidateObject(TValue obj)
		{
			return true;
		}

		protected override void Validate(ValidationResult result)
		{
			base.Validate(result);
		}
	}
}

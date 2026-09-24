using System;
using Sirenix.Config;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class TypeRegistryIllegalInstanceValidator<T> : ValueValidator<T>
	{
		public override bool CanValidateProperty(InspectorProperty property)
		{
			return !typeof(UnityEngine.Object).IsAssignableFrom(property.ValueEntry.BaseValueType);
		}

		protected override void Validate(ValidationResult result)
		{
			Type baseType = base.Property.ValueEntry.BaseValueType;
			if (GlobalConfig<TypeRegistryUserConfig>.Instance.IsIllegal(baseType))
			{
				result.AddWarning($"The base type of this property is an illegal type: '{baseType}'.");
			}
			if (base.Property.ValueEntry.ValueState != PropertyValueState.NullReference && base.ValueEntry.SerializationBackend.SupportsPolymorphism)
			{
				Type valueType = base.Property.ValueEntry.TypeOfValue;
				if (GlobalConfig<TypeRegistryUserConfig>.Instance.IsIllegal(valueType))
				{
					result.AddWarning($"The current value is of type '{valueType}', which is considered illegal.");
				}
			}
		}
	}
}

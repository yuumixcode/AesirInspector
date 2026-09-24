using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class AssetsOnlyValidator<T> : AttributeValidator<AssetsOnlyAttribute, T> where T : Object
	{
		protected override void Validate(ValidationResult result)
		{
			T value = base.ValueEntry.SmartValue;
			if (value != null && !AssetDatabase.Contains(value))
			{
				string name = value.name;
				Component component = value as Component;
				if (component != null)
				{
					name = "from " + component.gameObject.name;
				}
				result.ResultType = ValidationResultType.Error;
				result.Message = value.GetType().GetNiceName() + " " + name + " is not an asset.";
			}
		}
	}
}

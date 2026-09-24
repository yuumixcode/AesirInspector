using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	[NoValidationInInspector]
	public class ChildGameObjectsOnlyValidator<T> : AttributeValidator<ChildGameObjectsOnlyAttribute, T> where T : Object
	{
		protected override void Validate(ValidationResult result)
		{
			GameObject ownerGo = result.Setup.Root as GameObject;
			if (ownerGo == null)
			{
				Component component = result.Setup.Root as Component;
				if (component != null)
				{
					ownerGo = component.gameObject;
				}
			}
			GameObject valueGo = base.ValueEntry.SmartValue as GameObject;
			if (valueGo == null)
			{
				Component component2 = base.ValueEntry.SmartValue as Component;
				if (component2 != null)
				{
					valueGo = component2.gameObject;
				}
			}
			if (ownerGo == null || valueGo == null)
			{
				result.ResultType = ValidationResultType.IgnoreResult;
				return;
			}
			if (base.Attribute.IncludeSelf && ownerGo == valueGo)
			{
				result.ResultType = ValidationResultType.Valid;
				return;
			}
			Transform current = valueGo.transform;
			while (true)
			{
				current = current.parent;
				if (current == null)
				{
					break;
				}
				if (current.gameObject == ownerGo)
				{
					result.ResultType = ValidationResultType.Valid;
					return;
				}
			}
			result.ResultType = ValidationResultType.Error;
			result.Message = valueGo.name + " must be a child of " + ownerGo.name;
		}
	}
}

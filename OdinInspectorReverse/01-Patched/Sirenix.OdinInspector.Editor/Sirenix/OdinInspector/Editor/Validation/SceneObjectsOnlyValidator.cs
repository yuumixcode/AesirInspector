using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class SceneObjectsOnlyValidator<T> : AttributeValidator<SceneObjectsOnlyAttribute, T> where T : Object
	{
		protected override void Validate(ValidationResult result)
		{
			T value = base.ValueEntry.SmartValue;
			if (!(value != null))
			{
				return;
			}
			PrefabKind kind = OdinPrefabUtility.GetPrefabKind(value);
			if ((kind & (PrefabKind.InstanceInScene | PrefabKind.NonPrefabInstance)) == 0)
			{
				string name = value.name;
				Component component = value as Component;
				if (component != null)
				{
					name = "from " + component.gameObject.name;
				}
				result.AddError(value.GetType().GetNiceName() + " " + name + " cannot be an asset.");
			}
		}
	}
}

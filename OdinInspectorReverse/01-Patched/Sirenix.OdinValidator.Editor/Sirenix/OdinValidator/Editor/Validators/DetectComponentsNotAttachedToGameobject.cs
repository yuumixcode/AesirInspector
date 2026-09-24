using Sirenix.OdinInspector.Editor.Validation;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor.Validators
{
	public class DetectComponentsNotAttachedToGameobject<T> : RootObjectValidator<T> where T : Component
	{
		protected override void Validate(ValidationResult result)
		{
			if (!base.Object.gameObject)
			{
				result.AddError("Component is not attached to gameobject. This can happen when a component changed from being a Component/MonoBehaviour to ScriptableObejct.");
			}
		}
	}
}

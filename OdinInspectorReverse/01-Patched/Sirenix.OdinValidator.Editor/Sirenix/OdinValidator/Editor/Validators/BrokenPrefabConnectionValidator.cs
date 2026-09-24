using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor.Validators
{
	public class BrokenPrefabConnectionValidator : RootObjectValidator<GameObject>
	{
		protected override void Validate(ValidationResult result)
		{
			GameObject go = base.ValueEntry.SmartValue;
			PrefabInstanceStatus status = PrefabUtility.GetPrefabInstanceStatus(go);
			if (status == PrefabInstanceStatus.MissingAsset)
			{
				if (go == PrefabUtility.GetOutermostPrefabInstanceRoot(go))
				{
					result.ResultType = ValidationResultType.Error;
					result.Message = "The source Prefab or Model has been deleted";
				}
			}
			else if ((OdinPrefabUtility.GetPrefabKind(go) & PrefabKind.PrefabAsset) != PrefabKind.None && PrefabUtility.GetOutermostPrefabInstanceRoot(go) != null && PrefabUtility.GetCorrespondingObjectFromOriginalSource(go) != null && PrefabUtility.GetCorrespondingObjectFromSource(go) == null && PrefabUtility.GetNearestPrefabInstanceRoot(go) == null)
			{
				result.ResultType = ValidationResultType.Error;
				result.Message = "The source Prefab or Model has been deleted";
			}
		}
	}
}

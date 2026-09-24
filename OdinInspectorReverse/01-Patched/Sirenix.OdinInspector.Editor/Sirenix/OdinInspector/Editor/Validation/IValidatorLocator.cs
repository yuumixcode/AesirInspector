using System;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public interface IValidatorLocator
	{
		Func<Type, bool> CustomValidatorFilter { get; set; }

		IList<SceneValidator> GetSceneValidators(SceneReference scene);

		bool PotentiallyHasValidatorsFor(InspectorProperty property);

		IList<Validator> GetValidators(InspectorProperty property);
	}
}

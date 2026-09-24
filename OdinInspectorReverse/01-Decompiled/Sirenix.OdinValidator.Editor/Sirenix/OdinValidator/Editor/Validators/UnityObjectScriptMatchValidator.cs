using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor.Validators
{
	public class UnityObjectScriptMatchValidator : RootObjectValidator<Object>
	{
		[Tooltip("The severity of the validation message.")]
		public ValidatorSeverity Severity;

		protected override void Validate(ValidationResult result)
		{
			Object obj = base.Object;
			if (obj is ScriptableObject so)
			{
				MonoScript monoScript = MonoScript.FromScriptableObject(so);
				if (monoScript == null)
				{
					string typeName = base.Object.GetType().GetNiceName();
					result.Add(Severity, "The script for the ScriptableObject '" + typeName + "' may have been deleted or the filename for the script does not match its class name.");
				}
			}
			else if (obj is MonoBehaviour mb)
			{
				MonoScript monoScript2 = MonoScript.FromMonoBehaviour(mb);
				if (monoScript2 == null)
				{
					string typeName2 = base.Object.GetType().GetNiceName();
					result.Add(Severity, "The script for the MonoBehaviour '" + typeName2 + "' may have been deleted or the filename for the script does not match its class name.");
				}
			}
		}
	}
}

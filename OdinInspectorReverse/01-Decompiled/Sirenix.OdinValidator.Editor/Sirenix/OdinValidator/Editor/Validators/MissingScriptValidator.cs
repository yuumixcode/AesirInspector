using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor.Validators
{
	public class MissingScriptValidator : RootObjectValidator<GameObject>
	{
		private class FixArgs
		{
			[InfoBox("Automatically removing components is only as good as Unity's GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go) which does not work in all cases.", InfoMessageType.Info, null)]
			[OnInspectorGUI]
			private void OnGUI()
			{
			}
		}

		[Tooltip("The severity of the validation result.")]
		public ValidatorSeverity Severity = ValidatorSeverity.Warning;

		protected override void Validate(ValidationResult result)
		{
			Component[] cmps = base.Object.GetComponents(typeof(Component));
			foreach (Component cmp in cmps)
			{
				if (!cmp)
				{
					result.Add(Severity, "Missing Component, the associated script can not be loaded.").WithFix(Fix.Create<FixArgs>("Remove Missing Components", delegate
					{
						int num = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(base.Object);
					}));
					break;
				}
			}
		}
	}
}

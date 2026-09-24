using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.GettingStarted;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Utilities;
using UnityEditor;

namespace Sirenix.OdinValidator.Editor
{
	public class OdinValidatorWelcomeError : RootObjectValidator<GlobalValidationConfig>
	{
		private class Fix
		{
			[GUIColor(0f, 1f, 0f, 1f)]
			[Button]
			public void OpenSetupWizard()
			{
				GettingStartedWindow.ShowWindow();
			}

			[Button]
			[GUIColor(1f, 0.5f, 0f, 1f)]
			public void SkipSetup()
			{
				GlobalConfig<GlobalValidationConfig>.Instance.HasShownValidationConfig = true;
				EditorUtility.SetDirty(GlobalConfig<GlobalValidationConfig>.Instance);
				string path = AssetDatabase.GetAssetPath(GlobalConfig<GlobalValidationConfig>.Instance);
				string guid = AssetDatabase.AssetPathToGUID(path);
				if (!string.IsNullOrWhiteSpace(guid))
				{
					foreach (ValidationSession item in ValidationSession.ActiveValidationSessions)
					{
						item.Enqueue(ValidationWorkItem.CreateForAssetGuid(guid, ProjectEventSource.Other), insert: true);
					}
				}
				AssetDatabase.SaveAssets();
			}
		}

		protected override void Validate(ValidationResult result)
		{
			if (!base.Object.HasShownValidationConfig)
			{
				result.AddError("Welcome to Odin Validator! Please read the full message below.\n\nRun the <b>Odin Validator Setup Wizard</b> from the Getting Started window and configure the validator to your liking.\n\nYou can open the Getting Started window from the bottom right corner of this window.\n\n").WithFix<Fix>("Run Validation Setup Wizard", delegate
				{
					GettingStartedWindow.ShowWindow();
				});
			}
		}
	}
}

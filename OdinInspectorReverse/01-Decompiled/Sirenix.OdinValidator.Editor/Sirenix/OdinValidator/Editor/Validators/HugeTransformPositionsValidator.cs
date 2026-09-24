using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor.Validators
{
	public class HugeTransformPositionsValidator : RootObjectValidator<Transform>, IDefinesGenericMenuItems
	{
		[Tooltip("The size limit in any x, y, or z dimension.")]
		public float SizeLimit = 100000f;

		[Tooltip("The severity of the validation result.")]
		public ValidatorSeverity Severity = ValidatorSeverity.Warning;

		protected override void Validate(ValidationResult result)
		{
			Vector3 p = base.Object.position;
			if (Math.Abs(p.x) > SizeLimit || Math.Abs(p.y) > SizeLimit || Math.Abs(p.z) > SizeLimit)
			{
				result.Add(Severity, "Due to floating-point precision limitations, it is recommended to bring the world coordinates of the GameObject within a smaller range.");
			}
		}

		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			genericMenu.AddItem(new GUIContent("See Rule Settings/For me only"), on: false, delegate
			{
				ValidationSessionEditor.OpenRuleSettingsWindow(typeof(HugeTransformPositionsValidator), ConfigSourceType.Local);
			});
			genericMenu.AddItem(new GUIContent("See Rule Settings/For everyone"), on: false, delegate
			{
				ValidationSessionEditor.OpenRuleSettingsWindow(typeof(HugeTransformPositionsValidator), ConfigSourceType.Project);
			});
		}
	}
}

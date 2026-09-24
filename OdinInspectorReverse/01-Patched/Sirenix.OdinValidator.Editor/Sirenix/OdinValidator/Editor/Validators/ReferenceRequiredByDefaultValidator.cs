using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor.Validators
{
	public class ReferenceRequiredByDefaultValidator : ValueValidator<object>, IDefinesGenericMenuItems
	{
		[Tooltip("The severity of the validation result.")]
		public ValidatorSeverity ValidatorSeverity;

		[Tooltip("If true, all namespaces will be included by default, and you can exclude specific namespaces.")]
		[ToggleLeft]
		public bool IncludeAllByDefault;

		[Tooltip("Namespaces to include in the validation. Only used if IncludeAllByDefault is false.")]
		[Optional]
		[DisableIf("IncludeAllByDefault")]
		public string[] IncludedNamespaces = new string[0];

		[Tooltip("Namespaces to exclude from the validation. Only used if IncludeAllByDefault is true.")]
		[Optional]
		public string[] ExcludedNamespaces = new string[0];

		public override bool CanValidateProperty(InspectorProperty property)
		{
			if (property.ValueEntry.TypeOfValue.IsClass)
			{
				return property.ValueEntry.TypeOfValue != typeof(string);
			}
			return false;
		}

		protected override void Validate(ValidationResult result)
		{
			if (Filter() && (base.Value == null || (base.Value is Object && (base.Value as Object).SafeIsUnityNull())) && base.Property.GetAttribute<OptionalAttribute>() == null)
			{
				result.Add(ValidatorSeverity, "\"" + base.Property.NiceName + "\" should not be null.");
			}
		}

		private bool Filter()
		{
			string name = base.Property.ParentType.GetNiceFullName();
			for (int i = 0; i < ExcludedNamespaces.Length; i++)
			{
				if (name.StartsWith(ExcludedNamespaces[i]))
				{
					return false;
				}
			}
			if (IncludeAllByDefault)
			{
				return true;
			}
			for (int j = 0; j < IncludedNamespaces.Length; j++)
			{
				if (name.StartsWith(IncludedNamespaces[j]))
				{
					return true;
				}
			}
			return false;
		}

		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			genericMenu.AddItem(new GUIContent("See Rule Settings/For me only"), on: false, delegate
			{
				ValidationSessionEditor.OpenRuleSettingsWindow(typeof(ReferenceRequiredByDefaultValidator), ConfigSourceType.Local);
			});
			genericMenu.AddItem(new GUIContent("See Rule Settings/For everyone"), on: false, delegate
			{
				ValidationSessionEditor.OpenRuleSettingsWindow(typeof(ReferenceRequiredByDefaultValidator), ConfigSourceType.Project);
			});
		}
	}
}

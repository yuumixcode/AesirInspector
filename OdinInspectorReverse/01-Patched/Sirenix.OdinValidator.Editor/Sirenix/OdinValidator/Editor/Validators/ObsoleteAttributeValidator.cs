using System;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor.Validators
{
	public class ObsoleteAttributeValidator<T> : RootObjectValidator<T> where T : UnityEngine.Object
	{
		protected override bool CanValidateObject(T obj)
		{
			return typeof(T).IsDefined<ObsoleteAttribute>();
		}

		protected override bool CanValidateRootProperty(InspectorProperty rootProperty)
		{
			return typeof(T).IsDefined<ObsoleteAttribute>();
		}

		protected override void Validate(ValidationResult result)
		{
			ObsoleteAttribute attr = base.Property.GetAttribute<ObsoleteAttribute>();
			bool error = attr.IsError;
			result.Message = typeof(T).GetNiceFullName() + " is marked obsolete.";
			if (!string.IsNullOrEmpty(attr.Message))
			{
				ref string message = ref result.Message;
				message = message + " " + attr.Message;
			}
			result.ResultType = (error ? ValidationResultType.Error : ValidationResultType.Warning);
		}
	}
}

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;

namespace Sirenix.OdinValidator.Editor
{
	public class OdinValidationPolicy
	{
		public static OdinValidationPolicy Default;

		private static HashSet<Type> typesDrawnByOdin;

		static OdinValidationPolicy()
		{
			Default = new OdinValidationPolicy();
			typesDrawnByOdin = InspectorTypeDrawingConfigDrawer.GetAllTypesDrawnByOdin();
		}

		public virtual bool ShouldValidate(OdinValidationRunner runner, object value)
		{
			return true;
		}

		public virtual bool ShouldDeeplyValidate(OdinValidationRunner runner, object value)
		{
			bool shouldValidate = true;
			Type type = value.GetType();
			if (!runner.Config.DeepValidation)
			{
				shouldValidate = typesDrawnByOdin.Contains(type);
				if (!shouldValidate)
				{
					Type editorType = GlobalConfig<InspectorConfig>.Instance.DrawingConfig.GetEditorType(type);
					if (editorType != null)
					{
						shouldValidate = editorType.InheritsFrom(typeof(OdinEditor));
					}
				}
			}
			return shouldValidate;
		}

		public virtual bool CustomValidatorFilter(OdinValidationRunner runner, Type type)
		{
			return true;
		}
	}
}

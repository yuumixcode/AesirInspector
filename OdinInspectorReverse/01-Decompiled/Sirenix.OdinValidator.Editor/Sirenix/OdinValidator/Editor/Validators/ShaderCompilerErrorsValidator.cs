using System.Linq;
using System.Runtime.Serialization;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor.Validators
{
	public class ShaderCompilerErrorsValidator : RootObjectValidator<Shader>
	{
		[Tooltip("The severity of the validation result.")]
		public ValidatorSeverity ErrorSeverity;

		[Tooltip("The severity of the validation result.")]
		public ValidatorSeverity WarningSeverity = ValidatorSeverity.Warning;

		[Tooltip("Shaders to ignore, even if they have errors. You can also ignore shaders by right-clicking shader issues in the validator.")]
		public Shader[] IgnoredShaders = new Shader[0];

		[OnDeserializing]
		private void OnDeserializing()
		{
			ErrorSeverity = ValidatorSeverity.Error;
			WarningSeverity = ValidatorSeverity.Warning;
		}

		protected override void Validate(ValidationResult result)
		{
			if (ErrorSeverity == ValidatorSeverity.Ignore && WarningSeverity == ValidatorSeverity.Ignore)
			{
				return;
			}
			Shader shader = base.Object;
			if (IgnoredShaders.Contains(shader))
			{
				return;
			}
			ShaderMessage[] arr = ShaderUtil.GetShaderMessages(shader);
			if (arr == null)
			{
				return;
			}
			ShaderMessage[] array = arr;
			for (int i = 0; i < array.Length; i++)
			{
				ShaderMessage m = array[i];
				ValidatorSeverity severity = ErrorSeverity;
				if (m.severity == ShaderCompilerMessageSeverity.Warning)
				{
					severity = WarningSeverity;
				}
				result.Add(severity, m.message).WithMetaData("Platform", m.platform).WithMetaData("Line", m.line)
					.WithMetaData("Message Details", m.messageDetails)
					.WithModifyRuleDataContextClick("Ignore errors from this shader", delegate(ShaderCompilerErrorsValidator data)
					{
						data.IgnoredShaders = data.IgnoredShaders.Append(shader).ToArray();
					});
			}
		}
	}
}

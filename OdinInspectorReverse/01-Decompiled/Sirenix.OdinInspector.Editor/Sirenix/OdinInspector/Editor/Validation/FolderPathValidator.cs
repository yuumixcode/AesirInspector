using System.IO;
using Sirenix.OdinInspector.Editor.ValueResolvers;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public sealed class FolderPathValidator : AttributeValidator<FolderPathAttribute, string>
	{
		private bool requireExistingPath;

		private ValueResolver<string> parentPathProvider;

		protected override void Initialize()
		{
			requireExistingPath = base.Attribute.RequireExistingPath;
			if (requireExistingPath)
			{
				parentPathProvider = ValueResolver.GetForString(base.Property, base.Attribute.ParentFolder);
			}
		}

		protected override void Validate(ValidationResult result)
		{
			if (requireExistingPath)
			{
				string path = base.ValueEntry.SmartValue ?? string.Empty;
				string parent = parentPathProvider.GetValue() ?? string.Empty;
				if (!string.IsNullOrEmpty(parent))
				{
					path = Path.Combine(parent, path);
				}
				if (Directory.Exists(path.TrimEnd('/', '\\') + "/"))
				{
					result.ResultType = ValidationResultType.Valid;
					return;
				}
				result.ResultType = ValidationResultType.Error;
				result.Message = "The path '" + path + "' does not exist.";
			}
			else
			{
				result.ResultType = ValidationResultType.IgnoreResult;
			}
		}
	}
}

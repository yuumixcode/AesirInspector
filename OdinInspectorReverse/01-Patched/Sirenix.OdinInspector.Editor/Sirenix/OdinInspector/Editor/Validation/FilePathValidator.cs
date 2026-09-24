using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector.Editor.ValueResolvers;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public sealed class FilePathValidator : AttributeValidator<FilePathAttribute, string>
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
				if (!base.Attribute.IncludeFileExtension)
				{
					string directoryPath = Path.GetDirectoryName(path);
					if (directoryPath == null)
					{
						return;
					}
					string fileName = Path.GetFileName(path);
					string[] files = Directory.GetFiles(directoryPath, fileName + ".*");
					if (string.IsNullOrEmpty(base.Attribute.Extensions))
					{
						if (files.Length != 0)
						{
							result.ResultType = ValidationResultType.Valid;
							return;
						}
						result.ResultType = ValidationResultType.Error;
						result.Message = "The path does not exist.";
						return;
					}
					IEnumerable<string> splitExtensions = from e in base.Attribute.Extensions.Replace(" ", "").Split(new char[1] { ',' })
						select (!e.StartsWith(".")) ? ("." + e) : e;
					bool foundFile = false;
					string[] array = files;
					foreach (string file in array)
					{
						string fileExtension = Path.GetExtension(file);
						if (splitExtensions.Contains(fileExtension))
						{
							result.ResultType = ValidationResultType.Valid;
							foundFile = true;
							break;
						}
					}
					if (!foundFile)
					{
						result.ResultType = ValidationResultType.Error;
						result.Message = "The path does not exist. These are the valid paths:\n\n" + string.Join("\n", splitExtensions.Select((string e) => path + e));
					}
				}
				else if (File.Exists(path))
				{
					result.ResultType = ValidationResultType.Valid;
				}
				else
				{
					result.ResultType = ValidationResultType.Error;
					result.Message = "The path does not exist.";
				}
			}
			else
			{
				result.ResultType = ValidationResultType.IgnoreResult;
			}
		}
	}
}

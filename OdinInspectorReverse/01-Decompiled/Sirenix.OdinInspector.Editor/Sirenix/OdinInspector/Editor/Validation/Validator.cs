using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public abstract class Validator : IValidator
	{
		public class MetaData : List<ResultItemMetaData>
		{
			public void Add(string key, object value)
			{
				Add(new ResultItemMetaData(key, value));
			}
		}

		public InspectorProperty Property { get; private set; }

		public virtual RevalidationCriteria RevalidationCriteria => RevalidationCriteria.Always;

		public void Initialize(InspectorProperty property)
		{
			Property = property;
			Initialize();
		}

		public virtual bool CanValidateProperty(InspectorProperty property)
		{
			return true;
		}

		public virtual void RunValidation(ref ValidationResult result)
		{
		}

		protected virtual void Initialize()
		{
		}

		public void InitializeResult(ref ValidationResult result)
		{
			if (result == null)
			{
				result = new ValidationResult();
			}
			result.Setup = new ValidationSetup
			{
				ParentInstance = Property.ParentValues[0],
				Validator = this,
				Root = (Property.SerializationRoot.ValueEntry.WeakValues[0] as Object)
			};
			result.Path = Property.Path;
			result.ResultType = ValidationResultType.Valid;
			result.Message = "";
		}
	}
}

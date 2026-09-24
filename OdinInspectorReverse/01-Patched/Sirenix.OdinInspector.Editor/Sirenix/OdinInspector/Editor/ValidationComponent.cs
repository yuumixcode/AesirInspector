using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public sealed class ValidationComponent : PropertyComponent, IDisposable
	{
		public readonly IValidatorLocator ValidatorLocator;

		private IList<Validator> validators;

		private static readonly Stopwatch Stopwatch = new Stopwatch();

		public ValidationComponent(InspectorProperty property, IValidatorLocator validatorLocator)
			: base(property)
		{
			ValidatorLocator = validatorLocator;
		}

		public void Dispose()
		{
			if (validators == null)
			{
				return;
			}
			for (int i = 0; i < validators.Count; i++)
			{
				if (validators[i] is IDisposable disposable)
				{
					try
					{
						disposable.Dispose();
					}
					catch (Exception exception)
					{
						UnityEngine.Debug.LogException(exception);
					}
				}
			}
			validators = null;
		}

		public IList<Validator> GetValidators()
		{
			if (validators == null)
			{
				if (ValidatorLocator.PotentiallyHasValidatorsFor(Property))
				{
					validators = ValidatorLocator.GetValidators(Property);
				}
				else
				{
					validators = new Validator[0];
				}
			}
			return validators;
		}

		public override void Reset()
		{
			validators = null;
		}

		public void ValidateProperty(ref List<ValidationResult> results, bool explodeMultiResults = false)
		{
			if (results == null)
			{
				results = new List<ValidationResult>();
			}
			if (validators == null)
			{
				GetValidators();
			}
			for (int i = 0; i < validators.Count; i++)
			{
				Validator validator = validators[i];
				ValidationResult result = new ValidationResult();
				try
				{
					Stopwatch.Restart();
					validator.RunValidation(ref result);
					Stopwatch.Stop();
					if (result != null)
					{
						if (explodeMultiResults)
						{
							result.Explode(ref results, Stopwatch.Elapsed.TotalMilliseconds);
							continue;
						}
						result.ValidationTimeMS = Stopwatch.Elapsed.TotalMilliseconds;
						results.Add(result);
					}
				}
				catch (Exception innerException)
				{
					Stopwatch.Stop();
					while (innerException is TargetInvocationException)
					{
						innerException = innerException.InnerException;
					}
					List<ValidationResult> obj = results;
					ValidationResult validationResult = new ValidationResult();
					validationResult.Message = "Exception was thrown during validation of property " + Property.NiceName + ": " + innerException.ToString();
					validationResult.ResultType = ValidationResultType.Error;
					validationResult.Setup = new ValidationSetup
					{
						ParentInstance = Property.ParentValues[0],
						Root = Property.SerializationRoot.ValueEntry.WeakValues[0],
						Validator = validator
					};
					validationResult.Path = Property.Path;
					validationResult.ValidationTimeMS = Stopwatch.Elapsed.TotalMilliseconds;
					obj.Add(validationResult);
				}
			}
		}
	}
}

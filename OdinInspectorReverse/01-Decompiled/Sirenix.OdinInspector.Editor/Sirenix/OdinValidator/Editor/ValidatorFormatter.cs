using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Serialization;

namespace Sirenix.OdinValidator.Editor
{
	public class ValidatorFormatter<T> : ReflectionOrEmittedBaseFormatter<T> where T : Validator, new()
	{
		protected override T GetUninitializedObject()
		{
			return new T();
		}
	}
}

using Sirenix.OdinInspector.Editor.ValueResolvers;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public class IfAttributeHelper
	{
		private readonly ValueResolver<object> valueResolver;

		private bool result;

		public bool DefaultResult;

		public string ErrorMessage { get; private set; }

		public IfAttributeHelper(InspectorProperty property, string memberName, bool defaultResult = false)
		{
			valueResolver = ValueResolver.Get<object>(property, memberName);
			ErrorMessage = valueResolver.ErrorMessage;
			DefaultResult = defaultResult;
		}

		public bool GetValue(object value)
		{
			if (ErrorMessage == null)
			{
				result = false;
				object resolvedValue = valueResolver.GetValue();
				if (resolvedValue is Object)
				{
					result = (Object)resolvedValue != null;
				}
				else if (resolvedValue is bool)
				{
					result = (bool)resolvedValue;
				}
				else if (resolvedValue is string)
				{
					result = !string.IsNullOrEmpty((string)resolvedValue);
				}
				else if (value == null)
				{
					if (resolvedValue != null)
					{
						result = true;
					}
				}
				else if (object.Equals(resolvedValue, value))
				{
					result = true;
				}
				return result;
			}
			return DefaultResult;
		}
	}
}

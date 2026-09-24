using System;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public static class ValidationEvents
	{
		public static event Action<ValidationStateChangeInfo> OnValidationStateChanged;

		internal static void InvokeOnValidationStateChanged(ValidationStateChangeInfo info)
		{
			if (ValidationEvents.OnValidationStateChanged != null)
			{
				ValidationEvents.OnValidationStateChanged(info);
			}
		}
	}
}

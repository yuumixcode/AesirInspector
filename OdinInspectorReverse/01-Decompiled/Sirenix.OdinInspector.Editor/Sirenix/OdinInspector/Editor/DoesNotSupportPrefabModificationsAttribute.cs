using System;

namespace Sirenix.OdinInspector.Editor
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	internal sealed class DoesNotSupportPrefabModificationsAttribute : Attribute
	{
	}
}

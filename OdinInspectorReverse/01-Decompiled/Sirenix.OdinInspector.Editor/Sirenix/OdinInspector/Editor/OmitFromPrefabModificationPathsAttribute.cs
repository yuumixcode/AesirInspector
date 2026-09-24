using System;

namespace Sirenix.OdinInspector.Editor
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	internal sealed class OmitFromPrefabModificationPathsAttribute : Attribute
	{
	}
}

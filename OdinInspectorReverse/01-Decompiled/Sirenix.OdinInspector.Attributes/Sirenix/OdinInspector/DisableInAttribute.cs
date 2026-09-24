using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Disables a member based on which type of a prefab and instance it is in. 
	/// </summary>
	[DontApplyToListElements]
	[AttributeUsage(AttributeTargets.All)]
	[Conditional("UNITY_EDITOR")]
	public class DisableInAttribute : Attribute
	{
		public PrefabKind PrefabKind;

		public DisableInAttribute(PrefabKind prefabKind)
		{
			PrefabKind = prefabKind;
		}
	}
}

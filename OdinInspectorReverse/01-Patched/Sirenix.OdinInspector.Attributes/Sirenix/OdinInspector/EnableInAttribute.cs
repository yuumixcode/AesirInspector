using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Enables a member based on which type of a prefab and instance it is. 
	/// </summary>
	[DontApplyToListElements]
	[AttributeUsage(AttributeTargets.All)]
	[Conditional("UNITY_EDITOR")]
	public class EnableInAttribute : Attribute
	{
		public PrefabKind PrefabKind;

		public EnableInAttribute(PrefabKind prefabKind)
		{
			PrefabKind = prefabKind;
		}
	}
}

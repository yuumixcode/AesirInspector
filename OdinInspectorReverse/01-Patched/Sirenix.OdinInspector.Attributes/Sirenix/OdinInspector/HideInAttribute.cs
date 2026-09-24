using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Hides a member based on which type of a prefab and instance it is in. 
	/// </summary>
	[AttributeUsage(AttributeTargets.All)]
	[Conditional("UNITY_EDITOR")]
	public class HideInAttribute : Attribute
	{
		public PrefabKind PrefabKind;

		public HideInAttribute(PrefabKind prefabKind)
		{
			PrefabKind = prefabKind;
		}
	}
}

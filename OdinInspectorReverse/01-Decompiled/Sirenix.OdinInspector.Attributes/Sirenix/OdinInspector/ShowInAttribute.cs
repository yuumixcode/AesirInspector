using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Shows a member based on which type of a prefab and instance it is in. 
	/// </summary>
	[AttributeUsage(AttributeTargets.All)]
	[Conditional("UNITY_EDITOR")]
	public class ShowInAttribute : Attribute
	{
		public PrefabKind PrefabKind;

		public ShowInAttribute(PrefabKind prefabKind)
		{
			PrefabKind = prefabKind;
		}
	}
}

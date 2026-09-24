using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Makes a member required based on which type of a prefab and instance it is in. 
	/// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public class RequiredInAttribute : Attribute
	{
		public string ErrorMessage;

		public PrefabKind PrefabKind;

		public RequiredInAttribute(PrefabKind kind)
		{
			PrefabKind = kind;
		}
	}
}

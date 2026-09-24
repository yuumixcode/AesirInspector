using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// The ChildGameObjectsOnly attribute can be used on Components and GameObject fields and will prepend a small button next to the object-field that
	/// will search through all child gameobjects for assignable objects and present them in a dropdown for the user to choose from.
	/// </summary>
	/// <seealso cref="T:System.Attribute" />
	[Conditional("UNITY_EDITOR")]
	public class ChildGameObjectsOnlyAttribute : Attribute
	{
		public bool IncludeSelf = true;

		public bool IncludeInactive;
	}
}

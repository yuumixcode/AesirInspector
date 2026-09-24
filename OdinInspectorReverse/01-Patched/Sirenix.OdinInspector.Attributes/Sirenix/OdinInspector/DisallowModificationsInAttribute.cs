using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// DisallowModificationsIn disables / grays out members, preventing modifications from being made and enables validation,
	/// providing error messages in case a modification was made prior to introducing the attribute.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.DisableInAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.HideInAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.RequiredAttribute" />
	[Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public sealed class DisallowModificationsInAttribute : Attribute
	{
		public PrefabKind PrefabKind;

		public DisallowModificationsInAttribute(PrefabKind kind)
		{
			PrefabKind = kind;
		}
	}
}

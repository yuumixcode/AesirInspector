using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Hides a property if it is drawn from a non prefab instance or asset.
	/// </summary>
	[DontApplyToListElements]
	[AttributeUsage(AttributeTargets.All)]
	[Conditional("UNITY_EDITOR")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use [HideIn(PrefabKind.NonPrefabInstance)] instead.", false)]
	public class HideInNonPrefabsAttribute : Attribute
	{
	}
}

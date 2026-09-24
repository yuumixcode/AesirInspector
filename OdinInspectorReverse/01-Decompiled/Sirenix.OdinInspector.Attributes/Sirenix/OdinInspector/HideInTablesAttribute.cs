using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// The HideInTables attribute is used to prevent members from showing up as columns in tables drawn using the <see cref="T:Sirenix.OdinInspector.TableListAttribute" />.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
	[Conditional("UNITY_EDITOR")]
	public class HideInTablesAttribute : Attribute
	{
	}
}

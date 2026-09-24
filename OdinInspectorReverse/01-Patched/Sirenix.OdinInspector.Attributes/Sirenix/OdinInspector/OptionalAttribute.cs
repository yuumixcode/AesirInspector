using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Overrides the 'Reference Required by Default' rule to allow for null values.
	/// Has no effect if the rule is disabled.
	///
	/// This attribute does not do anything unless you have Odin Validator and the 'Reference Required by Default' rule is enabled.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public class OptionalAttribute : Attribute
	{
	}
}

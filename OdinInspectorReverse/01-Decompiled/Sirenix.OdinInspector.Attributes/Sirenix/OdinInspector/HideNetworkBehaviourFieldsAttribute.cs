using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Apply HideNetworkBehaviourFields to your class to prevent the special "Network Channel" and "Network Send Interval" properties from being shown in the inspector for a NetworkBehaviour.
	/// This attribute has no effect on classes that are not derived from NetworkBehaviour.
	/// </summary>
	/// <example>
	/// <para>The following example shows how to use this attribute.</para>
	/// <code>
	/// [HideNetworkBehaviourFields]
	/// public class MyComponent : NetworkBehaviour
	/// {
	///     // The "Network Channel" and "Network Send Interval" properties will not be shown for this component in the inspector
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:System.Attribute" />
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public sealed class HideNetworkBehaviourFieldsAttribute : Attribute
	{
	}
}

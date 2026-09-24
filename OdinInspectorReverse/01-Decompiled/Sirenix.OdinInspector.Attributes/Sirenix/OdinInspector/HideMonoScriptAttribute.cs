using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Apply HideMonoScript to your class to prevent the Script property from being shown in the inspector.
	/// <remarks>
	/// <para>This attribute has the same effect on a single type that the global configuration option "Show Mono Script In Editor" in "Preferences -&gt; Odin Inspector -&gt; General -&gt; Drawers" has globally when disabled.</para>
	/// </remarks>
	/// </summary>
	/// <example>
	/// <para>The following example shows how to use this attribute.</para>
	/// <code>
	/// [HideMonoScript]
	/// public class MyComponent : MonoBehaviour
	/// {
	///     // The Script property will not be shown for this component in the inspector
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:System.Attribute" />
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public sealed class HideMonoScriptAttribute : Attribute
	{
	}
}

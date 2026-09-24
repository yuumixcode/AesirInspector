using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>
	/// ShowPropertyResolver shows the property resolver responsible for bringing the member into the property tree.
	/// This is useful in situations where you want to debug why a particular member that is normally not shown in the inspector suddenly is.
	/// </para>
	/// </summary>
	/// <example>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	/// 	[ShowPropertyResolver]
	/// 	public int IndentedInt;
	/// }
	/// </code>
	/// </example>
	[Conditional("UNITY_EDITOR")]
	public class ShowPropertyResolverAttribute : Attribute
	{
	}
}

using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>Draws an enum in a horizontal button group instead of a dropdown.</para>
	/// </summary>
	/// <example>
	/// <code>
	/// public class MyComponent : MonoBehvaiour
	/// {
	///     [EnumToggleButtons]
	///     public MyBitmaskEnum MyBitmaskEnum;
	///
	///     [EnumToggleButtons]
	///     public MyEnum MyEnum;
	/// }
	///
	/// [Flags]
	/// public enum MyBitmaskEnum
	/// {
	///     A = 1 &lt;&lt; 1, // 1
	///     B = 1 &lt;&lt; 2, // 2
	///     C = 1 &lt;&lt; 3, // 4
	///     ALL = A | B | C
	/// }
	///
	/// public enum MyEnum
	/// {
	///     A,
	///     B,
	///     C
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:System.Attribute" />
	[Conditional("UNITY_EDITOR")]
	public class EnumToggleButtonsAttribute : Attribute
	{
	}
}

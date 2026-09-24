using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>Draws an enum selector in the inspector with next and previous buttons to let you cycle through the available values for the enum property.</para>
	/// </summary>
	/// <example>
	/// <code>
	/// public enum MyEnum
	/// {
	///     One,
	///     Two,
	///     Three,
	/// }
	///
	/// public class MyMonoBehaviour : MonoBehaviour
	/// {
	///     [EnumPaging]
	///     public MyEnum Value;
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:System.Attribute" />
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
	[Conditional("UNITY_EDITOR")]
	public class EnumPagingAttribute : Attribute
	{
	}
}

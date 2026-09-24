using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>ShowInInspector is used on any member, and shows the value in the inspector. Note that the value being shown due to this attribute DOES NOT mean that the value is being serialized.</para>
	/// </summary>
	/// <remarks>
	/// <para>This can for example be combined with <see cref="T:Sirenix.OdinInspector.ReadOnlyAttribute" /> to allow for live debugging of values.</para>
	/// <note type="note"></note>
	/// </remarks>
	/// <example>
	/// <para>The following example shows how ShowInInspector is used to show properties in the inspector, that otherwise wouldn't.</para>
	/// <code>
	///             	public class MyComponent : MonoBehaviour
	///             	{
	///             		[ShowInInspector]
	///             		private int myField;
	///
	///             		[ShowInInspector]
	///             		public int MyProperty { get; set; }
	///             	}
	/// </code>
	/// </example>
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
	[Conditional("UNITY_EDITOR")]
	public class ShowInInspectorAttribute : Attribute
	{
	}
}

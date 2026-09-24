using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>DisableIf is used on any property, and can disable or enable the property in the inspector.</para>
	/// <para>Use this to disable properties when they are irrelevant.</para>
	/// </summary>
	/// <example>
	/// <para>The following example shows how a property can be disabled by the state of a field.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	///             		public bool DisableProperty;
	///
	///             		[DisableIf("DisableProperty")]
	///             		public int MyInt;
	///
	///             	    public SomeEnum SomeEnumField;
	///
	///             		[DisableIf("SomeEnumField", SomeEnum.SomeEnumMember)]
	///             		public string SomeString;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// <para>The following examples show how a property can be disabled by a function.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	///             		[EnableIf("MyDisableFunction")]
	///             		public int MyInt;
	///
	///             		private bool MyDisableFunction()
	///             		{
	///             			// ...
	///             		}
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.EnableIfAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ShowIfAttribute" />
	[DontApplyToListElements]
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public sealed class DisableIfAttribute : Attribute
	{
		/// <summary>
		/// A resolved string that defines the condition to check the value of, such as a member name or an expression.
		/// </summary>
		public string Condition;

		/// <summary>
		/// The optional condition value.
		/// </summary>
		public object Value;

		/// <summary>
		/// The name of a bool member field, property or method. Obsolete; use the Condition member instead.
		/// </summary>
		[Obsolete("Use the Condition member instead.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public string MemberName
		{
			get
			{
				return Condition;
			}
			set
			{
				Condition = value;
			}
		}

		/// <summary>
		/// Disables a property in the inspector, based on the value of a resolved string.
		/// </summary>
		/// <param name="condition">A resolved string that defines the condition to check the value of, such as a member name or an expression.</param>
		public DisableIfAttribute(string condition)
		{
			Condition = condition;
		}

		/// <summary>
		/// Disables a property in the inspector, if the resolved string evaluates to the specified value.
		/// </summary>
		/// <param name="condition">A resolved string that defines the condition to check the value of, such as a member name or an expression.</param>
		/// <param name="optionalValue">Value to check against.</param>
		public DisableIfAttribute(string condition, object optionalValue)
		{
			Condition = condition;
			Value = optionalValue;
		}
	}
}

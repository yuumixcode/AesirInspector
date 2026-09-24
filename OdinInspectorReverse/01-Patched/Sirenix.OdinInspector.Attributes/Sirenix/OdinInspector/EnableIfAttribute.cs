using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>EnableIf is used on any property, and can enable or disable the property in the inspector.</para>
	/// <para>Use this to enable properties when they are relevant.</para>
	/// </summary>
	/// <example>
	/// <para>The following example shows how a property can be enabled by the state of a field.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	///             		public bool EnableProperty;
	///
	///             		[EnableIf("EnableProperty")]
	///             		public int MyInt;
	///
	///             	    public SomeEnum SomeEnumField;
	///
	///             		[EnableIf("SomeEnumField", SomeEnum.SomeEnumMember)]
	///             		public string SomeString;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// <para>The following examples show how a property can be enabled by a function.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	///             		[EnableIf("MyEnableFunction")]
	///             		public int MyInt;
	///
	///             		private bool MyEnableFunction()
	///             		{
	///             			// ...
	///             		}
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.DisableIfAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ShowIfAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.HideIfAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DisableInEditorModeAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DisableInPlayModeAttribute" />
	[DontApplyToListElements]
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public sealed class EnableIfAttribute : Attribute
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
		/// Enables a property in the inspector, based on the value of a resolved string.
		/// </summary>
		/// <param name="condition">A resolved string that defines the condition to check the value of, such as a member name or an expression.</param>
		public EnableIfAttribute(string condition)
		{
			Condition = condition;
		}

		/// <summary>
		/// Enables a property in the inspector, if the resolved string evaluates to the specified value.
		/// </summary>
		/// <param name="condition">A resolved string that defines the condition to check the value of, such as a member name or an expression.</param>
		/// <param name="optionalValue">Value to check against.</param>
		public EnableIfAttribute(string condition, object optionalValue)
		{
			Condition = condition;
			Value = optionalValue;
		}
	}
}

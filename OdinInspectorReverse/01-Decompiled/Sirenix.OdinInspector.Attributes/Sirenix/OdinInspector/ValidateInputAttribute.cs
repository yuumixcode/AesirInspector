using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>ValidateInput is used on any property, and allows to validate input from inspector.</para>
	/// <para>Use this to enforce correct values.</para>
	/// </summary>
	/// <remarks>
	/// <note type="note">ValidateInput refuses invalid values.</note>
	/// <note type="note">ValidateInput only works in the editor. Values changed through scripting will not be validated.</note>
	/// </remarks>
	/// <example>
	/// <para>The following examples shows how a speed value can be forced to be above 0.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	///             		[ValidateInput("ValidateInput")]
	///             		public float Speed;
	///
	///             		// Specify custom output message and message type.
	///             		[ValidateInput("ValidateInput", "Health must be more than 0!", InfoMessageType.Warning)]
	///             		public float Health;
	///
	///             		private bool ValidateInput(float property)
	///             		{
	///             			return property &gt; 0f;
	///             		}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// <para>The following example shows how a static function could also be used.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	///             		[ValidateInput("StaticValidateFunction")]
	///             		public int MyInt;
	///
	///             		private static bool StaticValidateFunction(int property)
	///             		{
	///             			return property != 0;
	///             		}
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.InfoBoxAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.RequiredAttribute" />
	[DontApplyToListElements]
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public sealed class ValidateInputAttribute : Attribute
	{
		/// <summary>
		/// Default message for invalid values.
		/// </summary>
		public string DefaultMessage;

		/// <summary>
		/// A resolved string that should evaluate to a boolean value, and which should validate the input. Note that in expressions, the $value named parameter, and in methods, a parameter named value, can be used to get the validated value instead of referring to the value by its containing member. This makes it easier to reuse validation strings.
		/// </summary>
		public string Condition;

		/// <summary>
		/// The type of the message.
		/// </summary>
		public InfoMessageType MessageType;

		/// <summary>
		/// Whether to also trigger validation when changes to child values happen. This is true by default.
		/// </summary>
		public bool IncludeChildren;

		/// <summary>
		/// If true, the validation method will not only be executed when the User has changed the value. It'll run once every frame in the inspector.
		/// </summary>
		[LabelWidth(170f)]
		public bool ContinuousValidationCheck;

		/// <summary>
		/// OBSOLETE; use the Condition member instead.
		/// A resolved string that should evaluate to a boolean value, and which should validate the input. Note that in expressions, the $value named parameter, and in methods, a parameter named value, can be used to get the validated value instead of referring to the value by its containing member. This makes it easier to reuse validation strings.
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

		[Obsolete("Use the ContinuousValidationCheck member instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ContiniousValidationCheck
		{
			get
			{
				return ContinuousValidationCheck;
			}
			set
			{
				ContinuousValidationCheck = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.ValidateInputAttribute" /> class.
		/// </summary>
		/// <param name="condition">A resolved string that should evaluate to a boolean value, and which should validate the input. Note that in expressions, the $value named parameter, and in methods, a parameter named value, can be used to get the validated value instead of referring to the value by its containing member. This makes it easier to reuse validation strings.</param>
		/// <param name="defaultMessage">Default message for invalid values.</param>
		/// <param name="messageType">Type of the message.</param>
		public ValidateInputAttribute(string condition, string defaultMessage = null, InfoMessageType messageType = InfoMessageType.Error)
		{
			Condition = condition;
			DefaultMessage = defaultMessage;
			MessageType = messageType;
			IncludeChildren = true;
		}

		/// <summary>
		/// Obsolete. Rejecting invalid input is no longer supported. Use the other constructors instead.
		/// </summary>
		/// <param name="condition">Obsolete overload.</param>
		/// <param name="message">Obsolete overload.</param>
		/// <param name="messageType">Obsolete overload.</param>
		/// <param name="rejectedInvalidInput">Obsolete overload.</param>
		[Obsolete("Rejecting invalid input is no longer supported. Use the other constructor instead.", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ValidateInputAttribute(string condition, string message, InfoMessageType messageType, bool rejectedInvalidInput)
		{
			Condition = condition;
			DefaultMessage = message;
			MessageType = messageType;
			IncludeChildren = true;
		}
	}
}

using System;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Indicates that an instance field or auto-property was previously serialized with a different name, so that values serialized with the old name will be properly deserialized into this member.
	///
	/// This does the same as Unity's FormerlySerializedAs attribute, except it can also be applied to properties.
	/// </summary>
	/// <seealso cref="T:System.Attribute" />
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class PreviouslySerializedAsAttribute : Attribute
	{
		/// <summary>
		/// The former name.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Serialization.PreviouslySerializedAsAttribute" /> class.
		/// </summary>
		/// <param name="name">The former name.</param>
		public PreviouslySerializedAsAttribute(string name)
		{
			Name = name;
		}
	}
}

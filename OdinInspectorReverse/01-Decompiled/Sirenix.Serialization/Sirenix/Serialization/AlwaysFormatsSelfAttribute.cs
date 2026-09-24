using System;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Use this attribute to specify that a type that implements the <see cref="T:Sirenix.Serialization.ISelfFormatter" />
	/// interface should *always* format itself regardless of other formatters being specified.
	/// <para />
	/// This means that the interface will be used to format all types derived from the type that
	/// is decorated with this attribute, regardless of custom formatters for the derived types.
	/// </summary>
	/// <seealso cref="T:System.Attribute" />
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
	public sealed class AlwaysFormatsSelfAttribute : Attribute
	{
	}
}

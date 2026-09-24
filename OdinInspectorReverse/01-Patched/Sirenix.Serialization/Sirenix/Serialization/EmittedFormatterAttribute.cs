using System;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Indicates that this formatter type has been emitted. Never put this on a type!
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class EmittedFormatterAttribute : Attribute
	{
	}
}

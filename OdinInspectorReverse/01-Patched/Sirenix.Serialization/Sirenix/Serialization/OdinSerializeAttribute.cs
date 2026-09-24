using System;
using JetBrains.Annotations;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Indicates that an instance field or auto-property should be serialized by Odin.
	/// </summary>
	/// <seealso cref="T:System.Attribute" />
	[MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class OdinSerializeAttribute : Attribute
	{
	}
}

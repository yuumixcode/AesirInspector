using System;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// When this attribute is added is added to another attribute, then attributes from that attribute
	/// will also be added to the property in the attribute processing step.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class IncludeMyAttributesAttribute : Attribute
	{
	}
}

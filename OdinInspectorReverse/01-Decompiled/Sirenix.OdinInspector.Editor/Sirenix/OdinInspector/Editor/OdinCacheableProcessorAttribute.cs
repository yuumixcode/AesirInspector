using System;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Marks an <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeProcessor" /> or <see cref="T:Sirenix.OdinInspector.Editor.OdinPropertyProcessor" /> as cacheable.
	/// </summary>
	/// <remarks>
	/// Only mark a processor as cacheable if it always produces the same attributes for the same properties in the same order.  
	/// Caching is applied only when all processors that run on a property are cacheable; if any running processor is not, the result will not be cached.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class OdinCacheableProcessorAttribute : Attribute
	{
	}
}

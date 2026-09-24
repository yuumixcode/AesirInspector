using System;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Priority for <see cref="T:Sirenix.OdinInspector.Editor.OdinPropertyResolver" /> and <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeProcessor" /> types.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class ResolverPriorityAttribute : Attribute
	{
		/// <summary>
		/// Priority of the resolver.
		/// </summary>
		public readonly double Priority;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.ResolverPriorityAttribute" /> class.
		/// </summary>
		/// <param name="priority">The higher the priority, the earlier it will be processed.</param>
		public ResolverPriorityAttribute(double priority)
		{
			Priority = priority;
		}
	}
}

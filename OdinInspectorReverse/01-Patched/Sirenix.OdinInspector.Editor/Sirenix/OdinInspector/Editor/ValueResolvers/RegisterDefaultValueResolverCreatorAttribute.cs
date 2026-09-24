using System;

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
	/// <summary>
	/// This attribute can be placed on an assembly to register a value resolver creator that should be queried when a value resolver is being created.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class RegisterDefaultValueResolverCreatorAttribute : Attribute
	{
		public Type ResolverCreatorType;

		public double Order;

		/// <summary>
		/// This attribute can be placed on an assembly to register a value resolver creator that should be queried when a value resolver is being created.
		/// </summary>
		/// <param name="resolverCreatorType">The resolver </param>
		/// <param name="order"></param>
		public RegisterDefaultValueResolverCreatorAttribute(Type resolverCreatorType, double order)
		{
			ResolverCreatorType = resolverCreatorType;
			Order = order;
		}
	}
}

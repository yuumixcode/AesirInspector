using System;

namespace Sirenix.OdinInspector.Editor.ActionResolvers
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class RegisterDefaultActionResolverAttribute : Attribute
	{
		public Type ResolverType;

		public double Order;

		public RegisterDefaultActionResolverAttribute(Type resolverType, double order)
		{
			ResolverType = resolverType;
			Order = order;
		}
	}
}

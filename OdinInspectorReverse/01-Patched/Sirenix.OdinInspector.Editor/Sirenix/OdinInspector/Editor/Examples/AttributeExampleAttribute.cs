using System;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
	public class AttributeExampleAttribute : Attribute
	{
		public Type AttributeType;

		public string Name;

		public string Description;

		public float Order;

		public AttributeExampleAttribute(Type type)
		{
			AttributeType = type;
		}

		public AttributeExampleAttribute(Type type, string description)
		{
			AttributeType = type;
			Description = description;
		}
	}
}

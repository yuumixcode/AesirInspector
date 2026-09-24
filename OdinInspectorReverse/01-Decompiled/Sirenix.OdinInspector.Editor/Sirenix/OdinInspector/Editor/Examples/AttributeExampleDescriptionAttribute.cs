using System;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
	public class AttributeExampleDescriptionAttribute : Attribute
	{
		public string Description;

		public AttributeExampleDescriptionAttribute(string description)
		{
			Description = description;
		}
	}
}

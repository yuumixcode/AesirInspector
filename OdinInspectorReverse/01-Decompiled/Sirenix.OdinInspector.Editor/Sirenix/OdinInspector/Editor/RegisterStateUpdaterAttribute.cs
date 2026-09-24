using System;

namespace Sirenix.OdinInspector.Editor
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
	public class RegisterStateUpdaterAttribute : Attribute
	{
		public readonly Type Type;

		public readonly double Priority;

		public RegisterStateUpdaterAttribute(Type type, double priority = 0.0)
		{
			Type = type;
			Priority = priority;
		}
	}
}

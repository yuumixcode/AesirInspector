using System;

namespace Sirenix.Utilities.Editor
{
	internal class ShimNameAttribute : Attribute
	{
		public string Name;

		public ShimNameAttribute(string name)
		{
			Name = name;
		}
	}
}

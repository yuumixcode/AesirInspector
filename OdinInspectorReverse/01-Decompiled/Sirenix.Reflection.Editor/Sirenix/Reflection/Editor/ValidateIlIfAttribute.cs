using System;

namespace Sirenix.Reflection.Editor
{
	public class ValidateIlIfAttribute : Attribute
	{
		public string Expression;

		public ValidateIlIfAttribute(string expression)
		{
			Expression = expression;
		}
	}
}

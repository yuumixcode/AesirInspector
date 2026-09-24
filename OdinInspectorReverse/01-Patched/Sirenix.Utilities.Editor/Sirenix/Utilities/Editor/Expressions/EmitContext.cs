using System;

namespace Sirenix.Utilities.Editor.Expressions
{
	public class EmitContext
	{
		public bool IsStatic;

		public Type Type;

		public Type ReturnType;

		public Type[] Parameters;

		public string[] ParameterNames;
	}
}

using System.Reflection;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public struct ValidationPathStep
	{
		public string StepString;

		public object Value;

		public MemberInfo Member;
	}
}

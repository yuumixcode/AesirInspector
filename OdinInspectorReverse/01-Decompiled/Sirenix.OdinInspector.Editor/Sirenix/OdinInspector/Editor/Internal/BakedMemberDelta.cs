using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal struct BakedMemberDelta
	{
		public MemberInfo Member;

		public WeakValueSetter Setter;

		public object Value;

		public BakedMemberDelta(ref MemberDelta delta)
		{
			Member = delta.Member;
			Setter = DesignerAttributeSetters.GetSetter(delta.Member);
			Value = delta.Value;
		}
	}
}

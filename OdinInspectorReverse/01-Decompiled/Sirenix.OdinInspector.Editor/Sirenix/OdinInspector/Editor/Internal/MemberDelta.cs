using System;
using System.Reflection;

namespace Sirenix.OdinInspector.Editor.Internal
{
	[Serializable]
	internal struct MemberDelta
	{
		public string SerializedMember;

		[NonSerialized]
		public MemberInfo Member;

		public object Value;
	}
}

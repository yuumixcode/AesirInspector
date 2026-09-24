using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Serialization;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public class SerializationPolicyMemberSelector : IMemberSelector
	{
		public readonly ISerializationPolicy Policy;

		public SerializationPolicyMemberSelector(ISerializationPolicy policy)
		{
			Policy = policy;
		}

		public IList<MemberInfo> SelectMembers(Type type)
		{
			return FormatterUtilities.GetSerializableMembers(type, Policy);
		}
	}
}

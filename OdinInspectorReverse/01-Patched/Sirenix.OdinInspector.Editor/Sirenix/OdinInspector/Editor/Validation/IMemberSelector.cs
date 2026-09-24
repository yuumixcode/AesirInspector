using System;
using System.Collections.Generic;
using System.Reflection;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public interface IMemberSelector
	{
		IList<MemberInfo> SelectMembers(Type type);
	}
}

using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	[ResolverPriority(-1000000.0)]
	[OdinCacheableProcessor]
	public class DelayedAttributeProcessor<T> : OdinPropertyProcessor<T, DelayedAttribute> where T : struct
	{
		public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos)
		{
			for (int i = 0; i < propertyInfos.Count; i++)
			{
				propertyInfos[i].GetEditableAttributesList().Add(new DelayedAttribute());
			}
		}
	}
}

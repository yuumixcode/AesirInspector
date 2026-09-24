using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor
{
	[ResolverPriority(-1000000.0)]
	[OdinCacheableProcessor]
	public class DelayedPropertyAttributeProcessor<T> : OdinPropertyProcessor<T, DelayedPropertyAttribute> where T : struct
	{
		public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos)
		{
			for (int i = 0; i < propertyInfos.Count; i++)
			{
				propertyInfos[i].GetEditableAttributesList().Add(new DelayedPropertyAttribute());
			}
		}
	}
}

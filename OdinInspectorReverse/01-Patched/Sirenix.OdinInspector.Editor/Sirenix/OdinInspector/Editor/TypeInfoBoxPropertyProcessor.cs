using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor
{
	[OdinCacheableProcessor]
	[ResolverPriority(-10.0)]
	public class TypeInfoBoxPropertyProcessor<T> : OdinPropertyProcessor<T, TypeInfoBoxAttribute>
	{
		public override void ProcessMemberProperties(List<InspectorPropertyInfo> memberInfos)
		{
			TypeInfoBoxAttribute attr = base.Property.GetAttribute<TypeInfoBoxAttribute>();
			memberInfos.AddDelegate($"InjectedTypeInfoBox({typeof(T).Name},{memberInfos.Count})", delegate
			{
			}, -100000f, new InfoBoxAttribute(attr.Message), new OnInspectorGUIAttribute("@"), new ExcludeInOdinDesignerAttribute());
		}
	}
}

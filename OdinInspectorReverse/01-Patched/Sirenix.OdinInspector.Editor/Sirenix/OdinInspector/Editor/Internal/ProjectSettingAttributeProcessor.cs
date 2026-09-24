using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.Internal
{
	public class ProjectSettingAttributeProcessor<TSetting, TValue> : OdinAttributeProcessor<TSetting> where TSetting : ProjectSetting<TValue>
	{
		public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
		{
			attributes.Add(new CustomContextMenuAttribute("Reset Config Value", "Reset"));
			if (member.Name != "Value")
			{
				return;
			}
			ImmutableList<Attribute> parentAttrs = parentProperty.Attributes;
			foreach (Attribute attr in parentAttrs)
			{
				if (!(attr is InlinePropertyAttribute) && !(attr is PropertyGroupAttribute) && !(attr is CustomContextMenuAttribute) && !(attr is ProjectSettingKeyAttribute) && !(attr is SuffixLabelAttribute))
				{
					attributes.Add(attr);
				}
			}
		}

		public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
		{
			attributes.Add(new SuffixLabelAttribute("@$value.LocalOverride ? \"Local\" : \"Global\""));
			attributes.Add(new CustomContextMenuAttribute("Reset Config Value", "@$value.Reset()"));
			attributes.Add(new SuppressInvalidAttributeErrorAttribute());
		}
	}
}

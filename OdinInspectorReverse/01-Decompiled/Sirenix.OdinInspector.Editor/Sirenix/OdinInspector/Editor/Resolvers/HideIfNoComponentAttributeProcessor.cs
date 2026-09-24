using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Resolvers
{
	public class HideIfNoComponentAttributeProcessor<T> : OdinAttributeProcessor<T>
	{
		private const string AttributeTypeName = "HideIfNoComponentAttribute";

		public override bool CanProcessSelfAttributes(InspectorProperty property)
		{
			return property.Attributes.Any((Attribute a) => a.GetType().Name == "HideIfNoComponentAttribute");
		}

		public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member)
		{
			return false;
		}

		public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
		{
			Attribute hideIfNoComponentAttribute = attributes.FirstOrDefault((Attribute a) => a.GetType().Name == "HideIfNoComponentAttribute");
			if (hideIfNoComponentAttribute == null)
			{
				return;
			}
			attributes.Remove(hideIfNoComponentAttribute);
			Type componentType = hideIfNoComponentAttribute.GetType().GetField("ComponentType", BindingFlags.Instance | BindingFlags.Public)?.GetValue(hideIfNoComponentAttribute) as Type;
			if (!(componentType == null))
			{
				MonoBehaviour obj = property.SerializationRoot.ValueEntry.WeakSmartValue as MonoBehaviour;
				if (obj == null || !obj.TryGetComponent(componentType, out var _))
				{
					attributes.Add(new HideIfAttribute("@true"));
				}
			}
		}
	}
}

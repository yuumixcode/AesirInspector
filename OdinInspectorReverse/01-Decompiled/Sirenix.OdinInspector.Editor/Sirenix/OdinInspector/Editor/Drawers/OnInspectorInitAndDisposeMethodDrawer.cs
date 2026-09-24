using System;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public class OnInspectorInitAndDisposeMethodDrawer : MethodDrawer
	{
		protected override bool CanDrawMethodProperty(InspectorProperty property)
		{
			ImmutableList<Attribute> attrs = property.Attributes;
			if (!attrs.HasAttribute<OnInspectorDisposeAttribute>())
			{
				return attrs.HasAttribute<OnInspectorInitAttribute>();
			}
			return true;
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
		}
	}
}

using System;
using Sirenix.OdinInspector.Editor;
using UnityEngine.Events;

namespace Sirenix.OdinValidator.Editor
{
	public class DontValidateUnityEventsDeeplyAttributeResolver<T> : BaseMemberPropertyResolver<T> where T : UnityEventBase
	{
		public override bool CanResolveForPropertyFilter(InspectorProperty property)
		{
			return !property.Tree.TreeIsSetupForIMGUIDrawing_TEMP_INTERNAL;
		}

		protected override InspectorPropertyInfo[] GetPropertyInfos()
		{
			return Array.Empty<InspectorPropertyInfo>();
		}
	}
}

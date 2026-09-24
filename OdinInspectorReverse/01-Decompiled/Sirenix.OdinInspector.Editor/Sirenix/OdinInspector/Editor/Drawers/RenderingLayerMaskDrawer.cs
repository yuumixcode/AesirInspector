using System;
using System.Reflection;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	[OdinDontRegister]
	[DrawerPriority(0.0, 0.0, 0.25)]
	public class RenderingLayerMaskDrawer<TStruct> : OdinValueDrawer<TStruct> where TStruct : struct
	{
		private delegate TStruct DoFieldDelegate(GUIContent content, TStruct value, GUILayoutOption[] options);

		private static DoFieldDelegate doField;

		protected override void Initialize()
		{
			if (doField == null)
			{
				MethodInfo doFieldInfo = RenderingLayerMaskReflection.RenderingLayerMaskFieldInfo;
				if (doFieldInfo == null)
				{
					base.SkipWhenDrawing = true;
				}
				else
				{
					doField = (DoFieldDelegate)Delegate.CreateDelegate(typeof(DoFieldDelegate), doFieldInfo);
				}
			}
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<TStruct> entry = base.ValueEntry;
			entry.SmartValue = doField(label, entry.SmartValue, Array.Empty<GUILayoutOption>());
		}
	}
}

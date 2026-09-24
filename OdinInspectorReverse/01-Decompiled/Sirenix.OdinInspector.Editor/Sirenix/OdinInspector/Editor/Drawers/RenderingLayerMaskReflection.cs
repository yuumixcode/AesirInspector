using System;
using System.Reflection;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal static class RenderingLayerMaskReflection
	{
		private const string TYPE_NAME = "UnityEngine.RenderingLayerMask, UnityEngine.CoreModule";

		private const string DRAW_FIELD_METHOD_NAME = "RenderingLayerMaskField";

		public static readonly Type RenderingLayerMaskType;

		public static readonly MethodInfo RenderingLayerMaskFieldInfo;

		static RenderingLayerMaskReflection()
		{
			RenderingLayerMaskType = TwoWaySerializationBinder.Default.BindToType("UnityEngine.RenderingLayerMask, UnityEngine.CoreModule");
			if (!(RenderingLayerMaskType == null))
			{
				RenderingLayerMaskFieldInfo = typeof(EditorGUILayout).GetMethod("RenderingLayerMaskField", BindingFlags.Static | BindingFlags.Public, null, new Type[3]
				{
					typeof(GUIContent),
					RenderingLayerMaskType,
					typeof(GUILayoutOption[])
				}, null);
				_ = RenderingLayerMaskFieldInfo == null;
			}
		}
	}
}

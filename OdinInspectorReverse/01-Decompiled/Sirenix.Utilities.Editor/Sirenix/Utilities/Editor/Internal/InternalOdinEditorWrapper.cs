using System;
using Sirenix.Serialization;
using UnityEngine;

namespace Sirenix.Utilities.Editor.Internal
{
	/// <summary> Temporary. </summary>
	/// <warning>This implementation <b>will</b> get refactored.</warning>
	internal static class InternalOdinEditorWrapper
	{
		private delegate UnityEngine.Object DrawUnityObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects, bool readOnly = false);

		private delegate object DrawPolymorphicObjectField(Rect position, object value, Type baseType, bool allowSceneObjects, bool hasKeyboardFocus, int id, bool disallowNullValues = false, bool readOnly = false, bool showBaseType = true, string title = null);

		private delegate object DoObjectPickerZone(Rect rect, Rect popupRect, object value, Type type, bool allowSceneObjects, int id);

		private static DrawUnityObjectField callDrawUnityObjectField;

		private static DrawPolymorphicObjectField callDrawPolymorphicObjectField;

		private static DoObjectPickerZone callDoObjectPickerZone;

		static InternalOdinEditorWrapper()
		{
			Type fields = TwoWaySerializationBinder.Default.BindToType("Sirenix.OdinInspector.Editor.Internal.OdinInternalEditorFields, Sirenix.OdinInspector.Editor");
			Type[] unityObjectFieldSignature = new Type[6]
			{
				typeof(Rect),
				typeof(GUIContent),
				typeof(UnityEngine.Object),
				typeof(Type),
				typeof(bool),
				typeof(bool)
			};
			callDrawUnityObjectField = (DrawUnityObjectField)fields.GetMethod("UnityObjectFieldWrapper", unityObjectFieldSignature).CreateDelegate(typeof(DrawUnityObjectField));
			callDrawPolymorphicObjectField = (DrawPolymorphicObjectField)fields.GetMethod("PolymorphicObjectFieldWrapper").CreateDelegate(typeof(DrawPolymorphicObjectField));
			Type dragAndDropsUtils = TwoWaySerializationBinder.Default.BindToType("Sirenix.OdinInspector.Editor.Internal.OdinInternalDragAndDropUtils, Sirenix.OdinInspector.Editor");
			callDoObjectPickerZone = (DoObjectPickerZone)dragAndDropsUtils.GetMethod("DoObjectPickerZoneWrapper").CreateDelegate(typeof(DoObjectPickerZone));
		}

		public static UnityEngine.Object UnityObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects)
		{
			return callDrawUnityObjectField(rect, label, value, objectType, allowSceneObjects);
		}

		public static object PolymorphicObjectField(Rect position, object value, Type baseType, bool allowSceneObjects, bool hasKeyboardFocus, int id, bool disallowNullValues = false, bool readOnly = false, bool showBaseType = true, string title = null)
		{
			return callDrawPolymorphicObjectField(position, value, baseType, allowSceneObjects, hasKeyboardFocus, id, disallowNullValues, readOnly, showBaseType, title);
		}

		public static object ObjectPickerZone(Rect rect, Rect popupRect, object value, Type type, bool allowSceneObjects, int id)
		{
			return callDoObjectPickerZone(rect, popupRect, value, type, allowSceneObjects, id);
		}
	}
}

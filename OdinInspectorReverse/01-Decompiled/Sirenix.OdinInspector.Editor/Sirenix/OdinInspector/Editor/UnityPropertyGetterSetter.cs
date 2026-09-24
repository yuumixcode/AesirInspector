using System;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public sealed class UnityPropertyGetterSetter<TOwner, TValue> : IValueGetterSetter<TOwner, TValue>, IValueGetterSetter
	{
		private static readonly Func<SerializedProperty, TValue> ValueGetter = SerializedPropertyUtilities.GetValueGetter<TValue>();

		private static readonly Action<SerializedProperty, TValue> ValueSetter = SerializedPropertyUtilities.GetValueSetter<TValue>();

		private InspectorProperty property;

		public bool IsReadonly => !property.Info.IsEditable;

		public Type OwnerType => typeof(TOwner);

		public Type ValueType => typeof(TValue);

		public UnityPropertyGetterSetter(InspectorProperty property)
		{
			this.property = property;
		}

		public TValue GetValue(ref TOwner owner)
		{
			if (ValueGetter == null || ValueSetter == null)
			{
				Debug.LogError("Can't get a value of type " + typeof(TValue).GetNiceName() + " directly from a Unity property.");
				return default(TValue);
			}
			SerializedProperty unityProp = property.Tree.GetUnityPropertyForPath(property.UnityPropertyPath);
			if (unityProp == null || unityProp.serializedObject.targetObject is EmittedScriptableObject)
			{
				Debug.LogError("Could not get Unity property at path " + property.UnityPropertyPath + " on root object of type " + property.Tree.TargetType.GetNiceName());
				return default(TValue);
			}
			return ValueGetter(unityProp);
		}

		public object GetValue(object owner)
		{
			TOwner castOwner = (TOwner)owner;
			return GetValue(ref castOwner);
		}

		public void SetValue(ref TOwner owner, TValue value)
		{
			if (ValueGetter == null || ValueSetter == null)
			{
				Debug.LogError("Can't set a value of type " + typeof(TValue).GetNiceName() + " directly to a Unity property.");
				return;
			}
			SerializedProperty unityProp = property.Tree.GetUnityPropertyForPath(property.UnityPropertyPath);
			if (unityProp == null || unityProp.serializedObject.targetObject is EmittedScriptableObject)
			{
				Debug.LogError("Could not get Unity property at path " + property.UnityPropertyPath);
			}
			else
			{
				ValueSetter(unityProp, value);
			}
		}

		public void SetValue(object owner, object value)
		{
			TOwner castOwner = (TOwner)owner;
			SetValue(ref castOwner, (TValue)value);
		}
	}
}

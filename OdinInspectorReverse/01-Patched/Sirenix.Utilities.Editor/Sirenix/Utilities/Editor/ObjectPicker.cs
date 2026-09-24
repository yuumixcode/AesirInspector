using System;
using System.Reflection;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// Not yet documented.
	/// </summary>
	public sealed class ObjectPicker<T>
	{
		private readonly ObjectPicker picker;

		private static readonly object objectPickerConfigKey = new object();

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public T CurrentSelectedObject => (T)picker.CurrentSelectedObject;

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public bool IsReadyToClaim => picker.IsReadyToClaim;

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public bool IsPickerOpen => picker.IsPickerOpen;

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static ObjectPicker<T> GetObjectPicker(object key)
		{
			GUIContext<ObjectPicker<T>> objectPicker = GUIHelper.GetTemporaryNullableContext<ObjectPicker<T>>(objectPickerConfigKey, key);
			objectPicker.Value = objectPicker.Value ?? new ObjectPicker<T>(ObjectPicker.GetObjectPicker(key, typeof(T)));
			objectPicker.Value.Update();
			return objectPicker;
		}

		private void Update()
		{
			picker.Update();
		}

		private ObjectPicker(ObjectPicker picker)
		{
			this.picker = picker;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public T ClaimObject()
		{
			object obj = picker.ClaimObject();
			if (obj == null)
			{
				return default(T);
			}
			return (T)obj;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public void ShowObjectPicker(object value, bool allowSceneObjects, Rect buttonRect = default(Rect), bool isUnitySerialized = false)
		{
			picker.ShowObjectPicker(value, allowSceneObjects, buttonRect, isUnitySerialized);
		}
	}
	/// <summary>
	/// Not yet documented.
	/// </summary>
	public sealed class ObjectPicker
	{
		private static EditorWindow WindowPickedFrom;

		private static readonly object objectPickerConfigKey = new object();

		private readonly bool isUnityObject;

		private readonly bool isUnityComponent;

		private readonly bool isString;

		private readonly bool isClass;

		private readonly bool isInterface;

		private readonly bool isStruct;

		private readonly bool isAbstract;

		private readonly Type type;

		private int controlId;

		private bool isPickerOpen;

		private bool disallowNullValues;

		private EditorWindow window;

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public object CurrentSelectedObject { get; private set; }

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public bool IsReadyToClaim { get; private set; }

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public bool IsPickerOpen => isPickerOpen;

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static ObjectPicker GetObjectPicker(object key, Type type)
		{
			GUIContext<ObjectPicker> objectPicker = GUIHelper.GetTemporaryNullableContext<ObjectPicker>(objectPickerConfigKey, key);
			objectPicker.Value = objectPicker.Value ?? new ObjectPicker(type);
			if (objectPicker.Value.type != type)
			{
				objectPicker.Value = new ObjectPicker(type);
			}
			objectPicker.Value.window = GUIHelper.CurrentWindow;
			objectPicker.Value.Update();
			return objectPicker;
		}

		private ObjectPicker(Type type)
		{
			this.type = type;
			isString = this.type == typeof(string);
			isUnityObject = this.type.InheritsFrom(typeof(UnityEngine.Object));
			isClass = this.type.IsClass;
			isInterface = this.type.IsInterface;
			isAbstract = this.type.IsAbstract;
			isStruct = !isString && !isUnityObject && !isClass && !isInterface;
			isUnityComponent = this.type.InheritsFrom(typeof(Component));
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public object ClaimObject()
		{
			if (IsReadyToClaim)
			{
				GUIHelper.RequestRepaint();
				IsReadyToClaim = false;
				isPickerOpen = false;
				object obj = CurrentSelectedObject;
				CurrentSelectedObject = null;
				return obj;
			}
			GUIHelper.RequestRepaint();
			isPickerOpen = false;
			IsReadyToClaim = false;
			Debug.LogError("No object is ready to be claimed.");
			return null;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public void ShowObjectPicker(bool allowSceneObjects, Rect buttonRect = default(Rect), bool disallowNullValues = false)
		{
			ShowObjectPicker(null, allowSceneObjects, buttonRect, disallowNullValues);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public void ShowObjectPicker(object currValue, bool allowSceneObjects, Rect buttonRect = default(Rect), bool disallowNullValues = false)
		{
			isPickerOpen = true;
			this.disallowNullValues = disallowNullValues;
			if (isUnityObject)
			{
				if (UnityShims.Misc.GetEventModifiers(Event.current) == 2)
				{
					CurrentSelectedObject = null;
					IsReadyToClaim = true;
					return;
				}
				typeof(EditorGUIUtility).GetMethod("ShowObjectPicker", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(type).Invoke(null, new object[4] { currValue, allowSceneObjects, null, controlId });
				WindowPickedFrom = GUIHelper.CurrentWindow;
				CurrentSelectedObject = currValue;
				return;
			}
			if (isString)
			{
				CurrentSelectedObject = "";
				return;
			}
			if (isStruct)
			{
				if (disallowNullValues)
				{
					CurrentSelectedObject = UnitySerializationUtility.CreateDefaultUnityInitializedObject(type);
				}
				else if (type.IsValueType && !type.IsPrimitive)
				{
					CurrentSelectedObject = Activator.CreateInstance(type);
				}
				else
				{
					CurrentSelectedObject = UnitySerializationUtility.CreateDefaultUnityInitializedObject(type);
				}
				return;
			}
			if (typeof(Delegate).IsAssignableFrom(type))
			{
				CurrentSelectedObject = null;
				IsReadyToClaim = true;
				return;
			}
			if (isClass || isInterface)
			{
				if (disallowNullValues)
				{
					if (isInterface)
					{
						Debug.LogError("Property is serialized by Unity, where interfaces are not supported.");
						return;
					}
					if (isAbstract)
					{
						Debug.LogError("Property is serialized by Unity, where abstract classes are not supported.");
						return;
					}
					IsReadyToClaim = true;
					CurrentSelectedObject = UnitySerializationUtility.CreateDefaultUnityInitializedObject(type);
				}
				else if (buttonRect.width > 0f && buttonRect.height > 0f)
				{
					InstanceCreator.Show(type, controlId, buttonRect);
				}
				else
				{
					InstanceCreator.Show(type, controlId);
				}
				return;
			}
			throw new NotImplementedException();
		}

		internal void Update()
		{
			int newControlId = GUIUtility.GetControlID(FocusType.Passive);
			if (newControlId > 0)
			{
				controlId = newControlId;
			}
			if (!isPickerOpen)
			{
				IsReadyToClaim = false;
				return;
			}
			GUIHelper.RequestRepaint();
			if (isUnityObject)
			{
				if (Event.current.type == EventType.Layout)
				{
					return;
				}
				if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == controlId && controlId > 0 && window == WindowPickedFrom)
				{
					UnityEngine.Object val = EditorGUIUtility.GetObjectPickerObject();
					if (val == null)
					{
						CurrentSelectedObject = null;
					}
					else if (isUnityComponent)
					{
						CurrentSelectedObject = ((GameObject)val).GetComponent(type);
					}
					else
					{
						CurrentSelectedObject = val;
					}
					GUI.changed = true;
					IsReadyToClaim = false;
					Event.current.Use();
				}
				else if (Event.current.commandName == "ObjectSelectorClosed" && EditorGUIUtility.GetObjectPickerControlID() == controlId && controlId > 0 && window == WindowPickedFrom)
				{
					GUI.changed = true;
					IsReadyToClaim = true;
					Event.current.Use();
				}
				return;
			}
			if (isString || isStruct)
			{
				IsReadyToClaim = true;
				return;
			}
			if (isClass || isInterface)
			{
				if (Event.current != null && Event.current.type == EventType.Layout)
				{
					return;
				}
				if (disallowNullValues)
				{
					if (CurrentSelectedObject == null)
					{
						CurrentSelectedObject = UnitySerializationUtility.CreateDefaultUnityInitializedObject(type);
					}
					IsReadyToClaim = true;
				}
				else if (InstanceCreator.ControlID == controlId && controlId > 0 && InstanceCreator.HasCreatedInstance)
				{
					object val2 = InstanceCreator.GetCreatedInstance();
					if (val2 == null)
					{
						CurrentSelectedObject = null;
					}
					else
					{
						CurrentSelectedObject = val2;
					}
					IsReadyToClaim = true;
				}
				return;
			}
			throw new NotImplementedException();
		}
	}
}

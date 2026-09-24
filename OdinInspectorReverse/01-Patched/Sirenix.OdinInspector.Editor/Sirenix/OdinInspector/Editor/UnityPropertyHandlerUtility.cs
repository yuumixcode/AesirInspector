using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	internal static class UnityPropertyHandlerUtility
	{
		private delegate void FiveArgAction<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

		private const string ScriptAttributeUtilityName = "UnityEditor.ScriptAttributeUtility";

		private const string PropertyHandlerCacheName = "UnityEditor.PropertyHandlerCache";

		private const string PropertyHandlerName = "UnityEditor.PropertyHandler";

		private const string ScriptAttributeUtility_GetHandlerName = "GetHandler";

		private const string ScriptAttributeUtility_PropertyHandlerCacheName = "propertyHandlerCache";

		private const string PropertyHandlerCache_SetHandlerName = "SetHandler";

		private const string PropertyHandler_OnGUIName = "OnGUI";

		private const string PropertyHandler_GetHeightName = "GetHeight";

		private const string PropertyHandler_PropertyDrawerName = "m_PropertyDrawer";

		private const string PropertyHandler_AddMenuItemsName = "AddMenuItems";

		private const string PropertyHandler_PropertyDrawersName_2021_1 = "m_PropertyDrawers";

		private static readonly Func<SerializedProperty, object> ScriptAttributeUtility_GetHandler_Func;

		private static readonly Func<object> ScriptAttributeUtility_GetPropertyHandlerCache;

		private static readonly Action<object, SerializedProperty, object> PropertyHandlerCache_SetHandler;

		private static readonly Func<object> PropertyHandler_Create;

		private static readonly FiveArgAction<object, Rect, SerializedProperty, GUIContent, bool> PropertyHandler_OnGUI;

		private static readonly Func<object, SerializedProperty, GUIContent, bool, float> PropertyHandler_GetHeight;

		private static readonly Action<object, PropertyDrawer> PropertyHandler_SetPropertyDrawer;

		private static readonly Action<object, SerializedProperty, GenericMenu> PropertyHandler_AddMenuItems_Func;

		private static readonly Type ScriptAttributeUtility;

		private static readonly Type PropertyHandlerCache;

		private static readonly Type PropertyHandler;

		public static bool IsAvailable { get; private set; }

		static UnityPropertyHandlerUtility()
		{
			ScriptAttributeUtility = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ScriptAttributeUtility");
			PropertyHandlerCache = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.PropertyHandlerCache");
			PropertyHandler = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.PropertyHandler");
			try
			{
				if (ScriptAttributeUtility == null)
				{
					CouldNotFindTypeError("UnityEditor.ScriptAttributeUtility");
					return;
				}
				if (PropertyHandlerCache == null)
				{
					CouldNotFindTypeError("UnityEditor.PropertyHandlerCache");
					return;
				}
				if (PropertyHandler == null)
				{
					CouldNotFindTypeError("UnityEditor.PropertyHandler");
					return;
				}
				PropertyInfo propertyHandlerCacheProperty = ScriptAttributeUtility.GetProperty("propertyHandlerCache", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				MethodInfo getHandler = ScriptAttributeUtility.GetMethod("GetHandler", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(SerializedProperty) }, null);
				MethodInfo setHandlerMethod = PropertyHandlerCache.GetMethod("SetHandler", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				MethodInfo onGUIMethod = PropertyHandler.GetMethod("OnGUI", BindingFlags.Instance | BindingFlags.Public, null, new Type[4]
				{
					typeof(Rect),
					typeof(SerializedProperty),
					typeof(GUIContent),
					typeof(bool)
				}, null);
				MethodInfo getHeightMethod = PropertyHandler.GetMethod("GetHeight", BindingFlags.Instance | BindingFlags.Public, null, new Type[3]
				{
					typeof(SerializedProperty),
					typeof(GUIContent),
					typeof(bool)
				}, null);
				MethodInfo addMenuItems = PropertyHandler.GetMethod("AddMenuItems", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
				{
					typeof(SerializedProperty),
					typeof(GenericMenu)
				}, null);
				FieldInfo drawerField = PropertyHandler.GetField("m_PropertyDrawer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				FieldInfo drawersField = null;
				if (drawerField == null)
				{
					drawersField = PropertyHandler.GetField("m_PropertyDrawers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				}
				if (propertyHandlerCacheProperty == null)
				{
					CouldNotFindMemberError(ScriptAttributeUtility, "propertyHandlerCache");
					return;
				}
				if (getHandler == null)
				{
					CouldNotFindMemberError(ScriptAttributeUtility, "GetHandler");
					return;
				}
				if (setHandlerMethod == null)
				{
					CouldNotFindMemberError(PropertyHandlerCache, "SetHandler");
					return;
				}
				if (onGUIMethod == null)
				{
					CouldNotFindMemberError(PropertyHandler, "OnGUI");
					return;
				}
				if (getHeightMethod == null)
				{
					CouldNotFindMemberError(PropertyHandler, "GetHeight");
					return;
				}
				if (drawerField == null)
				{
					if (!UnityVersion.IsVersionOrGreater(2021, 1))
					{
						CouldNotFindMemberError(PropertyHandler, "m_PropertyDrawer");
						return;
					}
					if (drawersField == null)
					{
						CouldNotFindMemberError(PropertyHandler, "m_PropertyDrawers");
						return;
					}
				}
				if (addMenuItems == null)
				{
					CouldNotFindMemberError(PropertyHandler, "AddMenuItems");
					return;
				}
				ScriptAttributeUtility_GetPropertyHandlerCache = () => propertyHandlerCacheProperty.GetValue(null, null);
				ScriptAttributeUtility_GetHandler_Func = (SerializedProperty property) => getHandler.Invoke(null, new object[1] { property });
				PropertyHandlerCache_SetHandler = delegate(object instance, SerializedProperty property, object handler)
				{
					setHandlerMethod.Invoke(instance, new object[2] { property, handler });
				};
				PropertyHandler_Create = () => Activator.CreateInstance(PropertyHandler);
				PropertyHandler_OnGUI = delegate(object instance, Rect rect, SerializedProperty property, GUIContent label, bool includeChildren)
				{
					onGUIMethod.Invoke(instance, new object[4] { rect, property, label, includeChildren });
				};
				PropertyHandler_GetHeight = (object instance, SerializedProperty property, GUIContent label, bool includeChildren) => (float)getHeightMethod.Invoke(instance, new object[3] { property, label, includeChildren });
				PropertyHandler_AddMenuItems_Func = delegate(object handler, SerializedProperty property, GenericMenu menu)
				{
					addMenuItems.Invoke(handler, new object[2] { property, menu });
				};
				if (drawerField == null)
				{
					PropertyHandler_SetPropertyDrawer = delegate(object instance, PropertyDrawer drawer)
					{
						List<PropertyDrawer> list = (List<PropertyDrawer>)drawersField.GetValue(instance);
						if (list == null)
						{
							list = new List<PropertyDrawer>();
							drawersField.SetValue(instance, list);
						}
						list.Add(drawer);
					};
				}
				else
				{
					PropertyHandler_SetPropertyDrawer = delegate(object instance, PropertyDrawer drawer)
					{
						drawerField.SetValue(instance, drawer);
					};
				}
				IsAvailable = true;
			}
			catch (Exception innerException)
			{
				Debug.LogException(new Exception("UnityPropertyHandlerUtility initialization failed with an exception; cannot correctly set internal Unity state for drawing of custom Unity property drawers - drawers which call EditorGUI.PropertyField or EditorGUILayout.PropertyField will be drawn partially twice.", innerException));
			}
		}

		private static void CouldNotFindTypeError(string typeName)
		{
			Debug.LogError("Could not find the internal Unity type '" + typeName + "'; cannot correctly set internal Unity state for drawing of custom Unity property drawers - drawers which call EditorGUI.PropertyField or EditorGUILayout.PropertyField will be drawn partially twice.");
		}

		private static void CouldNotFindMemberError(Type type, string memberName)
		{
			Debug.LogError("Could not find the member '" + memberName + "' on internal Unity type '" + type.GetNiceFullName() + "'; cannot correctly set internal Unity state for drawing of custom Unity property drawers - drawers which call EditorGUI.PropertyField or EditorGUILayout.PropertyField will be drawn partially twice.");
		}

		public static void PropertyHandlerOnGUI(object handler, Rect rect, SerializedProperty property, GUIContent label, bool includeChildren)
		{
			if (!IsAvailable)
			{
				return;
			}
			try
			{
				object cache = ScriptAttributeUtility_GetPropertyHandlerCache();
				PropertyHandlerCache_SetHandler(cache, property, handler);
				PropertyHandler_OnGUI(handler, rect, property, label, includeChildren);
			}
			catch (TargetInvocationException ex)
			{
				if (ex.IsExitGUIException())
				{
					throw ex.AsExitGUIException();
				}
				throw ex;
			}
		}

		public static float PropertyHandlerGetHeight(object handler, SerializedProperty property, GUIContent label, bool includeChildren)
		{
			if (!IsAvailable)
			{
				return 0f;
			}
			try
			{
				object cache = ScriptAttributeUtility_GetPropertyHandlerCache();
				PropertyHandlerCache_SetHandler(cache, property, handler);
				return PropertyHandler_GetHeight(handler, property, label, includeChildren);
			}
			catch (TargetInvocationException ex)
			{
				if (ex.IsExitGUIException())
				{
					throw ex.AsExitGUIException();
				}
				throw ex;
			}
		}

		public static object ScriptAttributeUtility_GetHandler(SerializedProperty property)
		{
			if (!IsAvailable)
			{
				return null;
			}
			return ScriptAttributeUtility_GetHandler_Func(property);
		}

		public static void PropertyHandler_AddMenuItems(object handler, SerializedProperty property, GenericMenu menu)
		{
			if (IsAvailable)
			{
				PropertyHandler_AddMenuItems_Func(handler, property, menu);
			}
		}

		public static object CreatePropertyHandler(PropertyDrawer drawer)
		{
			if (!IsAvailable)
			{
				return null;
			}
			object handler = PropertyHandler_Create();
			PropertyHandler_SetPropertyDrawer(handler, drawer);
			return handler;
		}
	}
}

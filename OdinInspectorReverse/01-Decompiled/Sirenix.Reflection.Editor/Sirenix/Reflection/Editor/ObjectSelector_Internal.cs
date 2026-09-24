using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Reflection.Editor
{
	public static class ObjectSelector_Internal
	{
		public const string OBJECT_SELECTOR_CLOSED_COMMAND = "ObjectSelectorClosed";

		public const string OBJECT_SELECTOR_UPDATED_COMMAND = "ObjectSelectorUpdated";

		public const string OBJECT_SELECTOR_CANCELED_COMMAND = "ObjectSelectorCanceled";

		public const string OBJECT_SELECTOR_SELECTION_DONE_COMMAND = "ObjectSelectorSelectionDone";

		public static FieldInfo mObjectBeingEdited_Field = typeof(UnityEditor.ObjectSelector).GetField("m_ObjectBeingEdited", BindingFlags.Instance | BindingFlags.NonPublic);

		public static FieldInfo m_OnObjectSelectorClosed = typeof(UnityEditor.ObjectSelector).GetField("m_OnObjectSelectorClosed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

		public static FieldInfo m_OnObjectSelectorUpdated = typeof(UnityEditor.ObjectSelector).GetField("m_OnObjectSelectorUpdated", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

		public static Type[] NewShowMethodRequiredParams = new Type[4]
		{
			typeof(UnityEngine.Object),
			typeof(Type),
			typeof(UnityEngine.Object),
			typeof(bool)
		};

		public static MethodInfo Show_Method = TryGetNewShowMethod(typeof(UnityEditor.ObjectSelector), NewShowMethodRequiredParams);

		private static MethodInfo SetupObjectSelector_Method = typeof(EditorGUIUtility).GetMethod("SetupObjectSelector", BindingFlags.Static | BindingFlags.NonPublic);

		public static bool CanUseCallbacks
		{
			get
			{
				if (m_OnObjectSelectorClosed != null)
				{
					return m_OnObjectSelectorUpdated != null;
				}
				return false;
			}
		}

		public static void ShowObjectSelector(UnityEngine.Object obj, Type objType, UnityEngine.Object objectBeingEdited, bool allowSceneObjects, string searchFilter, int controlID, Action<UnityEngine.Object> onClosed, Action<UnityEngine.Object> onUpdated)
		{
			if (Event.current?.commandName == "ObjectSelectorClosed")
			{
				EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, (EditorApplication.CallbackFunction)delegate
				{
					SetupObjectSelectorWithObjectBeingEdited(obj, objType, objectBeingEdited, allowSceneObjects, searchFilter, controlID, onClosed, onUpdated);
				});
			}
			else
			{
				SetupObjectSelectorWithObjectBeingEdited(obj, objType, objectBeingEdited, allowSceneObjects, searchFilter, controlID, onClosed, onUpdated);
			}
		}

		public static void ShowObjectSelector(UnityEngine.Object obj, Type objType, UnityEngine.Object objectBeingEdited, bool allowSceneObjects, string searchFilter, int controlID)
		{
			ShowObjectSelector(obj, objType, objectBeingEdited, allowSceneObjects, searchFilter, controlID, null, null);
		}

		private static void SetupObjectSelectorWithObjectBeingEdited(UnityEngine.Object obj, Type objType, UnityEngine.Object objectBeingEdited, bool allowSceneObjects, string searchFilter, int controlID, Action<UnityEngine.Object> onClosed, Action<UnityEngine.Object> onUpdated)
		{
			if (Show_Method != null)
			{
				Show_Method.Invoke(UnityEditor.ObjectSelector.get, GetShowArguments(obj, objType, objectBeingEdited, allowSceneObjects));
				UnityEditor.ObjectSelector.get.objectSelectorID = controlID;
				if (!string.IsNullOrEmpty(searchFilter))
				{
					if (string.IsNullOrEmpty(UnityEditor.ObjectSelector.get.searchFilter))
					{
						UnityEditor.ObjectSelector.get.searchFilter = searchFilter;
					}
					else
					{
						UnityEditor.ObjectSelector get = UnityEditor.ObjectSelector.get;
						get.searchFilter = get.searchFilter + " " + searchFilter;
					}
				}
				SetupCallbacks(onClosed, onUpdated);
				return;
			}
			if (SetupObjectSelector_Method != null)
			{
				if (mObjectBeingEdited_Field != null)
				{
					mObjectBeingEdited_Field.SetValue(UnityEditor.ObjectSelector.get, objectBeingEdited);
				}
				SetupObjectSelector_Method.Invoke(null, new object[5] { obj, objType, allowSceneObjects, searchFilter, controlID });
				if (mObjectBeingEdited_Field != null)
				{
					mObjectBeingEdited_Field.SetValue(UnityEditor.ObjectSelector.get, objectBeingEdited);
				}
			}
			SetupCallbacks(onClosed, onUpdated);
		}

		internal static void SetupCallbacks(Action<UnityEngine.Object> onClosed, Action<UnityEngine.Object> onUpdated)
		{
			if (onClosed != null && !(m_OnObjectSelectorClosed == null))
			{
				m_OnObjectSelectorClosed.SetValue(UnityEditor.ObjectSelector.get, onClosed);
			}
			if (onUpdated != null && !(m_OnObjectSelectorUpdated == null))
			{
				m_OnObjectSelectorUpdated.SetValue(UnityEditor.ObjectSelector.get, onUpdated);
			}
		}

		public static MethodInfo TryGetNewShowMethod(Type type, Type[] requiredParams)
		{
			MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo method in methods)
			{
				if (method.Name != "Show")
				{
					continue;
				}
				ParameterInfo[] parameters = method.GetParameters();
				if (parameters.Length < requiredParams.Length)
				{
					continue;
				}
				int parameterIndex = 0;
				bool shouldContinue = false;
				for (; parameterIndex < requiredParams.Length; parameterIndex++)
				{
					if (parameters[parameterIndex].ParameterType != requiredParams[parameterIndex])
					{
						shouldContinue = true;
						break;
					}
				}
				if (shouldContinue)
				{
					continue;
				}
				for (; parameterIndex < parameters.Length; parameterIndex++)
				{
					if (!parameters[parameterIndex].HasDefaultValue)
					{
						shouldContinue = true;
						break;
					}
				}
				if (!shouldContinue)
				{
					return method;
				}
			}
			return null;
		}

		private static object[] GetShowArguments(UnityEngine.Object obj, Type objType, UnityEngine.Object objectBeingEdited, bool allowSceneObjects)
		{
			ParameterInfo[] parameters = Show_Method.GetParameters();
			object[] result = new object[parameters.Length];
			int index = 0;
			result[index++] = obj;
			result[index++] = objType;
			result[index++] = objectBeingEdited;
			result[index++] = allowSceneObjects;
			for (; index < parameters.Length; index++)
			{
				result[index] = parameters[index].DefaultValue;
			}
			return result;
		}
	}
}

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Sirenix.OdinInspector.Editor.Internal
{
	public static class OdinEditorWebUtility
	{
		private static readonly MethodInfo UnityWebRequest_SendWebRequest_Method;

		private static readonly EventInfo AsyncOperation_Completed_Event;

		private static readonly PropertyInfo UnityWebRequest_IsError_Property;

		private static readonly PropertyInfo UnityWebRequest_IsHttpError_Property;

		private static readonly PropertyInfo UnityWebRequest_IsNetworkError_Property;

		private static readonly PropertyInfo UnityWebRequest_Result_Property;

		public static readonly bool RequiredUnityApiIsAvailable;

		static OdinEditorWebUtility()
		{
			RequiredUnityApiIsAvailable = true;
			UnityWebRequest_SendWebRequest_Method = typeof(UnityWebRequest).GetMethod("SendWebRequest", BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null) ?? typeof(UnityWebRequest).GetMethod("Send", BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null);
			if (UnityWebRequest_SendWebRequest_Method == null)
			{
				RequiredUnityApiIsAvailable = false;
			}
			AsyncOperation_Completed_Event = typeof(AsyncOperation).GetEvent("completed", BindingFlags.Instance | BindingFlags.Public);
			UnityWebRequest_IsError_Property = typeof(UnityWebRequest).GetProperty("isError", BindingFlags.Instance | BindingFlags.Public);
			UnityWebRequest_IsHttpError_Property = typeof(UnityWebRequest).GetProperty("isHttpError", BindingFlags.Instance | BindingFlags.Public);
			UnityWebRequest_IsNetworkError_Property = typeof(UnityWebRequest).GetProperty("isNetworkError", BindingFlags.Instance | BindingFlags.Public);
			UnityWebRequest_Result_Property = typeof(UnityWebRequest).GetProperty("result", BindingFlags.Instance | BindingFlags.Public);
			if (UnityWebRequest_IsError_Property == null && UnityWebRequest_IsHttpError_Property == null && UnityWebRequest_IsNetworkError_Property == null && UnityWebRequest_Result_Property == null)
			{
				RequiredUnityApiIsAvailable = false;
			}
		}

		public static AsyncOperation SendWebRequest(UnityWebRequest request)
		{
			return (AsyncOperation)UnityWebRequest_SendWebRequest_Method.Invoke(request, null);
		}

		public static bool RequestIsError(UnityWebRequest request)
		{
			if (UnityWebRequest_Result_Property != null)
			{
				object value = UnityWebRequest_Result_Property.GetValue(request, null);
				int enumValue = Convert.ToInt32(value);
				return enumValue > 1;
			}
			if (UnityWebRequest_IsNetworkError_Property != null && (bool)UnityWebRequest_IsNetworkError_Property.GetValue(request, null))
			{
				return true;
			}
			if (UnityWebRequest_IsHttpError_Property != null && (bool)UnityWebRequest_IsHttpError_Property.GetValue(request, null))
			{
				return true;
			}
			if (UnityWebRequest_IsError_Property != null && (bool)UnityWebRequest_IsError_Property.GetValue(request, null))
			{
				return true;
			}
			return false;
		}

		public static void SubscribeOnCompleted(AsyncOperation operation, Action<AsyncOperation> action)
		{
			if (operation.isDone)
			{
				try
				{
					action(operation);
					return;
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
					return;
				}
			}
			if (AsyncOperation_Completed_Event != null)
			{
				AsyncOperation_Completed_Event.AddEventHandler(operation, action);
				return;
			}
			EditorApplication.CallbackFunction update = null;
			update = delegate
			{
				if (operation.isDone)
				{
					EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.update, update);
					try
					{
						action(operation);
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
					}
				}
			};
			EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, update);
		}
	}
}

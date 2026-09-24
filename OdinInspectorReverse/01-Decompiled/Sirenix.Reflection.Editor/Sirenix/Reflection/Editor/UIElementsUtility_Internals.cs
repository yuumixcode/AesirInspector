using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace Sirenix.Reflection.Editor
{
	public static class UIElementsUtility_Internals
	{
		public static object test;

		private static readonly FieldInfo s_BeginContainerCallbackField;

		private static readonly FieldInfo s_EndContainerCallbackField;

		private static readonly Func<IMGUIContainer> s_GetCurrentIMGUIContainerDelegate;

		public static event Action<IMGUIContainer> EndContainerCallback
		{
			add
			{
				Action<IMGUIContainer> current = (Action<IMGUIContainer>)s_EndContainerCallbackField.GetValue(null);
				s_EndContainerCallbackField.SetValue(null, (Action<IMGUIContainer>)Delegate.Combine(current, value));
			}
			remove
			{
				Action<IMGUIContainer> current = (Action<IMGUIContainer>)s_EndContainerCallbackField.GetValue(null);
				s_EndContainerCallbackField.SetValue(null, (Action<IMGUIContainer>)Delegate.Remove(current, value));
			}
		}

		public static event Action<IMGUIContainer> BeginContainerCallback
		{
			add
			{
				Action<IMGUIContainer> current = (Action<IMGUIContainer>)s_BeginContainerCallbackField.GetValue(null);
				s_BeginContainerCallbackField.SetValue(null, (Action<IMGUIContainer>)Delegate.Combine(current, value));
			}
			remove
			{
				Action<IMGUIContainer> current = (Action<IMGUIContainer>)s_BeginContainerCallbackField.GetValue(null);
				s_BeginContainerCallbackField.SetValue(null, (Action<IMGUIContainer>)Delegate.Remove(current, value));
			}
		}

		static UIElementsUtility_Internals()
		{
			test = 2;
			Assembly assembly = typeof(UnityEngine.UIElements.UIElementsUtility).Assembly;
			Type[] types = new Type[3]
			{
				assembly.GetType("UnityEngine.UIElements.IMGUIContainer"),
				assembly.GetType("UnityEngine.UIElements.UIElementsIMGUIUtility"),
				assembly.GetType("UnityEngine.UIElements.UIElementsUtility")
			};
			TryGetIMGUIContainerCallbackFields(types, out s_BeginContainerCallbackField, out s_EndContainerCallbackField);
			s_GetCurrentIMGUIContainerDelegate = TryGetCurrentIMGUIContainerDelegate(types);
		}

		private static bool TryGetIMGUIContainerCallbackFields(Type[] targetTypes, out FieldInfo beginContainerCallbackField, out FieldInfo endContainerCallbackField)
		{
			foreach (Type targetType in targetTypes)
			{
				if (!(targetType == null))
				{
					beginContainerCallbackField = targetType.GetField("s_BeginContainerCallback", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					endContainerCallbackField = targetType.GetField("s_EndContainerCallback", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					if (beginContainerCallbackField != null && endContainerCallbackField != null)
					{
						return true;
					}
				}
			}
			beginContainerCallbackField = null;
			endContainerCallbackField = null;
			return false;
		}

		private static Func<IMGUIContainer> TryGetCurrentIMGUIContainerDelegate(Type[] targetTypes)
		{
			foreach (Type targetType in targetTypes)
			{
				if (!(targetType == null))
				{
					MethodInfo getContainerMethod = targetType.GetMethod("GetCurrentIMGUIContainer", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					if (getContainerMethod != null)
					{
						return (Func<IMGUIContainer>)Delegate.CreateDelegate(typeof(Func<IMGUIContainer>), getContainerMethod);
					}
				}
			}
			return null;
		}

		public static IMGUIContainer GetCurrentIMGUIContainer()
		{
			return s_GetCurrentIMGUIContainerDelegate?.Invoke();
		}
	}
}

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Reflection.Editor
{
	public static class EditorWindow_Internal
	{
		private delegate Rect FitRectToScreenDelegate(Rect rect, Vector2 uiPositionToFindScreen, bool forceCompletelyVisible, UnityEditor.ContainerWindow windowForBorderCalculation);

		private delegate Rect FitWindowRectToScreenDelegate(UnityEditor.ContainerWindow self, Rect r, bool forceCompletelyVisible, bool useMouseScreen);

		private static readonly FitRectToScreenDelegate FitRectToScreen;

		private static readonly FitWindowRectToScreenDelegate FitWindowRectToScreen;

		static EditorWindow_Internal()
		{
			MethodInfo fitRectToScreenInfo = GetContainerWindowMethod("FitRectToScreen", typeof(Rect), typeof(Rect), typeof(Vector2), typeof(bool), typeof(UnityEditor.ContainerWindow));
			if (fitRectToScreenInfo != null)
			{
				FitRectToScreen = (FitRectToScreenDelegate)Delegate.CreateDelegate(typeof(FitRectToScreenDelegate), fitRectToScreenInfo);
			}
			MethodInfo fitWindowRectToScreenInfo = GetContainerWindowMethod("FitWindowRectToScreen", typeof(Rect), typeof(Rect), typeof(bool), typeof(bool));
			if (fitWindowRectToScreenInfo != null)
			{
				FitWindowRectToScreen = (FitWindowRectToScreenDelegate)Delegate.CreateDelegate(typeof(FitWindowRectToScreenDelegate), fitWindowRectToScreenInfo);
			}
			if (FitRectToScreen == null && FitWindowRectToScreen == null)
			{
				Debug.LogError("[Sirenix.Reflection.Editor]: Neither 'ContainerWindow.FitRectToScreen' nor 'ContainerWindow.FitWindowRectToScreen' was found.");
			}
		}

		public static void ShowPopupAux(EditorWindow window)
		{
			if (!(window.m_Parent != null))
			{
				window.ShowPopup();
				if (EditorWindow.focusedWindow != window)
				{
					window.Focus();
				}
				else
				{
					window.Repaint();
				}
				if (!(window.m_Parent == null))
				{
					((UnityEditor.GUIView)window.m_Parent).AddToAuxWindowList();
					window.m_Parent.window.m_DontSaveToLayout = true;
				}
			}
		}

		public static Rect FitPositionInWorkingArea(EditorWindow window, Rect position, bool useMouseScreen)
		{
			if (window.m_Parent == null || window.m_Parent.window == null)
			{
				return position;
			}
			if (FitRectToScreen != null)
			{
				return FitRectToScreen(position, position.center, forceCompletelyVisible: true, window.m_Parent.window);
			}
			if (FitWindowRectToScreen != null)
			{
				return FitWindowRectToScreen(window.m_Parent.window, position, forceCompletelyVisible: true, useMouseScreen);
			}
			return position;
		}

		private static MethodInfo GetContainerWindowMethod(string name, Type returnType, BindingFlags flags, params Type[] args)
		{
			MethodInfo info = typeof(UnityEditor.ContainerWindow).GetMethod(name, flags, null, args, null);
			if (info == null || (returnType != null && info.ReturnType != returnType))
			{
				return null;
			}
			return info;
		}

		private static MethodInfo GetContainerWindowMethod(string name, Type returnType, params Type[] args)
		{
			return GetContainerWindowMethod(name, returnType, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, args);
		}

		private static bool HasContainer(EditorWindow window)
		{
			if (window != null && window.m_Parent != null)
			{
				return window.m_Parent.window != null;
			}
			return false;
		}

		public static void ShowPopupNoLayout(EditorWindow window)
		{
			if (!(window.m_Parent != null))
			{
				window.ShowPopupWithMode(UnityEditor.ShowMode.PopupMenu, giveFocus: false);
				if (EditorWindow.focusedWindow != window)
				{
					window.Focus();
				}
				else
				{
					window.Repaint();
				}
				if (!(window.m_Parent == null))
				{
					window.m_Parent.window.m_DontSaveToLayout = true;
				}
			}
		}

		public static void DontSaveToLayout(EditorWindow window)
		{
			if (HasContainer(window))
			{
				window.m_Parent.window.m_DontSaveToLayout = true;
			}
		}

		public static bool IsTransientWindow(EditorWindow window)
		{
			if (!HasContainer(window))
			{
				return false;
			}
			switch (window.m_Parent.window.showMode)
			{
			case UnityEditor.ShowMode.NormalWindow:
			case UnityEditor.ShowMode.NoShadow:
			case UnityEditor.ShowMode.MainWindow:
				return false;
			case UnityEditor.ShowMode.PopupMenu:
			case UnityEditor.ShowMode.Utility:
			case UnityEditor.ShowMode.AuxWindow:
			case UnityEditor.ShowMode.Tooltip:
			case UnityEditor.ShowMode.ModalUtility:
				return true;
			default:
				return false;
			}
		}
	}
}

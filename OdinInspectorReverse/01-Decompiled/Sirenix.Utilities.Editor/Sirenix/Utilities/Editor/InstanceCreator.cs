using System;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// Not yet documented.
	/// </summary>
	public static class InstanceCreator
	{
		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static int ControlID { get; private set; }

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static Type CurrentSelectedType { get; internal set; }

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static bool HasCreatedInstance { get; internal set; }

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static Type Type { get; private set; }

		internal static object CreatedInstance { get; set; }

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static object GetCreatedInstance()
		{
			if (HasCreatedInstance)
			{
				Type = null;
				ControlID = 0;
				object instance = CreatedInstance;
				CreatedInstance = null;
				HasCreatedInstance = false;
				return instance;
			}
			Debug.LogError("Check if HasCreatedInstance is true before calling GetCreatedInstance.");
			return null;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static void Show(Type type, int controlId, Rect buttonRect = default(Rect))
		{
			Type = type;
			ControlID = controlId;
			InstanceCreatorWindow window = ScriptableObject.CreateInstance<InstanceCreatorWindow>();
			window.Initialize();
			if (Event.current != null && UnityShims.Misc.GetEventModifiers(Event.current) == 2)
			{
				UnityEngine.Object.DestroyImmediate(window);
				return;
			}
			if (buttonRect.width > 0f && buttonRect.height > 0f)
			{
				float prevWidth = buttonRect.width;
				buttonRect.width = Mathf.Clamp(prevWidth, 250f, 500f);
				buttonRect.x += prevWidth - buttonRect.width;
				Vector2 windowSize = new Vector2(buttonRect.width, Mathf.Min(buttonRect.width, window.GetWindowHeight()));
				if (Event.current != null)
				{
					buttonRect.position = GUIUtility.GUIToScreenPoint(buttonRect.position);
				}
				window.ShowAsDropDown(buttonRect, windowSize);
			}
			else
			{
				Vector2 windowSize2 = new Vector2(500f, 500f);
				Rect windowRect = GUIHelper.GetEditorWindowRect();
				window.ShowAuxWindow();
				window.position = UnityShims.Rect.Ctor(windowRect.center - windowSize2 * 0.5f, new Vector2(windowSize2.x, window.GetWindowHeight()));
			}
			window.minSize = new Vector2(20f, 20f);
			window.maxSize = window.position.size;
		}
	}
}

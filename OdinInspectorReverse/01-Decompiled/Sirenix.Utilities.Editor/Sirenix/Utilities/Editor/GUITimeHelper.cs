using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sirenix.Reflection.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// A utility class for getting delta time for the GUI editor.
	/// </summary>
	public class GUITimeHelper
	{
		private struct Key : IEquatable<Key>
		{
			public int WindowHash;

			public IMGUIContainer CurrentContainer;

			public override bool Equals(object obj)
			{
				if (obj is Key key)
				{
					return Equals(key);
				}
				return false;
			}

			public bool Equals(Key other)
			{
				if (WindowHash == other.WindowHash)
				{
					return EqualityComparer<IMGUIContainer>.Default.Equals(CurrentContainer, other.CurrentContainer);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return WindowHash + CurrentContainer.GetHashCode();
			}
		}

		private const float fallbackDeltaTime = 0.02f;

		private static Dictionary<IMGUIContainer, (GUITimeHelper layout, GUITimeHelper repaint)> handlers = new Dictionary<IMGUIContainer, (GUITimeHelper, GUITimeHelper)>();

		private static Stopwatch sw = Stopwatch.StartNew();

		private float deltaTime;

		private double lastTime;

		private EventType trackingEvent;

		public static float RepaintDeltaTime
		{
			get
			{
				IMGUIContainer key = UIElementsUtility_Internals.GetCurrentIMGUIContainer();
				if (key != null && handlers.TryGetValue(key, out (GUITimeHelper, GUITimeHelper) val))
				{
					return val.Item2.deltaTime;
				}
				return 0.02f;
			}
		}

		public static int RepaintFPS => (int)(1f / RepaintDeltaTime);

		public static float LayoutDeltaTime
		{
			get
			{
				IMGUIContainer key = UIElementsUtility_Internals.GetCurrentIMGUIContainer();
				if (key != null && handlers.TryGetValue(key, out (GUITimeHelper, GUITimeHelper) val))
				{
					return val.Item1.deltaTime;
				}
				return 0.02f;
			}
		}

		public static int LayoutFPS => (int)(1f / LayoutDeltaTime);

		[InitializeOnLoadMethod]
		private static void Init()
		{
			UIElementsUtility_Internals.BeginContainerCallback += OnBeginContainer;
		}

		private static void OnBeginContainer(IMGUIContainer obj)
		{
			if (!handlers.TryGetValue(obj, out (GUITimeHelper, GUITimeHelper) val))
			{
				(GUITimeHelper, GUITimeHelper) tuple = (handlers[obj] = (new GUITimeHelper(EventType.Layout), new GUITimeHelper(EventType.Repaint)));
				val = tuple;
				obj.RegisterCallback<DetachFromPanelEvent>(delegate
				{
					handlers.Remove(obj);
				});
			}
			val.Item1.Update();
			val.Item2.Update();
		}

		private GUITimeHelper(EventType trackingEvent)
		{
			deltaTime = 0.02f;
			lastTime = sw.Elapsed.TotalSeconds;
			this.trackingEvent = trackingEvent;
		}

		private void Update()
		{
			if (Event.current.type == trackingEvent)
			{
				double time = sw.Elapsed.TotalSeconds;
				float newDeltaTime = (float)(time - lastTime);
				if (newDeltaTime <= 0.2f)
				{
					deltaTime = newDeltaTime;
				}
				lastTime = time;
			}
		}
	}
}

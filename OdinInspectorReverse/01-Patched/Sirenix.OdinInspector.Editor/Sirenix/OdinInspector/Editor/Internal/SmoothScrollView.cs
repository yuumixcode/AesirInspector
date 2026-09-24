using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal struct SmoothScrollView
	{
		public SirenixAnimationUtility.InterpolatedFloat InterpolatedPosition;

		public float Position;

		public float Destination => InterpolatedPosition.Destination;

		public void DrawVerticalScrollbar(Rect rect)
		{
		}

		public void Begin(Rect viewRect, Rect contentRect)
		{
			HandleInput(viewRect, contentRect);
			Position = InterpolatedPosition.GetValue();
			GUI.BeginScrollView(viewRect, new Vector2(0f, Position), contentRect, GUIStyle.none, GUIStyle.none);
			InterpolatedPosition.Move(3.3333333f, Easing.OutCubic);
		}

		public void End()
		{
			GUI.EndScrollView(handleScrollWheel: false);
		}

		public void HandleInput(Rect viewRect, Rect contentRect)
		{
			Event e = Event.current;
			if (!e.IsHovering(viewRect))
			{
				return;
			}
			EventType type = e.type;
			if (type == EventType.ScrollWheel)
			{
				float destination = Destination;
				destination += e.delta.y * 34f;
				if (destination < 0f)
				{
					destination = 0f;
				}
				if (destination + viewRect.height > contentRect.height)
				{
					destination = contentRect.height - viewRect.height;
				}
				InterpolatedPosition.ChangeDestination(destination);
				e.Use();
			}
		}
	}
}

using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// A utility class for properly counting frames and helps determine when a frame has started in an editor window.
	/// </summary>
	public class GUIFrameCounter
	{
		private int frameCount;

		private bool isNewFrame = true;

		private bool nextEventIsNew = true;

		/// <summary>
		/// Gets the frame count.
		/// </summary>
		public int FrameCount => frameCount;

		/// <summary>
		/// Gets a value indicating whether this instance is new frame.
		/// </summary>
		public bool IsNewFrame => isNewFrame;

		/// <summary>
		/// Updates the frame counter and returns itself.
		/// </summary>
		public GUIFrameCounter Update()
		{
			if (Event.current == null)
			{
				return this;
			}
			EventType e = Event.current.type;
			if (e == EventType.Repaint)
			{
				nextEventIsNew = true;
				isNewFrame = false;
				return this;
			}
			if (nextEventIsNew && e != EventType.Repaint)
			{
				frameCount++;
				nextEventIsNew = false;
				isNewFrame = true;
				return this;
			}
			isNewFrame = false;
			return this;
		}
	}
}

using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// Icon for using in editor GUI.
	/// </summary>
	public abstract class EditorIcon
	{
		private static Material blurWhenDownscalingMaterial;

		private GUIContent inactiveGUIContent;

		private GUIContent highlightedGUIContent;

		private GUIContent activeGUIContent;

		/// <summary>
		/// Gets the raw input icon texture.
		/// </summary>
		public abstract Texture2D Raw { get; }

		/// <summary>
		/// Gets the icon's highlighted texture.
		/// </summary>
		public abstract Texture Highlighted { get; }

		/// <summary>
		/// Gets the icon's active texture.
		/// </summary>
		public abstract Texture Active { get; }

		/// <summary>
		/// Gets the icon's inactive texture.
		/// </summary>
		public abstract Texture Inactive { get; }

		/// <summary>
		/// Gets a GUIContent object with the active texture.
		/// </summary>
		public GUIContent ActiveGUIContent
		{
			get
			{
				if (activeGUIContent == null || activeGUIContent.image == null)
				{
					activeGUIContent = new GUIContent(Inactive);
				}
				return activeGUIContent;
			}
		}

		/// <summary>
		/// Gets a GUIContent object with the inactive texture.
		/// </summary>
		public GUIContent InactiveGUIContent
		{
			get
			{
				if (inactiveGUIContent == null || inactiveGUIContent.image == null)
				{
					inactiveGUIContent = new GUIContent(Inactive);
				}
				return inactiveGUIContent;
			}
		}

		/// <summary>
		/// Gets a GUIContent object with the highlighted texture.
		/// </summary>
		public GUIContent HighlightedGUIContent
		{
			get
			{
				if (highlightedGUIContent == null || highlightedGUIContent.image == null)
				{
					highlightedGUIContent = new GUIContent(Inactive);
				}
				return highlightedGUIContent;
			}
		}

		/// <summary>
		/// Draws the icon in a square rect, with a custom shader that makes the icon look better when down-scaled.
		/// This also handles mouseover effects, and linier color spacing.
		/// </summary>
		public void Draw(Rect rect)
		{
			if (Event.current.type == EventType.Repaint)
			{
				Texture iconTex;
				if (!GUI.enabled)
				{
					iconTex = Inactive;
				}
				else if (rect.Contains(Event.current.mousePosition))
				{
					GUIHelper.RequestRepaint();
					iconTex = Highlighted;
				}
				else
				{
					iconTex = Active;
				}
				Draw(rect, iconTex);
			}
		}

		/// <summary>
		/// Draws the icon in a square rect, with a custom shader that makes the icon look better when down-scaled.
		/// This also handles mouseover effects, and linier color spacing.
		/// </summary>
		public void Draw(Rect rect, float drawSize)
		{
			if (Event.current.type == EventType.Repaint)
			{
				Texture iconTex;
				if (!GUI.enabled)
				{
					iconTex = Inactive;
				}
				else if (rect.Contains(Event.current.mousePosition))
				{
					GUIHelper.RequestRepaint();
					iconTex = Highlighted;
				}
				else
				{
					iconTex = Active;
				}
				rect = rect.AlignCenter(drawSize, drawSize);
				Draw(rect, iconTex);
			}
		}

		/// <summary>
		/// Draws the icon in a square rect, with a custom shader that makes the icon look better when down-scaled.
		/// This also handles mouseover effects, and linier color spacing.
		/// </summary>
		public void Draw(Rect rect, Texture texture)
		{
			if (Event.current.type == EventType.Repaint)
			{
				rect.x = (int)rect.x;
				rect.y = (int)rect.y;
				rect.width = (int)rect.width;
				rect.height = (int)rect.height;
				GUI.DrawTexture(rect, texture);
			}
		}
	}
}

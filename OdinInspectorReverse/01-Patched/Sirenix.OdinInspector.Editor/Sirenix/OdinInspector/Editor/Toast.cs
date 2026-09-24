using System;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public class Toast
	{
		public Rect CurrentRect;

		public float TimePassed;

		public readonly ToastPosition ToastPosition;

		public readonly SdfIconType Icon;

		public readonly string Header;

		public readonly string Body;

		public readonly Color Color;

		public readonly float ExpiryTime;

		public readonly string ButtonText;

		public readonly Action ButtonOnClick;

		private static GUIStyle style;

		private static GUIStyle styleHeader;

		private static GUIStyle styleCentered;

		public static GUIStyle Style
		{
			get
			{
				GUIStyle result = style ?? new GUIStyle(SirenixGUIStyles.MultiLineCenteredLabel)
				{
					richText = true,
					fontStyle = FontStyle.Normal,
					fontSize = 12,
					alignment = TextAnchor.UpperLeft,
					normal = 
					{
						textColor = Color.white
					},
					padding = new RectOffset(0, 0, 0, 0),
					margin = new RectOffset(0, 0, 0, 0)
				};
				style = result;
				return result;
			}
		}

		public static GUIStyle StyleHeader
		{
			get
			{
				GUIStyle result = styleHeader ?? new GUIStyle(SirenixGUIStyles.MultiLineCenteredLabel)
				{
					richText = true,
					fontStyle = FontStyle.Normal,
					fontSize = 12,
					alignment = TextAnchor.UpperLeft,
					normal = 
					{
						textColor = Color.white
					},
					padding = new RectOffset(0, 0, 0, 0),
					margin = new RectOffset(0, 0, 0, 0)
				};
				styleHeader = result;
				return result;
			}
		}

		public static GUIStyle StyleCentered
		{
			get
			{
				GUIStyle result = styleCentered ?? new GUIStyle(SirenixGUIStyles.MultiLineCenteredLabel)
				{
					richText = true,
					fontStyle = FontStyle.Normal,
					fontSize = 12,
					alignment = TextAnchor.MiddleCenter,
					normal = 
					{
						textColor = Color.white
					},
					padding = new RectOffset(0, 0, 0, 0),
					margin = new RectOffset(0, 0, 0, 0)
				};
				styleCentered = result;
				return result;
			}
		}

		public Toast(ToastPosition position, SdfIconType icon, string header, string body, Color color, float expiryTime, string buttonText, Action buttonOnClick)
		{
			CurrentRect = Rect.zero;
			ToastPosition = position;
			Icon = icon;
			Header = header;
			Body = body;
			Color = color;
			ExpiryTime = expiryTime;
			ButtonText = buttonText;
			ButtonOnClick = buttonOnClick;
		}
	}
}

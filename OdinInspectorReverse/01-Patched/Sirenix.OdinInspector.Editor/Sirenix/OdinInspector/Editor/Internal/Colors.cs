using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	public static class Colors
	{
		public static class DesignerOverviewWindow
		{
			public static readonly Color EditorBackground = new Color(0.1f, 0.1f, 0.1f);

			public static readonly Color StatusBarBackground = (EditorGUIUtility.isProSkin ? new Color(0.16f, 0.16f, 0.16f) : new Color(0.65f, 0.65f, 0.65f));

			public static readonly Color AreaSeparator = (EditorGUIUtility.isProSkin ? new Color(0.1f, 0.1f, 0.1f) : new Color(0.55f, 0.55f, 0.55f));
		}

		public static class Shadows
		{
			public static readonly Color DesignerEditorScrollView = (EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.761f, 0.761f, 0.761f));
		}

		public static class Editor
		{
			public static readonly Color Bg = (EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.761f, 0.761f, 0.761f));

			public static readonly Color HeaderBg = (EditorGUIUtility.isProSkin ? new Color(0.235f, 0.235f, 0.235f) : new Color(0.82f, 0.82f, 0.82f));

			public static readonly Color FooterBg = (EditorGUIUtility.isProSkin ? new Color(0.235f, 0.235f, 0.235f) : new Color(0.82f, 0.82f, 0.82f));
		}

		public static class Accents
		{
			public static readonly Color Fields = (EditorGUIUtility.isProSkin ? new Color(0.161f, 0.659f, 0.584f) : new Color(0f, 0.4f, 0.35f));

			public static readonly Color Properties = (EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f) : new Color(0.5f, 0f, 0.5f));

			public static readonly Color Method = (EditorGUIUtility.isProSkin ? new Color(0.878f, 0.722f, 0.51f) : new Color(0.55f, 0.38f, 0.15f));
		}

		public static class Icons
		{
			public static readonly Color Default = (EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.237f, 0.237f, 0.237f));

			public static readonly Color CaretAndTag = (EditorGUIUtility.isProSkin ? new Color(0.439f, 0.439f, 0.439f) : new Color(0.514f, 0.514f, 0.514f));

			public static readonly Color Hover = (EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.1f) : new Color(1f, 1f, 1f));
		}

		public static class Node
		{
			public static readonly Color LightBorder = (EditorGUIUtility.isProSkin ? new Color(0.1f, 0.1f, 0.1f) : new Color(0.498f, 0.498f, 0.498f));

			public static readonly Color Border = (EditorGUIUtility.isProSkin ? new Color(0.05f, 0.05f, 0.05f) : new Color(0.498f, 0.498f, 0.498f));

			public static readonly Color BgSelected = (EditorGUIUtility.isProSkin ? new Color(0.12f, 0.12f, 0.12f) : new Color(0.97f, 0.97f, 0.97f));

			public static readonly Color Bg = (EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.9f, 0.9f, 0.9f));

			public static readonly Color BgHover = (EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.839f, 0.839f, 0.839f));

			public static readonly Color BgHidden = (EditorGUIUtility.isProSkin ? new Color(0.16f, 0.16f, 0.16f) : new Color(0.761f, 0.761f, 0.761f));

			public static readonly Color Halo = (EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.575f, 0.575f, 0.575f));

			public static readonly Color PressedBg = (EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0.4f) : new Color(1f, 1f, 1f, 0.4f));

			public static readonly Color PressedBlur = new Color(0f, 0f, 0f, 0.2f);

			public static readonly Color ButtonBg = new Color(0f, 0f, 0f, 0.2f);

			public static readonly Color ButtonHover = (EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.05f) : new Color(1f, 1f, 1f, 0.5f));
		}

		public static class Button
		{
			public static readonly Color Border = (EditorGUIUtility.isProSkin ? new Color(0.12f, 0.12f, 0.12f) : new Color(0.498f, 0.498f, 0.498f));
		}

		public static class Group
		{
			public static readonly Color HeaderBg = (EditorGUIUtility.isProSkin ? new Color(0.189f, 0.189f, 0.189f) : new Color(0.867f, 0.867f, 0.867f));

			public static readonly Color Border = (EditorGUIUtility.isProSkin ? new Color(0.05f, 0.05f, 0.05f) : new Color(0.498f, 0.498f, 0.498f));

			public static readonly Color BorderHighlighted = (EditorGUIUtility.isProSkin ? new Color(0.5f, 0.5f, 0.5f) : new Color(0f, 0f, 0f));

			public static readonly Color Bg = (EditorGUIUtility.isProSkin ? new Color(0.251f, 0.251f, 0.251f) : new Color(0.82f, 0.82f, 0.82f));

			public static readonly Color ColumnInputBg = (EditorGUIUtility.isProSkin ? new Color(0.165f, 0.165f, 0.165f) : new Color(0.941f, 0.941f, 0.941f));

			public static readonly Color ColumnInputBorder = (EditorGUIUtility.isProSkin ? new Color(0.051f, 0.051f, 0.05f) : new Color(0.627f, 0.627f, 0.627f));

			public static readonly Color ColumnInputBorderHover = (EditorGUIUtility.isProSkin ? new Color(0.396f, 0.396f, 0.396f) : new Color(0.424f, 0.424f, 0.424f));
		}

		public static class AttributePopup
		{
			public static readonly Color TitleBarBg = (EditorGUIUtility.isProSkin ? new Color(0.16f, 0.16f, 0.16f) : new Color(0.647f, 0.647f, 0.647f));

			public static readonly Color HeaderBg = (EditorGUIUtility.isProSkin ? new Color(0.243f, 0.243f, 0.243f) : new Color(0.796f, 0.796f, 0.796f));

			public static readonly Color HeaderBgHover = (EditorGUIUtility.isProSkin ? new Color(0.278f, 0.278f, 0.278f) : new Color(0.839f, 0.839f, 0.839f));

			public static readonly Color Border = (EditorGUIUtility.isProSkin ? new Color(0.133f, 0.133f, 0.133f) : new Color(0.498f, 0.498f, 0.498f));

			public static readonly Color LightBorder = (EditorGUIUtility.isProSkin ? new Color(0.188f, 0.188f, 0.188f) : new Color(0.729f, 0.729f, 0.729f));

			public static Color ButtonBg = (EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.98f, 0.98f, 0.98f));

			public static readonly Color ButtonOverlayHover = (EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.7f, 0.7f, 0.7f));

			public static readonly Color ButtonHaloBg = (EditorGUIUtility.isProSkin ? new Color(0.3f, 0.3f, 0.3f) : new Color(1f, 1f, 1f));

			public static readonly Color ButtonOverlayActive = (EditorGUIUtility.isProSkin ? new Color(0.11f, 0.11f, 0.11f) : new Color(0.4f, 0.4f, 0.4f));

			public static readonly Color ButtonBgActive = (EditorGUIUtility.isProSkin ? new Color(0.1f, 0.1f, 0.1f) : new Color(1f, 1f, 1f));

			public static readonly Color SelectedAttribute = (EditorGUIUtility.isProSkin ? new Color(0.35f, 0.35f, 0.35f) : new Color(0.4f, 0.4f, 0.4f));

			public static readonly Color HoveringAttribute = (EditorGUIUtility.isProSkin ? new Color(0.28f, 0.28f, 0.28f) : new Color(0.95f, 0.95f, 0.95f));

			public static readonly Color CategoryBg = (EditorGUIUtility.isProSkin ? new Color(0.157f, 0.157f, 0.157f) : new Color(0.647f, 0.647f, 0.647f));

			public static readonly Color CategoryBgHover = (EditorGUIUtility.isProSkin ? new Color(0.192f, 0.192f, 0.192f) : new Color(0.55f, 0.55f, 0.55f));

			public static readonly Color CategoryBorder = (EditorGUIUtility.isProSkin ? new Color(0.113f, 0.113f, 0.113f) : new Color(0.45f, 0.45f, 0.45f));

			public static readonly string HintText = (EditorGUIUtility.isProSkin ? "#FFFFFF50" : "#00000088");

			public static readonly Color PinHalo = (EditorGUIUtility.isProSkin ? new Color(0.46f, 0.68f, 1f) : new Color(0.18f, 0.31f, 0.61f));

			public static readonly Color CloseHalo = (EditorGUIUtility.isProSkin ? new Color(1f, 0.27f, 0.26f) : new Color(0.47f, 0f, 0f));
		}

		public static readonly Color[] BreadcrumbColors = ((!EditorGUIUtility.isProSkin) ? new Color[8]
		{
			Color.blue,
			Color.red,
			Color.cyan,
			Color.green,
			Color.yellow,
			Color.magenta,
			Color.gray,
			Color.white
		} : new Color[8]
		{
			new Color(0.2f, 0.53f, 0.84f),
			new Color(0.93f, 0.93f, 0.73f),
			new Color(0.29f, 0.77f, 0.62f),
			new Color(0.85f, 0.55f, 0.45f, 1f),
			new Color(0.75f, 0.85f, 0.5f, 1f),
			new Color(0.55f, 0.5f, 0.75f, 1f),
			new Color(0.6f, 0.75f, 0.35f, 1f),
			new Color(0.85f, 0.65f, 0.8f, 1f)
		});

		public static readonly Color ListItemEven = (EditorGUIUtility.isProSkin ? new Color(0.24f, 0.24f, 0.24f) : new Color(0.761f, 0.761f, 0.761f));

		public static readonly Color ListItemOdd = (EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.85f, 0.85f, 0.85f));

		public static readonly Color ListItemHover = (EditorGUIUtility.isProSkin ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.939f, 0.939f, 0.939f));

		public static readonly Color ListItemSelected = (EditorGUIUtility.isProSkin ? new Color(0.24f, 0.37f, 0.59f) : new Color(0.3f, 0.48f, 0.78f));

		public static readonly Color ListItemHoverSelected = (EditorGUIUtility.isProSkin ? new Color(0.24f, 0.47f, 0.69f) : new Color(0.31f, 0.58f, 0.88f));

		public static readonly Color Shadow = (EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0.2f) : new Color(0f, 0f, 0f, 0.4f));
	}
}

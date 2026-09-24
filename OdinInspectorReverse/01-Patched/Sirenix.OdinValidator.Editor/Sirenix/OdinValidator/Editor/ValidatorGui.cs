using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	internal static class ValidatorGui
	{
		private const string SPINNING_SHADER_NAME = "Hidden/Sirenix/SpinningShader";

		public const int LineHeight = 21;

		private static Vector2 tmpSlideRectValue;

		private static GUIStyle whiteLabelVerticalCentered;

		private static GUIStyle blackLabelVerticalCentered;

		private static GUIStyle labelVerticalCentered;

		private static GUIStyle whiteLabelVertical;

		private static GUIStyle labelVertical;

		private static GUIStyle labelLowerCenterBold;

		private static Material drawColorMat;

		private static Material spinnerMat;

		public static Color ToolbarBgColor = (IsProSkin ? new Color(0.23529412f, 0.23529412f, 0.23529412f, 1f) : Color.Lerp(EditorWindowBgColor, Color.white, 0.15f));

		public static int LeftMenuLineHeight = 21;

		public static int IconButtonWidth = 30;

		public static float ContentPadding = 5f;

		public static Color BtnActiveBgColor = (IsProSkin ? new Color(14f / 51f, 32f / 85f, 0.4862745f, 1f) : new Color(0.22745098f, 38f / 85f, 0.6901961f, 1f));

		public static Color DarkSkinYellowWarningColor = new Color(1f, 0.75686276f, 0.02745098f);

		public static Color DarkSkinRedErrorColor = new Color(1f, 0.3254902f, 0.2901961f);

		public static Color Green = new Color(0.31294116f, 0.9937255f, 0.40627453f, 1f);

		public static Color GreenValidColor = new Color(0.2f, 0.75686276f, 0.02745098f);

		public static Color DarkRed = new Color(59f / 85f, 4f / 85f, 4f / 85f, 1f);

		public static Color RedErrorColor = (IsProSkin ? DarkSkinRedErrorColor : new Color(59f / 85f, 4f / 85f, 4f / 85f, 1f));

		public static Color YellowWarningColor = (IsProSkin ? DarkSkinYellowWarningColor : new Color(67f / 85f, 0.5921569f, 0f, 1f));

		public static Color BorderColor = (IsProSkin ? new Color(0.11f, 0.11f, 0.11f, 0.8f) : new Color(0f, 0f, 0f, 0.184f));

		public static Color BtnContentColor = (IsProSkin ? new Color(0.6901961f, 0.6901961f, 0.6901961f, 1f) : GrayIconColor);

		public static Color BtnMouseOverContentColor = (IsProSkin ? Color.white : Color.black);

		private static Texture2D prefabIcon;

		private static Texture sceneAssetIcon;

		public static Color BtnInActiveBgColor
		{
			get
			{
				if (!IsProSkin)
				{
					return SirenixGUIStyles.DefaultSelectedMenuTreeColorLightSkin;
				}
				return SirenixGUIStyles.DefaultSelectedMenuTreeColorDarkSkin;
			}
		}

		public static Color DarkSkinEditorWindowBgColor => new Color(0.22f, 0.22f, 0.22f, 1f);

		public static Color EditorWindowBgColor
		{
			get
			{
				if (!IsProSkin)
				{
					return new Color(0.76f, 0.76f, 0.76f, 1f);
				}
				return DarkSkinEditorWindowBgColor;
			}
		}

		public static Color BtnMouseOverBgColor
		{
			get
			{
				if (!IsProSkin)
				{
					return Color.Lerp(EditorWindowBgColor, Color.white, 0.15f);
				}
				return Color.Lerp(EditorWindowBgColor, Color.white, 0.1f);
			}
		}

		public static Color HighlightedBgColor
		{
			get
			{
				if (!IsProSkin)
				{
					return new Color(0.9372549f, 0.9372549f, 0.9372549f, 1f);
				}
				return Color.Lerp(EditorWindowBgColor, Color.white, 0.15f);
			}
		}

		public static Color GrayIconColor
		{
			get
			{
				if (!IsProSkin)
				{
					return new Color(1f / 3f, 1f / 3f, 1f / 3f, 1f);
				}
				return new Color(0.76862746f, 0.76862746f, 0.76862746f, 1f);
			}
		}

		public static GUIStyle WhiteLabelVerticalCentered
		{
			get
			{
				object obj = whiteLabelVerticalCentered;
				if (obj == null)
				{
					obj = new GUIStyle(SirenixGUIStyles.WhiteLabel)
					{
						alignment = TextAnchor.MiddleLeft,
						hover = SirenixGUIStyles.WhiteLabel.normal,
						onActive = SirenixGUIStyles.WhiteLabel.normal,
						onHover = SirenixGUIStyles.WhiteLabel.normal,
						onNormal = SirenixGUIStyles.WhiteLabel.normal,
						onFocused = SirenixGUIStyles.WhiteLabel.normal,
						focused = SirenixGUIStyles.WhiteLabel.normal
					};
					whiteLabelVerticalCentered = (GUIStyle)obj;
				}
				return (GUIStyle)obj;
			}
		}

		public static GUIStyle DarkLabelVerticalCentered
		{
			get
			{
				object obj = blackLabelVerticalCentered;
				if (obj == null)
				{
					obj = new GUIStyle(SirenixGUIStyles.BlackLabel)
					{
						alignment = TextAnchor.MiddleLeft,
						hover = SirenixGUIStyles.BlackLabel.normal,
						onActive = SirenixGUIStyles.BlackLabel.normal,
						onHover = SirenixGUIStyles.BlackLabel.normal,
						onNormal = SirenixGUIStyles.BlackLabel.normal,
						onFocused = SirenixGUIStyles.BlackLabel.normal,
						focused = SirenixGUIStyles.BlackLabel.normal
					};
					blackLabelVerticalCentered = (GUIStyle)obj;
				}
				return (GUIStyle)obj;
			}
		}

		public static GUIStyle LabelVerticalCentered
		{
			get
			{
				object obj = labelVerticalCentered;
				if (obj == null)
				{
					obj = new GUIStyle(SirenixGUIStyles.Label)
					{
						alignment = TextAnchor.MiddleLeft,
						hover = SirenixGUIStyles.Label.normal,
						onActive = SirenixGUIStyles.Label.normal,
						onHover = SirenixGUIStyles.Label.normal,
						onNormal = SirenixGUIStyles.Label.normal,
						onFocused = SirenixGUIStyles.Label.normal,
						focused = SirenixGUIStyles.Label.normal
					};
					labelVerticalCentered = (GUIStyle)obj;
				}
				return (GUIStyle)obj;
			}
		}

		public static GUIStyle WhiteLabelVertical
		{
			get
			{
				object obj = whiteLabelVertical;
				if (obj == null)
				{
					obj = new GUIStyle(SirenixGUIStyles.WhiteLabel)
					{
						alignment = TextAnchor.UpperLeft,
						hover = SirenixGUIStyles.WhiteLabel.normal,
						onActive = SirenixGUIStyles.WhiteLabel.normal,
						onHover = SirenixGUIStyles.WhiteLabel.normal,
						onNormal = SirenixGUIStyles.WhiteLabel.normal,
						onFocused = SirenixGUIStyles.WhiteLabel.normal,
						focused = SirenixGUIStyles.WhiteLabel.normal
					};
					whiteLabelVertical = (GUIStyle)obj;
				}
				return (GUIStyle)obj;
			}
		}

		public static GUIStyle LabelVertical
		{
			get
			{
				object obj = labelVertical;
				if (obj == null)
				{
					obj = new GUIStyle(SirenixGUIStyles.Label)
					{
						alignment = TextAnchor.UpperLeft,
						hover = SirenixGUIStyles.Label.normal,
						onActive = SirenixGUIStyles.Label.normal,
						onHover = SirenixGUIStyles.Label.normal,
						onNormal = SirenixGUIStyles.Label.normal,
						onFocused = SirenixGUIStyles.Label.normal,
						focused = SirenixGUIStyles.Label.normal
					};
					labelVertical = (GUIStyle)obj;
				}
				return (GUIStyle)obj;
			}
		}

		public static GUIStyle LabelLowerCenterBold
		{
			get
			{
				object obj = labelLowerCenterBold;
				if (obj == null)
				{
					obj = new GUIStyle(SirenixGUIStyles.BoldLabel)
					{
						alignment = TextAnchor.LowerCenter,
						hover = SirenixGUIStyles.BoldLabel.normal,
						onActive = SirenixGUIStyles.BoldLabel.normal,
						onHover = SirenixGUIStyles.BoldLabel.normal,
						onNormal = SirenixGUIStyles.BoldLabel.normal,
						onFocused = SirenixGUIStyles.BoldLabel.normal,
						focused = SirenixGUIStyles.BoldLabel.normal
					};
					labelLowerCenterBold = (GUIStyle)obj;
				}
				return (GUIStyle)obj;
			}
		}

		public static GUIStyle ActiveLabelVerticalCentered
		{
			get
			{
				if (!IsProSkin)
				{
					return DarkLabelVerticalCentered;
				}
				return WhiteLabelVerticalCentered;
			}
		}

		public static Material SpinnerMat
		{
			get
			{
				if (spinnerMat == null)
				{
					OdinEntityId matEntityId = OdinEntityId.GetSessionStateId("odin_validator_spinnerMat3", OdinEntityId.None);
					if (matEntityId.IsValid)
					{
						spinnerMat = matEntityId.ToObject() as Material;
					}
					if (spinnerMat == null)
					{
						Shader shader = Shader.Find("Hidden/Sirenix/SpinningShader");
						spinnerMat = new Material(shader);
						UnityEngine.Object.DontDestroyOnLoad(spinnerMat);
						spinnerMat.hideFlags = HideFlags.DontUnloadUnusedAsset;
						OdinEntityId.SetSessionStateId("odin_validator_spinnerMat3", OdinEntityId.FromObject(spinnerMat));
					}
				}
				return spinnerMat;
			}
		}

		public static bool IsProSkin => EditorGUIUtility.isProSkin;

		public static Texture PrefabIcon
		{
			get
			{
				if (!prefabIcon)
				{
					prefabIcon = EditorGUIUtility.FindTexture("Prefab Icon");
				}
				return prefabIcon;
			}
		}

		public static Texture SceneAssetIcon
		{
			get
			{
				if (!sceneAssetIcon)
				{
					sceneAssetIcon = EditorGUIUtility.IconContent("SceneAsset Icon").image;
				}
				return sceneAssetIcon;
			}
		}

		public static float VerticalMenuSlider(Rect rect, float width, float minWidth, float maxWidth, int direction = 1)
		{
			Rect slideRect = rect.Expand(5f, 0f);
			float newWidth = Mathf.Clamp(SlideRect(slideRect, new Vector2(width, 0f), MouseCursor.ResizeHorizontal, direction).x, minWidth, maxWidth);
			EditorGUI.DrawRect(rect, BorderColor);
			return newWidth;
		}

		public static bool FoldoutToggle(bool toggled, ref bool val, string name)
		{
			toggled = Foldout(toggled, name, 21);
			Rect toggleRect = GUILayoutUtility.GetLastRect().AlignLeft(21f);
			toggleRect.x += ContentPadding;
			val = EditorGUI.ToggleLeft(toggleRect, GUIContent.none, val);
			return toggled;
		}

		public static bool Foldout(bool val, string name, int leftIndent)
		{
			Rect rect = GUILayoutUtility.GetRect(0f, 21f);
			EditorGUI.DrawRect(rect, ToolbarBgColor);
			EditorGUI.DrawRect(rect.AlignBottom(1f), BorderColor);
			EditorGUI.DrawRect(rect.AlignTop(1f).AddY(-1f), BorderColor);
			Rect foldoutRect = rect.AlignCenterY(EditorGUIUtility.singleLineHeight);
			val = SirenixEditorGUI.Foldout(foldoutRect.HorizontalPadding(ContentPadding).AddXMin(leftIndent), val, new GUIContent(name));
			return val;
		}

		public static bool Foldout(bool val, string name)
		{
			Rect rect = GUILayoutUtility.GetRect(0f, 21f);
			EditorGUI.DrawRect(rect, ToolbarBgColor);
			EditorGUI.DrawRect(rect.AlignBottom(1f), BorderColor);
			EditorGUI.DrawRect(rect.AlignTop(1f).AddY(-1f), BorderColor);
			Rect foldoutRect = rect.AlignCenterY(EditorGUIUtility.singleLineHeight);
			val = SirenixEditorGUI.Foldout(foldoutRect.HorizontalPadding(ContentPadding), val, new GUIContent(name));
			return val;
		}

		public static bool Foldout(Rect rect, bool val, string name, Texture icon)
		{
			EditorGUI.DrawRect(rect, ToolbarBgColor);
			EditorGUI.DrawRect(rect.AlignBottom(1f), BorderColor);
			EditorGUI.DrawRect(rect.AlignTop(1f).AddY(-1f), BorderColor);
			Rect foldoutRect = rect.AlignCenterY(EditorGUIUtility.singleLineHeight);
			Rect iconRect = foldoutRect.AlignLeft(16f).AlignCenterY(16f);
			iconRect.x += 20f;
			val = SirenixEditorGUI.Foldout(foldoutRect.HorizontalPadding(ContentPadding), val, new GUIContent("      " + name));
			GUI.DrawTexture(iconRect, icon);
			return val;
		}

		public static Rect Header(string name, int height = 21)
		{
			Rect rect = GUILayoutUtility.GetRect(0f, height);
			EditorGUI.DrawRect(rect, ToolbarBgColor);
			EditorGUI.DrawRect(rect.AlignBottom(1f), BorderColor);
			EditorGUI.DrawRect(rect.AlignTop(1f).AddY(-1f), BorderColor);
			GUI.Label(rect.AlignBottom(21f).AlignCenterY(EditorGUIUtility.singleLineHeight).HorizontalPadding(ContentPadding), name);
			return rect;
		}

		public static void Header(Rect rect, string name, Texture icon)
		{
			EditorGUI.DrawRect(rect, ToolbarBgColor);
			EditorGUI.DrawRect(rect.AlignBottom(1f), BorderColor);
			EditorGUI.DrawRect(rect.AlignTop(1f).AddY(-1f), BorderColor);
			rect = rect.HorizontalPadding(ContentPadding);
			Rect headerRect = rect.AlignCenterY(EditorGUIUtility.singleLineHeight);
			int iconToggle = ((!(icon == null)) ? 1 : 0);
			Rect iconRect = headerRect.AlignLeft(16 * iconToggle).AlignCenterY(16 * iconToggle);
			GUI.Label(rect.AlignCenterY(EditorGUIUtility.singleLineHeight).AddXMin(iconRect.width), name);
			if (icon != null)
			{
				GUI.DrawTexture(iconRect, icon);
			}
		}

		public static float HorizontalMenuSlider(Rect rect, float height, float minHeight, float maxHeight)
		{
			Rect slideRect = rect.Expand(0f, 5f);
			float newHeight = Mathf.Clamp(SlideRect(slideRect, new Vector2(0f, height), MouseCursor.ResizeVertical).y, minHeight, maxHeight);
			EditorGUI.DrawRect(rect, BorderColor);
			return newHeight;
		}

		public static void HandleKeyboardNavigation(ref int selectedIndex)
		{
			if (Event.current.type == EventType.KeyDown)
			{
				switch (Event.current.keyCode)
				{
				case KeyCode.DownArrow:
					selectedIndex++;
					Event.current.Use();
					GUIHelper.RequestRepaint();
					break;
				case KeyCode.UpArrow:
					selectedIndex--;
					Event.current.Use();
					GUIHelper.RequestRepaint();
					break;
				}
			}
		}

		public static Vector2 SlideRect(Rect rect, Vector2 value, MouseCursor cursor = MouseCursor.SlideArrow, int direction = 1)
		{
			if (!GUI.enabled)
			{
				return value;
			}
			EditorGUIUtility.AddCursorRect(rect, cursor);
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			if (GUI.enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
			{
				GUIUtility.hotControl = controlID;
				Event.current.Use();
				tmpSlideRectValue = value;
			}
			else if (GUIUtility.hotControl == controlID)
			{
				if (Event.current.type == EventType.MouseDrag)
				{
					Event.current.Use();
					GUI.changed = true;
					tmpSlideRectValue += new Vector2(Event.current.delta.x * (float)direction, 0f - Event.current.delta.y);
					return tmpSlideRectValue;
				}
				if (Event.current.type == EventType.MouseUp)
				{
					GUIUtility.hotControl = 0;
					Event.current.Use();
				}
			}
			return value;
		}

		public static float SlideRect(Rect rect, float value, float minValue, float maxValue)
		{
			rect = SirenixEditorGUI.GetFeatureRichControl(rect, null, out var controlId, out var hasKeyboardFocus);
			if (Event.current.type == EventType.Layout)
			{
				return value;
			}
			if (GUI.enabled)
			{
				bool flag = false;
				if ((Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition)) || (GUIUtility.hotControl == controlId && (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag)))
				{
					GUIUtility.hotControl = controlId;
					value = minValue + Mathf.Abs(maxValue - minValue) * Mathf.Clamp01((Event.current.mousePosition.x - rect.xMin) / rect.width) * ((minValue <= maxValue) ? 1f : (-1f));
					flag = true;
				}
				else if (hasKeyboardFocus && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.RightArrow)
				{
					value += (float)((minValue < maxValue) ? 1 : (-1));
					flag = true;
				}
				else if (hasKeyboardFocus && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.LeftArrow)
				{
					value -= (float)((minValue < maxValue) ? 1 : (-1));
					flag = true;
				}
				else if (GUIUtility.hotControl == controlId && Event.current.rawType == EventType.MouseUp)
				{
					GUIUtility.hotControl = 0;
				}
				if (flag)
				{
					GUI.changed = true;
					float num = Math.Min(minValue, maxValue);
					float num2 = Math.Max(minValue, maxValue);
					value = ((value <= num) ? num : ((value >= num2) ? num2 : value));
					GUIHelper.RequestRepaint();
					Event.current.Use();
				}
			}
			return value;
		}

		public static void DrawErrorIcon(Rect iconRect, bool hasErrors, bool useDarkSkin)
		{
			if (useDarkSkin)
			{
				SdfIcons.DrawIcon(iconRect, SdfIconType.ExclamationOctagonFill, hasErrors ? DarkSkinRedErrorColor : GrayIconColor);
			}
			else
			{
				SdfIcons.DrawIcon(iconRect, SdfIconType.ExclamationOctagonFill, hasErrors ? RedErrorColor : GrayIconColor);
			}
		}

		public static void DrawErrorIcon(Rect iconRect, Color bgColor, bool hasErrors, bool useDarkSkin)
		{
			if (useDarkSkin)
			{
				SdfIcons.DrawIcon(iconRect, SdfIconType.ExclamationOctagonFill, hasErrors ? DarkSkinRedErrorColor : GrayIconColor, bgColor);
			}
			else
			{
				SdfIcons.DrawIcon(iconRect, SdfIconType.ExclamationOctagonFill, hasErrors ? RedErrorColor : GrayIconColor, bgColor);
			}
		}

		public static void DrawWarningIcon(Rect iconRect, bool hasWarnings, bool useDarkSkin)
		{
			if (useDarkSkin)
			{
				SdfIcons.DrawIcon(iconRect, SdfIconType.ExclamationTriangleFill, hasWarnings ? DarkSkinYellowWarningColor : GrayIconColor);
			}
			else
			{
				SdfIcons.DrawIcon(iconRect, SdfIconType.ExclamationTriangleFill, hasWarnings ? YellowWarningColor : GrayIconColor);
			}
		}

		public static void DrawWarningIcon(Rect iconRect, Color bgColor, bool hasWarnings, bool useDarkSkin)
		{
			if (useDarkSkin)
			{
				SdfIcons.DrawIcon(iconRect, SdfIconType.ExclamationTriangleFill, hasWarnings ? DarkSkinYellowWarningColor : GrayIconColor, bgColor);
			}
			else
			{
				SdfIcons.DrawIcon(iconRect, SdfIconType.ExclamationTriangleFill, hasWarnings ? YellowWarningColor : GrayIconColor, bgColor);
			}
		}

		public static void DrawValidIcon(Rect iconRect, bool hasValids, bool useDarkSkin)
		{
			SdfIcons.DrawIcon(iconRect, SdfIconType.CheckCircleFill, hasValids ? GreenValidColor : GrayIconColor);
		}

		public static void DrawValidIcon(Rect iconRect, Color bgColor, bool hasValids, bool useDarkSkin)
		{
			SdfIcons.DrawIcon(iconRect, SdfIconType.CheckCircleFill, hasValids ? GreenValidColor : GrayIconColor, bgColor);
		}

		public static void ProgressBar(Rect rect, float t, string text)
		{
			if (text == "")
			{
				text = null;
			}
			EditorGUI.ProgressBar(rect, t, text);
		}

		public static bool IconButton(Rect rect, SdfIconType icon, string tooltip, float vPadding = 3f)
		{
			Color col = ((!rect.Contains(Event.current.mousePosition)) ? BtnContentColor : BtnMouseOverContentColor);
			col.a *= (GUI.enabled ? 1f : 0.2f);
			SdfIcons.DrawIcon(rect.VerticalPadding(vPadding), icon, col);
			return GUI.Button(rect, new GUIContent("", tooltip), GUIStyle.none);
		}

		public static bool ToolbarBtn(Rect rect, bool on, SdfIconType icon, string text, string tooltip)
		{
			if (GUI.Button(rect, new GUIContent("", tooltip), GUIStyle.none))
			{
				on = !on;
			}
			if (Event.current.type == EventType.Repaint)
			{
				Color bgColor = ToolbarBgColor;
				GUIStyle style = LabelVerticalCentered;
				Color col = BtnContentColor;
				if (on)
				{
					bgColor = HighlightedBgColor;
					style = ActiveLabelVerticalCentered;
					col = BtnMouseOverContentColor;
				}
				if (GUI.enabled && rect.Contains(Event.current.mousePosition))
				{
					bgColor = BtnMouseOverBgColor;
					col = BtnMouseOverContentColor;
				}
				EditorGUI.DrawRect(rect, bgColor);
				int iconSize = ((icon != SdfIconType.None) ? ((int)(rect.height * 1.2f)) : 0);
				float textSize = ((text == null) ? 0f : style.CalcSize(new GUIContent(text)).x);
				float padding = rect.width - textSize - (float)iconSize;
				if (padding > 0f)
				{
					rect.x += padding / 2f;
					rect.width -= padding;
				}
				rect = rect.VerticalPadding(3f);
				SdfIcons.DrawIcon(rect.TakeFromLeft(iconSize), icon, col, bgColor);
				if (text != null)
				{
					GUI.Label(rect, text, style);
				}
			}
			return on;
		}

		public static bool ToolbarToggle(Rect rect, bool on, SdfIconType iconOn, SdfIconType iconOff, string text, string tooltip)
		{
			if (GUI.Button(rect, new GUIContent("", tooltip), GUIStyle.none))
			{
				return true;
			}
			if (Event.current.type == EventType.Repaint)
			{
				bool hover = rect.Contains(Event.current.mousePosition);
				GUIStyle style = LabelVertical;
				int iconSize = (int)rect.height;
				Color bgColor = (hover ? BtnMouseOverBgColor : ToolbarBgColor);
				EditorGUI.DrawRect(rect, bgColor);
				rect.TakeFromLeft(3f);
				Rect icon1 = rect.TakeFromLeft(iconSize);
				rect.TakeFromLeft(3f);
				Rect icon2 = rect.TakeFromLeft(iconSize);
				rect.TakeFromLeft(3f);
				Color col1 = (hover ? BtnMouseOverContentColor : BtnContentColor);
				Color col2 = (hover ? BtnMouseOverContentColor : BtnContentColor);
				if (on)
				{
					col2.a *= 0.2f;
				}
				else
				{
					col1.a *= 0.2f;
				}
				GUI.Label(rect, text, LabelVerticalCentered);
				SdfIcons.DrawIcon(icon1.VerticalPadding(3f), iconOn, col1, bgColor);
				SdfIcons.DrawIcon(icon2.VerticalPadding(3f), iconOff, col2, bgColor);
			}
			return false;
		}

		private static void Initialize()
		{
			if (drawColorMat == null)
			{
				drawColorMat = new Material(Shader.Find("UI/Default"));
			}
		}

		public static void DrawTriangles(Vector2[] sPoints, Color sColor)
		{
			if (Event.current.type.Equals(EventType.Repaint))
			{
				Initialize();
				GL.PushMatrix();
				drawColorMat.SetPass(0);
				GL.LoadPixelMatrix();
				GL.Begin(4);
				GL.Color(sColor);
				for (int i = 0; i < sPoints.Length; i++)
				{
					Vector2 tV = sPoints[i];
					GL.Vertex3(tV.x, tV.y, 0f);
				}
				GL.End();
				GL.PopMatrix();
			}
		}

		public static void DrawTriangle(Vector2 sA, Vector2 sB, Vector2 sC, Color sColor)
		{
			if (Event.current.type.Equals(EventType.Repaint))
			{
				Initialize();
				GL.PushMatrix();
				drawColorMat.SetPass(0);
				GL.LoadPixelMatrix();
				GL.Begin(4);
				GL.Color(sColor);
				GL.Vertex3(sA.x, sA.y, 0f);
				GL.Vertex3(sB.x, sB.y, 0f);
				GL.Vertex3(sC.x, sC.y, 0f);
				GL.End();
				GL.PopMatrix();
			}
		}
	}
}

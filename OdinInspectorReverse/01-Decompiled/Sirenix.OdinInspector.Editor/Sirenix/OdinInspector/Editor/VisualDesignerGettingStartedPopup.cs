using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	internal class VisualDesignerGettingStartedPopup
	{
		private class Node
		{
			public Rect Rect;

			public float Speed;

			public Color Color;

			public bool DrawClassButton;

			public string Name;

			public string Type;

			public float Direction;

			public int AttributeCount;
		}

		private static string[] nodeNames = new string[20]
		{
			"Health", "SetHealth", "Damage", "MaxHealth", "IsAlive", "GetPosition", "MoveTo", "Speed", "Velocity", "OnDeath",
			"Jump", "SetVelocity", "Rotation", "GetRotation", "Attack", "TakeDamage", "Mana", "UseMana", "IsVisible", "Target"
		};

		private static string[] nodeTypes = new string[20]
		{
			"int", "void", "void", "int", "bool", "Vector3", "void", "float", "Vector3", "void",
			"void", "void", "Quaternion", "Quaternion", "void", "void", "int", "void", "bool", "GameObject"
		};

		private static int[] nodeAttributeCount = new int[20]
		{
			12, 7, 19, 3, 16, 8, 20, 4, 11, 0,
			15, 2, 6, 18, 9, 1, 14, 10, 5, 13
		};

		private static Rect _rect;

		private static EditorWindow _window;

		private static SirenixAnimationUtility.InterpolatedFloat alpha = new SirenixAnimationUtility.InterpolatedFloat
		{
			Start = 0f,
			Destination = 1f
		};

		private static readonly List<Node> nodes = new List<Node>();

		private const float AnimMin = 50f;

		private const float AnimMax = 150f;

		private static GUIStyle _titleStyle;

		private static GUIStyle _popupTextStyle;

		private static GUIStyle _miniLabel;

		private static GUIStyle _labelStyle;

		private static GUIStyle TitleStyle
		{
			get
			{
				GUIStyle obj = _titleStyle ?? new GUIStyle(SirenixGUIStyles.SectionHeaderCentered)
				{
					clipping = TextClipping.Clip,
					wordWrap = true,
					richText = true
				};
				_titleStyle = obj;
				return obj;
			}
		}

		private static GUIStyle PopupTextStyle
		{
			get
			{
				GUIStyle obj = _popupTextStyle ?? new GUIStyle(SirenixGUIStyles.MultiLineWhiteLabel)
				{
					clipping = TextClipping.Clip,
					alignment = TextAnchor.MiddleCenter,
					wordWrap = true,
					richText = true
				};
				_popupTextStyle = obj;
				return obj;
			}
		}

		private static GUIStyle MiniLabel
		{
			get
			{
				if (_miniLabel == null)
				{
					GUIStyle baseStyle = (EditorGUIUtility.isProSkin ? SirenixGUIStyles.CenteredGreyMiniLabel : SirenixGUIStyles.MiniLabelCentered);
					_miniLabel = new GUIStyle(baseStyle);
				}
				return _miniLabel;
			}
		}

		private static GUIStyle LabelStyle
		{
			get
			{
				if (_labelStyle == null)
				{
					_labelStyle = new GUIStyle(SirenixGUIStyles.Label)
					{
						fontSize = 12
					};
				}
				return _labelStyle;
			}
		}

		public static bool Draw(Rect rect, EditorWindow window)
		{
			if (Event.current.type == EventType.Layout)
			{
				return false;
			}
			GUIHelper.RequestRepaint();
			if (_window != window)
			{
				alpha.Reset(0f);
				_window = window;
				nodes.Clear();
			}
			if (_rect != rect)
			{
				nodes.Clear();
				int totalRows = Mathf.FloorToInt(rect.height / 30f);
				int j = 0;
				for (int row = 0; row < totalRows; row += 2)
				{
					if (row != totalRows)
					{
						float y = rect.y + 40f + (float)row * 30f;
						float rn = Random.Range(0f, 1f);
						int count = (((double)rn < 0.6) ? 1 : (((double)rn < 0.9) ? 2 : 4));
						float speed = Random.Range(50f, 150f);
						int direction = ((j % 2 != 0) ? 1 : (-1));
						for (int i = 0; i < count; i++)
						{
							float spawnX = ((direction != -1) ? (rect.xMin - (float)(i + 1) * 310f + 10f) : (rect.xMax + (float)i * 310f));
							spawnX += speed * 9999f * (float)direction;
							nodes.Add(new Node
							{
								Rect = new Rect(spawnX, y, 300f, 30f),
								Speed = speed,
								Color = Color.HSVToRGB(Random.Range(0f, 1f), 0.3f, 0.5f),
								DrawClassButton = (Random.Range(0, 2) == 0),
								Name = nodeNames[(j + i) % nodeNames.Length],
								Type = nodeTypes[(j + i) % nodeTypes.Length],
								AttributeCount = nodeAttributeCount[(j + i) % nodeAttributeCount.Length],
								Direction = direction
							});
						}
						j++;
					}
				}
				_rect = rect;
			}
			alpha.Move(0.7f, Easing.OutSine);
			EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.75f * (float)alpha));
			rect = rect.AlignCenter(window.position.width * 0.8f, window.position.height * 0.7f);
			if (!rect.Contains(Event.current.mousePosition) && Event.current.OnMouseDown(0))
			{
				return true;
			}
			Rect ogRect = rect;
			Color bgColor = new Color(0.2f, 0.2f, 0.2f, alpha);
			Color borderColor = new Color(1f, 1f, 1f, alpha);
			Color gridColor1 = new Color(0.25f, 0.25f, 0.25f, alpha);
			Color gridColor2 = new Color(0.3f, 0.3f, 0.3f, alpha);
			GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, bgColor, 0f, 10f);
			DrawGrid(rect, 20f, gridColor1, Vector2.zero);
			DrawGrid(rect, 40f, gridColor2, Vector2.zero);
			for (int k = 0; k < nodes.Count; k++)
			{
				Node node = nodes[k];
				node.Rect.x += node.Speed * GUITimeHelper.LayoutDeltaTime * node.Direction;
				if (Mathf.Abs(node.Direction - -1f) < 0.001f)
				{
					if (node.Rect.xMax < 0f)
					{
						node.Rect = new Rect(rect.width, node.Rect.y, node.Rect.width, node.Rect.height);
					}
				}
				else if (node.Rect.xMin > rect.width)
				{
					node.Rect = new Rect(0f - node.Rect.width, node.Rect.y, node.Rect.width, node.Rect.height);
				}
			}
			GUI.BeginClip(rect);
			foreach (Node node2 in nodes)
			{
				DrawNode(node2.Rect, node2.Name, node2.Type, node2.Color, node2.DrawClassButton, alpha, node2.AttributeCount);
			}
			GUI.EndClip();
			GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, new Color(0f, 0f, 0f, 0.7f * (float)alpha), Vector4.zero, new Vector4(0f, 0f, 10f, 10f));
			GUI.DrawTexture(ogRect.Expand(1.25f), Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, borderColor, 1.25f, 10f);
			return DrawTextbox(rect, "<b><color=#FFFFFF>Introducing the Visual Designer</color></b>", "The Visual Designer is a new workflow for <b><color=#FFFFFF>customizing how types appear in the Inspector — all without writing code</color></b>. It lets you apply and manage attributes visually, right inside the Editor. This is especially useful if you prefer a <b><color=#FFFFFF>visual workflow</color></b>, need to make quick layout adjustments, or want to empower <b><color=#FFFFFF>non-programmers</color></b> to tweak inspector layouts and functionality.\n\nTake a look at our short \"Get Started\" guide to see it in action.");
		}

		private static bool DrawTextbox(Rect screenRect, string title, string text)
		{
			float targetWidth = screenRect.width * 0.6f;
			float textContentWidth = targetWidth - 60f;
			float measuredTitleH = TitleStyle.CalcHeight(new GUIContent(title), targetWidth - 40f);
			float measuredTextH = PopupTextStyle.CalcHeight(new GUIContent(text), textContentWidth);
			float desiredHeight = 20f + measuredTitleH + 10f + 30f + measuredTextH + 30f + 10f + 50f + 20f;
			float maxHeight = Mathf.Min(screenRect.height * 0.9f, screenRect.height);
			float finalHeight = Mathf.Min(desiredHeight, maxHeight);
			Rect rect = screenRect.AlignCenter(targetWidth, finalHeight);
			GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, new Color(0.1f, 0.1f, 0.1f, alpha), 0f, 6f);
			GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, new Color(0.3f, 0.3f, 0.3f, alpha), 1f, 6f);
			Rect titleRect = rect.TakeFromTop(measuredTitleH + 20f).HorizontalPadding(20f);
			GUI.Label(titleRect, title, TitleStyle);
			Rect btnRectOuter = rect.TakeFromBottom(70f);
			Rect btnRect = btnRectOuter.Padding(20f);
			Rect contentArea = rect.Padding(0f);
			contentArea.yMin = titleRect.yMax + 10f;
			contentArea.yMax = btnRectOuter.yMin - 10f;
			Rect textInner = contentArea.Padding(30f);
			GUI.Label(textInner, text, PopupTextStyle);
			Rect goBtnRect = btnRect.Split(0, 2).HorizontalPadding(0f, 5f);
			Rect closeBtnRect = btnRect.Split(1, 2).HorizontalPadding(5f, 0f);
			Color goBtnColor = (Event.current.IsHovering(goBtnRect) ? new Color(0.16f, 0.33f, 0.53f) : new Color(0.16f, 0.29f, 0.47f, alpha));
			GUI.DrawTexture(goBtnRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, goBtnColor, 0f, 4f);
			GUI.Label(goBtnRect, "Get Started", SirenixGUIStyles.WhiteLabelCentered);
			if (Event.current.OnMouseUp(goBtnRect, 0))
			{
				Application.OpenURL("https://odininspector.com/visual-designer-getting-started");
			}
			Color closeBtnColor = (Event.current.IsHovering(closeBtnRect) ? new Color(0.25f, 0.25f, 0.25f, alpha) : new Color(0.2f, 0.2f, 0.2f, alpha));
			GUI.DrawTexture(closeBtnRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, closeBtnColor, 0f, 4f);
			GUI.Label(closeBtnRect, "Close", SirenixGUIStyles.WhiteLabelCentered);
			if (Event.current.OnMouseUp(closeBtnRect, 0))
			{
				return true;
			}
			return false;
		}

		private static void DrawGrid(Rect rect, float spacing, Color color, Vector2 offset)
		{
			float xOffset = offset.x % spacing;
			float yOffset = offset.y % spacing;
			int verticalLines = Mathf.FloorToInt(rect.width / spacing);
			int horizontalLines = Mathf.FloorToInt(rect.height / spacing);
			for (int i = 0; i <= verticalLines; i++)
			{
				float x = rect.x + (float)i * spacing + xOffset;
				EditorGUI.DrawRect(new Rect(x, rect.y, 1f, rect.height), color);
			}
			for (int j = 0; j <= horizontalLines; j++)
			{
				float y = rect.y + (float)j * spacing + yOffset;
				EditorGUI.DrawRect(new Rect(rect.x, y, rect.width, 1f), color);
			}
		}

		private static void DrawNode(Rect rect, string name, string type, Color color, bool drawClassButton, float alpha, int attributeCount)
		{
			Rect unchangedRect = rect;
			Color bgColor = Colors.Node.Bg;
			SirenixEditorGUI.DrawRoundRect(unchangedRect.Expand(1f), Color.clear, 3f, Colors.Node.LightBorder, 1.5f);
			SirenixEditorGUI.DrawRoundRect(unchangedRect, bgColor, 3f);
			string attributeCountLabel = attributeCount.ToString();
			float attributeCountWidth = ((attributeCount >= 10) ? 16f : 10f);
			float typeLabelWidth = MiniLabel.CalcWidth(type);
			float attributeButtonWidth = rect.height;
			float classButtonWidth = rect.height;
			Rect working = rect;
			working.TakeFromRight(6f);
			Rect accentRect = working.TakeFromRight(4f).VerticalPadding(6f);
			working.TakeFromRight(6f);
			Rect attributeCountRect = working.TakeFromRight(attributeCountWidth);
			Rect attributeButtonRect = working.TakeFromRight(attributeButtonWidth);
			Rect classButtonRect = working.TakeFromRight(classButtonWidth).AddY(1f);
			Rect typeLabelRect = working.TakeFromRight(typeLabelWidth);
			working.TakeFromRight(6f);
			working.TakeFromLeft(9f);
			Rect nameAreaRect = working;
			SdfIcons.DrawIcon(attributeButtonRect.Padding(8f), SdfIconType.PencilSquare, Colors.Icons.Default);
			SdfIcons.DrawIcon(classButtonRect.Padding(8f), SdfIconType.BoxArrowInUpRight, Colors.Icons.Default);
			GUI.Label(attributeCountRect, attributeCountLabel, MiniLabel);
			SirenixEditorGUI.DrawRoundRect(accentRect, color, float.MaxValue);
			GUI.Label(typeLabelRect, type, MiniLabel);
			GUI.Label(nameAreaRect, name, LabelStyle);
		}
	}
}

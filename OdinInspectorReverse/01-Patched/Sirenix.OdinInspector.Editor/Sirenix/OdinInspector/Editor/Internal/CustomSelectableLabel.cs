using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class CustomSelectableLabel
	{
		private readonly struct RichMap
		{
			private readonly int[] _visToRaw;

			private readonly int[] _rawToVis;

			public string Visible { get; }

			private RichMap(string visible, int[] visToRaw, int[] rawToVis)
			{
				Visible = visible;
				_visToRaw = visToRaw;
				_rawToVis = rawToVis;
			}

			public int VisToRaw(int visIndex)
			{
				visIndex = Mathf.Clamp(visIndex, 0, _visToRaw.Length - 1);
				return _visToRaw[visIndex];
			}

			public int RawToVis(int rawIndex)
			{
				rawIndex = Mathf.Clamp(rawIndex, 0, _rawToVis.Length - 1);
				return _rawToVis[rawIndex];
			}

			public static RichMap Build(string s)
			{
				if (string.IsNullOrEmpty(s))
				{
					return new RichMap(string.Empty, new int[1], new int[1]);
				}
				_ranges.Clear();
				FindRichTagRanges(s, _ranges);
				_sb.Length = 0;
				_visibleRaw.Clear();
				int len = s.Length;
				int rIndex = 0;
				int rangeIdx = 0;
				while (rIndex < len)
				{
					if (rangeIdx < _ranges.Count && rIndex >= _ranges[rangeIdx].start && rIndex < _ranges[rangeIdx].end)
					{
						rIndex = _ranges[rangeIdx].end;
						rangeIdx++;
					}
					else
					{
						_visibleRaw.Add(rIndex);
						_sb.Append(s[rIndex]);
						rIndex++;
					}
				}
				string visible = _sb.ToString();
				int visLen = _visibleRaw.Count;
				int rawLen = len;
				int[] visToRaw = new int[visLen + 1];
				if (visLen > 0)
				{
					visToRaw[0] = _visibleRaw[0];
					for (int i = 1; i < visLen; i++)
					{
						visToRaw[i] = _visibleRaw[i];
					}
					visToRaw[visLen] = rawLen;
				}
				else
				{
					visToRaw[0] = rawLen;
				}
				int[] rawToVis = new int[rawLen + 1];
				int k = 0;
				for (int r = 0; r <= rawLen; r++)
				{
					for (; k < visLen && r > _visibleRaw[k]; k++)
					{
					}
					rawToVis[r] = k;
				}
				return new RichMap(visible, visToRaw, rawToVis);
			}
		}

		private struct TagRange
		{
			public int start;

			public int end;
		}

		private static readonly Color MatchColor = new Color(1f, 0.95f, 0.3f, 0.25f);

		private static readonly List<TagRange> _ranges = new List<TagRange>(32);

		private static readonly List<int> _visibleRaw = new List<int>(256);

		private static readonly StringBuilder _sb = new StringBuilder(256);

		public static void Draw(Rect rect, string text, GUIStyle style)
		{
			GUIStyle richStyle = (style.richText ? style : new GUIStyle(style)
			{
				richText = true
			});
			RichMap map = RichMap.Build(text);
			int controlId = GUIUtility.GetControlID(FocusType.Keyboard, rect);
			TextEditor textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), controlId);
			textEditor.text = map.Visible;
			TextEditorVersionAgnostic.SetMultiline(textEditor, value: true);
			int selVStart = Mathf.Min(textEditor.cursorIndex, textEditor.selectIndex);
			int selVEnd = Mathf.Max(textEditor.cursorIndex, textEditor.selectIndex);
			HandleInput(rect, controlId, text, map, textEditor, richStyle);
			int selRStart = map.VisToRaw(selVStart);
			int selREnd = map.VisToRaw(selVEnd);
			DrawSelectionHighlight(rect, richStyle, text, selRStart, selREnd, GUI.skin.settings.selectionColor);
			DrawOtherMatchHighlights(rect, richStyle, text, map, selVStart, selVEnd, MatchColor);
			GUI.Label(rect, text, richStyle);
		}

		private static void HandleInput(Rect rect, int controlId, string rawText, RichMap map, TextEditor textEditor, GUIStyle style)
		{
			Event e = Event.current;
			if (e.OnMouseDown(rect, 0))
			{
				GUIUtility.keyboardControl = controlId;
				GUIUtility.hotControl = controlId;
				int rawIdx = IndexFromMouseRaw(e.mousePosition);
				int visIdx = map.RawToVis(rawIdx);
				if (e.clickCount == 2)
				{
					string v = map.Visible;
					int len = v.Length;
					int i = Mathf.Clamp(visIdx, 0, len);
					int start = i;
					int end = i;
					if (i > 0 && IsWordChar(v[i - 1]))
					{
						start = i - 1;
						while (start > 0 && IsWordChar(v[start - 1]))
						{
							start--;
						}
						for (end = i; end < len && IsWordChar(v[end]); end++)
						{
						}
					}
					else if (i < len && IsWordChar(v[i]))
					{
						start = i;
						while (start > 0 && IsWordChar(v[start - 1]))
						{
							start--;
						}
						for (end = i + 1; end < len && IsWordChar(v[end]); end++)
						{
						}
					}
					else if (i < len)
					{
						start = i;
						end = i + 1;
					}
					else
					{
						start = i;
						end = i;
					}
					textEditor.selectIndex = start;
					textEditor.cursorIndex = end;
				}
				else if (e.shift)
				{
					textEditor.cursorIndex = visIdx;
				}
				else
				{
					textEditor.selectIndex = visIdx;
					textEditor.cursorIndex = visIdx;
				}
			}
			else if (e.OnMouseUp(0, useEvent: false))
			{
				GUIHelper.RemoveFocusControl();
			}
			else if (GUIUtility.hotControl == controlId && e.OnMouseMoveDrag())
			{
				int rawIdx2 = IndexFromMouseRaw(e.mousePosition);
				int visIdx2 = map.RawToVis(rawIdx2);
				textEditor.cursorIndex = visIdx2;
				e.Use();
			}
			else
			{
				if (e.type != EventType.KeyDown)
				{
					return;
				}
				bool action = e.control || e.command;
				if (action && e.keyCode == KeyCode.A)
				{
					textEditor.selectIndex = 0;
					textEditor.cursorIndex = map.Visible.Length;
					e.Use();
				}
				else if (action && e.keyCode == KeyCode.C)
				{
					int a = Mathf.Min(textEditor.cursorIndex, textEditor.selectIndex);
					int b = Mathf.Max(textEditor.cursorIndex, textEditor.selectIndex);
					if (b > a)
					{
						EditorGUIUtility.systemCopyBuffer = map.Visible.Substring(a, b - a);
					}
					e.Use();
				}
			}
			int IndexFromMouseRaw(Vector2 mousePosition)
			{
				Vector2 local = new Vector2(mousePosition.x - rect.x, mousePosition.y - rect.y);
				return style.GetCursorStringIndex(rect, GUIHelper.TempContent(rawText), local);
			}
		}

		private static void DrawSelectionHighlight(Rect rect, GUIStyle style, string text, int selectionStartRaw, int selectionEndRaw, Color color)
		{
			GUIContent content = GUIHelper.TempContent(text);
			Vector2 startCursorPosition = style.GetCursorPixelPosition(rect, content, selectionStartRaw);
			for (int i = selectionStartRaw; i < selectionEndRaw; i++)
			{
				Vector2 nextCursorPosition = style.GetCursorPixelPosition(rect, content, i + 1);
				if (nextCursorPosition.y > startCursorPosition.y)
				{
					float lineHeight = Mathf.Max(style.lineHeight, nextCursorPosition.y - startCursorPosition.y);
					float width = rect.xMax - startCursorPosition.x;
					EditorGUI.DrawRect(new Rect(startCursorPosition.x, startCursorPosition.y, width, lineHeight), color);
					startCursorPosition.x = 0f;
					startCursorPosition.y = nextCursorPosition.y;
				}
				if (i == selectionEndRaw - 1)
				{
					float width2 = Mathf.Max(0f, nextCursorPosition.x - startCursorPosition.x);
					float height = Mathf.Max(style.lineHeight, nextCursorPosition.y - startCursorPosition.y);
					EditorGUI.DrawRect(new Rect(rect.x + startCursorPosition.x, rect.y + startCursorPosition.y, width2, height), color);
				}
			}
		}

		private static void DrawOtherMatchHighlights(Rect rect, GUIStyle style, string rawText, RichMap map, int selVStart, int selVEnd, Color color)
		{
			int len = selVEnd - selVStart;
			if (len <= 2)
			{
				return;
			}
			string selection = map.Visible.Substring(selVStart, len);
			foreach (char c in selection)
			{
				if (c == '\n' || c == '\r')
				{
					return;
				}
			}
			int i = 0;
			while (true)
			{
				i = map.Visible.IndexOf(selection, i, StringComparison.Ordinal);
				if (i >= 0)
				{
					if (i != selVStart)
					{
						int rStart = map.VisToRaw(i);
						int rEnd = map.VisToRaw(i + len);
						DrawSelectionHighlight(rect, style, rawText, rStart, rEnd, color);
					}
					i += len;
					continue;
				}
				break;
			}
		}

		private static bool IsWordChar(char c)
		{
			if (!char.IsLetterOrDigit(c))
			{
				return c == '_';
			}
			return true;
		}

		private static void FindRichTagRanges(string s, List<TagRange> outRanges)
		{
			int len = s.Length;
			for (int i = 0; i < len; i++)
			{
				if (s[i] == '<' && TryGetRichTagRange(s, i, out var endExclusive))
				{
					outRanges.Add(new TagRange
					{
						start = i,
						end = endExclusive
					});
					i = endExclusive - 1;
				}
			}
		}

		private static bool TryGetRichTagRange(string s, int i, out int endExclusive)
		{
			endExclusive = -1;
			int len = s.Length;
			int j = i + 1;
			if (j >= len)
			{
				return false;
			}
			bool closing = s[j] == '/';
			if (closing)
			{
				j++;
				if (j >= len)
				{
					return false;
				}
			}
			int nameStart = j;
			for (; j < len && char.IsLetter(s[j]); j++)
			{
			}
			if (j == nameStart)
			{
				return false;
			}
			string name = s.Substring(nameStart, j - nameStart);
			switch (name)
			{
			default:
				if (!(name == "size"))
				{
					return false;
				}
				break;
			case "b":
			case "i":
			case "color":
				break;
			}
			for (; j < len && char.IsWhiteSpace(s[j]); j++)
			{
			}
			if (!closing && (name == "color" || name == "size"))
			{
				if (j < len && s[j] == '=')
				{
					j++;
				}
				else
				{
					int k;
					for (k = j; k < len && char.IsWhiteSpace(s[k]); k++)
					{
					}
					if (k >= len || s[k] != '=')
					{
						return false;
					}
					j = k + 1;
				}
				int gt = s.IndexOf('>', j);
				if (gt < 0)
				{
					return false;
				}
				endExclusive = gt + 1;
				return true;
			}
			for (; j < len && char.IsWhiteSpace(s[j]); j++)
			{
			}
			if (j < len && s[j] == '>')
			{
				endExclusive = j + 1;
				return true;
			}
			return false;
		}
	}
}

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class ColorStringOverviewWindow : FancyWindow
	{
		private struct Slot
		{
			public bool IsHeader;

			public string Header;

			public Color Color;

			public static Slot HeaderSlot(string header)
			{
				return new Slot
				{
					IsHeader = true,
					Header = header
				};
			}

			public static Slot ColorSlot(Color color)
			{
				return new Slot
				{
					IsHeader = false,
					Color = color
				};
			}
		}

		public struct Palette
		{
			public string Name;

			public List<Color> Colors;

			public Palette(string name, List<Color> colors)
			{
				Name = name;
				Colors = colors;
			}
		}

		public string Selected;

		public Action<string> OnSelect;

		private const int Size = 30;

		public readonly List<Palette> Palettes = new List<Palette>();

		private const float OuterPadding = 10f;

		private const float CellInnerPadding = 4f;

		private const float Gap = 0f;

		private const float PaletteHeaderHeight = 18f;

		private const float PaletteHeaderGap = 6f;

		private const float PaletteGapAfter = 10f;

		private const float ScrollbarTrackWidth = 20f;

		private const float ClipYPadding = 1f;

		private readonly VirtualizedScrollView scrollView = new VirtualizedScrollView(16, 60f);

		private readonly List<Slot> slots = new List<Slot>(512);

		private ValueResolver<Color> colorResolver;

		private InspectorProperty property;

		public static ColorStringOverviewWindow Show(InspectorProperty property, string currentlySelectedColor)
		{
			ColorStringOverviewWindow window = ScriptableObject.CreateInstance<ColorStringOverviewWindow>();
			window.hideFlags = HideFlags.HideAndDontSave;
			window.Title = "Select Color";
			window.property = property;
			window.Selected = currentlySelectedColor;
			window.Palettes.Clear();
			window.Palettes.Add(new Palette("Default Odin Colors", BuildDefaultPalette()));
			foreach (ColorPalette colorPalette in GlobalConfig<ColorPaletteManager>.Instance.ColorPalettes)
			{
				window.Palettes.Add(new Palette(colorPalette.Name, colorPalette.Colors));
			}
			UnityShims.Rect.Ctor(out var contextRect, GUIUtility.GUIToScreenPoint(Event.current.mousePosition) - new Vector2(2f, 2f), new Vector2(4f, 4f));
			float width = 320f;
			float height = 270f;
			Rect windowRect = new Rect(contextRect.x + contextRect.width - width, contextRect.yMax + 2f, width, height);
			Rect fittedWindowRect = EditorWindow_Internal.FitPositionInWorkingArea(window, windowRect, useMouseScreen: false);
			if (!contextRect.Overlaps(fittedWindowRect))
			{
				window.position = fittedWindowRect;
			}
			else
			{
				windowRect.y = contextRect.y - height - 2f;
				window.position = EditorWindow_Internal.FitPositionInWorkingArea(window, windowRect, useMouseScreen: false);
			}
			EditorWindow_Internal.ShowPopupNoLayout(window);
			return window;
		}

		private static List<Color> BuildDefaultPalette()
		{
			List<Color> list = new List<Color>(ColorValueResolverCreator.colorMap.Count);
			foreach (KeyValuePair<string, Color> item in ColorValueResolverCreator.colorMap)
			{
				list.Add(item.Value);
			}
			return list;
		}

		private void OnEnable()
		{
			base.titleContent = new GUIContent("Colors");
		}

		protected override void OnDraw()
		{
			Event e = Event.current;
			Color currentColor = ResolveColor(Selected);
			Rect viewportRect = ContentRect;
			Rect footerRect = viewportRect.TakeFromBottom(31f);
			EditorGUI.DrawRect(footerRect.TakeFromTop(1f), SirenixGUIStyles.BorderColor);
			Rect colorPickerRect = footerRect.Padding(4f);
			EditorGUI.BeginChangeCheck();
			Color pickedColor = EditorGUI.ColorField(colorPickerRect, currentColor);
			if (EditorGUI.EndChangeCheck())
			{
				Select(pickedColor);
			}
			Rect contentRect = viewportRect.Padding(10f, 10f, 10f, 10f);
			BuildLayout(viewportRect, contentRect, contentRect.width, out var contentHeight);
			if (contentHeight > viewportRect.height + 0.01f)
			{
				float contentWidth = Mathf.Max(1f, contentRect.width - 20f);
				BuildLayout(viewportRect, contentRect, contentWidth, out var _);
			}
			List<VirtualizedScrollView.VisibleSlot> visibleSlots = scrollView.GetVisibleSlots(viewportRect, viewportRect);
			scrollView.Begin();
			if (e.type == EventType.Repaint || e.type == EventType.MouseDown)
			{
				for (int i = 0; i < visibleSlots.Count; i++)
				{
					VirtualizedScrollView.VisibleSlot vs = visibleSlots[i];
					Slot slot = slots[vs.Index];
					Rect rect = vs.Rect;
					rect.y += 1f;
					if (slot.IsHeader)
					{
						if (e.type == EventType.Repaint)
						{
							GUI.Label(rect, slot.Header, SirenixGUIStyles.BoldLabel);
						}
						continue;
					}
					Rect inner = rect.Padding(4f);
					if (inner.Contains(e.mousePosition))
					{
						inner = inner.Expand(3f);
					}
					if (e.type == EventType.Repaint)
					{
						if (slot.Color.a == 0f)
						{
							Color c = slot.Color;
							SirenixEditorGUI.DrawRoundRect(inner, c, 3f, new Color(c.r, c.g, c.b, 1f), 2f);
						}
						else
						{
							SirenixEditorGUI.DrawRoundRect(inner, slot.Color, 3f);
						}
					}
					if (e.type == EventType.MouseDown && e.button == 0 && inner.Contains(e.mousePosition))
					{
						Select(slot.Color);
						Close();
						e.Use();
						break;
					}
				}
			}
			scrollView.End();
		}

		private void Select(Color color)
		{
			if (OnSelect != null)
			{
				Selected = color.ToString();
				EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, (EditorApplication.CallbackFunction)delegate
				{
					OnSelect(Selected);
				});
			}
		}

		private Color ResolveColor(string resolvedString)
		{
			colorResolver = ValueResolver.Get<Color>(property, resolvedString);
			return colorResolver.GetValue();
		}

		private void BuildLayout(Rect viewportRect, Rect contentRect, float contentWidth, out float contentHeight)
		{
			scrollView.Reset();
			slots.Clear();
			float x0 = contentRect.x;
			float y = contentRect.y;
			for (int p = 0; p < Palettes.Count; p++)
			{
				Palette palette = Palettes[p];
				if (palette.Colors != null && palette.Colors.Count != 0)
				{
					if (!string.IsNullOrEmpty(palette.Name))
					{
						Rect headerRect = new Rect(x0, y, contentWidth, 18f);
						scrollView.AllocateRect(headerRect);
						slots.Add(Slot.HeaderSlot(palette.Name));
						y += 24f;
					}
					ComputeGrid(contentWidth, out var columns, out var cellSize);
					int rows = Mathf.CeilToInt((float)palette.Colors.Count / (float)columns);
					float gridHeight = (float)rows * cellSize + (float)(rows - 1) * 0f;
					for (int i = 0; i < palette.Colors.Count; i++)
					{
						int col = i % columns;
						int row = i / columns;
						float cx = x0 + (float)col * (cellSize + 0f);
						float cy = y + (float)row * (cellSize + 0f);
						Rect cellRect = new Rect(cx, cy, cellSize, cellSize);
						scrollView.AllocateRect(cellRect);
						slots.Add(Slot.ColorSlot(palette.Colors[i]));
					}
					y += gridHeight + 10f;
				}
			}
			contentHeight = y - viewportRect.y;
		}

		private static void ComputeGrid(float availableWidth, out int columns, out float cellSize)
		{
			float minCell = 30f;
			columns = Mathf.Max(1, Mathf.FloorToInt((availableWidth + 0f) / (minCell + 0f)));
			cellSize = (availableWidth - (float)(columns - 1) * 0f) / (float)columns;
			if (cellSize < minCell)
			{
				cellSize = minCell;
			}
		}
	}
}

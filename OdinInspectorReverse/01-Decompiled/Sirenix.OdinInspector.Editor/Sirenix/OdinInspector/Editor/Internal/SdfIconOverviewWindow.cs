using System;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	public class SdfIconOverviewWindow : EditorWindow
	{
		[NonSerialized]
		private SdfIcon[] icons;

		private string prevSearch = "123";

		private string searchFilter;

		private GUIStyle padding;

		private int size = 50;

		private float scrollPos;

		private float scrollMax;

		private Color backColor = new Color(0.1f, 0.1f, 0.1f, 1f);

		private Color iconColor = new Color(1f, 1f, 1f, 1f);

		private float f;

		internal Action<SdfIconType> onSelect;

		internal SdfIconType? selected;

		public SdfIconType? MouseOverIcon { get; private set; }

		private void OnEnable()
		{
			base.titleContent = new GUIContent("Sdf Icon Overview");
		}

		public static void ShowWindow()
		{
			EditorWindow.GetWindow<SdfIconOverviewWindow>();
		}

		private void OnGUI()
		{
			if (Event.current.type == EventType.Layout && (prevSearch != searchFilter || icons == null || icons.Length == 0))
			{
				if (string.IsNullOrEmpty(searchFilter))
				{
					icons = SdfIcons.AllIcons;
				}
				else
				{
					icons = SdfIcons.AllIcons.Where((SdfIcon x) => FuzzySearch.Contains(searchFilter, x.Name)).ToArray();
				}
				prevSearch = searchFilter;
				this.padding = this.padding ?? new GUIStyle
				{
					padding = new RectOffset(20, 20, 10, 10)
				};
			}
			GUILayout.BeginHorizontal(this.padding);
			GUILayout.BeginVertical();
			searchFilter = EditorGUILayout.TextField("Search", searchFilter);
			this.size = (int)EditorGUILayout.Slider("Size", this.size, 10f, 128f);
			GUILayout.EndVertical();
			GUILayout.BeginVertical();
			backColor = EditorGUILayout.ColorField("Preview back color", backColor);
			iconColor = EditorGUILayout.ColorField("Preview icon color", iconColor);
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			scrollPos = EditorGUILayout.BeginScrollView(new Vector2(0f, scrollPos)).y;
			Rect area = GUILayoutUtility.GetRect(0f, scrollMax, GUIStyle.none, GUILayoutOptions.ExpandHeight());
			EditorGUI.DrawRect(area, backColor);
			if (Event.current.type == EventType.Repaint || Event.current.type == EventType.MouseDown)
			{
				int selectedIndex = (int)(selected.HasValue ? selected.Value : ((SdfIconType)(-1)));
				float yMax = 0f;
				float padding = 10f;
				float s = (float)this.size + padding;
				int num = (int)(area.width / s);
				float remain = area.width % s / (float)num;
				s += remain;
				area.width += 1f;
				Vector2 mp = Event.current.mousePosition;
				int mouseOver = -1;
				Rect mouseOverRect = default(Rect);
				for (int i = 0; i < icons.Length; i++)
				{
					Rect cell = area.SplitGrid(s, s, i);
					yMax = Math.Max(yMax, cell.bottom);
					if (cell.Contains(mp))
					{
						mouseOver = i;
						cell = cell.AlignCenter((float)this.size + padding * 2f, (float)this.size + padding * 2f);
						mouseOverRect = cell;
					}
					else
					{
						cell = cell.AlignCenter(this.size, this.size);
					}
					if (selectedIndex == i)
					{
						Color selectedBgColor = iconColor;
						selectedBgColor.a *= 0.4f;
						GUI.DrawTexture(cell.Expand(5f, 5f), Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: true, 0f, selectedBgColor, 0f, 5f);
					}
					SdfIcons.DrawIcon(cell, icons[i], iconColor, backColor);
				}
				scrollMax = yMax;
				MouseOverIcon = null;
				if (mouseOver >= 0)
				{
					GUIStyle style = new GUIStyle(SirenixGUIStyles.WhiteLabel);
					style.fontStyle = FontStyle.Bold;
					string name = icons[mouseOver].Name;
					MouseOverIcon = (SdfIconType)mouseOver;
					Vector2 size = style.CalcSize(new GUIContent(name));
					Vector2 pos = Event.current.mousePosition + new Vector2(30f, 5f);
					UnityShims.Rect.Ctor(out var rect, pos, size);
					float push = rect.xMax - area.xMax + 20f;
					if (push > 0f)
					{
						rect.x -= push;
					}
					EditorGUI.DrawRect(rect.Expand(10f, 5f), Color.black);
					GUI.Label(rect, name, style);
					if (mouseOverRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && onSelect != null)
					{
						EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, (EditorApplication.CallbackFunction)delegate
						{
							onSelect((SdfIconType)icons[mouseOver].Index);
						});
					}
				}
			}
			EditorGUILayout.EndScrollView();
			Repaint();
			if (Event.current.type == EventType.MouseDown)
			{
				GUIHelper.RemoveFocusControl();
			}
		}

		public static Rect Split(Rect rect, int index, int length)
		{
			if (length == 1)
			{
				return rect;
			}
			int count = Math.Max(1, (int)Math.Sqrt(length - 1)) + 1;
			int x = index % count;
			int y = index / count;
			rect.width /= count;
			rect.height /= count;
			rect.x += (float)x * rect.width;
			rect.y += (float)y * rect.height;
			return rect;
		}
	}
}

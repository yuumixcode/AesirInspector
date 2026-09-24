using System;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	public class OdinAttributeExampleItem
	{
		private static GUIStyle tabStyle;

		private static float codeHeight = 200f;

		private static GUIStyle headerGroupStyle;

		private static GUIStyle tabGroupStyle;

		private Type attributeType;

		private OdinRegisterAttributeAttribute registration;

		private AttributeExamplePreview[] examples;

		private Vector2 pos;

		private Rect dragRect;

		private AttributeExamplePreview selectedExample;

		public readonly string Name;

		public bool DrawCodeExample { get; set; }

		public bool HasBeenRegistered => registration != null;

		public OdinAttributeExampleItem(Type attributeType, OdinRegisterAttributeAttribute registration)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			this.attributeType = attributeType;
			this.registration = registration;
			Name = this.attributeType.GetNiceName().SplitPascalCase();
			DrawCodeExample = true;
			AttributeExampleInfo[] exampleInfos = AttributeExampleUtilities.GetAttributeExampleInfos(attributeType);
			examples = new AttributeExamplePreview[exampleInfos.Length];
			for (int i = 0; i < exampleInfos.Length; i++)
			{
				examples[i] = new AttributeExamplePreview(exampleInfos[i]);
			}
			selectedExample = examples.FirstOrDefault();
		}

		public void Draw()
		{
			DrawInternal(drawCodeExample: true);
		}

		public void Draw(bool drawCodeExample)
		{
			DrawInternal(drawCodeExample);
		}

		private void DrawInternal(bool drawCodeExample)
		{
			headerGroupStyle = headerGroupStyle ?? new GUIStyle
			{
				padding = new RectOffset(10, 10, 10, 20)
			};
			tabGroupStyle = tabGroupStyle ?? new GUIStyle
			{
				padding = new RectOffset(20, 20, 20, 20)
			};
			if (drawCodeExample)
			{
				codeHeight -= SirenixEditorGUI.SlideRect(dragRect.Expand(5f).AddY(2f), MouseCursor.SplitResizeUpDown).y;
			}
			Rect headerRect = EditorGUILayout.BeginVertical(headerGroupStyle);
			EditorGUI.DrawRect(headerRect, SirenixGUIStyles.BoxBackgroundColor);
			GUILayout.Label(Name, SirenixGUIStyles.SectionHeader);
			if (!string.IsNullOrEmpty(registration.DocumentationUrl))
			{
				Rect rect = GUILayoutUtility.GetLastRect().AlignCenterY(20f).AlignRight(120f);
				if (GUI.Button(rect, "Documentation", SirenixGUIStyles.MiniButton))
				{
					Help.BrowseURL(registration.DocumentationUrl);
				}
			}
			if (!string.IsNullOrEmpty(registration.Description))
			{
				GUILayout.Space(10f);
				GUILayout.Label(registration.Description, SirenixGUIStyles.MultiLineLabel);
			}
			EditorGUILayout.EndVertical();
			if (examples.Length == 0)
			{
				GUILayout.Label("No examples available.");
				return;
			}
			if (examples.Length > 1)
			{
				Rect toolbarRect = EditorGUILayout.BeginHorizontal();
				if (Event.current.type == EventType.Repaint)
				{
					EditorGUI.DrawRect(toolbarRect, SirenixGUIStyles.BoxBackgroundColor);
					EditorGUI.DrawRect(toolbarRect.AlignTop(1f), new Color(0f, 0f, 0f, 0.3f));
				}
				tabStyle = tabStyle ?? new GUIStyle
				{
					padding = new RectOffset(5, 5, 7, 7)
				};
				AttributeExamplePreview[] array = examples;
				foreach (AttributeExamplePreview item in array)
				{
					GUIContent label = GUIHelper.TempContent(" " + item.ExampleInfo.Name, GUIHelper.GetAssetThumbnail(null, typeof(MonoBehaviour), preferObjectPreviewOverFileIcon: false));
					Vector2 prev = EditorGUIUtility.GetIconSize();
					EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
					Rect rect2 = GUILayoutUtility.GetRect(label, tabStyle).Expand(0.5f);
					if (item == selectedExample)
					{
						Color bg = (EditorGUIUtility.isProSkin ? SirenixGUIStyles.DarkEditorBackground : new Color(0.78f, 0.78f, 0.78f, 1f));
						EditorGUI.DrawRect(rect2, bg);
						SirenixEditorGUI.DrawBorders(rect2.Expand(1f, 1f, 0f, 0f), 1, 1, 1, 0);
					}
					else
					{
						SirenixEditorGUI.DrawBorders(rect2, 0, 0, 0, 1);
						EditorGUI.DrawRect(rect2.AlignRight(1f), new Color(0f, 0f, 0f, 0.3f));
					}
					if (GUI.Button(rect2, GUIContent.none, GUIStyle.none))
					{
						selectedExample = item;
					}
					if (selectedExample != item && rect2.Contains(Event.current.mousePosition))
					{
						GUIHelper.PushColor(new Color(1f, 1f, 1f, 0.4f));
						EditorGUI.DrawRect(rect2, SirenixGUIStyles.DarkEditorBackground);
						SirenixEditorGUI.DrawBorders(rect2, 0, 0, 1, 0);
						GUIHelper.PopColor();
					}
					GUIStyle activeLabel = (EditorGUIUtility.isProSkin ? SirenixGUIStyles.WhiteLabelCentered : SirenixGUIStyles.LabelCentered);
					GUI.Label(rect2, label, (selectedExample == item) ? activeLabel : SirenixGUIStyles.LabelCentered);
					EditorGUIUtility.SetIconSize(prev);
				}
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				EditorGUI.DrawRect(headerRect.AlignBottom(1f), SirenixGUIStyles.BorderColor);
			}
			pos = EditorGUILayout.BeginScrollView(pos, GUILayoutOptions.ExpandWidth());
			GUILayout.BeginVertical(tabGroupStyle);
			selectedExample.Draw();
			GUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
			GUILayout.FlexibleSpace();
			if (drawCodeExample)
			{
				if (Event.current.type == EventType.Repaint)
				{
					dragRect = GUILayoutUtility.GetRect(0f, 4f);
				}
				else
				{
					GUILayoutUtility.GetRect(0f, 4f);
				}
				selectedExample.DrawCode(codeHeight);
			}
		}

		public void OnDeselected()
		{
			AttributeExamplePreview[] array = examples;
			foreach (AttributeExamplePreview example in array)
			{
				example.OnDeselected();
			}
		}
	}
}

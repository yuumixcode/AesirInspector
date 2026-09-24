using System;
using System.IO;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	internal class AttributeExamplePreview
	{
		private static GUIStyle codeTextStyle;

		public AttributeExampleInfo ExampleInfo;

		private PropertyTree tree;

		private string highlightedCode;

		private string highlightedCodeAsComponent;

		private Vector2 scrollPosition;

		private bool showComponent;

		public AttributeExamplePreview(AttributeExampleInfo exampleInfo)
		{
			ExampleInfo = exampleInfo;
			try
			{
				highlightedCode = SyntaxHighlighter.Parse(ExampleInfo.Code);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				highlightedCode = ExampleInfo.Code;
			}
			try
			{
				highlightedCodeAsComponent = SyntaxHighlighter.Parse(ExampleInfo.CodeAsComponent);
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
				highlightedCodeAsComponent = ExampleInfo.CodeAsComponent;
			}
		}

		public void Draw()
		{
			bool hasOdinSerializedMembers = ExampleInfo.ExampleType.IsDefined(typeof(ShowOdinSerializedPropertiesInInspectorAttribute), inherit: false);
			if (ExampleInfo.Description != null || hasOdinSerializedMembers)
			{
				GUILayout.BeginVertical(SirenixGUIStyles.ContentPadding);
				if (ExampleInfo.Description != null)
				{
					GUILayout.Label(ExampleInfo.Description, SirenixGUIStyles.MultiLineLabel);
				}
				GUILayout.EndVertical();
				Rect seperatorRect = GUILayoutUtility.GetRect(0f, 25f);
				seperatorRect.xMin -= 20f;
				seperatorRect.xMax += 20f;
				seperatorRect.y += 10f;
				seperatorRect.height = 5f;
				SirenixEditorGUI.DrawThickHorizontalSeperator(seperatorRect);
				if (hasOdinSerializedMembers)
				{
					SirenixEditorGUI.InfoMessageBox("Note that this example requires Odin's serialization to be enabled to work, since it uses types that Unity will not serialize. If you copy the example as a component using the 'Copy Component' or 'Create Component Script' buttons, the code will have been set up with Odin's serialization enabled already.");
					GUILayout.Space(9f);
				}
			}
			if (tree == null)
			{
				tree = PropertyTree.Create(ExampleInfo.PreviewObject);
			}
			GUILayout.BeginVertical(SirenixGUIStyles.ContentPadding);
			tree.Draw(applyUndo: false);
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
		}

		public void DrawCode(float height)
		{
			Rect rect = SirenixEditorGUI.BeginToolbarBox();
			SirenixEditorGUI.DrawSolidRect(rect, SyntaxHighlighter.BackgroundColor);
			EditorGUI.DrawRect(rect.AlignTop(1f).AddY(-1f), SirenixGUIStyles.BorderColor);
			SirenixEditorGUI.BeginToolbarBoxHeader();
			GUILayout.Space(-4f);
			if (SirenixEditorGUI.ToolbarButton(showComponent ? "View Shortened Code" : "View Component Code"))
			{
				showComponent = !showComponent;
			}
			GUILayout.FlexibleSpace();
			if (SirenixEditorGUI.ToolbarButton("Copy View"))
			{
				if (showComponent)
				{
					Clipboard.Copy(ExampleInfo.CodeAsComponent);
				}
				else
				{
					Clipboard.Copy(ExampleInfo.Code);
				}
			}
			if (ExampleInfo.CodeAsComponent != null && SirenixEditorGUI.ToolbarButton("Save Component Script"))
			{
				string filePath = EditorUtility.SaveFilePanelInProject("Create Component File", ExampleInfo.ExampleType.Name + "Component.cs", "cs", "Choose a location to save the example as a component script.");
				if (!string.IsNullOrEmpty(filePath))
				{
					File.WriteAllText(filePath, ExampleInfo.CodeAsComponent);
					AssetDatabase.Refresh();
				}
				GUIHelper.ExitGUI(removeFocusControl: true);
			}
			GUILayout.Space(-4f);
			SirenixEditorGUI.EndToolbarBoxHeader();
			if (codeTextStyle == null)
			{
				codeTextStyle = new GUIStyle(SirenixGUIStyles.MultiLineLabel);
				codeTextStyle.normal.textColor = SyntaxHighlighter.TextColor;
				codeTextStyle.active.textColor = SyntaxHighlighter.TextColor;
				codeTextStyle.focused.textColor = SyntaxHighlighter.TextColor;
				codeTextStyle.wordWrap = false;
			}
			GUIContent codeContent = (showComponent ? GUIHelper.TempContent(highlightedCodeAsComponent) : GUIHelper.TempContent(highlightedCode));
			Vector2 size = codeTextStyle.CalcSize(codeContent);
			GUILayout.BeginVertical();
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUILayout.Height(height));
			Rect codeRect = GUILayoutUtility.GetRect(size.x + 50f, size.y).AddXMin(4f).AddY(2f);
			EditorGUI.SelectableLabel(codeRect, codeContent.text, codeTextStyle);
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			SirenixEditorGUI.EndToolbarBox();
		}

		public void OnDeselected()
		{
			if (tree != null)
			{
				tree.Dispose();
				tree = null;
			}
		}
	}
}

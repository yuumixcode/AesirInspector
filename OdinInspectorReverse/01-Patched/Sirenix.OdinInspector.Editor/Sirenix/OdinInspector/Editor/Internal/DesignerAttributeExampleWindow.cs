using System;
using System.Linq;
using Sirenix.OdinInspector.Editor.Examples;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class DesignerAttributeExampleWindow : EditorWindow
	{
		public DesignerAttributePopup PopupThatOpenedThisWindow;

		public Type AttributeType;

		private OdinAttributeExampleItem Example;

		private bool isDragging;

		private Vector2 dragStartPosition;

		public static DesignerAttributeExampleWindow Open(Rect rect, Type attributeType)
		{
			OdinAttributeExampleItem example = AttributeExampleUtilities.GetExample(attributeType);
			if (!example.HasBeenRegistered)
			{
				return null;
			}
			DesignerAttributeExampleWindow window = Resources.FindObjectsOfTypeAll<DesignerAttributeExampleWindow>().FirstOrDefault();
			if (window == null)
			{
				window = ScriptableObject.CreateInstance<DesignerAttributeExampleWindow>();
				window.position = UnityShims.Rect.Ctor(rect.position, new Vector2(700f, 550f));
			}
			window.Example?.OnDeselected();
			window.Example = example;
			window.AttributeType = attributeType;
			EditorWindow_Internal.ShowPopupNoLayout(window);
			return window;
		}

		public static void CloseAllDesignerAttributeExampleWindows()
		{
			DesignerAttributeExampleWindow[] windows = Resources.FindObjectsOfTypeAll<DesignerAttributeExampleWindow>();
			DesignerAttributeExampleWindow[] array = windows;
			foreach (DesignerAttributeExampleWindow window in array)
			{
				window.Close();
			}
		}

		private void OnGUI()
		{
			Rect headerRect = GUILayoutUtility.GetRect(0f, 26f, GUILayoutOptions.ExpandWidth().ExpandHeight(expand: false));
			EditorGUI.DrawRect(headerRect, Colors.AttributePopup.TitleBarBg);
			EditorGUI.DrawRect(headerRect.AlignBottom(1f).AddY(1f), SirenixGUIStyles.BorderColor);
			DesignerGUI.DrawHaloBar(headerRect, "DesignerAttributePopup_Header", Colors.AttributePopup.ButtonHaloBg, Colors.AttributePopup.PinHalo, Colors.AttributePopup.CloseHalo, 0.15f, 0.15f);
			Rect popoutRect = headerRect.TakeFromLeft(headerRect.height);
			Rect closeRect = headerRect.TakeFromRight(headerRect.height);
			if (GUI.Button(closeRect, new GUIContent("", DesignerGUI.Tooltips.CloseWindow), GUIStyle.none))
			{
				Close();
			}
			if (GUI.Button(popoutRect, new GUIContent("", DesignerGUI.Tooltips.PopoutExample), GUIStyle.none))
			{
				Popout();
			}
			SdfIcons.DrawIcon(popoutRect.Padding(6f), Event.current.IsHovering(popoutRect) ? SdfIconType.PipFill : SdfIconType.Pip);
			SdfIcons.DrawIcon(closeRect.Padding(6f), SdfIconType.X);
			Example?.Draw(drawCodeExample: false);
			SirenixEditorGUI.DrawBorders(base.position.SetPosition(Vector2.zero), 1, Colors.AttributePopup.Border);
			int viewId = GUIUtility.GetControlID(FocusType.Passive, base.position);
			(isDragging, dragStartPosition) = DesignerEditorWindow.HandleWindowMovement(isDragging, dragStartPosition, this, viewId);
			Repaint();
		}

		private void Popout()
		{
			AttributesExampleWindow window = AttributesExampleWindow.OpenWindow(AttributeType);
			window.position = new Rect(base.position.x, base.position.y, window.position.width, window.position.height);
			CloseAllDesignerAttributeExampleWindows();
		}

		private void OnDestroy()
		{
			Example?.OnDeselected();
		}
	}
}

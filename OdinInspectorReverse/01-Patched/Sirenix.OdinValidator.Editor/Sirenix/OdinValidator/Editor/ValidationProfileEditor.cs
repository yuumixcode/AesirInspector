using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	[CustomEditor(typeof(ValidationProfile), true)]
	internal class ValidationProfileEditor : OdinEditor
	{
		public override bool UseDefaultMargins()
		{
			return false;
		}

		public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
		{
			ValidationProfile profile = base.target as ValidationProfile;
			if ((bool)profile)
			{
				Texture2D icon = SdfIcons.CreateTransparentIconTexture(profile.Icon, SirenixGUIStyles.HighlightedTextColor, width, height, 0);
				icon.hideFlags = HideFlags.DontUnloadUnusedAsset;
				Object.DontDestroyOnLoad(icon);
				return icon;
			}
			return base.RenderStaticPreview(assetPath, subAssets, width, height);
		}

		protected override void OnHeaderGUI()
		{
			ValidationProfile profile = base.target as ValidationProfile;
			Rect headerRect = GUILayoutUtility.GetRect(0f, 56f);
			EditorGUI.DrawRect(headerRect.TakeFromBottom(1f), SirenixGUIStyles.BorderColor);
			headerRect = headerRect.Padding(10f, 5f, 9f, 10f);
			Rect iconRect = headerRect.TakeFromLeft(34f).SetSize(24f, 24f);
			Rect titleRect = headerRect.TakeFromTop(EditorStyles.largeLabel.fontSize).AddY(-5f).SetHeight(20f);
			Rect buttonsRect = headerRect.TakeFromTop(EditorStyles.popup.fixedHeight).AddY(6f);
			GUI.Label(titleRect, profile.name, EditorStyles.largeLabel);
			if (GUI.Button(buttonsRect.TakeFromRight(150f), "Open Validator"))
			{
				ValidationSessionEditor editor = ((!(profile == MainValidationProfile.Instance)) ? OdinValidatorWindow.OpenWindow(profile) : OdinValidatorWindow.OpenWindow());
				editor.SelectedMenu = ValidationSessionEditor.MenuOptions.FilterResults;
				editor.MenuVisibility = true;
			}
			SdfIconType? mouseOverIcon;
			SdfIconType newIcon = SdfIconSelector.DrawIconSelectorDropdownField(buttonsRect, null, profile.icon, out mouseOverIcon);
			SdfIcons.DrawIcon(iconRect, mouseOverIcon ?? profile.Icon);
			if (GUI.changed)
			{
				Undo.RecordObject(profile, "Changed icon");
				profile.icon = newIcon;
			}
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
		}
	}
}

using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Adds menu items to the Unity Editor, draws the About window, and the preference window found under Edit &gt; Preferences &gt; Odin Inspector.
	/// </summary>
	public class OdinInspectorAboutWindow : EditorWindow
	{
		private void OnGUI()
		{
			GUILayout.BeginArea(new Rect(10f, 10f, base.position.width - 20f, base.position.height - 5f));
			string subtitle = OdinInspectorVersion.BuildName;
			SirenixEditorGUI.Title("Odin Inspector & Serializer", subtitle, TextAlignment.Left, horizontalLine: true);
			if (OdinInspectorVersion.HasLicensee)
			{
				GUILayout.Label("Licensed to " + OdinInspectorVersion.Licensee, SirenixGUIStyles.LeftAlignedGreyMiniLabel);
			}
			DrawAboutGUI();
			GUILayout.EndArea();
			this.RepaintIfRequested();
		}

		internal static void DrawAboutGUI()
		{
			UnityShims.Rect.Ctor(out var result, EditorGUILayout.GetControlRect());
			result.height = 90f;
			Rect position = result;
			GUI.DrawTexture(position.SetWidth(86f).SetHeight(75f).AddY(4f)
				.AddX(-5f), EditorIcons.OdinInspectorLogo, ScaleMode.ScaleAndCrop);
			UnityShims.Rect.Ctor(out result, position);
			result.x = position.x + 82f;
			result.y = position.y + 0f - 2f;
			result.height = 18f;
			GUI.Label(result, OdinInspectorVersion.Version, SirenixGUIStyles.LeftAlignedGreyMiniLabel);
			UnityShims.Rect.Ctor(out result, position);
			result.x = position.x + 82f;
			result.y = position.y + 20f - 2f;
			result.height = 18f;
			GUI.Label(result, "Developed and published by Sirenix", SirenixGUIStyles.LeftAlignedGreyMiniLabel);
			UnityShims.Rect.Ctor(out result, position);
			result.x = position.x + 82f;
			result.y = position.y + 40f - 2f;
			result.height = 18f;
			GUI.Label(result, "All rights reserved", SirenixGUIStyles.LeftAlignedGreyMiniLabel);
			GUIStyle linkStyle = EditorStyles.miniButton;
			float width = linkStyle.CalcSize(GUIHelper.TempContent("www.odininspector.com")).x;
			UnityShims.Rect.Ctor(out result, position);
			result.x = position.xMax - width;
			result.y = position.y + 0f;
			result.width = width;
			result.height = 14f;
			DrawLink(result, "www.odininspector.com", "https://odininspector.com", linkStyle);
		}

		private static void DrawLink(Rect rect, string label, string link, GUIStyle style)
		{
			if (GUI.Button(rect, label, style))
			{
				Application.OpenURL(link);
			}
		}
	}
}

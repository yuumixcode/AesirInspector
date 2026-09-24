using System;
using Sirenix.OdinInspector.Editor.GettingStarted;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sirenix.OdinInspector.Editor
{
	public class AcceptEULAWindow
	{
		public static string HAS_ACCEPTED_EULA_PREFS_KEY = "ACCEPTED_ODIN_3_0_PERSONAL_EULA";

		private static bool hasReadAndUnderstood;

		private static bool isUnderRevenueCap;

		private static SirenixAnimationUtility.InterpolatedFloat t1;

		private static SirenixAnimationUtility.InterpolatedFloat t2;

		private static SirenixAnimationUtility.InterpolatedFloat t3;

		public static bool HasAcceptedEULA => EditorPrefs.GetBool(HAS_ACCEPTED_EULA_PREFS_KEY, defaultValue: false);

		private static bool IsHeadlessMode => SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;

		[InitializeOnLoadMethod]
		private static void OpenIfNotAccepted()
		{
			EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, new EditorApplication.CallbackFunction(OnUpdate));
		}

		private static void OnUpdate()
		{
			if (HasAcceptedEULA || IsHeadlessMode || InternalEditorUtility.inBatchMode)
			{
				EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.update, new EditorApplication.CallbackFunction(OnUpdate));
			}
			else if (!EditorApplication.isCompiling)
			{
				try
				{
					GettingStartedWindow.ShowWindow();
					EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.update, new EditorApplication.CallbackFunction(OnUpdate));
				}
				catch (Exception innerException)
				{
					EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.update, new EditorApplication.CallbackFunction(OnUpdate));
					Debug.LogException(new Exception("An exception happened while attempting to open Odin's EULA popup window.", innerException));
				}
			}
		}

		public static bool Draw(Rect rect, bool closeWindowAfterAccept, EditorWindow window)
		{
			GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, new Color(0.22f, 0.22f, 0.22f), 0f, 10f);
			GUI.DrawTexture(rect.Expand(1.25f), Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, Color.white, 1.25f, 10f);
			Rect headerRect = rect.TakeFromTop(41f);
			EditorGUI.DrawRect(headerRect.TakeFromBottom(1f), SirenixGUIStyles.BorderColor);
			GUI.DrawTexture(headerRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, new Color(0.16f, 0.16f, 0.16f), Vector4.zero, new Vector4(10f, 10f, 0f, 0f));
			Color prevSectionHeaderColor = SirenixGUIStyles.SectionHeaderCentered.normal.textColor;
			SirenixGUIStyles.SectionHeaderCentered.normal.textColor = Color.white;
			GUI.Label(headerRect, "Odin Personal EULA", SirenixGUIStyles.SectionHeaderCentered);
			SirenixGUIStyles.SectionHeaderCentered.normal.textColor = prevSectionHeaderColor;
			rect.TakeFromTop(20f);
			Rect logoRect = rect.TakeFromTop(40f);
			GUI.Label(logoRect, EditorIcons.OdinInspectorLogo, SirenixGUIStyles.LabelCentered);
			rect.TakeFromTop(10f);
			Rect messageRect = rect.TakeFromTop(50f).HorizontalPadding(40f);
			Color prevMultiLineCenteredLabelColor = SirenixGUIStyles.MultiLineCenteredLabel.normal.textColor;
			SirenixGUIStyles.MultiLineCenteredLabel.normal.textColor = Color.white;
			GUI.Label(messageRect, "In order to use Odin Personal, you must read and accept the Odin Personal EULA! Most notably, the EULA restricts the use of the Odin Personal license by people or entities with revenue or funding in excess of $200,000 USD in the past 12 months.", SirenixGUIStyles.MultiLineCenteredLabel);
			SirenixGUIStyles.MultiLineCenteredLabel.normal.textColor = prevMultiLineCenteredLabelColor;
			rect.TakeFromTop(10f);
			Rect eulaButtonRect = rect.TakeFromTop(31f).AlignCenterX(130f);
			if (GUI.Button(eulaButtonRect, "Read the full EULA"))
			{
				Application.OpenURL("https://odininspector.com/eula");
			}
			rect.TakeFromTop(40f);
			Rect step1Rect = rect.TakeFromTop(50f).HorizontalPadding(40f);
			if (DrawToggle(step1Rect, delegate(Rect r)
			{
				GUI.Label(r, "I have read and understood the EULA, and the restrictions that apply to the use of Odin Personal", SirenixGUIStyles.MultiLineLabel);
			}, hasReadAndUnderstood, ref t1))
			{
				hasReadAndUnderstood = !hasReadAndUnderstood;
			}
			rect.TakeFromTop(20f);
			Rect step2Rect = rect.TakeFromTop(50f).HorizontalPadding(40f);
			if (DrawToggle(step2Rect, delegate(Rect r)
			{
				GUI.Label(r, "I or the entity I work for had less than $200,000 USD revenue or funding in the past 12 months", SirenixGUIStyles.MultiLineLabel);
			}, isUnderRevenueCap, ref t2))
			{
				isUnderRevenueCap = !isUnderRevenueCap;
			}
			rect.TakeFromTop(20f);
			bool accepted = false;
			Rect step3Rect = rect.TakeFromTop(50f).HorizontalPadding(40f);
			DrawToggle(step3Rect, delegate(Rect r)
			{
				EditorGUI.BeginDisabledGroup(!hasReadAndUnderstood || !isUnderRevenueCap);
				if (GUI.Button(r.AlignLeft(130f), "Accept"))
				{
					EditorPrefs.SetBool(HAS_ACCEPTED_EULA_PREFS_KEY, value: true);
					if (closeWindowAfterAccept)
					{
						accepted = true;
						window.Close();
					}
				}
				EditorGUI.EndDisabledGroup();
			}, hasReadAndUnderstood && isUnderRevenueCap, ref t3);
			return accepted;
		}

		private static bool DrawToggle(Rect rect, Action<Rect> action, bool value, ref SirenixAnimationUtility.InterpolatedFloat t)
		{
			Rect ogRect = rect;
			t.ChangeDestination(value ? 1f : 0f);
			t.Move(4f);
			GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, Event.current.IsHovering(rect) ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.16f, 0.16f, 0.16f), 0f, 3f);
			GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false, 1f, SirenixGUIStyles.BorderColor, 1.25f, 3f);
			rect = rect.Padding(10f);
			Rect iconRect = rect.TakeFromLeft(16f);
			SdfIcons.DrawIcon(iconRect, SdfIconType.CheckCircleFill, new Color(0.22f, 0.71f, 0.29f, t));
			SdfIcons.DrawIcon(iconRect, SdfIconType.XCircleFill, new Color(0.95f, 0.38f, 0.24f, 1f - (float)t));
			action?.Invoke(rect.HorizontalPadding(10f, 0f));
			return Event.current.OnMouseDown(ogRect, 0);
		}
	}
}

using System.Collections.Generic;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.GettingStarted
{
	public class WizardPage : GettingStartedPage
	{
		private static GUIStyle multiLineCenteredTopText;

		public List<WizardPageStep> Steps = new List<WizardPageStep>();

		public int CurrentWizardStage;

		public int NextWizardStage;

		private int nextNextWizardStage;

		public float NextStageTransitionProgress;

		public override void EnterPage()
		{
			CurrentWizardStage = 0;
			NextStageTransitionProgress = 0f;
			NextWizardStage = 0;
			base.EnterPage();
		}

		public WizardPage(string title)
		{
			Title = title;
			TitleIcon = SdfIconType.Magic;
			FooterSize = 80f;
		}

		public override void DrawPage(Rect rect)
		{
			if (Event.current.type == EventType.Layout)
			{
				nextNextWizardStage = NextWizardStage;
				nextNextWizardStage = Mathf.Clamp(nextNextWizardStage, 0, Steps.Count - 1);
			}
			if (nextNextWizardStage == CurrentWizardStage)
			{
				NextStageTransitionProgress = 0f;
				Steps[CurrentWizardStage].OnGUI(rect);
				return;
			}
			if (Event.current.type == EventType.Layout)
			{
				float targetT = Mathf.MoveTowards(NextStageTransitionProgress, 1f, GUITimeHelper.LayoutDeltaTime * 2f);
				if (targetT != NextStageTransitionProgress)
				{
					NextStageTransitionProgress = targetT;
					GUIHelper.RequestRepaint();
				}
				else
				{
					CurrentWizardStage = nextNextWizardStage;
					NextStageTransitionProgress = 1f;
				}
			}
			Rect r1 = rect;
			Rect r2 = rect;
			Rect left = rect;
			Rect right = rect;
			bool direction = nextNextWizardStage > CurrentWizardStage;
			float t = (direction ? NextStageTransitionProgress : (1f - NextStageTransitionProgress));
			t = t * t * (3f - 2f * t);
			t = t * t * (3f - 2f * t);
			left.x -= t * rect.width;
			right.x = left.xMax;
			int from = CurrentWizardStage;
			int to = nextNextWizardStage;
			if (!direction)
			{
				from = nextNextWizardStage;
				to = CurrentWizardStage;
			}
			Color prevCol = GUI.color;
			GUI.color = prevCol * new Color(1f, 1f, 1f, 1f - t);
			Steps[from].OnGUI(left);
			GUI.color = prevCol * new Color(1f, 1f, 1f, t);
			Steps[to].OnGUI(right);
			GUI.color = prevCol;
		}

		public override void DrawFooter(Rect rect)
		{
			int currentStep = CurrentWizardStage;
			GUIHelper.PushGUIEnabled(currentStep < Steps.Count - 1 && NextWizardStage < Steps.Count - 1);
			if (GettingStartedWindow.Button(ref rect, "Next", SdfIconType.ChevronRight, Direction.Right, Direction.Right))
			{
				GoToNextStage();
			}
			GUIHelper.PopGUIEnabled();
			Color green = SirenixGUIStyles.ValidatorGreen;
			Color black = SirenixGUIStyles.DarkEditorBackground;
			Color grey = new Color(0.4352942f, 0.4352942f, 0.4352942f, 1f);
			grey.a = GUI.color.a;
			black.a = GUI.color.a;
			green.a = GUI.color.a;
			float t = ((nextNextWizardStage <= CurrentWizardStage) ? 0f : NextStageTransitionProgress);
			float stepSize = rect.width / (float)Steps.Count;
			Rect bgLine = rect.AlignCenterY(5f).HorizontalPadding(stepSize * 0.5f);
			EditorGUI.DrawRect(bgLine, grey);
			bgLine.width = stepSize * (float)currentStep + t * stepSize;
			EditorGUI.DrawRect(bgLine, green);
			for (int i = 0; i < Steps.Count; i++)
			{
				Rect sectionRect = rect.Split(i, Steps.Count).AlignCenterY(20f);
				Rect circleRect = sectionRect.AlignCenterX(sectionRect.height);
				Rect innerCircleRect = circleRect.Padding(6f);
				if (i == currentStep)
				{
					GUI.DrawTexture(circleRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, alphaBlend: true, 1f, green, 0f, 100f);
					Color white = Color.white;
					Rect checkRect = innerCircleRect;
					float s1 = innerCircleRect.width * (1f - t);
					float s2 = innerCircleRect.width * t;
					Color b = black;
					b.a *= 1f - t;
					checkRect = checkRect.AlignCenter(s2, s2);
					GUI.DrawTexture(innerCircleRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, alphaBlend: true, 1f, b, 0f, 100f);
					SdfIcons.DrawIcon(checkRect, SdfIconType.Check, black);
				}
				else if (i < currentStep)
				{
					GUI.DrawTexture(circleRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, alphaBlend: true, 1f, green, 0f, 100f);
					SdfIcons.DrawIcon(innerCircleRect, SdfIconType.Check, black);
				}
				else if (i - 1 == currentStep)
				{
					Color b2 = black;
					b2.a *= t;
					Color g1 = grey;
					Color g2 = green;
					g1.a *= 1f - t;
					g2.a *= t;
					GUI.DrawTexture(circleRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, alphaBlend: true, 1f, g1, 0f, 100f);
					GUI.DrawTexture(circleRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, alphaBlend: true, 1f, g2, 0f, 100f);
					GUI.DrawTexture(innerCircleRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, alphaBlend: true, 1f, b2, 0f, 100f);
				}
				else
				{
					GUI.DrawTexture(circleRect, Texture2D.whiteTexture, ScaleMode.ScaleAndCrop, alphaBlend: true, 1f, grey, 0f, 100f);
				}
				UnityShims.Rect.Ctor(out var titleRect, new Vector2(circleRect.center.x - stepSize * 0.5f + 5f, circleRect.yMax + 5f), new Vector2(stepSize - 10f, 60f));
				multiLineCenteredTopText = multiLineCenteredTopText ?? new GUIStyle(SirenixGUIStyles.MultiLineCenteredLabel);
				multiLineCenteredTopText.alignment = TextAnchor.UpperCenter;
				GUI.Label(titleRect, Steps[i].Name, multiLineCenteredTopText);
			}
		}

		public virtual bool GoToNextStage()
		{
			if (NextWizardStage < Steps.Count)
			{
				NextWizardStage++;
				return true;
			}
			return false;
		}

		public override void GoBack()
		{
			if (CurrentWizardStage <= 0)
			{
				base.GoBack();
			}
			else
			{
				NextWizardStage--;
			}
		}
	}
}

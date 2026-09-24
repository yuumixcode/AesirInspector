using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.GettingStarted;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	internal class SetupWizard : WizardPage
	{
		private struct BackgroundValidationSetup : IEquatable<BackgroundValidationSetup>
		{
			public bool KeepMainValidationSessionAliveInBackground;

			public bool ValidateScenesOnSceneLoad;

			public bool PopulateQueueOnGameObjectDeleted;

			public bool QueueAssetsOnLoad;

			public bool PopulateQueueOnAssetDeleted;

			public static BackgroundValidationSetup AssetsAndScenes;

			public static BackgroundValidationSetup OnlyScenes;

			public static BackgroundValidationSetup OnlyWhatYoureWorkingOn;

			public static BackgroundValidationSetup Nothing;

			static BackgroundValidationSetup()
			{
				AssetsAndScenes.KeepMainValidationSessionAliveInBackground = true;
				AssetsAndScenes.ValidateScenesOnSceneLoad = true;
				AssetsAndScenes.PopulateQueueOnGameObjectDeleted = true;
				AssetsAndScenes.QueueAssetsOnLoad = true;
				AssetsAndScenes.PopulateQueueOnAssetDeleted = true;
				OnlyScenes.KeepMainValidationSessionAliveInBackground = true;
				OnlyScenes.ValidateScenesOnSceneLoad = true;
				OnlyScenes.PopulateQueueOnGameObjectDeleted = true;
				OnlyWhatYoureWorkingOn.KeepMainValidationSessionAliveInBackground = true;
				Nothing.KeepMainValidationSessionAliveInBackground = false;
			}

			public override bool Equals(object obj)
			{
				if (obj is BackgroundValidationSetup setup)
				{
					return Equals(setup);
				}
				return false;
			}

			public bool Equals(BackgroundValidationSetup other)
			{
				if (KeepMainValidationSessionAliveInBackground == other.KeepMainValidationSessionAliveInBackground && ValidateScenesOnSceneLoad == other.ValidateScenesOnSceneLoad && PopulateQueueOnGameObjectDeleted == other.PopulateQueueOnGameObjectDeleted && QueueAssetsOnLoad == other.QueueAssetsOnLoad)
				{
					return PopulateQueueOnAssetDeleted == other.PopulateQueueOnAssetDeleted;
				}
				return false;
			}

			public override int GetHashCode()
			{
				int hashCode = -2127315670;
				hashCode = hashCode * -1521134295 + KeepMainValidationSessionAliveInBackground.GetHashCode();
				hashCode = hashCode * -1521134295 + ValidateScenesOnSceneLoad.GetHashCode();
				hashCode = hashCode * -1521134295 + PopulateQueueOnGameObjectDeleted.GetHashCode();
				hashCode = hashCode * -1521134295 + QueueAssetsOnLoad.GetHashCode();
				return hashCode * -1521134295 + PopulateQueueOnAssetDeleted.GetHashCode();
			}

			public static bool operator ==(BackgroundValidationSetup left, BackgroundValidationSetup right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(BackgroundValidationSetup left, BackgroundValidationSetup right)
			{
				return !(left == right);
			}

			internal void Apply()
			{
				GlobalConfig<GlobalValidationConfig>.Instance.KeepMainValidationSessionAliveInBackground.Value = KeepMainValidationSessionAliveInBackground;
				GlobalConfig<GlobalValidationConfig>.Instance.ValidateScenesOnSceneLoad.Value = ValidateScenesOnSceneLoad;
				GlobalConfig<GlobalValidationConfig>.Instance.PopulateQueueOnGameObjectDeleted.Value = PopulateQueueOnGameObjectDeleted;
				GlobalConfig<GlobalValidationConfig>.Instance.QueueAssetsOnLoad.Value = QueueAssetsOnLoad;
				GlobalConfig<GlobalValidationConfig>.Instance.PopulateQueueOnAssetDeleted.Value = PopulateQueueOnAssetDeleted;
				EditorUtility.SetDirty(GlobalConfig<GlobalValidationConfig>.Instance);
			}
		}

		[Serializable]
		private class MigrationDrawerWrapper
		{
			[HideInInspector]
			public Action OnGUI;

			public MigrationDrawerWrapper(Action onGUI)
			{
				OnGUI = onGUI;
			}

			[OnInspectorGUI]
			private void OnInspectorGUI()
			{
				OnGUI();
			}
		}

		private const float sectionPadding = 20f;

		private bool forceCustom;

		private static GUIStyle radio;

		private static ValidationSessionEditor.ValidationProfileEditor drawer;

		private static EditorPrefEnum<ConfigSourceType> selectedRuleSrc;

		private static RuleDataWrapper rules;

		[SerializeField]
		private ValidationProfile currentSelectedProfile;

		private int selectedRuleIndex;

		private PropertyTree ruleDataDrawer;

		private static bool hasFinished;

		public SetupWizard()
			: base("Odin Validator Setup Wizard")
		{
			Steps = new List<WizardPageStep>();
			Steps.Add(new WizardPageStep("Welcome", DrawWelcome));
			Steps.Add(new WizardPageStep("Profiles", DrawProfileSetup));
			Steps.Add(new WizardPageStep("Rules", DrawRuleSetup));
			Steps.Add(new WizardPageStep("Events", DrawEventSetup));
			if (OldValidator.IsInstalled)
			{
				Steps.Add(new WizardPageStep("Cleanup", DrawOldValidatorCleanup));
			}
			Steps.Add(new WizardPageStep("Config", DrawBackgroundValidation));
			Steps.Add(new WizardPageStep("Finish", DrawFinish));
		}

		private void DrawBackgroundValidation(Rect rect)
		{
			rect = rect.Padding(20f);
			rect = rect.AlignCenter(450f);
			GUIStyle boldLabel = SirenixGUIStyles.BoldLabelCentered;
			GUIStyle label = SirenixGUIStyles.LabelCentered;
			RefText(ref rect, "Background validation", boldLabel, 1f);
			rect.TakeFromTop(2f);
			RefText(ref rect, "Would you like the validator to run in the background for\n you and your team?", label, 1f);
			BackgroundValidationSetup curr = new BackgroundValidationSetup
			{
				KeepMainValidationSessionAliveInBackground = GlobalConfig<GlobalValidationConfig>.Instance.KeepMainValidationSessionAliveInBackground.Value,
				ValidateScenesOnSceneLoad = GlobalConfig<GlobalValidationConfig>.Instance.ValidateScenesOnSceneLoad.Value,
				PopulateQueueOnGameObjectDeleted = GlobalConfig<GlobalValidationConfig>.Instance.PopulateQueueOnGameObjectDeleted.Value,
				QueueAssetsOnLoad = GlobalConfig<GlobalValidationConfig>.Instance.QueueAssetsOnLoad.Value,
				PopulateQueueOnAssetDeleted = GlobalConfig<GlobalValidationConfig>.Instance.PopulateQueueOnAssetDeleted.Value
			};
			bool assetsAndScenes = curr == BackgroundValidationSetup.AssetsAndScenes;
			bool onlyScenes = curr == BackgroundValidationSetup.OnlyScenes;
			bool onlyWhatYoureWorkingOn = curr == BackgroundValidationSetup.OnlyWhatYoureWorkingOn;
			bool nothing = curr == BackgroundValidationSetup.Nothing;
			bool custom = !(nothing || assetsAndScenes || onlyScenes || onlyWhatYoureWorkingOn);
			rect.TakeFromTop(20f);
			GUIHelper.PushGUIEnabled(curr.KeepMainValidationSessionAliveInBackground);
			if (RadioButton(rect.TakeFromTop(20f), "Yes, always validate open scenes and assets in the background", assetsAndScenes))
			{
				BackgroundValidationSetup.AssetsAndScenes.Apply();
				forceCustom = false;
			}
			if (RadioButton(rect.TakeFromTop(20f), "Yes, but only validate open scenes to prevent potential asset loading stutter", onlyScenes))
			{
				BackgroundValidationSetup.OnlyScenes.Apply();
				forceCustom = false;
			}
			if (RadioButton(rect.TakeFromTop(20f), "Yes, but only monitor changes on the things I'm working on", onlyWhatYoureWorkingOn))
			{
				BackgroundValidationSetup.OnlyWhatYoureWorkingOn.Apply();
				forceCustom = false;
			}
			if (RadioButton(rect.TakeFromTop(20f), "No, I will open the Validator window when I want to run validation", nothing))
			{
				BackgroundValidationSetup.Nothing.Apply();
				forceCustom = false;
			}
			GUIHelper.PopGUIEnabled();
			if (nothing || !GlobalConfig<GlobalValidationConfig>.Instance.ShowWidget.IsDefault)
			{
				bool isHidden = !GlobalConfig<GlobalValidationConfig>.Instance.ShowWidget.Value;
				bool newIsHidden = EditorGUI.ToggleLeft(rect.TakeFromTop(20f).AddXMin(20f), "Also hide the Scene Widget", isHidden);
				if (isHidden != newIsHidden)
				{
					GlobalConfig<GlobalValidationConfig>.Instance.ShowWidget.Value = !GlobalConfig<GlobalValidationConfig>.Instance.ShowWidget.Value;
					SceneView.RepaintAll();
				}
			}
			if (RadioButton(rect.TakeFromTop(20f), "Custom", custom || forceCustom))
			{
				forceCustom = true;
			}
			rect.TakeFromTop(20f);
			RefText(ref rect, "Advanced", boldLabel, 1f);
			GUIHelper.PushGUIEnabled(custom || forceCustom);
			BeginScrollableLayoutPage(rect, 0);
			Rect rr = EditorGUILayout.BeginVertical();
			ValidationSessionEditor.DrawBackgroundValidationConfig(localOverride: false, drawProfileSelector: false, allowBold: false);
			EditorGUILayout.EndVertical();
			EndScrollableLayoutPage();
			GUIHelper.PopGUIEnabled();
		}

		private static bool RadioButton(Rect rect, string label, bool on)
		{
			if (Event.current.type == EventType.Repaint)
			{
				bool isHover = rect.Contains(Event.current.mousePosition);
				bool isActive = false;
				if (radio == null)
				{
					radio = new GUIStyle(EditorStyles.radioButton);
					radio.richText = true;
				}
				radio.Draw(rect, "  " + label, isHover, isActive, on, hasKeyboardFocus: false);
			}
			if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
			{
				GUIHelper.RemoveFocusControl();
				return true;
			}
			return false;
		}

		private void DrawWelcome(Rect rect)
		{
			rect.TakeFromTop(20f);
			GUI.Label(rect.TakeFromTop(20f), "Welcome!", SirenixGUIStyles.LabelCentered);
			rect.TakeFromTop(20f);
			GUI.Label(rect.TakeFromTop(20f), "This setup wizard will help you get started with Odin Validator.", SirenixGUIStyles.LabelCentered);
			DrawNextButton(ref rect, "Start");
		}

		private void DrawOldValidatorCleanup(Rect rect)
		{
			rect.TakeFromTop(20f);
			if (OldValidator.IsInstalled)
			{
				RefText(ref rect, "The old validator is installed", SirenixGUIStyles.BoldLabelCentered);
				rect.TakeFromTop(2f);
				RefText(ref rect, "The new validator is a completely separate install from the old one. Would you like to delete the old validator now? Note that any unmigrated configuration from the old validator will be permanently lost, and we recommend you take a backup or use source control before doing anything. The following actions will be taken:", SirenixGUIStyles.MultiLineLabel);
				rect.TakeFromTop(10f);
				string validatorPath = SirenixAssetPaths.SirenixPluginPath + "Odin Validator/";
				float oldRectXMin = rect.xMin;
				rect = rect.AddXMin(rect.width * 0.2f);
				RefText(ref rect, "1) Delete folder '" + validatorPath + "Editor/Scripts'", SirenixGUIStyles.MultiLineLabel, 1f);
				RefText(ref rect, "2) Delete asset '" + validatorPath + "Editor/Config/OdinValidationConfig.asset'", SirenixGUIStyles.MultiLineLabel, 1f);
				RefText(ref rect, "3) Delete all remaining assets of type 'ValidationProfileAsset'", SirenixGUIStyles.MultiLineLabel, 1f);
				rect.xMin = oldRectXMin;
				if (RefButton(ref rect, "Yes, delete the old validator"))
				{
					OldValidator.DeleteOldValidator();
				}
			}
			else
			{
				RefText(ref rect, "The old validator has been deleted.", SirenixGUIStyles.BoldLabelCentered);
			}
		}

		private void SplitRectWithPad(Rect rect, out Rect left, out Rect right, float t = 0.5f)
		{
			rect = rect.Padding(20f);
			left = rect.TakeFromLeft(rect.width * t);
			right = rect;
			left.x -= 10f;
			right.xMin += 10f;
		}

		private void DrawProfileSetup(Rect rect)
		{
			SplitRectWithPad(rect, out var left, out var right);
			RefText(ref left, "Validation Profiles", SirenixGUIStyles.BoldLabelCentered, 1f);
			left.TakeFromTop(2f);
			RefText(ref left, "Validation profiles specify which parts of your project should be validated. Here you can create and configure the profiles that you will have available. If you're not sure what you'll need yet, then don't worry; you can also create and change these later from within the Validator window.", SirenixGUIStyles.MultiLineLabel, 0.9f);
			if (OldValidator.IsInstalled)
			{
				left.TakeFromTop(20f);
				RefText(ref left, "The old validator is currently installed", SirenixGUIStyles.BoldLabelCentered, 1f);
				RefText(ref left, "Would you like to migrate any profiles from the old validator to the new? Note that if you left the default profiles unchanged on the old validator, you will likely also be happy with the default settings in the new validator.", SirenixGUIStyles.MultiLineLabel, 0.9f);
				left.TakeFromTop(20f);
				Rect buttonRect = left.TakeFromTop(30f);
				string btnText = "Migrate old profiles";
				float btnWidth = GettingStartedWindow.CalcButtonWidth(buttonRect, btnText);
				buttonRect = buttonRect.AlignCenterX(btnWidth);
				if (GettingStartedWindow.Button(ref buttonRect, btnText, SdfIconType.ArrowUpCircle, Direction.Left, Direction.Left))
				{
					OdinEditorWindow wnd = OdinEditorWindow.InspectObject(new MigrationDrawerWrapper(MigrationWizard.DrawMigrateProfiles), forceSerializeInspectedObject: true);
					wnd.titleContent = new GUIContent("Migrate old profiles");
				}
			}
			if (currentSelectedProfile == null)
			{
				currentSelectedProfile = ValidationProfile.MainValidationProfile;
			}
			if (drawer == null)
			{
				drawer = new ValidationSessionEditor.ValidationProfileEditor
				{
					DataSources = new List<IValidationProfile>
					{
						currentSelectedProfile,
						MainLocalValidationProfile.Instance
					}
				};
			}
			drawer.DataSources[0] = currentSelectedProfile;
			Rect r = right;
			EditorGUI.DrawRect(r, SirenixGUIStyles.BoxBackgroundColor);
			SirenixEditorGUI.DrawBorders(r.Expand(1f), 1);
			ValidationProfileDropdown(r.TakeFromTop(21f).AddXMax(1f), currentSelectedProfile.name);
			EditorGUI.DrawRect(r.TakeFromTop(1f), SirenixGUIStyles.BorderColor);
			BeginScrollableLayoutPage(r, 0);
			drawer.Draw();
			EndScrollableLayoutPage();
		}

		private void DrawRuleSetup(Rect rect)
		{
			SplitRectWithPad(rect, out var left, out var right);
			RefText(ref left, "Rules", SirenixGUIStyles.BoldLabelCentered);
			left.TakeFromTop(2f);
			RefText(ref left, "Rules are configurable validators that can be enabled or disabled, both on a project-wide or per-machine basis. You can also configure the setup of individual rules by clicking the cog icon next to a rule. This way, you can for example often control the severity of any given rule, as well as the specific way it applies in your project.", SirenixGUIStyles.MultiLineLabel, 0.9f);
			left.TakeFromTop(10f);
			RefText(ref left, "Now, which rules would you like to enable for your project? Remember, you can always change your rule setup from the main Validator window at any time.", SirenixGUIStyles.MultiLineLabel, 0.9f);
			left.TakeFromTop(20f);
			Rect r = right;
			EditorGUI.DrawRect(r, SirenixGUIStyles.BoxBackgroundColor);
			SirenixEditorGUI.DrawBorders(r.Expand(1f), 1);
			BeginScrollableLayoutPage(r, 0);
			rules = rules ?? GlobalConfig<RuleConfig>.Instance.GetRuleDataWrapper();
			selectedRuleSrc = selectedRuleSrc ?? new EditorPrefEnum<ConfigSourceType>("Odin_Validator_Editor_WizzardselectedRuleSrc", ConfigSourceType.Project);
			ValidationSessionEditor.DrawRules(selectedRuleSrc, rules, ref selectedRuleIndex, ref ruleDataDrawer);
			EndScrollableLayoutPage();
		}

		private void DrawEventSetup(Rect rect)
		{
			SplitRectWithPad(rect, out var left, out var right);
			RefText(ref left, "Events", SirenixGUIStyles.BoldLabelCentered, 1f);
			left.TakeFromTop(2f);
			RefText(ref left, "The validator can be set to run automatically at different times, and respond to warnings or errors in specific ways. For example, you might want to validate the current scene when entering play mode, or validate the entire project when doing a build. Here you can set all that up.", SirenixGUIStyles.MultiLineLabel, 0.9f);
			left.TakeFromTop(20f);
			if (OldValidator.IsInstalled)
			{
				RefText(ref left, "The old validator is installed", SirenixGUIStyles.BoldLabelCentered, 1f);
				left.TakeFromTop(2f);
				RefText(ref left, "If you were using the hook system in the old validator, you can migrate your old hook setup to the new event system.", SirenixGUIStyles.MultiLineLabel, 0.9f);
				left.TakeFromTop(20f);
				Rect buttonRect = left.TakeFromTop(30f);
				string btnText = "Migrate old hooks";
				float btnWidth = GettingStartedWindow.CalcButtonWidth(buttonRect, btnText);
				buttonRect = buttonRect.AlignCenterX(btnWidth);
				if (GettingStartedWindow.Button(ref buttonRect, btnText, SdfIconType.ArrowUpCircle, Direction.Left, Direction.Left))
				{
					OdinEditorWindow wnd = OdinEditorWindow.InspectObject(new MigrationDrawerWrapper(MigrationWizard.DrawMigrateHooks), forceSerializeInspectedObject: true);
					wnd.titleContent = new GUIContent("Migrate old hooks");
				}
			}
			Rect r = right;
			EditorGUI.DrawRect(r, SirenixGUIStyles.BoxBackgroundColor);
			SirenixEditorGUI.DrawBorders(r.Expand(1f), 1);
			BeginScrollableLayoutPage(r, 0);
			rules = rules ?? GlobalConfig<RuleConfig>.Instance.GetRuleDataWrapper();
			selectedRuleSrc = selectedRuleSrc ?? new EditorPrefEnum<ConfigSourceType>("Odin_Validator_Editor_WizzardselectedRuleSrc", ConfigSourceType.Project);
			ValidationSessionEditor.DrawEvents();
			EndScrollableLayoutPage();
		}

		private void DrawFinish(Rect rect)
		{
			if (!hasFinished)
			{
				if (!GlobalConfig<GlobalValidationConfig>.Instance.HasShownValidationConfig)
				{
					GlobalConfig<GlobalValidationConfig>.Instance.HasShownValidationConfig = true;
					EditorUtility.SetDirty(GlobalConfig<GlobalValidationConfig>.Instance);
					string path = AssetDatabase.GetAssetPath(GlobalConfig<GlobalValidationConfig>.Instance);
					string guid = AssetDatabase.AssetPathToGUID(path);
					if (!string.IsNullOrWhiteSpace(guid))
					{
						foreach (ValidationSession item in ValidationSession.ActiveValidationSessions)
						{
							item.Enqueue(ValidationWorkItem.CreateForAssetGuid(guid, ProjectEventSource.Other), insert: true);
						}
					}
					AssetDatabase.SaveAssets();
				}
				hasFinished = true;
			}
			rect.TakeFromTop(20f);
			RefText(ref rect, "The setup wizard has completed. If you're new to Odin Validator, we recommend you get started by looking through our various available tutorials and resources.");
			if (RefButton(ref rect, "Get started using the validator"))
			{
				Window.Pages.Clear();
				GettingStartedWindowData.OdinValidatorGettingStartedPage.Window = Window;
				GettingStartedWindowData.OdinValidatorGettingStartedPage.EnterPage();
			}
		}

		private void RefText(ref Rect rect, string text, GUIStyle style = null, float widthMul = 0.6f)
		{
			style = style ?? SirenixGUIStyles.MultiLineCenteredLabel;
			GUIContent lbl = GUIHelper.TempContent(text);
			float width = rect.width * widthMul;
			float height = style.CalcHeight(lbl, width);
			GUI.Label(rect.TakeFromTop(height).AlignCenterX(width), lbl, style);
		}

		private bool RefButton(ref Rect rect, string btnText = "", float btnSize = 30f)
		{
			rect.TakeFromTop(20f);
			GUIStyle style = GUI.skin.button;
			GUIContent lbl = GUIHelper.TempContent(btnText);
			float width = style.CalcSize(lbl).x;
			Rect btnRect = rect.TakeFromTop(btnSize).AlignCenterX(width);
			if (GUI.Button(btnRect, lbl))
			{
				return true;
			}
			return false;
		}

		private void DrawNextButton(ref Rect rect, string text = "Next")
		{
			if (RefButton(ref rect, "    " + text + "    "))
			{
				GoToNextStage();
			}
		}

		private void ValidationProfileDropdown(Rect rect, string label)
		{
			SdfIconType type = (currentSelectedProfile ? currentSelectedProfile.icon : SdfIconType.None);
			if (!SirenixEditorGUI.SDFIconButton(rect, label, type, IconAlignment.LeftOfText, EditorStyles.toolbarDropDown))
			{
				return;
			}
			ValidationProfileSelector selector = new ValidationProfileSelector(currentSelectedProfile);
			OdinEditorWindow wnd = OdinEditorWindow.InspectObjectInDropDown(selector);
			wnd.WindowPadding = Vector4.zero;
			selector.Selector.SelectionConfirmed += delegate(IEnumerable<ValidationProfile> x)
			{
				currentSelectedProfile = x.FirstOrDefault();
				EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, (EditorApplication.CallbackFunction)delegate
				{
					wnd.Close();
				});
			};
		}
	}
}

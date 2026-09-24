using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sirenix.OdinValidator.Editor
{
	[OdinValidatorConfig]
	public class AutomationConfig : GlobalConfig<AutomationConfig>
	{
		[Serializable]
		[HideLabel]
		[InlineProperty]
		public struct ValidationSetup
		{
			public enum RunKind
			{
				GlobalValidation,
				Custom
			}

			[HideLabel]
			public RunKind Profile;

			[AssetSelector]
			public ValidationProfile ProfileAsset;
		}

		[Flags]
		public enum OnPlayModeActions
		{
			None = 0,
			OpenValidator = 1,
			StopPlayMode = 2,
			LogToConsole = 4
		}

		[Flags]
		public enum OnBuildActions
		{
			None = 0,
			OpenValidator = 1,
			StopBuild = 2,
			LogToConsole = 4
		}

		[Flags]
		public enum OnProjectStartupActions
		{
			None = 0,
			OpenValidator = 1,
			LogToConsole = 2
		}

		[ToggleGroup("OnPlayMode", "On Play Mode", CollapseOthersOnExpand = false)]
		public bool OnPlayMode;

		[ToggleGroup("OnPlayMode", 0f, null)]
		[LabelText("If Warnings")]
		public OnPlayModeActions OnPlayModeIfWarnings;

		[ToggleGroup("OnPlayMode", 0f, null)]
		[LabelText("If Errors")]
		public OnPlayModeActions OnPlayModeIfErrors = OnPlayModeActions.OpenValidator | OnPlayModeActions.StopPlayMode;

		[LabelText("Always Complete Validation")]
		[ToggleGroup("OnPlayMode", 0f, null)]
		public bool OnPlayModeAlwaysCompleteValidationFully;

		[LabelText("Flash Screen")]
		[ToggleGroup("OnPlayMode", 0f, null)]
		public bool OnPlayModeFlashScreen;

		[ToggleGroup("OnPlayMode", 0f, null)]
		public ValidationSetup OnPlayModeSetup;

		[ToggleGroup("OnBuild", "On Build", CollapseOthersOnExpand = false)]
		public bool OnBuild;

		[ToggleGroup("OnBuild", 0f, null)]
		[LabelText("If Warnings")]
		public OnBuildActions OnBuildIfWarnings;

		[ToggleGroup("OnBuild", 0f, null)]
		[LabelText("If Errors")]
		public OnBuildActions OnBuildIfErrors = OnBuildActions.OpenValidator | OnBuildActions.StopBuild;

		[ToggleGroup("OnBuild", 0f, null)]
		[LabelText("Always Complete Validation")]
		public bool OnBuildAlwaysCompleteValidationFully;

		[LabelText("Flash Screen")]
		[ToggleGroup("OnBuild", 0f, null)]
		public bool OnBuildFlashScreen;

		[ToggleGroup("OnBuild", 0f, null)]
		public ValidationSetup OnBuildSetup;

		[ToggleGroup("OnProjectStartup", "On Project Startup", CollapseOthersOnExpand = false)]
		public bool OnProjectStartup;

		[LabelText("If Warnings")]
		[ToggleGroup("OnProjectStartup", 0f, null)]
		public OnProjectStartupActions OnProjectStartupIfWarnings;

		[LabelText("If Errors")]
		[ToggleGroup("OnProjectStartup", 0f, null)]
		public OnProjectStartupActions OnProjectStartupIfErrors = OnProjectStartupActions.OpenValidator;

		[LabelText("Always Complete Validation")]
		[ToggleGroup("OnProjectStartup", 0f, null)]
		public bool OnProjectStartupAlwaysCompleteValidationFully;

		[LabelText("Flash Screen")]
		[ToggleGroup("OnProjectStartup", 0f, null)]
		public bool OnProjectStartupFlashScreen;

		[ToggleGroup("OnProjectStartup", 0f, null)]
		public ValidationSetup OnProjectStartupSetup;

		public static bool HasReloadedBeforeThisSession
		{
			get
			{
				return SessionState.GetBool("OdinValidator_HasReloadedBeforeThisSession", defaultValue: false);
			}
			set
			{
				SessionState.SetBool("OdinValidator_HasReloadedBeforeThisSession", value);
			}
		}

		private static bool IsHeadlessOrBatchMode
		{
			get
			{
				if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null)
				{
					return InternalEditorUtility.inBatchMode;
				}
				return true;
			}
		}

		[InitializeOnLoadMethod]
		private static void InitHooks()
		{
			if (IsHeadlessOrBatchMode)
			{
				if (!GlobalConfig<AutomationConfig>.HasInstanceLoaded)
				{
					GlobalConfig<AutomationConfig>.LoadInstanceIfAssetExists();
				}
				if (!GlobalConfig<AutomationConfig>.HasInstanceLoaded)
				{
					UnityEngine.Debug.LogError("Odin's validation config asset could not be loaded during InitializeOnLoad of batch mode or headless run of the Unity Editor. Delaying validation hook initialization to a fallback event that takes place during asset post processing.");
					AutomationConfigFallbackPostProcessor.InvokeOnNextPostProcessSomeAsset = delegate
					{
						if (!GlobalConfig<AutomationConfig>.HasInstanceLoaded)
						{
							GlobalConfig<AutomationConfig>.LoadInstanceIfAssetExists();
						}
						if (!GlobalConfig<AutomationConfig>.HasInstanceLoaded)
						{
							UnityEngine.Debug.LogError("Could not load validation config asset during the fallback hook event either. No validation hook has taken place, whether it was configured to or not.");
						}
						else
						{
							UnityEngine.Debug.Log("Now successfully executing fallback validation hook initialization.");
							ActuallyInitHooks();
						}
					};
				}
				else
				{
					ActuallyInitHooks();
				}
			}
			else
			{
				UnityEditorEventUtility.DelayAction(ActuallyInitHooks, excludeGuiEventHooks: true);
			}
		}

		private static void ActuallyInitHooks()
		{
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			AutomationConfig inst = GlobalConfig<AutomationConfig>.Instance;
			if (inst.OnProjectStartup && !HasReloadedBeforeThisSession)
			{
				RunValidationScan("On Project Startup", inst.OnProjectStartupSetup, allowOpenClosedScenes: true, null, (inst.OnProjectStartupIfErrors & OnProjectStartupActions.LogToConsole) != 0, (inst.OnProjectStartupIfWarnings & OnProjectStartupActions.LogToConsole) != 0, stopOnErrors: false, stopOnWarnings: false, (inst.OnProjectStartupIfErrors & OnProjectStartupActions.OpenValidator) != 0, (inst.OnProjectStartupIfWarnings & OnProjectStartupActions.OpenValidator) != 0, inst.OnProjectStartupAlwaysCompleteValidationFully, inst.OnProjectStartupFlashScreen);
			}
			HasReloadedBeforeThisSession = true;
		}

		private static void OnPlayModeStateChanged(PlayModeStateChange change)
		{
			AutomationConfig inst = GlobalConfig<AutomationConfig>.Instance;
			if (!inst.OnPlayMode || change != PlayModeStateChange.ExitingEditMode)
			{
				return;
			}
			Type t = TwoWaySerializationBinder.Default.BindToType("UnityEditor.TestTools.TestRunner.PlaymodeLauncher");
			if (t != null)
			{
				StackFrame[] frames = new StackTrace().GetFrames();
				foreach (StackFrame frame in frames)
				{
					if (frame.GetMethod()?.DeclaringType == t)
					{
						return;
					}
				}
			}
			RunValidationScan("On Play Mode", inst.OnPlayModeSetup, allowOpenClosedScenes: false, delegate
			{
				EditorApplication.isPlaying = false;
			}, (inst.OnPlayModeIfErrors & OnPlayModeActions.LogToConsole) != 0, (inst.OnPlayModeIfWarnings & OnPlayModeActions.LogToConsole) != 0, (inst.OnPlayModeIfErrors & OnPlayModeActions.StopPlayMode) != 0, (inst.OnPlayModeIfWarnings & OnPlayModeActions.StopPlayMode) != 0, (inst.OnPlayModeIfErrors & OnPlayModeActions.OpenValidator) != 0, (inst.OnPlayModeIfWarnings & OnPlayModeActions.OpenValidator) != 0, inst.OnPlayModeAlwaysCompleteValidationFully, inst.OnPlayModeFlashScreen);
		}

		public static void TriggerOnBuild()
		{
			AutomationConfig inst = GlobalConfig<AutomationConfig>.Instance;
			if (inst.OnBuild)
			{
				RunValidationScan("On Build", inst.OnBuildSetup, allowOpenClosedScenes: true, delegate
				{
					throw new BuildFailedException("'On Build' validation hook throwing exception to stop build process");
				}, (inst.OnBuildIfErrors & OnBuildActions.LogToConsole) != 0, (inst.OnBuildIfWarnings & OnBuildActions.LogToConsole) != 0, (inst.OnBuildIfErrors & OnBuildActions.StopBuild) != 0, (inst.OnBuildIfWarnings & OnBuildActions.StopBuild) != 0, (inst.OnBuildIfErrors & OnBuildActions.OpenValidator) != 0, (inst.OnBuildIfWarnings & OnBuildActions.OpenValidator) != 0, inst.OnBuildAlwaysCompleteValidationFully, inst.OnBuildFlashScreen);
			}
		}

		private static void RunValidationScan(string name, ValidationSetup setup, bool allowOpenClosedScenes, Action stopEvent, bool logErrors, bool logWarnings, bool stopOnErrors, bool stopOnWarnings, bool openValidatorIfErrors, bool openValidatorIfWarnings, bool completeValidationFully, bool flashScreen)
		{
			if (!logErrors && !logWarnings && !stopOnErrors && !stopOnWarnings && !openValidatorIfErrors && !openValidatorIfWarnings)
			{
				return;
			}
			UnityEngine.Debug.Log("Running " + name + " validation hook...");
			ValidationProfile profile = ((setup.Profile != ValidationSetup.RunKind.GlobalValidation) ? setup.ProfileAsset : ValidationProfile.MainValidationProfile);
			ValidationSessionAssetHandle handle = profile.ClaimSessionHandle();
			try
			{
				ValidationSession session = handle.Session;
				IEnumerable<PersistentValidationResultBatch> enumerator = session.ValidateEverythingEnumeratorBatched(allowOpenClosedScenes, !IsHeadlessOrBatchMode);
				bool hadErrors = false;
				bool hadWarnings = false;
				foreach (PersistentValidationResultBatch result in enumerator)
				{
					if (result.Count == 0)
					{
						continue;
					}
					ref PersistentResultItem actualResult = ref result.HighestSeverityResult;
					if (actualResult.ResultType == ValidationResultType.Error)
					{
						hadErrors = true;
						if (logErrors)
						{
							UnityEngine.Debug.LogError(result.ToNiceLogString());
						}
						if (!completeValidationFully && (logErrors || stopOnErrors || openValidatorIfErrors))
						{
							break;
						}
					}
					else if (actualResult.ResultType == ValidationResultType.Warning)
					{
						hadWarnings = true;
						if (logWarnings)
						{
							UnityEngine.Debug.LogWarning(result.ToNiceLogString());
						}
						if (!completeValidationFully && (logWarnings || stopOnWarnings || openValidatorIfWarnings))
						{
							break;
						}
					}
				}
				bool shouldStopEvent = false;
				bool openValidator = false;
				if (hadErrors)
				{
					shouldStopEvent = shouldStopEvent || stopOnErrors;
					openValidator = openValidator || openValidatorIfErrors;
				}
				if (hadWarnings)
				{
					shouldStopEvent = shouldStopEvent || stopOnWarnings;
					openValidator = openValidator || openValidatorIfWarnings;
				}
				if (openValidator && !IsHeadlessOrBatchMode)
				{
					OdinValidatorWindow.OpenWindow(profile);
				}
				if (shouldStopEvent)
				{
					UnityEngine.Debug.LogError("Stopping " + name + " because validation failed!");
					stopEvent();
					if (!IsHeadlessOrBatchMode && flashScreen)
					{
						FlashScreen();
					}
				}
			}
			finally
			{
				handle.Dispose();
			}
		}

		private static void FlashScreen()
		{
			Color color = new Color(1f, 0f, 0f, 0.5f);
			Action<SceneView> flashScreen = null;
			double timeStarted = EditorApplication.timeSinceStartup;
			double flashSpeed = 0.3;
			int flashCount = 3;
			EditorWindow[] windowsToRepaint = Resources.FindObjectsOfTypeAll<EditorWindow>();
			flashScreen = delegate(SceneView sceneView2)
			{
				double timeSinceStartup = EditorApplication.timeSinceStartup;
				if (timeSinceStartup - timeStarted >= flashSpeed * (double)flashCount)
				{
					UnityEditorEventUtility.DuringSceneGUI -= flashScreen;
				}
				else
				{
					double num = timeSinceStartup - timeStarted;
					double num2 = 1.0 - Math.Abs(num % flashSpeed - flashSpeed * 0.5) / (flashSpeed / 2.0);
					Color color2 = color;
					color2.a *= (float)num2;
					Rect position = sceneView2.position;
					position.position = Vector2.zero;
					position.height -= 18f;
					Handles.BeginGUI();
					SirenixEditorGUI.DrawSolidRect(position, color2);
					Handles.EndGUI();
				}
				sceneView2.Repaint();
			};
			UnityEditorEventUtility.DuringSceneGUI += flashScreen;
			foreach (object view in SceneView.sceneViews)
			{
				if (view is SceneView sceneView && sceneView != null)
				{
					sceneView.Repaint();
				}
			}
		}
	}
}

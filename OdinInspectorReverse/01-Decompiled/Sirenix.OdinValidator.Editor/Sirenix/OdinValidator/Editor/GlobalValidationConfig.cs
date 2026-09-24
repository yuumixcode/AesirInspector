using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Utilities;

namespace Sirenix.OdinValidator.Editor
{
	[OdinValidatorConfig]
	public class GlobalValidationConfig : ProjectSettingsGlobalConfig<GlobalValidationConfig>
	{
		[ProjectSettingKey("ValidateScenesOnSceneLoad", false)]
		public ProjectSettingBool ValidateScenesOnSceneLoad;

		[ProjectSettingKey("ValidateMainProfileOnLoad", false)]
		public ProjectSettingBool ValidateMainProfileOnLoad;

		[ProjectSettingKey("QueueAssetsOnLoad", false)]
		public ProjectSettingBool QueueAssetsOnLoad;

		[ProjectSettingKey("QueueScenesOnLoad", false)]
		public ProjectSettingBool QueueScenesOnLoad;

		[ProjectSettingKey("PopulateQueueOnAssetDeleted", false)]
		public ProjectSettingBool PopulateQueueOnAssetDeleted;

		[ProjectSettingKey("PopulateQueueOnGameObjectDeleted", false)]
		public ProjectSettingBool PopulateQueueOnGameObjectDeleted;

		[ProjectSettingKey("RunMainValidationSessionOnLoad", true)]
		public ProjectSettingBool RunMainValidationSessionOnLoad;

		[ProjectSettingKey("DeepValidation", false)]
		public ProjectSettingBool DeepValidation;

		[ProjectSettingKey("ContinuouslyValidateVisibleIssues", true)]
		public ProjectSettingBool ContinuouslyValidateVisibleIssues;

		[ProjectSettingKey("PauseValidationWhileWorkingInSceneView", true)]
		public ProjectSettingBool PauseValidationWhileWorkingInSceneView;

		[ProjectSettingKey("PingOnDoubleClick", true)]
		public ProjectSettingBool PingOnDoubleClick;

		[ProjectSettingKey("FocusObjectOnDoubleClick", true)]
		public ProjectSettingBool FocusObjectOnDoubleClick;

		[ProjectSettingKey("FrameSelection", true)]
		public ProjectSettingBool FrameSelection;

		[ProjectSettingKey("SelectNextIssueOnFix", true)]
		public ProjectSettingBool SelectNextIssueOnFix;

		[ProjectSettingKey("SkipFirstSample", true)]
		public ProjectSettingBool SkipFirstSample;

		[ProjectSettingKey("ShowWidget", true)]
		public ProjectSettingBool ShowWidget;

		[ProjectSettingKey("DebugMode", false)]
		public ProjectSettingBool DebugMode;

		[ProjectSettingKey("ShowWidgetOnlyWhenErrorOrWarnings", false)]
		public ProjectSettingBool ShowWidgetOnlyWhenErrorOrWarnings;

		[ProjectSettingKey("EnableLeakDetection", true)]
		public ProjectSettingBool EnableLeakDetection;

		[ProjectSettingKey("SupressAssetLoadErrorsFromUnityLogger", true)]
		public ProjectSettingBool SupressAssetLoadErrorsFromUnityLogger;

		[ProjectSettingKey("OpenComponentInInspectorAndCloseOthers", false)]
		public ProjectSettingBool OpenComponentInInspectorAndCloseOthers;

		[ProjectSettingKey("WatchForChanges", true)]
		public ProjectSettingBool WatchForChanges;

		[ProjectSettingKey("ValidateInBackground", true)]
		public ProjectSettingBool ValidateInBackground;

		[ProjectSettingKey("KeepMainValidationSessionAliveInBackground", true)]
		public ProjectSettingBool KeepMainValidationSessionAliveInBackground;

		public bool HasShownValidationConfig;

		public static readonly EditorPrefEnum<SceneValidationWidget.WidgetAnchor> WidgetAnchor = new EditorPrefEnum<SceneValidationWidget.WidgetAnchor>("Odin_Validator_WidgetAnchor", SceneValidationWidget.WidgetAnchor.BottomLeft);

		public static readonly EditorPrefFloat WidgetOffsetX = new EditorPrefFloat("Odin_Validator_WidgetOffsetX", 20f);

		public static readonly EditorPrefFloat WidgetOffsetY = new EditorPrefFloat("Odin_Validator_WidgetOffsetY", 20f);

		public static string DefaultConfigFolderPath => SirenixAssetPaths.SirenixPluginPath + "Odin Validator/Editor/Config/";
	}
}

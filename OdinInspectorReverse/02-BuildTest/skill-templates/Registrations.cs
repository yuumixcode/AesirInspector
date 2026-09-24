#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor.Validation;
[assembly: RegisterValidationRule(typeof(NotEmptyStringValidator), Description = "检查字符串字段不为空。")]
[assembly: RegisterValidationRule(typeof(GameSettingsValidator), Name = "Game Settings", EnabledByDefault = false)]
[assembly: RegisterValidator(typeof(MustBePowerOfTwoValidator))]
#endif

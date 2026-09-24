using System.IO;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor.Expressions;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	public static class ValidatorScriptTemplates
	{
		public static readonly string Rule_RootObjectValidatorFileTemplate = "#if UNITY_EDITOR\r\nusing Sirenix.OdinInspector.Editor.Validation;\r\nusing UnityEngine;\r\nusing UnityEditor;\r\n\r\n[assembly: RegisterValidationRule(typeof({name}), Name = \"{name}\", Description = \"Some description text.\")]\r\n\r\npublic class {name} : RootObjectValidator<{target}>\r\n{\r\n    // Introduce serialized fields here to make your validator\r\n    // configurable from the validator window under rules.\r\n    public int SerializedConfig;\r\n\r\n    protected override void Validate(ValidationResult result)\r\n    {\r\n        // var obj = this.Object;\r\n        // if (obj has something wrong with it)\r\n        // {\r\n        //     result.AddError(\"Something is wrong\");\r\n        // }\r\n    }\r\n}\r\n#endif\r\n";

		public static readonly string Standard_RootObjectValidatorFileTemplate = "#if UNITY_EDITOR\r\nusing Sirenix.OdinInspector.Editor.Validation;\r\nusing UnityEngine;\r\nusing UnityEditor;\r\n\r\n[assembly: RegisterValidator(typeof({name}))]\r\n\r\npublic class {name} : RootObjectValidator<{target}>\r\n{\r\n    protected override void Validate(ValidationResult result)\r\n    {\r\n        // var obj = this.Object;\r\n        // if (obj has something wrong with it)\r\n        // {\r\n        //     result.AddError(\"Something is wrong\");\r\n        // }\r\n    }\r\n}\r\n#endif\r\n";

		public static readonly string Rule_ValueValidatorFileTemplate_Class = "#if UNITY_EDITOR\r\nusing Sirenix.OdinInspector.Editor.Validation;\r\nusing UnityEngine;\r\nusing UnityEditor;\r\n\r\n[assembly: RegisterValidationRule(typeof({name}), Name = \"{name}\", Description = \"Some description text.\")]\r\n\r\npublic class {name} : ValueValidator<{target}>\r\n{\r\n    // Introduce serialized fields here to make your validator\r\n    // configurable from the validator window under rules.\r\n    public int SerializedConfig;\r\n\r\n    protected override void Validate(ValidationResult result)\r\n    {\r\n        //var val = this.Value;\r\n        \r\n        //if (val has something wrong with it)\r\n        //{\r\n        //    result.AddError(\"Something is wrong\");\r\n        //}\r\n    }\r\n}\r\n#endif\r\n";

		public static readonly string Standard_ValueValidatorFileTemplate_Class = "#if UNITY_EDITOR\r\nusing Sirenix.OdinInspector.Editor.Validation;\r\nusing UnityEngine;\r\nusing UnityEditor;\r\n\r\n[assembly: RegisterValidator(typeof({name}<>))]\r\n\r\npublic class {name}<T> : ValueValidator<T>\r\n    where T : {target}\r\n{\r\n    protected override void Validate(ValidationResult result)\r\n    {\r\n        //var val = this.Value;\r\n        \r\n        //if (val has something wrong with it)\r\n        //{\r\n        //    result.AddError(\"Something is wrong\");\r\n        //}\r\n    }\r\n}\r\n#endif\r\n";

		public static readonly string Rule_ValueValidatorFileTemplate_Struct = "#if UNITY_EDITOR\r\nusing Sirenix.OdinInspector.Editor.Validation;\r\nusing UnityEngine;\r\nusing UnityEditor;\r\n\r\n[assembly: RegisterValidationRule(typeof({name}), Name = \"{name}\", Description = \"Some description text.\")]\r\n\r\npublic class {name} : ValueValidator<{target}>\r\n{\r\n    // Introduce serialized fields here to make your validator\r\n    // configurable from the validator window under rules.\r\n    public int SerializedConfig;\r\n\r\n    protected override void Validate(ValidationResult result)\r\n    {\r\n        //var val = this.Value;\r\n        \r\n        //if (val has something wrong with it)\r\n        //{\r\n        //    result.AddError(\"Something is wrong\");\r\n        //}\r\n    }\r\n}\r\n#endif\r\n";

		public static readonly string Standard_SceneValidatorFileTemplate = "#if UNITY_EDITOR\r\nusing Sirenix.OdinInspector.Editor.Validation;\r\nusing UnityEngine;\r\nusing UnityEditor;\r\n\r\n[assembly: RegisterValidator(typeof({name}))]\r\n\r\npublic class {name} : SceneValidator\r\n{\r\n    protected override void Validate(ValidationResult result)\r\n    {\r\n        // Scene Validators have many useful methods for querying the scene:\r\n\r\n        // var theScene = this.ValidatedScene;\r\n        // var camera = this.FindComponentInSceneOfType<Camera>();\r\n        // var cameras = this.FindAllComponentsInSceneOfType<Camera>();\r\n        // var allGameObjects = this.GetAllGameObjectsInScene();\r\n        // var rootGameObjects = this.GetSceneRoots();\r\n        // var gameObjectAtPath = this.GetGameObjectAtPath(\"Managers/Spawner\");\r\n        // \r\n        // if (scene has something wrong)\r\n        // {\r\n        //     result.AddError(\"Something is wrong\")\r\n        //         .SetSelectionObject(objectInScene); // Optional\r\n        // }\r\n    }\r\n}\r\n#endif";

		public static readonly string Rule_SceneValidatorFileTemplate = "#if UNITY_EDITOR\r\nusing Sirenix.OdinInspector.Editor.Validation;\r\nusing UnityEngine;\r\nusing UnityEditor;\r\n\r\n[assembly: RegisterValidationRule(typeof({name}))]\r\n\r\npublic class {name} : SceneValidator\r\n{\r\n    // Introduce serialized fields here to make your validator\r\n    // configurable from the validator window under rules.\r\n    public string SpawnerManagerPath = \"Managers/Spawner\";\r\n\r\n    protected override void Validate(ValidationResult result)\r\n    {\r\n        // Scene Validators have many useful methods for querying the scene:\r\n\r\n        // var theScene = this.ValidatedScene;\r\n        // var camera = this.FindComponentInSceneOfType<Camera>();\r\n        // var cameras = this.FindAllComponentsInSceneOfType<Camera>();\r\n        // var allGameObjects = this.GetAllGameObjectsInScene();\r\n        // var rootGameObjects = this.GetSceneRoots();\r\n        // var gameObjectAtPath = this.GetGameObjectAtPath(SpawnerManagerPath);\r\n        // \r\n        // if (scene has something wrong)\r\n        // {\r\n        //     result.AddError(\"Something is wrong\")\r\n        //         .SetSelectionObject(objectInScene); // Optional\r\n        // }\r\n    }\r\n}\r\n#endif";

		public static readonly string Standard_ValueValidatorFileTemplate_Struct = "\r\n#if UNITY_EDITOR\r\nusing Sirenix.OdinInspector.Editor.Validation;\r\nusing UnityEngine;\r\nusing UnityEditor;\r\n\r\n[assembly: RegisterValidator(typeof({name}))]\r\n\r\npublic class {name} : ValueValidator<{target}>\r\n{\r\n    protected override void Validate(ValidationResult result)\r\n    {\r\n        //var val = this.Value;\r\n        \r\n        //if (val has something wrong with it)\r\n        //{\r\n        //    result.AddError(\"Something is wrong\");\r\n        //}\r\n    }\r\n}\r\n#endif\r\n";

		public static readonly string Rule_AttributeValidatorFileTemplate = "#if UNITY_EDITOR\r\nusing Sirenix.OdinInspector.Editor.Validation;\r\nusing UnityEngine;\r\nusing UnityEditor;\r\n\r\n[assembly: RegisterValidationRule(typeof({name}), Name = \"{name}\", Description = \"Some description text.\")]\r\n\r\npublic class {name} : AttributeValidator<{target}, TargetValueType>\r\n{\r\n    // Introduce serialized fields here to make your validator\r\n    // configurable from the validator window under rules.\r\n    public int SerializedConfig;\r\n\r\n    protected override void Validate(ValidationResult result)\r\n    {\r\n        //var attr = this.Attribute;\r\n        //var val = this.Value;\r\n\r\n        //if (val has something wrong with it)\r\n        //{\r\n        //    result.AddError(\"Something is wrong\");\r\n        //}\r\n    }\r\n}\r\n\r\n// Alternative version that does not target a specific type of value (above must be deleted or commented out for the below to work)\r\n\r\n//[assembly: RegisterValidator(typeof({name}))]\r\n\r\n//public class {name} : AttributeValidator<{target}>\r\n//{\r\n//    // Introduce serialized fields here to make your validator\r\n//    // configurable from the validator window under rules.\r\n//    public int SerializedConfig;\r\n//\r\n//    protected override void Validate(ValidationResult result)\r\n//    {\r\n//        //var attr = this.Attribute;\r\n//\r\n//        //if (if something is wrong)\r\n//        //{\r\n//        //    result.AddError(\"Something is wrong\");\r\n//        //}\r\n//    }\r\n//}\r\n#endif\r\n";

		public static readonly string Standard_GlobalValidatorFileTemplate = "#if UNITY_EDITOR\r\nusing Sirenix.OdinInspector.Editor.Validation;\r\nusing UnityEngine;\r\nusing UnityEditor;\r\nusing Sirenix.OdinValidator.Editor;\r\nusing System.Collections;\r\nusing Sirenix.OdinInspector;\r\nusing Sirenix.OdinInspector.Editor;\r\nusing System.Linq;\r\n\r\n[assembly: RegisterValidationRule(typeof(MyGlobalValidator), Name = \"My Global Validator\", Description = \"Some description text.\")]\r\n\r\npublic class MyGlobalValidator : GlobalValidator\r\n{\r\n    public BuildTarget[] ValidBuildTargets = new BuildTarget[] { BuildTarget.Android, BuildTarget.iOS };\r\n\r\n    public override IEnumerable RunValidation(ValidationResult result)\r\n    {\r\n        // Example:\r\n\r\n        if (ValidBuildTargets.Contains(EditorUserBuildSettings.activeBuildTarget) == false)\r\n        {\r\n            ref var error = ref result.AddError($\"Your current build target ({EditorUserBuildSettings.activeBuildTarget}) is not supported by this project.\");\r\n            foreach (var item in ValidBuildTargets)\r\n            {\r\n                var buildTarget = item;\r\n                error.WithButton($\"Switch to {buildTarget}\", () => { EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(buildTarget), buildTarget); });\r\n            }\r\n        }\r\n\r\n        // Similar to coroutins, you can yield return null for long running validators similar to coroutins.\r\n        // Each yield will return the control flow back to the editor,\r\n        // and validation will continue next frame.\r\n        return null;\r\n    }\r\n}\r\n#endif\r\n";

		public static readonly string Standard_AttributeValidatorFileTemplate = "#if UNITY_EDITOR\r\nusing Sirenix.OdinInspector.Editor.Validation;\r\nusing UnityEngine;\r\nusing UnityEditor;\r\n\r\n[assembly: RegisterValidator(typeof({name}<>))]\r\n\r\npublic class {name}<T> : AttributeValidator<{target}, T>\r\n    where T : TargetValueType\r\n{\r\n    protected override void Validate(ValidationResult result)\r\n    {\r\n        //var attr = this.Attribute;\r\n        //var val = this.Value;\r\n\r\n        //if (val has something wrong with it)\r\n        //{\r\n        //    result.AddError(\"Something is wrong\");\r\n        //}\r\n    }\r\n}\r\n\r\n// Alternative version that does not target a specific type of value (above must be deleted or commented out for the below to work)\r\n\r\n//[assembly: RegisterValidator(typeof({name}))]\r\n\r\n//public class {name} : AttributeValidator<{target}>\r\n//{\r\n//    protected override void Validate(ValidationResult result)\r\n//    {\r\n//        //var attr = this.Attribute;\r\n//\r\n//        //if (if something is wrong)\r\n//        //{\r\n//        //    result.AddError(\"Something is wrong\");\r\n//        //}\r\n//    }\r\n//}\r\n#endif\r\n";

		private static EditorPrefString lastTemplateFolderPath;

		public static EditorPrefString LastTemplateFolderPath
		{
			get
			{
				if (lastTemplateFolderPath == null)
				{
					lastTemplateFolderPath = new EditorPrefString("SIRENIX_ODINVALIDATOR_LASTTEMPLATEPATH + " + PlayerSettings.productGUID, "Assets");
					if (!Directory.Exists(lastTemplateFolderPath.Value))
					{
						lastTemplateFolderPath.Value = "Assets";
					}
				}
				return lastTemplateFolderPath;
			}
		}

		public static void CreateTemplateScriptFile(string filePath, string template, string defaultTargetName)
		{
			string validatorName = Path.GetFileNameWithoutExtension(filePath);
			string targetName = ((!validatorName.FastEndsWith("Validator")) ? defaultTargetName : validatorName.Substring(0, validatorName.Length - "Validator".Length));
			File.WriteAllText(filePath, template.Replace("{name}", validatorName).Replace("{target}", targetName));
			AssetDatabase.Refresh();
			string fullFolderPath = new DirectoryInfo(Path.GetDirectoryName(filePath)).FullName;
			if (PathUtilities.TryMakeRelative(Directory.GetCurrentDirectory(), fullFolderPath, out var relativePath))
			{
				LastTemplateFolderPath.Value = relativePath;
			}
			else
			{
				LastTemplateFolderPath.Value = fullFolderPath;
			}
		}

		public static void AddRuleTemplateScriptCreationToGenericMenu(GenericMenu menu, string prependMenuPath = null)
		{
			if (prependMenuPath != null && !prependMenuPath.EndsWith("/"))
			{
				prependMenuPath += "/";
			}
			menu.AddItem(new GUIContent(prependMenuPath + "Root Object Validator"), on: false, delegate
			{
				CreateRuleRootObjectValidator(LastTemplateFolderPath);
			});
			menu.AddItem(new GUIContent(prependMenuPath + "Value Validator"), on: false, delegate
			{
				CreateRuleValueValidator(LastTemplateFolderPath);
			});
			menu.AddItem(new GUIContent(prependMenuPath + "Attribute Validator"), on: false, delegate
			{
				CreateRuleAttributeValidator(LastTemplateFolderPath);
			});
			menu.AddItem(new GUIContent(prependMenuPath + "Scene Validator"), on: false, delegate
			{
				CreateRuleSceneValidator(LastTemplateFolderPath);
			});
		}

		public static void AddValidatorTemplateScriptCreationToGenericMenu(GenericMenu menu, string prependMenuPath = null)
		{
			if (prependMenuPath != null && !prependMenuPath.EndsWith("/"))
			{
				prependMenuPath += "/";
			}
			menu.AddItem(new GUIContent(prependMenuPath + "Root Object Validator"), on: false, delegate
			{
				CreateStandardRootObjectValidator(LastTemplateFolderPath);
			});
			menu.AddItem(new GUIContent(prependMenuPath + "Value Validator"), on: false, delegate
			{
				CreateStandardValueValidator(LastTemplateFolderPath);
			});
			menu.AddItem(new GUIContent(prependMenuPath + "Attribute Validator"), on: false, delegate
			{
				CreateStandardAttributeValidator(LastTemplateFolderPath);
			});
			menu.AddItem(new GUIContent(prependMenuPath + "Scene Validator"), on: false, delegate
			{
				CreateStandardSceneValidator(LastTemplateFolderPath);
			});
			menu.AddItem(new GUIContent(prependMenuPath + "Global Validator"), on: false, delegate
			{
				CreateStandardGlobalValidator(LastTemplateFolderPath);
			});
		}

		public static void AddRuleAndValidatorTemplateScriptCreationToGenericMenu(GenericMenu menu, string prependMenuPath = null)
		{
			if (prependMenuPath != null && !prependMenuPath.EndsWith("/"))
			{
				prependMenuPath += "/";
			}
			AddRuleTemplateScriptCreationToGenericMenu(menu, prependMenuPath + "Create Rule");
			AddValidatorTemplateScriptCreationToGenericMenu(menu, prependMenuPath + "Create Validator");
		}

		public static void CreateStandardAttributeValidator(string saveFilePanelFolderPath)
		{
			string filePath = EditorUtility.SaveFilePanelInProject("Create Attribute Validator Script", "MyAttributeValidator.cs", "cs", "Choose a file path to create an attribute validator script file at.", saveFilePanelFolderPath);
			if (!string.IsNullOrEmpty(filePath))
			{
				CreateTemplateScriptFile(filePath, Standard_AttributeValidatorFileTemplate, "MyAttributeType");
			}
		}

		public static void CreateStandardGlobalValidator(string saveFilePanelFolderPath)
		{
			string filePath = EditorUtility.SaveFilePanelInProject("Create Global Validator Script", "MyGlobalValidator.cs", "cs", "Choose a file path to create a global validator script file at.", saveFilePanelFolderPath);
			if (!string.IsNullOrEmpty(filePath))
			{
				CreateTemplateScriptFile(filePath, Standard_GlobalValidatorFileTemplate, "MyAttributeType");
			}
		}

		public static void CreateStandardSceneValidator(string saveFilePanelFolderPath)
		{
			string filePath = EditorUtility.SaveFilePanelInProject("Create Scene Validator Script", "MySceneValidator.cs", "cs", "Choose a file path to create a scene validator script file at.", saveFilePanelFolderPath);
			if (!string.IsNullOrEmpty(filePath))
			{
				CreateTemplateScriptFile(filePath, Standard_SceneValidatorFileTemplate, "");
			}
		}

		public static void CreateRuleSceneValidator(string saveFilePanelFolderPath)
		{
			string filePath = EditorUtility.SaveFilePanelInProject("Create Scene Validator Script", "MySceneValidator.cs", "cs", "Choose a file path to create a scene validator script file at.", saveFilePanelFolderPath);
			if (!string.IsNullOrEmpty(filePath))
			{
				CreateTemplateScriptFile(filePath, Rule_SceneValidatorFileTemplate, "");
			}
		}

		public static void CreateStandardValueValidator(string saveFilePanelFolderPath)
		{
			string filePath = EditorUtility.SaveFilePanelInProject("Create Value Validator Script", "MyValueValidator.cs", "cs", "Choose a file path to create a value validator script file at.", saveFilePanelFolderPath);
			if (string.IsNullOrEmpty(filePath))
			{
				return;
			}
			string name = Path.GetFileNameWithoutExtension(filePath);
			bool useStructTemplate = false;
			if (name.FastEndsWith("Validator"))
			{
				string target = name.Substring(0, name.Length - "Validator".Length);
				if (ExpressionUtility.TryParseTypeNameAsCSharpIdentifier(target, out var type) && type.IsValueType)
				{
					useStructTemplate = true;
				}
			}
			CreateTemplateScriptFile(filePath, useStructTemplate ? Standard_ValueValidatorFileTemplate_Struct : Standard_ValueValidatorFileTemplate_Class, "MyValueType");
		}

		public static void CreateRuleRootObjectValidator(string saveFilePanelFolderPath)
		{
			string filePath = EditorUtility.SaveFilePanelInProject("Create Root Object Validator Script", "MyRootObjectValidator.cs", "cs", "Choose a file path to create a root object validator script file at.", saveFilePanelFolderPath);
			if (!string.IsNullOrEmpty(filePath))
			{
				CreateTemplateScriptFile(filePath, Rule_RootObjectValidatorFileTemplate, "MyUnityObjectType");
			}
		}

		public static void CreateRuleValueValidator(string saveFilePanelFolderPath)
		{
			string filePath = EditorUtility.SaveFilePanelInProject("Create Value Validator Script", "MyValueValidator.cs", "cs", "Choose a file path to create a value validator script file at.", saveFilePanelFolderPath);
			if (string.IsNullOrEmpty(filePath))
			{
				return;
			}
			string name = Path.GetFileNameWithoutExtension(filePath);
			bool useStructTemplate = false;
			if (name.FastEndsWith("Validator"))
			{
				string target = name.Substring(0, name.Length - "Validator".Length);
				if (ExpressionUtility.TryParseTypeNameAsCSharpIdentifier(target, out var type) && type.IsValueType)
				{
					useStructTemplate = true;
				}
			}
			CreateTemplateScriptFile(filePath, useStructTemplate ? Rule_ValueValidatorFileTemplate_Struct : Rule_ValueValidatorFileTemplate_Class, "MyValueType");
		}

		public static void CreateRuleAttributeValidator(string saveFilePanelFolderPath)
		{
			string filePath = EditorUtility.SaveFilePanelInProject("Create Attribute Validator Script", "MyAttributeValidator.cs", "cs", "Choose a file path to create an attribute validator script file at.", saveFilePanelFolderPath);
			if (!string.IsNullOrEmpty(filePath))
			{
				CreateTemplateScriptFile(filePath, Rule_AttributeValidatorFileTemplate, "MyAttributeType");
			}
		}

		public static void CreateStandardRootObjectValidator(string saveFilePanelFolderPath)
		{
			string filePath = EditorUtility.SaveFilePanelInProject("Create Root Object Validator Script", "MyRootObjectValidator.cs", "cs", "Choose a file path to create a root object validator script file at.", saveFilePanelFolderPath);
			if (!string.IsNullOrEmpty(filePath))
			{
				CreateTemplateScriptFile(filePath, Standard_RootObjectValidatorFileTemplate, "MyUnityObjectType");
			}
		}
	}
}

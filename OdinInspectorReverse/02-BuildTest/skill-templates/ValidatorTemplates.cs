// =============================================================================
// Odin Validator 自定义规则模板库（odin-validator 技能 · assets/）
// -----------------------------------------------------------------------------
// 用法：
//   1. 复制你需要的那一「段」（region）到自己的 .cs 文件，保留顶部所需 using
//   2. 文件必须放在 **Editor 程序集**（Editor 文件夹 或 editor-only asmdef）
//   3. 取消注释该段顶部的 [assembly: ...] 注册行
//   4. 命名空间按项目改；类名建议以 Validator 结尾
//
// 注册行说明：
//   [assembly: RegisterValidationRule(typeof(X), ...)]  → 可配置规则（出现在 Rules 列表，
//                                                         可启停、可改 public 字段）
//     硬约束：非泛型 + 有公共无参构造 + 继承五个基类之一
//   [assembly: RegisterValidator(typeof(X))]            → 仅声明存在（允许 typeof(X<>) 泛型）
//
// 本文件所有模板均基于官方 ValidatorScriptTemplates 还原并补全
// （Sirenix.OdinValidator.Editor/ValidatorScriptTemplates.cs），API 已对照反编译源码核实，
// 并已用 Roslyn csc 对原厂程序集编译验证通过。
// =============================================================================

#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.OdinValidator.Editor;
using UnityEditor;
using UnityEngine;

// -----------------------------------------------------------------------------
// ① ValueValidator<T> — 检查某个「值」（字段/属性的值）
//    适用：字符串非空、数值越界、自定义 struct 合法性
// -----------------------------------------------------------------------------
// [assembly: RegisterValidationRule(typeof(NotEmptyStringValidator), Description = "检查字符串字段不为空。")]

public class NotEmptyStringValidator : ValueValidator<string>
{
    // public 字段会出现在 Validator 窗口的 Rules 配置里，并随 Local/Project 档持久化
    [Tooltip("该问题的严重级别。")]
    public ValidatorSeverity ValidatorSeverity = ValidatorSeverity.Warning;

    // 可选剪枝：只在满足条件时对该属性跑这条规则（默认 true 表示全跑）
    public override bool CanValidateProperty(InspectorProperty property)
    {
        // 尊重使用者的豁免意图：带 [Optional] 的字段不检查（内置规则也是这么做的）
        return property.GetAttribute<OptionalAttribute>() == null;
    }

    protected override void Validate(ValidationResult result)
    {
        // this.Value    = ValueEntry.SmartValue
        // this.Property = 当前 InspectorProperty（可拿 NiceName / Path / ParentType ...）
        if (string.IsNullOrWhiteSpace(Value))
        {
            // 注意：Add* 返回 ref ResultItem，必须 ref 接收，否则后续链式调用改的是副本
            ref var item = ref result.Add(ValidatorSeverity, $"\"{Property.NiceName}\" 不应为空。");
            item.WithFix("填入占位文本", () => Value = "TODO");
        }
    }
}

// -----------------------------------------------------------------------------
// ② RootObjectValidator<T> — 检查一个「Unity 对象整体」（ScriptableObject / 资产）
//    适用：配置资产完整性、预制体结构要求
// -----------------------------------------------------------------------------
// [assembly: RegisterValidationRule(typeof(GameSettingsValidator), Description = "检查 GameSettings 资产的必填项。")]

public class GameSettingsValidator : RootObjectValidator<GameSettings>
{
    protected override void Validate(ValidationResult result)
    {
        // this.Object 就是被验证的 GameSettings 实例
        if (Object.Icon == null)
        {
            result.AddError($"{Object.name} 未设置 Icon。")
                  .SetSelectionObject(Object);   // 双击结果时选中该资产
        }
    }

    // 可选：进一步剪枝（IsTreeRoot 由基类强制判定，无需自己处理）
    protected override bool CanValidateObject(GameSettings obj) => obj != null && obj.Enabled;
}

// -----------------------------------------------------------------------------
// ③ AttributeValidator<TAttr, TValue> — 检查「打了某个 Attribute 的成员」
//    变体：AttributeValidator<TAttr>（只关心 Attribute，不关心值类型）
// -----------------------------------------------------------------------------
// [assembly: RegisterValidationRule(typeof(MustBePowerOfTwoValidator), Description = "检查 [PowerOfTwo] 字段。")]

public class PowerOfTwoAttribute : System.Attribute { }

public class MustBePowerOfTwoValidator : AttributeValidator<PowerOfTwoAttribute, int>
{
    protected override void Validate(ValidationResult result)
    {
        // this.Attribute = 属性上的 PowerOfTwoAttribute 实例
        // this.Value     = 成员值
        if (Value <= 0 || (Value & (Value - 1)) != 0)
        {
            result.AddError($"{Property.NiceName} = {Value} 不是 2 的幂。");
        }
    }
}

// -----------------------------------------------------------------------------
// ④ SceneValidator — 检查「整个场景」（跨对象、层级结构、必需节点）
//    只在 Validator 窗口/会话中跑，不会出现在 Inspector 的即时校验里
// -----------------------------------------------------------------------------
// [assembly: RegisterValidationRule(typeof(RequiredSceneHierarchyValidator), Description = "检查场景必需节点是否存在。")]

public class RequiredSceneHierarchyValidator : SceneValidator
{
    // 可在 Rules 里按项目调整
    public string RequiredPath = "Managers/Spawner";
    public bool RequireMainCamera = true;

    protected override void Validate(ValidationResult result)
    {
        // 场景查询 API（见 SceneValidator.cs）：
        //   ValidatedScene / GetSceneRoots() / GetSceneRoot(name)
        //   GetAllGameObjectsInScene() / GetGameObjectAtPath(path)
        //   FindComponentInSceneOfType<T>() / FindAllComponentsInSceneOfType<T>()
        //   GetComponentAtPath<T>(path) / LoadSceneIfNotLoaded(askToSave)

        if (GetGameObjectAtPath(RequiredPath) == null)
        {
            result.AddError($"场景中缺少必需节点 \"{RequiredPath}\"。")
                  .SetSelectionObject(GetSceneRoots().FirstOrDefault());
        }

        if (RequireMainCamera && FindComponentInSceneOfType<Camera>() == null)
        {
            result.AddWarning("场景中没有 Camera。");
        }
    }

    // 可选：按场景剪枝
    public override bool CanValidateScene(SceneReference scene) => scene != null && scene.IsValid;
}

// -----------------------------------------------------------------------------
// ⑤ GlobalValidator — 项目级检查（不依附任何对象/属性）
//    支持 yield return null 分帧；适合构建设置、全局资源、耗时扫描
// -----------------------------------------------------------------------------
// [assembly: RegisterValidationRule(typeof(SupportedBuildTargetValidator), Description = "检查当前构建目标是否被项目支持。")]

public class SupportedBuildTargetValidator : GlobalValidator
{
    public BuildTarget[] ValidBuildTargets = { BuildTarget.Android, BuildTarget.iOS };

    public override IEnumerable RunValidation(ValidationResult result)
    {
        if (!ValidBuildTargets.Contains(EditorUserBuildSettings.activeBuildTarget))
        {
            // 注意 GlobalValidator 里 AddError 返回的 ref 必须 ref 接收
            ref var error = ref result.AddError(
                $"当前构建目标（{EditorUserBuildSettings.activeBuildTarget}）不被本项目支持。");

            foreach (var target in ValidBuildTargets)
            {
                var captured = target;   // 闭包捕获：必须复制到局部变量
                error.WithButton($"切换到 {captured}", () =>
                    EditorUserBuildSettings.SwitchActiveBuildTarget(
                        BuildPipeline.GetBuildTargetGroup(captured), captured));
            }
        }

        // 需要把长任务分摊到多帧就 yield return null；不需要则返回 null
        return null;
    }
}

// -----------------------------------------------------------------------------
// ⑥ 可配置规则的右键入口：实现 IDefinesGenericMenuItems
//    → 属性右键菜单 "Rule Settings/For me only（仅本机）" 与 "For everyone（全项目）"
// -----------------------------------------------------------------------------
// [assembly: RegisterValidationRule(typeof(MinValueRuleValidator), Description = "检查数值不小于配置的最小值。")]

public class MinValueRuleValidator : ValueValidator<float>, IDefinesGenericMenuItems
{
    public float Min = 0f;
    public ValidatorSeverity Severity = ValidatorSeverity.Error;

    protected override void Validate(ValidationResult result)
    {
        if (Value < Min)
            result.Add(Severity, $"{Property.NiceName} 小于最小值 {Min}。");
    }

    public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
    {
        genericMenu.AddItem(new GUIContent("Rule Settings/For me only"), false, () =>
            ValidationSessionEditor.OpenRuleSettingsWindow(typeof(MinValueRuleValidator), ConfigSourceType.Local));
        genericMenu.AddItem(new GUIContent("Rule Settings/For everyone"), false, () =>
            ValidationSessionEditor.OpenRuleSettingsWindow(typeof(MinValueRuleValidator), ConfigSourceType.Project));
    }
}

// -----------------------------------------------------------------------------
// ⑦ 批处理 / CI 入口（无 GUI 跑全量验证并给出进程退出码）
//    Unity 命令行：
//      Unity -batchmode -nographics -quit -projectPath <proj> \
//            -executeMethod ValidateCI.Run -logFile validate.log
// -----------------------------------------------------------------------------

public static class ValidateCI
{
    public static void Run()
    {
        var profile = ValidationProfile.MainValidationProfile;
        // ValidationSessionAssetHandle 是引用计数的：必须 Dispose，否则会话常驻
        using (var handle = profile.ClaimSessionHandle())
        {
            var session = handle.Session;
            session.PopulateQueue(clearCurrentQueue: true, populateUnloadedScenes: true);

            int errors = 0, warnings = 0;
            foreach (var batch in session.ValidateEverythingEnumeratorBatched(openClosedScenes: true))
            {
                if (batch.Count == 0) continue;
                ref var item = ref batch.HighestSeverityResult;
                switch (item.ResultType)
                {
                    case ValidationResultType.Error:
                        errors++;
                        Debug.LogError($"{batch.Path}: {item.Message}");
                        break;
                    case ValidationResultType.Warning:
                        warnings++;
                        Debug.LogWarning($"{batch.Path}: {item.Message}");
                        break;
                }
            }

            // 可选：导出 HTML 报告挂到 CI 产物
            // System.IO.File.WriteAllText("odin-validator-report.html", session.ToHtml());

            Debug.Log($"Odin Validator: {errors} errors, {warnings} warnings");
            EditorApplication.Exit(errors > 0 ? 1 : 0);
        }
    }
}

// -----------------------------------------------------------------------------
// 附：模板 ② 用到的示例目标类型。复制到项目时删除，换成你自己的类型。
// -----------------------------------------------------------------------------

public class GameSettings : ScriptableObject
{
    public bool Enabled = true;
    public Texture2D Icon;
}

#endif

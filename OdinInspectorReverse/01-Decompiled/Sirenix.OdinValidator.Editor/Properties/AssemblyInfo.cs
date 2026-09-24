using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.OdinValidator.Editor;
using Sirenix.OdinValidator.Editor.Validators;
using Sirenix.Utilities;

[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: Guid("afbf832b-c461-49f5-a291-e87cab63e46d")]
[assembly: ComVisible(false)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCopyright("Copyright © 2022")]
[assembly: AssemblyProduct("Sirenix.OdinValidator.Editor")]
[assembly: AssemblyCompany("Sirenix ApS")]
[assembly: PersistentAssembly]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyTitle("Sirenix.OdinValidator.Editor")]
[assembly: CLSCompliant(false)]
[assembly: InternalsVisibleTo("Sirenix.Internal")]
[assembly: RegisterValidator(typeof(OdinValidatorWelcomeError))]
[assembly: AssemblyDescription("")]
[assembly: RegisterValidationRule(typeof(BrokenPrefabConnectionValidator), null, null, true, Description = "Checks if the source Prefab or Model has been deleted and is valid.")]
[assembly: RegisterValidationRule(typeof(DuplicateComponentsValidator), null, null, true, Description = "Checks if the GameObject has duplicate components.")]
[assembly: RegisterValidationRule(typeof(UnityObjectScriptMatchValidator), null, null, true, Description = "Ensures that custom ScriptableObject and MonoBehaviour class names matches their corresponding script filenames.")]
[assembly: RegisterValidator(typeof(UnityEventValidator))]
[assembly: RegisterValidationRule(typeof(UICanvasChildElementValidator), null, null, true, Description = "Checks if a UI object is a child of a canvas.")]
[assembly: RegisterValidationRule(typeof(ShaderCompilerErrorsValidator), null, null, true, Description = "Checks if a shader has any compiler errors.")]
[assembly: RegisterValidationRule(typeof(SerializeReferenceValidator), null, null, true, Description = "Checks if the instance or base type specified for a SerializeReference is valid.")]
[assembly: RegisterValidationRule(typeof(SceneNotInBuildSettingsValidator), null, null, true, EnabledByDefault = false, Description = "Checks if specified scenes are present in Unity's build settings.")]
[assembly: RegisterValidationRule(typeof(ReferenceRequiredByDefaultValidator), null, null, false, Description = "Checks if a reference is null or missing, unless it is marked with the [Optional] attribute.")]
[assembly: RegisterValidator(typeof(DetectComponentsNotAttachedToGameobject<>))]
[assembly: RegisterValidator(typeof(ObsoleteAttributeValidator<>))]
[assembly: RegisterValidationRule(typeof(MustBeAPrefabValidator), null, null, false, Description = "Checks if specified GameObject components are part of a prefab. If any components are found in a GameObject that is not a prefab, it flags an error. For each error detected, a fix is offered to convert the non-prefab GameObject into a prefab.")]
[assembly: RegisterValidationRule(typeof(MissingUnityObjectReferenceValidator), null, null, true, Description = "Validates all unity object refernces and checks if any of them has gone missing. A missing value is likely caused by the referenced object being deleted.", EnabledByDefault = true)]
[assembly: RegisterValidationRule(typeof(MissingScriptValidator), null, null, true, Description = "Checks if any GameObject has missing or unassociated scripts. This validator scans through each GameObject's components, verifying that all scripts are properly linked. If a component is detected without an associated script, a warning is issued along with an optional automatic fix to remove such components. Please note, this automated fix relies on Unity's GameObjectUtility.RemoveMonoBehavioursWithMissingScript method, which may not resolve all instances of missing scripts.")]
[assembly: RegisterValidationRule(typeof(MeshRendererValidator), null, null, true, Description = "Checks if Mesh Renderers in your Unity projects have modified properties not allowed in their prefabs, or if they're missing or have broken materials. Upon detecting prefab modifications, it offers a fix to clear them. When encountering missing materials, it provides a resolution to add a new material. Moreover, it assesses each material in the Mesh Renderer for potential errors. Detected errors are labeled with their corresponding severity levels.")]
[assembly: RegisterValidationRule(typeof(MaterialValidator), null, null, true, Description = "Checks if a Material object has a missing or broken shader, or if it's associated with the incorrect render pipeline. For a missing shader, it notifies the issue and provides an automatic fix using an available shader. If the shader is found but contains errors, the validator flags it. Lastly, it verifies whether the material aligns with the current render pipeline, suggesting an upgrade if it's not compatible.")]
[assembly: RegisterValidationRule(typeof(ListNullElementValidator), null, null, false, Description = "Checks if a list contains null elements.")]
[assembly: RegisterValidationRule(typeof(InvalidLayerValidator), null, null, true, Description = "Checks if a GameObject in Unity has a valid Layer assigned. If the assigned Layer is null or missing, it registers an error and provides a fix by assigning a valid Layer from the available ones.")]
[assembly: RegisterValidationRule(typeof(HugeTransformPositionsValidator), null, null, true, Description = "Checks if the position of GameObjects in a Unity project exceed a defined size limit, specifically 100,000 units in any x, y, or z dimension. If a GameObject's position does exceed this limit, a warning is issued due to the potential floating-point precision limitations.")]
[assembly: RegisterValidationRule(typeof(NoEmptyStringsValidator), null, null, false, Description = "Checks if any string values in your Unity project are empty or only contain whitespace, with the ability to exclude or include specific namespaces.")]
[assembly: AssemblyVersion("1.0.0.0")]

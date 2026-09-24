using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Sirenix.OdinValidator.Editor.Validators
{
	public class MaterialValidator : RootObjectValidator<Material>
	{
		private class ShaderFixer
		{
			[ValueDropdown("GetShaders")]
			public Shader Shader;

			public IEnumerable<ValueDropdownItem> GetShaders()
			{
				foreach (Shader item in from x in Resources.FindObjectsOfTypeAll<Shader>()
					orderby x.name.Length
					select x)
				{
					if (!item.name.StartsWith("Hidden/"))
					{
						yield return new ValueDropdownItem(item.name, item);
					}
				}
			}
		}

		internal static class MaterialUpgradeUtil
		{
			public enum Pipeline
			{
				HDRP,
				URP,
				Builtin
			}

			private const string hdrpUpgraders = "UnityEditor.Rendering.HighDefinition.UpgradeStandardShaderMaterials, Unity.RenderPipelines.HighDefinition.Editor";

			private const string urpUpgraders = "UnityEditor.Rendering.Universal.UniversalRenderPipelineMaterialUpgrader, Unity.RenderPipelines.Universal.Editor";

			private static readonly MethodInfo getUpgraderMethodInfo;

			private static readonly MethodInfo upgradeMethodInfo;

			private static readonly Type upgradeFlagsType;

			private static readonly object upgradeFlags_None;

			private static readonly object upgraders;

			private static readonly object pipelineAsset;

			private static readonly Pipeline pipeline;

			private static readonly Dictionary<string, object> shaderToUpgrader;

			private static readonly bool canUpgrade;

			private static readonly bool useLegacyGetUpgrader;

			static MaterialUpgradeUtil()
			{
				shaderToUpgrader = new Dictionary<string, object>();
				bool localCanUpgrade = true;
				bool localUseLegacy = false;
				PropertyInfo currentRenderPipelineAssetPropertyInfo = typeof(GraphicsSettings).GetProperty("currentRenderPipeline", BindingFlags.Static | BindingFlags.Public);
				if (currentRenderPipelineAssetPropertyInfo == null)
				{
					canUpgrade = false;
					useLegacyGetUpgrader = false;
					return;
				}
				pipelineAsset = currentRenderPipelineAssetPropertyInfo.GetValue(null, null);
				pipeline = GetCurrentRenderPipeline();
				if (pipeline == Pipeline.Builtin)
				{
					canUpgrade = false;
					useLegacyGetUpgrader = false;
					return;
				}
				Type materialUpgraderType = Type.GetType("UnityEditor.Rendering.MaterialUpgrader, Unity.RenderPipelines.Core.Editor");
				if (materialUpgraderType == null)
				{
					canUpgrade = false;
					useLegacyGetUpgrader = false;
					return;
				}
				upgradeFlagsType = Type.GetType("UnityEditor.Rendering.MaterialUpgrader+UpgradeFlags, Unity.RenderPipelines.Core.Editor");
				if (upgradeFlagsType == null)
				{
					canUpgrade = false;
					useLegacyGetUpgrader = false;
					return;
				}
				upgradeFlags_None = Enum.Parse(upgradeFlagsType, "None");
				MethodInfo candidateGetUpgrader = materialUpgraderType.GetMethod("GetUpgrader", BindingFlags.Static | BindingFlags.NonPublic);
				Type listOfMaterialUpgraderType = typeof(List<>).MakeGenericType(materialUpgraderType);
				if (candidateGetUpgrader != null)
				{
					ParameterInfo[] parameters = candidateGetUpgrader.GetParameters();
					if (parameters.Length == 2 && parameters[0].ParameterType == listOfMaterialUpgraderType && parameters[1].ParameterType == typeof(Material))
					{
						localUseLegacy = true;
						getUpgraderMethodInfo = candidateGetUpgrader;
					}
				}
				upgradeMethodInfo = materialUpgraderType.GetMethod("Upgrade", BindingFlags.Instance | BindingFlags.Public, null, new Type[2]
				{
					typeof(Material),
					upgradeFlagsType
				}, null);
				if (upgradeMethodInfo == null)
				{
					canUpgrade = false;
					useLegacyGetUpgrader = localUseLegacy;
					return;
				}
				object localUpgraders;
				switch (pipeline)
				{
				case Pipeline.HDRP:
				{
					Type upgraderType2 = Type.GetType("UnityEditor.Rendering.HighDefinition.UpgradeStandardShaderMaterials, Unity.RenderPipelines.HighDefinition.Editor");
					if (upgraderType2 == null)
					{
						canUpgrade = false;
						useLegacyGetUpgrader = localUseLegacy;
						return;
					}
					MethodInfo getUpgradersMethodInfo2 = upgraderType2.GetMethod("GetHDUpgraders", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					if (getUpgradersMethodInfo2 == null)
					{
						canUpgrade = false;
						useLegacyGetUpgrader = localUseLegacy;
						return;
					}
					localUpgraders = getUpgradersMethodInfo2.Invoke(null, Array.Empty<object>());
					break;
				}
				case Pipeline.URP:
				{
					Type upgraderType = Type.GetType("UnityEditor.Rendering.Universal.UniversalRenderPipelineMaterialUpgrader, Unity.RenderPipelines.Universal.Editor");
					if (upgraderType == null)
					{
						canUpgrade = false;
						useLegacyGetUpgrader = localUseLegacy;
						return;
					}
					MethodInfo getUpgradersMethodInfo = upgraderType.GetMethod("GetUpgraders", BindingFlags.Static | BindingFlags.NonPublic);
					if (getUpgradersMethodInfo == null)
					{
						canUpgrade = false;
						useLegacyGetUpgrader = localUseLegacy;
						return;
					}
					object[] upgradersRef = new object[1] { Activator.CreateInstance(listOfMaterialUpgraderType) };
					getUpgradersMethodInfo.Invoke(null, upgradersRef);
					localUpgraders = upgradersRef[0];
					break;
				}
				default:
					canUpgrade = false;
					useLegacyGetUpgrader = localUseLegacy;
					return;
				}
				if (localUpgraders == null)
				{
					canUpgrade = false;
					useLegacyGetUpgrader = localUseLegacy;
					return;
				}
				upgraders = localUpgraders;
				if (localUseLegacy)
				{
					canUpgrade = true;
					useLegacyGetUpgrader = true;
					return;
				}
				PropertyInfo oldShaderPathProp = materialUpgraderType.GetProperty("OldShaderPath", BindingFlags.Instance | BindingFlags.Public);
				if (oldShaderPathProp == null)
				{
					canUpgrade = false;
					useLegacyGetUpgrader = false;
					return;
				}
				if (!(upgraders is IEnumerable enumerable))
				{
					canUpgrade = false;
					useLegacyGetUpgrader = false;
					return;
				}
				foreach (object upgrader in enumerable)
				{
					if (upgrader != null)
					{
						string shaderName = oldShaderPathProp.GetValue(upgrader, null) as string;
						if (!string.IsNullOrEmpty(shaderName) && !shaderToUpgrader.ContainsKey(shaderName))
						{
							shaderToUpgrader.Add(shaderName, upgrader);
						}
					}
				}
				if (shaderToUpgrader.Count == 0)
				{
					localCanUpgrade = false;
				}
				canUpgrade = localCanUpgrade;
				useLegacyGetUpgrader = false;
			}

			public static bool HasToBeUpgraded(Material material)
			{
				if (!canUpgrade || material == null)
				{
					return false;
				}
				if (useLegacyGetUpgrader)
				{
					object upgrader = getUpgraderMethodInfo.Invoke(null, new object[2] { upgraders, material });
					return upgrader != null;
				}
				if (material.shader == null)
				{
					return false;
				}
				return shaderToUpgrader.ContainsKey(material.shader.name);
			}

			public static void Upgrade(Material material)
			{
				if (!canUpgrade || material == null)
				{
					return;
				}
				object upgraderObj;
				if (useLegacyGetUpgrader)
				{
					object upgrader = getUpgraderMethodInfo.Invoke(null, new object[2] { upgraders, material });
					if (upgrader != null)
					{
						upgradeMethodInfo.Invoke(upgrader, new object[2] { material, upgradeFlags_None });
					}
				}
				else if (!(material.shader == null) && shaderToUpgrader.TryGetValue(material.shader.name, out upgraderObj))
				{
					upgradeMethodInfo.Invoke(upgraderObj, new object[2] { material, upgradeFlags_None });
				}
			}

			private static Pipeline GetCurrentRenderPipeline()
			{
				if (pipelineAsset == null)
				{
					return Pipeline.Builtin;
				}
				string name = pipelineAsset.GetType().Name;
				if (!(name == "HDRenderPipelineAsset"))
				{
					if (name == "UniversalRenderPipelineAsset")
					{
						return Pipeline.URP;
					}
					return Pipeline.Builtin;
				}
				return Pipeline.HDRP;
			}
		}

		[FormerlySerializedAs("Severity")]
		[Tooltip("The severity of the validation result.")]
		public ValidatorSeverity MissingShaderSeverity;

		[Tooltip("The severity of the validation result.")]
		[FormerlySerializedAs("Severity")]
		public ValidatorSeverity BrokenShaderSeverity;

		[FormerlySerializedAs("Severity")]
		[Tooltip("The severity of the validation result.")]
		public ValidatorSeverity WrongRenderPipelineSeverity;

		protected override void Validate(ValidationResult result)
		{
			ValidateMaterial(base.Object, "Material (" + base.Object.name + ")", MissingShaderSeverity, BrokenShaderSeverity, WrongRenderPipelineSeverity, result);
		}

		public static bool ValidateMaterial(Material mat, string target, ValidatorSeverity missingShaderSeverity, ValidatorSeverity brokenShaderSeverity, ValidatorSeverity wrongRenderPipelineSeverity, ValidationResult result)
		{
			if (mat.shader == null)
			{
				result.Add(missingShaderSeverity, target + " has a missing shader");
				return true;
			}
			if (string.IsNullOrEmpty(mat.shader.name))
			{
				result.Add(missingShaderSeverity, target + " has a missing shader").WithFix(delegate(ShaderFixer x)
				{
					mat.shader = x.Shader;
				});
				return true;
			}
			if (mat.shader.name == "Hidden/InternalErrorShader")
			{
				result.Add(missingShaderSeverity, target + " has a missing shader").WithFix(delegate(ShaderFixer x)
				{
					mat.shader = x.Shader;
				});
				return true;
			}
			if (OdinShaderUtil.ShaderHasErrorIsSupported && OdinShaderUtil.ShaderHasError(mat.shader))
			{
				result.Add(brokenShaderSeverity, target + " uses a shader with errors").WithMetaData(new MetaData
				{
					{ "Material", mat },
					{ "Shader", mat.shader }
				});
				return true;
			}
			if (MaterialUpgradeUtil.HasToBeUpgraded(mat))
			{
				result.Add(wrongRenderPipelineSeverity, target + " has to be upgraded to work with your current render pipeline.").WithFix(Fix.Create("Upgrade", delegate
				{
					MaterialUpgradeUtil.Upgrade(mat);
				}));
				return true;
			}
			return false;
		}
	}
}

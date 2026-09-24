using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor.Validators
{
	public class MeshRendererValidator : RootObjectValidator<MeshRenderer>
	{
		[ShowOdinSerializedPropertiesInInspector]
		internal class FixArgs
		{
			[Required]
			public Material Material;
		}

		[Tooltip("The severity of the validation result.")]
		public ValidatorSeverity PrefabModificationsSeverity = ValidatorSeverity.Ignore;

		[Tooltip("The severity of the validation result.")]
		public ValidatorSeverity MissingMaterialSeverity;

		[Tooltip("The severity of the validation result.")]
		public ValidatorSeverity BrokenMaterialSeverity;

		protected override void Validate(ValidationResult result)
		{
			MeshRenderer obj = base.Object;
			if (PrefabModificationsSeverity != ValidatorSeverity.Ignore && !string.IsNullOrEmpty(obj.gameObject.scene.path))
			{
				PropertyModification[] mods = PrefabUtility.GetPropertyModifications(obj);
				if (mods != null)
				{
					MeshRenderer correspondingObj = PrefabUtility.GetCorrespondingObjectFromSource(obj);
					if (!string.IsNullOrEmpty(base.Object.gameObject.scene.path))
					{
						for (int i = 0; i < mods.Length; i++)
						{
							if (mods[i].target == correspondingObj)
							{
								result.Add(PrefabModificationsSeverity, "Mesh Renderer contains prefab modifications which is not allowed in this project. ").WithFix(Fix.Create("Clear prefab modification", RemovePrefabMods));
								break;
							}
						}
					}
				}
			}
			Material[] materials = base.Object.sharedMaterials;
			if (materials.Length == 0)
			{
				result.Add(MissingMaterialSeverity, base.Object.gameObject.name + " is missing a material").WithFix(Fix.Create(delegate(FixArgs args)
				{
					base.Object.sharedMaterial = args.Material;
				}));
				return;
			}
			for (int i2 = 0; i2 < materials.Length; i2++)
			{
				int localI = i2;
				Material sharedMat = materials[i2];
				if (sharedMat == null)
				{
					result.Add(MissingMaterialSeverity, base.Object.gameObject.name + " is missing a material").WithFix(Fix.Create(delegate(FixArgs args)
					{
						Material[] sharedMaterials = base.Object.sharedMaterials;
						sharedMaterials[localI] = args.Material;
						base.Object.sharedMaterials = sharedMaterials;
					}));
				}
				else
				{
					MaterialValidator.ValidateMaterial(sharedMat, base.Object.gameObject.name + "'s material", BrokenMaterialSeverity, BrokenMaterialSeverity, BrokenMaterialSeverity, result);
				}
			}
		}

		private void RemovePrefabMods()
		{
			MeshRenderer obj = base.Object;
			PropertyModification[] mods = PrefabUtility.GetPropertyModifications(obj);
			MeshRenderer correspondingObj = PrefabUtility.GetCorrespondingObjectFromSource(obj);
			bool isDirty = false;
			for (int i = mods.Length - 1; i >= 0; i--)
			{
				if (mods[i].target == correspondingObj)
				{
					mods = ArrayUtilities.CreateNewArrayWithRemovedElement(mods, i);
					isDirty = true;
				}
			}
			if (isDirty)
			{
				PrefabUtility.SetPropertyModifications(obj, mods);
			}
		}
	}
}

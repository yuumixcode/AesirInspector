using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor.Validators
{
	public class MustBeAPrefabValidator : RootObjectValidator<GameObject>
	{
		[ValueDropdown("GetAllCmpTypes", IsUniqueList = true)]
		[Tooltip("Component types that must be part of a prefab. For example, if you the Camera component to this list, you're saying that all game objects with a camera must be made into a prefab.")]
		public Type[] ComponentTypes = new Type[0];

		[FolderPath]
		[Tooltip("The location where the prefab will be saved.")]
		public string MakePrefabAssetLocation = "Assets/";

		private IEnumerable<ValueDropdownItem> GetAllCmpTypes()
		{
			foreach (Type item in TypeCache.GetTypesDerivedFrom(typeof(Component)))
			{
				yield return new ValueDropdownItem(item.GetNiceName(), item);
			}
		}

		protected override void Validate(ValidationResult result)
		{
			if (ComponentTypes.Length == 0)
			{
				return;
			}
			GameObject obj = base.Object;
			Component[] components = obj.GetComponents(typeof(Component));
			foreach (Component cmp in components)
			{
				if (!cmp || !ComponentTypes.Contains(cmp.GetType()) || PrefabUtility.IsPartOfPrefabInstance(obj) || !obj.gameObject.scene.IsValid())
				{
					continue;
				}
				result.AddError("GameObject must be a prefab").WithFix(Fix.Create("Convert to prefab", delegate
				{
					string path = MakePrefabAssetLocation + obj.gameObject.name + ".prefab";
					path = AssetDatabase.GenerateUniqueAssetPath(path);
					switch (PrefabUtility.GetPrefabInstanceStatus(obj))
					{
					case PrefabInstanceStatus.Disconnected:
						return;
					case PrefabInstanceStatus.MissingAsset:
						PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
						break;
					}
					PrefabUtility.SaveAsPrefabAsset(obj.gameObject, path);
				}));
				break;
			}
		}
	}
}

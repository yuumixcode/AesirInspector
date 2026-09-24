using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	[HideMonoScript]
	public class ValidationProfile : ScriptableObject, IValidationProfile
	{
		[HideInInspector]
		public List<ValidationItem> Include = new List<ValidationItem>
		{
			ValidationItem.FromOpenScenes(includeAssetDeps: true),
			ValidationItem.FromScenesInBuildOptions(includeAssetDeps: true),
			ValidationItem.FromSceneFolderPath("Assets"),
			ValidationItem.FromAssetPath("Assets")
		};

		[HideInInspector]
		public List<ValidationItem> Exclude = new List<ValidationItem>();

		[NonSerialized]
		[ShowInInspector]
		private ValidationSessionEditor.ValidationProfileEditor editor;

		[NonSerialized]
		internal HashSet<ValidationSessionAssetHandle> handles = new HashSet<ValidationSessionAssetHandle>();

		internal ValidationSession session;

		private static ValidationProfile mainValidationProfile;

		private static EditorPrefString mainValidationProfileGuid = new EditorPrefString("ODIN_VALIDATOR_mainValidationProfileGuid", "");

		[SerializeField]
		[HideInInspector]
		internal SdfIconType icon = SdfIconType.CircleFill;

		public static ValidationProfile MainValidationProfile
		{
			get
			{
				if (mainValidationProfile != null)
				{
					return mainValidationProfile;
				}
				string guid = mainValidationProfileGuid.Value;
				if (string.IsNullOrEmpty(guid))
				{
					string path = AssetDatabase.GetAssetPath(Sirenix.OdinValidator.Editor.MainValidationProfile.Instance);
					guid = AssetDatabase.AssetPathToGUID(path);
					mainValidationProfileGuid.Value = guid;
				}
				if (!string.IsNullOrEmpty(guid))
				{
					string path2 = AssetDatabase.GUIDToAssetPath(guid);
					if (!string.IsNullOrEmpty(path2))
					{
						ValidationProfile asset = AssetDatabase.LoadAssetAtPath<ValidationProfile>(path2);
						if (asset != null)
						{
							mainValidationProfile = asset;
						}
					}
				}
				if (mainValidationProfile == null)
				{
					mainValidationProfile = Sirenix.OdinValidator.Editor.MainValidationProfile.Instance;
				}
				return mainValidationProfile;
			}
			set
			{
				if (value == null)
				{
					mainValidationProfileGuid.Value = "";
					mainValidationProfile = null;
					return;
				}
				string path = AssetDatabase.GetAssetPath(value);
				string guid = AssetDatabase.AssetPathToGUID(path);
				mainValidationProfileGuid.Value = guid;
				mainValidationProfile = value;
			}
		}

		public static string DefaultConfigFolderPath => SirenixAssetPaths.SirenixPluginPath + "Odin Validator/Editor/Profiles/";

		IList<ValidationItem> IValidationProfile.Include => Include;

		IList<ValidationItem> IValidationProfile.Exclude => Exclude;

		public SessionConfigDataType Type => SessionConfigDataType.Persistent;

		public virtual SdfIconType Icon => icon;

		public void SaveChanges()
		{
			EditorUtility.SetDirty(this);
		}

		[OnInspectorInit]
		private void CreateEditor()
		{
			editor = new ValidationSessionEditor.ValidationProfileEditor
			{
				DataSources = new IValidationProfile[1] { this }
			};
		}

		public ValidationSessionAssetHandle ClaimSessionHandle()
		{
			return new ValidationSessionAssetHandle(this);
		}

		public static IEnumerable<ValidationProfile> FindAll()
		{
			yield return Sirenix.OdinValidator.Editor.MainValidationProfile.Instance;
			foreach (ValidationProfile item in (from x in AssetDatabase.FindAssets("t:ValidationProfile").Select(AssetDatabase.GUIDToAssetPath)
				orderby x descending
				select x).Select(AssetDatabase.LoadAssetAtPath<ValidationProfile>))
			{
				if (!(item == Sirenix.OdinValidator.Editor.MainValidationProfile.Instance) && !(item == null))
				{
					yield return item;
				}
			}
		}

		public void OpenValidatorWindow()
		{
			OdinValidatorWindow.OpenWindow(this);
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	public class MainLocalValidationProfile : IValidationProfile
	{
		private static EditorPrefString localDataPath;

		private static MainLocalValidationProfile instance;

		public List<ValidationItem> Include = new List<ValidationItem>();

		public List<ValidationItem> Exclude = new List<ValidationItem>();

		public static string DefaultLocalDataPath => Application.persistentDataPath.TrimEnd('/', '\\') + "/Odin Validator/localconfig.data";

		public static MainLocalValidationProfile Instance
		{
			get
			{
				if (instance == null)
				{
					if (File.Exists(LocalDataPath))
					{
						byte[] bytes = File.ReadAllBytes(LocalDataPath);
						try
						{
							instance = SerializationUtility.DeserializeValue<MainLocalValidationProfile>(bytes, DataFormat.Binary);
						}
						catch (Exception ex)
						{
							Debug.LogError($"Error while deserializing local validation datas from file '{LocalDataPath}'. Local validation datas have been lost. The exception thrown was: {ex}");
							Debug.LogException(ex);
							instance = new MainLocalValidationProfile();
						}
					}
					else
					{
						instance = new MainLocalValidationProfile();
					}
				}
				return instance;
			}
		}

		public static EditorPrefString LocalDataPath
		{
			get
			{
				if (localDataPath == null)
				{
					localDataPath = new EditorPrefString("SIRENIX_ODINVALIDATOR_LOCALDATAPATH", DefaultLocalDataPath);
				}
				if (string.IsNullOrEmpty(localDataPath.Value))
				{
					localDataPath.Value = DefaultLocalDataPath;
				}
				return localDataPath;
			}
		}

		IList<ValidationItem> IValidationProfile.Include => Include;

		IList<ValidationItem> IValidationProfile.Exclude => Exclude;

		public SessionConfigDataType Type => SessionConfigDataType.NonPersistent;

		public SdfIconType Icon => SdfIconType.PersonFill;

		public void SaveChanges()
		{
			byte[] bytes = SerializationUtility.SerializeValue(this, DataFormat.Binary);
			string dir = Path.GetDirectoryName(LocalDataPath);
			Directory.CreateDirectory(dir);
			File.WriteAllBytes(LocalDataPath, bytes);
		}

		private MainLocalValidationProfile()
		{
		}
	}
}

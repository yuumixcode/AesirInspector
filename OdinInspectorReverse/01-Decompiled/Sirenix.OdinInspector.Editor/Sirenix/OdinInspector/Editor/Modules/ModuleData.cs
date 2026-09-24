using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Serialization;

namespace Sirenix.OdinInspector.Editor.Modules
{
	public class ModuleData
	{
		public class ModuleFile
		{
			public string Path;

			public byte[] Data;
		}

		public string ID;

		public Version Version;

		public Version RequiresOdinVersion;

		public List<ModuleFile> Files;

		public ModuleManifest ToManifest()
		{
			return new ModuleManifest
			{
				ID = ID,
				Version = Version,
				RequiresOdinVersion = RequiresOdinVersion,
				Files = Files.Select((ModuleFile n) => n.Path).ToList()
			};
		}

		public static byte[] Serialize(ModuleData data)
		{
			return SerializationUtility.SerializeValue(data, DataFormat.Binary);
		}

		public static ModuleData Deserialize(byte[] bytes)
		{
			return SerializationUtility.DeserializeValue<ModuleData>(bytes, DataFormat.Binary);
		}
	}
}

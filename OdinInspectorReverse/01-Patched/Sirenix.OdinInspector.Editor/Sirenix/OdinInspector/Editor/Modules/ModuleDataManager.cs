using System.IO;

namespace Sirenix.OdinInspector.Editor.Modules
{
	public class ModuleDataManager
	{
		public string DataPath;

		public string InstallPath;

		public virtual void SaveData(string id, byte[] data)
		{
			string path = DataPath.TrimEnd('/', '\\') + "/" + id + ".data";
			FileInfo file = new FileInfo(path);
			if (!file.Directory.Exists)
			{
				file.Directory.Create();
			}
			using FileStream fileStream = new FileStream(file.FullName, FileMode.Create);
			fileStream.Write(data, 0, data.Length);
		}

		public virtual bool HasData(string id)
		{
			string path = DataPath.TrimEnd('/', '\\') + "/" + id + ".data";
			return File.Exists(path);
		}

		public virtual byte[] LoadData(string id)
		{
			string path = DataPath.TrimEnd('/', '\\') + "/" + id + ".data";
			if (!File.Exists(path))
			{
				return null;
			}
			using FileStream fileStream = new FileStream(path, FileMode.Open);
			byte[] bytes = new byte[fileStream.Length];
			fileStream.Read(bytes, 0, bytes.Length);
			return bytes;
		}
	}
}

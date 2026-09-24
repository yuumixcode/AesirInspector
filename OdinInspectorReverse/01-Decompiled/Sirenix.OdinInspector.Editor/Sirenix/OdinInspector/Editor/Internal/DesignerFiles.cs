using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerFiles
	{
		public static readonly List<DesignerFile> InvalidFiles;

		public static readonly Dictionary<string, Type> PathToTypeMap;

		public static readonly Dictionary<Type, DesignerFile> Files;

		static DesignerFiles()
		{
			InvalidFiles = new List<DesignerFile>(64);
			PathToTypeMap = new Dictionary<string, Type>(256);
			Files = new Dictionary<Type, DesignerFile>(256);
			foreach (string path in Directory.EnumerateFiles(Application.dataPath, "*.ovdf", SearchOption.AllDirectories))
			{
				Type type = DesignerUtils.GetTypeFromFile(path);
				string universalPath = path.Replace('\\', '/');
				DesignerFile file = new DesignerFile(universalPath);
				if (type == null)
				{
					InvalidFiles.Add(file);
					continue;
				}
				Files[type] = file;
				PathToTypeMap[universalPath] = type;
				if (type.IsGenericType)
				{
					DesignerRegistry.RegisterGenericVariant(type, validateTypeArgs: false);
				}
			}
		}

		public static DesignerFile GetOrCreate(Type targetType)
		{
			if (Files.TryGetValue(targetType, out var file))
			{
				PathToTypeMap[file.Path] = targetType;
				return file;
			}
			file = new DesignerFile
			{
				Path = DesignerUtils.CreateFilePathFromType(targetType)
			};
			Files[targetType] = file;
			PathToTypeMap[file.Path] = targetType;
			return file;
		}

		public static Type GetTypeAtPath(string path)
		{
			if (!PathToTypeMap.TryGetValue(path, out var result))
			{
				return null;
			}
			return result;
		}

		public static void Remove(DesignerFile file)
		{
			Type type = GetTypeAtPath(file.Path);
			Files.Remove(type);
			PathToTypeMap.Remove(file.Path);
		}
	}
}

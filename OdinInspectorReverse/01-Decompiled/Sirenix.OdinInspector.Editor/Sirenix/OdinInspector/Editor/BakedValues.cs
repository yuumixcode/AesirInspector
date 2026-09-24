using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.Serialization;
using Sirenix.Serialization.Utilities;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public static class BakedValues
	{
		public enum BakedValueType
		{
			Invalid,
			String,
			Int,
			DateTime,
			Bool
		}

		private static Dictionary<string, object> loadedValues;

		private static byte[] LoadBakedValueBytes()
		{
			string path = SirenixAssetPaths.OdinPath + "/Assets/Editor/ConfigData.bytes";
			if (!File.Exists(path))
			{
				return null;
			}
			return File.ReadAllBytes(path);
		}

		private static Dictionary<string, object> ReadValues(byte[] bytes)
		{
			Dictionary<string, object> values = new Dictionary<string, object>();
			using MemoryStream stream = new MemoryStream(bytes);
			using Cache<BinaryDataReader> readerCache = Cache<BinaryDataReader>.Claim();
			BinaryDataReader reader = readerCache.Value;
			reader.PrepareNewSerializationSession();
			reader.Stream = stream;
			stream.Position = 0L;
			if (!reader.EnterArray(out var count))
			{
				throw new ArgumentException();
			}
			for (int i = 0; i < count; i++)
			{
				if (!reader.EnterNode(out var _))
				{
					throw new ArgumentException();
				}
				string name = reader.CurrentNodeName;
				if (string.IsNullOrEmpty(name))
				{
					throw new ArgumentException();
				}
				if (!reader.ReadInt32(out var type))
				{
					throw new ArgumentException();
				}
				if (!reader.ReadInt32(out var startLength))
				{
					throw new ArgumentException();
				}
				if (!reader.ReadInt32(out var dataLength))
				{
					throw new ArgumentException();
				}
				if (!reader.ReadString(out var data))
				{
					throw new ArgumentException();
				}
				if (!reader.ExitNode())
				{
					throw new ArgumentException();
				}
				object value = GetValueFromData(data.Substring(startLength, dataLength), (BakedValueType)type);
				values.Add(name, value);
			}
			if (!reader.ExitArray())
			{
				throw new ArgumentException();
			}
			return values;
		}

		private unsafe static object GetValueFromData(string data, BakedValueType type)
		{
			switch (type)
			{
			case BakedValueType.String:
				return data.Trim();
			case BakedValueType.Int:
				if (data.Length != 2)
				{
					throw new ArgumentException();
				}
				fixed (char* ptr = data)
				{
					return *(int*)ptr;
				}
			case BakedValueType.DateTime:
				if (data.Length != 4)
				{
					throw new ArgumentException();
				}
				fixed (char* ptr2 = data)
				{
					return DateTime.FromBinary(*(long*)ptr2);
				}
			case BakedValueType.Bool:
				if (data.Length != 1)
				{
					throw new ArgumentException();
				}
				return data[0] != '\0';
			default:
				throw new ArgumentException();
			}
		}

		public static bool TryGetBakedValue<T>(string name, out T value)
		{
			if (loadedValues == null)
			{
				try
				{
					byte[] bytes = LoadBakedValueBytes();
					if (bytes != null)
					{
						loadedValues = ReadValues(bytes);
					}
					else
					{
						loadedValues = new Dictionary<string, object>();
					}
				}
				catch (Exception innerException)
				{
					Debug.LogException(new Exception("Could not load baked config values from file.", innerException));
					loadedValues = new Dictionary<string, object>();
				}
			}
			if (!loadedValues.TryGetValue(name, out var weakValue))
			{
				value = default(T);
				return false;
			}
			if (weakValue == null)
			{
				value = default(T);
				return typeof(T) == typeof(string);
			}
			if (!(weakValue is T))
			{
				value = default(T);
				return false;
			}
			value = (T)weakValue;
			return true;
		}
	}
}

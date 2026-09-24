using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.Serialization;

namespace Sirenix.OdinInspector.Editor.Internal
{
	[Serializable]
	internal struct DesignerLocalCacheEntryBlob
	{
		public BinaryBuffer Data;

		public DesignerLocalCacheEntryBlob(int capacity)
		{
			Data = new BinaryBuffer(capacity);
		}

		public DesignerLocalCacheEntryBlob(string file)
		{
			if (!File.Exists(file))
			{
				Data = new BinaryBuffer(4096);
				return;
			}
			byte[] fileData = File.ReadAllBytes(file);
			if (fileData.Length == 0)
			{
				Data = new BinaryBuffer(4096);
				return;
			}
			Data = new BinaryBuffer(RoundPow2.Round64(fileData.Length) + 1024);
			Data.Write(fileData);
			Data.ReadOffset = 0;
			if (Data.TryReadUInt64(out var checksum))
			{
				Data.RemoveRead();
				ulong actualChecksum = Data.CalcChecksum();
				if (actualChecksum != checksum)
				{
					Data.Clear();
				}
			}
		}

		public void WriteEntry(Type type, ref DesignerLocalCacheEntry entry)
		{
			if (type == null)
			{
				return;
			}
			string typeName = TwoWaySerializationBinder.Default.BindToName(type);
			if (!string.IsNullOrEmpty(typeName))
			{
				Data.WriteStringUTF8UInt16(typeName);
				Data.WriteStringUTF8UInt16(entry.Path);
				if (!string.IsNullOrEmpty(entry.Guid))
				{
					Data.WriteBool(value: true);
					Data.WriteGuid(entry.Guid);
				}
				else
				{
					Data.WriteBool(value: false);
				}
				Data.WriteInt64(entry.WriteTimeUtc.Ticks);
			}
		}

		public void Clear()
		{
			Data.Clear();
		}

		public void ReadEntries(Dictionary<string, DesignerLocalCacheEntry> output)
		{
			if (Data.Length == 0 || output == null)
			{
				return;
			}
			Data.ReadOffset = 0;
			Type type;
			DesignerLocalCacheEntry entry;
			while (TryReadEntry(out type, out entry))
			{
				if (!string.IsNullOrEmpty(entry.Guid))
				{
					type = DesignerUtils.GetTypeFromScriptGuid(entry.Guid);
				}
				if (!(type == null))
				{
					entry.Type = type;
					if (!string.IsNullOrEmpty(entry.Path))
					{
						output[PathUtils.GetCrossPlatformPath(entry.Path)] = entry;
					}
				}
			}
		}

		public bool TryReadEntry(out Type type, out DesignerLocalCacheEntry entry)
		{
			type = null;
			entry = default(DesignerLocalCacheEntry);
			if (!Data.TryReadStringUTF8UInt16(out var typeBinding))
			{
				return false;
			}
			type = TwoWaySerializationBinder.Default.BindToType(typeBinding);
			entry.Type = type;
			if (!Data.TryReadStringUTF8UInt16(out entry.Path))
			{
				return false;
			}
			if (!TryReadGuid(out entry.Guid))
			{
				return false;
			}
			if (!Data.TryReadInt64(out var writeTimeUtcTicks))
			{
				return false;
			}
			entry.WriteTimeUtc = new DateTime(writeTimeUtcTicks, DateTimeKind.Utc);
			return true;
		}

		internal bool TryReadGuid(out string guid)
		{
			if (!Data.TryReadBool(out var hasGuid))
			{
				guid = null;
				return false;
			}
			if (!hasGuid)
			{
				guid = string.Empty;
				return true;
			}
			return Data.TryReadGuid(out guid);
		}

		public void SaveToDisk(string path)
		{
			string directory = Path.GetDirectoryName(path);
			if (string.IsNullOrEmpty(directory))
			{
				return;
			}
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			using FileStream stream = File.OpenWrite(path);
			stream.SetLength(8 + Data.Length);
			ulong checksum = Data.CalcChecksum();
			byte[] checksumBytes = BitConverter.GetBytes(checksum);
			stream.Write(checksumBytes, 0, checksumBytes.Length);
			stream.Write(Data.Buffer, 0, Data.Length);
			stream.Flush(flushToDisk: true);
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	[Serializable]
	internal class DesignerLocalCacheState : ScriptableObject, ISerializationCallbackReceiver
	{
		[SerializeField]
		public bool HasBeenChanged;

		[SerializeField]
		public bool IsInitialized;

		[SerializeField]
		public DesignerLocalCacheEntryBlob EntriesBlob = new DesignerLocalCacheEntryBlob(256);

		[NonSerialized]
		public Dictionary<string, DesignerLocalCacheEntry> Entries = new Dictionary<string, DesignerLocalCacheEntry>(128, StringComparer.OrdinalIgnoreCase);

		private static Dictionary<string, DateTime> handledFiles = new Dictionary<string, DateTime>(2048, StringComparer.OrdinalIgnoreCase);

		private static HashSet<string> visitedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		private static List<string> scannedRoots = new List<string>(8);

		public void OnBeforeSerialize()
		{
			SyncBlobWithEntries();
		}

		public void OnAfterDeserialize()
		{
		}

		public void SyncBlobWithEntries()
		{
			EntriesBlob.Clear();
			foreach (KeyValuePair<string, DesignerLocalCacheEntry> entry2 in Entries)
			{
				DesignerLocalCacheEntry entry = entry2.Value;
				Type type = entry.Type;
				EntriesBlob.WriteEntry(type, ref entry);
			}
		}

		public void SyncEntriesWithBlob()
		{
			Entries.Clear();
			EntriesBlob.ReadEntries(Entries);
		}

		public void Save(string path)
		{
			if (HasBeenChanged)
			{
				SyncBlobWithEntries();
				EntriesBlob.SaveToDisk(path);
				EditorUtility.ClearDirty(this);
			}
		}

		public void InitializeIfNeeded()
		{
			if (!IsInitialized)
			{
				EntriesBlob = new DesignerLocalCacheEntryBlob(DesignerLocalCache.LocalFilePath);
				SyncEntriesWithBlob();
				IsInitialized = true;
			}
		}

		public List<string> SyncEntriesWithDiskFiles()
		{
			handledFiles.Clear();
			visitedFiles.Clear();
			scannedRoots.Clear();
			List<string> diskFiles = new List<string>(256);
			List<string> searchDirs = GlobalConfig<OdinVisualDesignerConfig>.Instance.GetEffectiveSearchRoots();
			foreach (KeyValuePair<string, DesignerLocalCacheEntry> entry4 in Entries)
			{
				DesignerLocalCacheEntry entry = entry4.Value;
				handledFiles[entry.Path] = entry.WriteTimeUtc;
			}
			bool isDirty = false;
			for (int rootIndex = 0; rootIndex < searchDirs.Count; rootIndex++)
			{
				string searchDir = PathUtils.GetCrossPlatformPath(Path.GetFullPath(searchDirs[rootIndex]));
				if (!Directory.Exists(searchDir) || IsAlreadyScanned(searchDir))
				{
					continue;
				}
				scannedRoots.Add(searchDir);
				foreach (string current in DesignerIOUtils.EnumerateFilesNoLinks(searchDir, "*.ovdf"))
				{
					string file = PathUtils.GetCrossPlatformPath(Path.GetFullPath(current));
					diskFiles.Add(file);
					visitedFiles.Add(file);
					bool isHandled = false;
					DateTime lastWriteTime = File.GetLastWriteTimeUtc(file);
					if (handledFiles.TryGetValue(file, out var handledTime))
					{
						isHandled = handledTime == lastWriteTime;
					}
					if (!isHandled)
					{
						if (!DesignerUtils.TryReadMetadataFromFile(file, out var type, out var guid) || type == null)
						{
							visitedFiles.Remove(file);
							continue;
						}
						DesignerLocalCacheEntry entry2 = new DesignerLocalCacheEntry(type, file, guid, lastWriteTime);
						Entries[file] = entry2;
						handledFiles[file] = lastWriteTime;
						isDirty = true;
					}
				}
			}
			List<string> entriesToDelete = new List<string>(64);
			foreach (KeyValuePair<string, DesignerLocalCacheEntry> kvp in Entries)
			{
				string path = kvp.Key;
				DesignerLocalCacheEntry entry3 = kvp.Value;
				if (!visitedFiles.Contains(entry3.Path))
				{
					entriesToDelete.Add(path);
				}
			}
			isDirty = isDirty || entriesToDelete.Count > 0;
			for (int i = 0; i < entriesToDelete.Count; i++)
			{
				Entries.Remove(entriesToDelete[i]);
			}
			HasBeenChanged = isDirty;
			return diskFiles;
		}

		private static bool IsAlreadyScanned(string searchDir)
		{
			for (int i = 0; i < scannedRoots.Count; i++)
			{
				if (IsPathInRoot(searchDir, scannedRoots[i]))
				{
					return true;
				}
			}
			return false;
		}

		private static bool IsPathInRoot(string path, string root)
		{
			path = PathUtils.GetCrossPlatformPath(Path.GetFullPath(path)).TrimEnd(new char[1] { '/' });
			root = PathUtils.GetCrossPlatformPath(Path.GetFullPath(root)).TrimEnd(new char[1] { '/' });
			if (!string.Equals(path, root, StringComparison.OrdinalIgnoreCase))
			{
				return path.StartsWith(root + "/", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
	}
}

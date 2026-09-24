using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	[InitializeOnLoad]
	internal static class DesignerLocalCache
	{
		public static readonly string LocalFilePath;

		public static bool HasSavedBinaryData;

		private static DesignerLocalCacheState stateBackingField;

		public static DesignerLocalCacheState State
		{
			get
			{
				if (stateBackingField != null)
				{
					return stateBackingField;
				}
				stateBackingField = PersistentObject.GetOrCreate<DesignerLocalCacheState>("PERSISTENT:DesignerLocalCache.State");
				bool wasInitialized = stateBackingField.IsInitialized;
				if (stateBackingField.EntriesBlob.Data == null)
				{
					stateBackingField.EntriesBlob = new DesignerLocalCacheEntryBlob(256);
				}
				if (stateBackingField.Entries == null)
				{
					stateBackingField.Entries = new Dictionary<string, DesignerLocalCacheEntry>(128, StringComparer.OrdinalIgnoreCase);
				}
				stateBackingField.InitializeIfNeeded();
				if (wasInitialized)
				{
					stateBackingField.SyncEntriesWithBlob();
				}
				return stateBackingField;
			}
		}

		static DesignerLocalCache()
		{
			LocalFilePath = GetLocalFilePath();
			HasSavedBinaryData = false;
			EditorApplication.quitting += OnExitUnityEditor;
		}

		public static bool TryAddEntryFromFile(string path, out Type type, out DesignerLocalCacheEntry entry)
		{
			type = null;
			entry = default(DesignerLocalCacheEntry);
			path = PathUtils.GetCrossPlatformPath(path);
			if (!File.Exists(path))
			{
				return false;
			}
			if (!DesignerUtils.TryReadMetadataFromFile(path, out type, out var guid))
			{
				return false;
			}
			if (type == null)
			{
				return false;
			}
			entry.Type = type;
			entry.Path = path;
			entry.Guid = guid;
			entry.WriteTimeUtc = File.GetLastWriteTimeUtc(path);
			State.Entries[path] = entry;
			State.HasBeenChanged = true;
			return true;
		}

		public static void UpdateEntry(Type type, string path)
		{
			UpdateEntry(type, path, null, providedGuid: false);
		}

		public static void UpdateEntry(Type type, string path, string guid)
		{
			UpdateEntry(type, path, guid, providedGuid: true);
		}

		internal static void UpdateEntry(Type type, string path, string guid, bool providedGuid)
		{
			if (!File.Exists(path))
			{
				State.Entries.Remove(PathUtils.GetCrossPlatformPath(path));
				State.HasBeenChanged = true;
				return;
			}
			DateTime writeTimeUtc = File.GetLastWriteTimeUtc(path);
			path = PathUtils.GetCrossPlatformPath(path);
			DesignerLocalCacheEntry entry = new DesignerLocalCacheEntry(type, path, providedGuid ? guid : DesignerUtils.TryGetScriptGuid(type), writeTimeUtc);
			State.Entries[path] = entry;
			State.HasBeenChanged = true;
		}

		public static bool TryGetEntry(string path, out DesignerLocalCacheEntry entry)
		{
			return State.Entries.TryGetValue(PathUtils.GetCrossPlatformPath(path), out entry);
		}

		public static void OnExitUnityEditor()
		{
			if (!HasSavedBinaryData)
			{
				State.Save(LocalFilePath);
				HasSavedBinaryData = true;
			}
		}

		internal static string GetLocalFilePath()
		{
			string projectDirectory = Directory.GetParent(Application.dataPath).FullName;
			string libraryDirectory = Path.Combine(projectDirectory, "Library");
			string odinLibraryDirectory = Path.Combine(libraryDirectory, "Odin");
			return Path.Combine(odinLibraryDirectory, "designer_cache.bin");
		}

		public static void RemoveEntry(Type type)
		{
			if (type == null)
			{
				return;
			}
			bool removed = false;
			List<string> entriesToRemove = new List<string>();
			foreach (KeyValuePair<string, DesignerLocalCacheEntry> kvp in State.Entries)
			{
				if (kvp.Value.Type == type)
				{
					entriesToRemove.Add(kvp.Key);
				}
			}
			for (int i = 0; i < entriesToRemove.Count; i++)
			{
				removed |= State.Entries.Remove(entriesToRemove[i]);
			}
			if (removed)
			{
				State.HasBeenChanged = true;
			}
		}

		public static void RemoveEntry(string path)
		{
			path = PathUtils.GetCrossPlatformPath(path);
			if (State.Entries.Remove(path))
			{
				State.HasBeenChanged = true;
			}
		}
	}
}

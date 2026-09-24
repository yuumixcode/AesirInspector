using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	[InitializeOnLoad]
	public static class WorkItemResultCountCache
	{
		[StructLayout(LayoutKind.Explicit)]
		public struct ItemWorkEntryCount
		{
			[FieldOffset(0)]
			public Guid Guid;

			[FieldOffset(16)]
			public uint Count;

			public ItemWorkEntryCount(Guid guid, uint count)
			{
				Guid = guid;
				Count = count;
			}
		}

		private static readonly object lookup_LOCK;

		private static bool isDirty;

		private static Dictionary<Guid, uint> lookup;

		private static Thread loadingThread;

		private static readonly string cacheFilePath;

		private static EditorPrefString lastCleanedDateTimePref_backing;

		private static BackgroundTaskHandle backgroundCleaningTask;

		private static EditorPrefString LastCleanedDateTimePref
		{
			get
			{
				if (lastCleanedDateTimePref_backing == null)
				{
					lastCleanedDateTimePref_backing = new EditorPrefString("OdinValidator_WorkItemResultCountCache_LastCleanedDateTime", GetDateTimeNowAsBinaryString());
				}
				return lastCleanedDateTimePref_backing;
			}
		}

		private static DateTime GetLastCleanedDateTime()
		{
			string binaryString = LastCleanedDateTimePref.Value;
			if (long.TryParse(binaryString, out var binary))
			{
				return DateTime.FromBinary(binary);
			}
			return DateTime.MinValue;
		}

		private static string GetDateTimeNowAsBinaryString()
		{
			return DateTime.Now.ToBinary().ToString("D", CultureInfo.InvariantCulture);
		}

		static WorkItemResultCountCache()
		{
			lookup_LOCK = new object();
			isDirty = false;
			cacheFilePath = Application.persistentDataPath.TrimEnd('/', '\\') + "/Odin Validator/WorkItemResultCountCache.data";
			loadingThread = new Thread(LoadCacheThread);
			loadingThread.Name = "WorkItemResultCountCache loader";
			loadingThread.IsBackground = true;
			loadingThread.Priority = System.Threading.ThreadPriority.BelowNormal;
			loadingThread.Start();
			AssemblyReloadEvents.beforeAssemblyReload += OnReload;
		}

		private static void OnReload()
		{
			try
			{
				if (loadingThread != null)
				{
					loadingThread.Abort();
					loadingThread = null;
				}
			}
			catch (Exception)
			{
			}
			SaveCacheIfDirty();
		}

		public unsafe static void SaveCacheIfDirty()
		{
			EnsureLoaded();
			lock (lookup_LOCK)
			{
				if (!isDirty)
				{
					return;
				}
				if (lookup != null)
				{
					byte[] bytes = new byte[lookup.Count * 20];
					fixed (byte* ptrBase = bytes)
					{
						ItemWorkEntryCount* ptr = (ItemWorkEntryCount*)ptrBase;
						foreach (KeyValuePair<Guid, uint> item in lookup.GFIterator())
						{
							*(ptr++) = new ItemWorkEntryCount(item.Key, item.Value);
						}
					}
					string dir = Path.GetDirectoryName(cacheFilePath);
					Directory.CreateDirectory(dir);
					File.WriteAllBytes(cacheFilePath, bytes);
				}
				isDirty = false;
			}
		}

		private unsafe static void LoadCacheThread()
		{
			try
			{
				if (!File.Exists(cacheFilePath))
				{
					return;
				}
				byte[] bytes = File.ReadAllBytes(cacheFilePath);
				if (bytes.Length % 20 != 0)
				{
					Debug.LogWarning($"WorkItemResultCountCache data file '{cacheFilePath}' was corrupted/had the wrong number of bytes '{bytes.Length}' on load (not divisible by 20). Cache data will be reset.");
					File.Delete(cacheFilePath);
					return;
				}
				int count = bytes.Length / 20;
				Dictionary<Guid, uint> tempLookup = new Dictionary<Guid, uint>();
				fixed (byte* ptrBase = bytes)
				{
					ItemWorkEntryCount* ptr = (ItemWorkEntryCount*)ptrBase;
					for (int i = 0; i < count; i++)
					{
						tempLookup.Add(ptr->Guid, ptr->Count);
						ptr++;
					}
				}
				lookup = tempLookup;
			}
			finally
			{
				loadingThread = null;
				Thread saveSometimesThread = new Thread(SaveCacheSometimesThread);
				saveSometimesThread.Name = "WorkItemResultCountCache background saver";
				saveSometimesThread.IsBackground = true;
				saveSometimesThread.Priority = System.Threading.ThreadPriority.Lowest;
				saveSometimesThread.Start();
			}
		}

		private static void SaveCacheSometimesThread()
		{
			while (true)
			{
				Thread.Sleep(30000);
				SaveCacheIfDirty();
			}
		}

		private static void EnsureLoaded()
		{
			try
			{
				if (loadingThread != null)
				{
					loadingThread.Join();
				}
			}
			catch (NullReferenceException)
			{
			}
		}

		public static bool TryGetLastWorkItemResultCount(string guid, out uint count)
		{
			if (string.IsNullOrWhiteSpace(guid))
			{
				count = 0u;
				return false;
			}
			try
			{
				return TryGetLastWorkItemResultCount(new Guid(guid), out count);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			count = 0u;
			return false;
		}

		public static bool TryGetLastWorkItemResultCount(Guid guid, out uint count)
		{
			EnsureLoaded();
			lock (lookup_LOCK)
			{
				count = 0u;
				return lookup != null && lookup.TryGetValue(guid, out count);
			}
		}

		public static void RegisterWorkItemResultCount(string guid, uint count)
		{
			if (!string.IsNullOrWhiteSpace(guid) && Guid.TryParse(guid, out var actualGuid))
			{
				RegisterWorkItemResultCount(actualGuid, count);
			}
		}

		public static void RegisterWorkItemResultCount(Guid guid, uint count)
		{
			EnsureLoaded();
			lock (lookup_LOCK)
			{
				if (lookup == null)
				{
					lookup = new Dictionary<Guid, uint>();
				}
				if (!lookup.TryGetValue(guid, out var oldCount) || count != oldCount)
				{
					lookup[guid] = count;
					isDirty = true;
				}
			}
			if (backgroundCleaningTask == null && (DateTime.Now - GetLastCleanedDateTime()).TotalHours >= 24.0)
			{
				backgroundCleaningTask = BackgroundTaskRunner.StartTask("Cleaning WorkItemResultCountCache", BackgroundCleaningTask());
			}
		}

		public static void ForceStartBackgroundCleaningTask()
		{
			if (backgroundCleaningTask != null)
			{
				backgroundCleaningTask.Kill();
			}
			backgroundCleaningTask = BackgroundTaskRunner.StartTask("Cleaning WorkItemResultCountCache", BackgroundCleaningTask());
		}

		private static IEnumerator BackgroundCleaningTask()
		{
			EnsureLoaded();
			if (lookup == null)
			{
				yield break;
			}
			Guid[] guidsToCheck;
			lock (lookup_LOCK)
			{
				guidsToCheck = lookup.Keys.ToArray();
			}
			for (int i = 0; i < guidsToCheck.Length; i++)
			{
				Guid guid = guidsToCheck[i];
				string guidStr = guid.ToString("N").ToLower();
				string assetPath = AssetDatabase.GUIDToAssetPath(guidStr);
				if (string.IsNullOrEmpty(assetPath))
				{
					lock (lookup_LOCK)
					{
						lookup.Remove(guid);
						isDirty = true;
					}
				}
				yield return null;
			}
			LastCleanedDateTimePref.Value = GetDateTimeNowAsBinaryString();
			backgroundCleaningTask = null;
		}
	}
}

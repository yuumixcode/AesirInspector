using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Persistent Context cache object.
	/// </summary>
	[InitializeOnLoad]
	public class PersistentContextCache
	{
		private static class CachePurger
		{
			private static readonly List<KeyValuePair<int, GlobalPersistentContext>> buffer = new List<KeyValuePair<int, GlobalPersistentContext>>();

			private static double lastUpdate;

			private static IEnumerator purger;

			public static void Run()
			{
				if (purger != null)
				{
					double start = EditorApplication.timeSinceStartup;
					do
					{
						if (!purger.MoveNext())
						{
							EndPurge();
							break;
						}
					}
					while (EditorApplication.timeSinceStartup - start < 0.004999999888241291);
				}
				else if (EditorApplication.timeSinceStartup - lastUpdate > 1.0)
				{
					lastUpdate = EditorApplication.timeSinceStartup;
					if (Instance.CacheSize > Instance.MaxCacheByteSize)
					{
						int count = (Instance.CacheSize - Instance.MaxCacheByteSize) / (Instance.CacheSize / Instance.EntryCount) + 1;
						purger = Purge(count);
					}
				}
			}

			private static void EndPurge()
			{
				if (purger != null)
				{
					purger = null;
					if (buffer != null)
					{
						buffer.Clear();
					}
					lastUpdate = EditorApplication.timeSinceStartup;
				}
			}

			private static IEnumerator Purge(int count)
			{
				_ = EditorApplication.timeSinceStartup;
				long newest = DateTime.Now.Ticks;
				for (int i = 0; i < Instance.EntryCount; i++)
				{
					KeyValuePair<ContextKey, GlobalPersistentContext> entry = Instance.cache.Get(i);
					bool added = false;
					if (entry.Value == null)
					{
						buffer.Insert(0, new KeyValuePair<int, GlobalPersistentContext>(i, entry.Value));
					}
					else if (entry.Value.TimeStamp < newest)
					{
						for (int j = 0; j < buffer.Count; j++)
						{
							if (buffer[j].Value != null && buffer[j].Value.TimeStamp >= entry.Value.TimeStamp)
							{
								if (buffer.Count >= count)
								{
									buffer[buffer.Count - 1] = new KeyValuePair<int, GlobalPersistentContext>(i, entry.Value);
								}
								else
								{
									buffer.Insert(j, new KeyValuePair<int, GlobalPersistentContext>(i, entry.Value));
								}
								break;
							}
						}
					}
					if (!added && buffer.Count < count)
					{
						buffer.Add(new KeyValuePair<int, GlobalPersistentContext>(i, entry.Value));
						added = true;
					}
					if (added)
					{
						GlobalPersistentContext val = buffer[buffer.Count - 1].Value;
						if (val != null)
						{
							newest = val.TimeStamp;
						}
					}
					yield return null;
				}
				foreach (KeyValuePair<int, GlobalPersistentContext> i2 in buffer.OrderByDescending((KeyValuePair<int, GlobalPersistentContext> e) => e.Key))
				{
					Instance.cache.RemoveAt(i2.Key);
					yield return null;
				}
			}
		}

		private static readonly object instance_LOCK;

		private static PersistentContextCache instance;

		private const int MAX_CACHE_SIZE_UPPER_LIMIT = 1000000;

		private static readonly string tempCacheFilename;

		private const int defaultApproximateSizePerEntry = 50;

		private static bool configsLoaded;

		private static bool internalEnableCaching;

		private static int internalMaxCacheByteSize;

		private static bool internalWriteToFile;

		[NonSerialized]
		private bool isInitialized;

		[NonSerialized]
		private DateTime lastSave = DateTime.MinValue;

		private int approximateSizePerEntry;

		[NonSerialized]
		private IndexedDictionary cache = new IndexedDictionary();

		public static PersistentContextCache Instance
		{
			get
			{
				if (instance == null)
				{
					lock (instance_LOCK)
					{
						if (instance == null)
						{
							instance = new PersistentContextCache();
						}
					}
				}
				return instance;
			}
		}

		/// <summary>
		/// Estimated cache size in bytes.
		/// </summary>
		public int CacheSize => ((approximateSizePerEntry > 0) ? approximateSizePerEntry : 50) * EntryCount;

		/// <summary>
		/// The current number of context entries in the cache.
		/// </summary>
		public int EntryCount => cache.Count;

		/// <summary>
		/// If <c>true</c> then persistent context is disabled entirely.
		/// </summary>
		[ShowInInspector]
		public bool EnableCaching
		{
			get
			{
				LoadConfigs();
				return internalEnableCaching;
			}
			set
			{
				internalEnableCaching = value;
				EditorPrefs.SetBool("PersistentContextCache.EnableCaching", value);
			}
		}

		/// <summary>
		/// If <c>true</c> the context will be saved to a file in the temp directory.
		/// </summary>
		[EnableIf("EnableCaching")]
		[ShowInInspector]
		public bool WriteToFile
		{
			get
			{
				LoadConfigs();
				return internalWriteToFile;
			}
			set
			{
				internalWriteToFile = value;
				EditorPrefs.SetBool("PersistentContextCache.WriteToFile", value);
			}
		}

		/// <summary>
		/// The max size of the cache in bytes.
		/// </summary>
		[CustomValueDrawer("DrawCacheSize")]
		[EnableIf("EnableCaching")]
		[ShowInInspector]
		[SuffixLabel("KB", false, Overlay = true)]
		public int MaxCacheByteSize
		{
			get
			{
				LoadConfigs();
				return internalMaxCacheByteSize;
			}
			private set
			{
				internalMaxCacheByteSize = value;
				EditorPrefs.SetInt("PersistentContextCache.MaxCacheByteSize", value);
			}
		}

		[ReadOnly]
		[ShowInInspector]
		[FilePath]
		private string CacheFileLocation
		{
			get
			{
				return Path.Combine(SirenixAssetPaths.OdinTempPath, tempCacheFilename).Replace("\\", "/");
			}
			set
			{
			}
		}

		[ReadOnly]
		[SuffixLabel("$CurrentCacheSizeSuffix", false, Overlay = true)]
		[ProgressBar(0.0, 100.0, 0.15f, 0.47f, 0.74f)]
		[ShowInInspector]
		private int CurrentCacheSize
		{
			get
			{
				LoadConfigs();
				return (int)((float)CacheSize / (float)MaxCacheByteSize * 100f);
			}
		}

		private string CurrentCacheSizeSuffix => StringUtilities.NicifyByteSize(CacheSize) + " / " + StringUtilities.NicifyByteSize(MaxCacheByteSize);

		private PersistentContextCache()
		{
		}

		static PersistentContextCache()
		{
			instance_LOCK = new object();
			tempCacheFilename = "PersistentContextCache_v3.cache";
			configsLoaded = false;
			UnityEditorEventUtility.DelayAction(delegate
			{
				Instance.EnsureIsInitialized();
			});
		}

		private void EnsureIsInitialized()
		{
			if (!isInitialized)
			{
				isInitialized = true;
				EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.update, new EditorApplication.CallbackFunction(UpdateCallback));
				EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, new EditorApplication.CallbackFunction(UpdateCallback));
				AppDomain.CurrentDomain.DomainUnload -= OnDomainUnload;
				AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
				EditorApplication.playmodeStateChanged = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.playmodeStateChanged, new EditorApplication.CallbackFunction(OnPlaymodeChanged));
				EditorApplication.playmodeStateChanged = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.playmodeStateChanged, new EditorApplication.CallbackFunction(OnPlaymodeChanged));
				LoadCache();
			}
		}

		private void OnPlaymodeChanged()
		{
			DateTime now = DateTime.Now;
			if (now - lastSave > TimeSpan.FromSeconds(1.0))
			{
				lastSave = now;
				SaveCache();
			}
		}

		private static string FormatSize(int size)
		{
			if (size <= 1000000)
			{
				if (size <= 1000)
				{
					return size + " bytes";
				}
				return size / 1000 + " kB";
			}
			return size / 1000000 + " MB";
		}

		private static void LoadConfigs()
		{
			if (!configsLoaded)
			{
				internalEnableCaching = EditorPrefs.GetBool("PersistentContextCache.EnableCaching", defaultValue: true);
				internalMaxCacheByteSize = EditorPrefs.GetInt("PersistentContextCache.MaxCacheByteSize", 1000000);
				internalWriteToFile = EditorPrefs.GetBool("PersistentContextCache.WriteToFile", defaultValue: true);
				configsLoaded = true;
			}
		}

		private static void UpdateCallback()
		{
			CachePurger.Run();
		}

		internal GlobalPersistentContext<TValue> GetContext<TValue>(int key1, int key2, int key3, int key4, int key5, out bool isNew)
		{
			ContextKey key6 = new ContextKey(key1, key2, key3, key4, key5);
			return TryGetContext<TValue>(key6, out isNew);
		}

		private int DrawCacheSize(int value, GUIContent label)
		{
			value /= 1000;
			value = SirenixEditorFields.DelayedIntField("Max Cache Size", value);
			value = Mathf.Clamp(value, 1, 1000000);
			return value * 1000;
		}

		private void OnDomainUnload(object sender, EventArgs e)
		{
			SaveCache();
		}

		[Button(ButtonSizes.Medium)]
		[ButtonGroup("_DefaultGroup", 0f)]
		[EnableIf("EnableCaching")]
		private void LoadCache()
		{
			string filePath = Path.Combine(SirenixAssetPaths.OdinTempPath, tempCacheFilename).Replace("\\", "/");
			FileInfo file = new FileInfo(filePath);
			try
			{
				approximateSizePerEntry = 50;
				if (file.Exists)
				{
					using (FileStream stream = file.OpenRead())
					{
						DeserializationContext context = new DeserializationContext();
						context.Config.DebugContext.LoggingPolicy = LoggingPolicy.Silent;
						context.Config.DebugContext.ErrorHandlingPolicy = ErrorHandlingPolicy.Resilient;
						cache = Sirenix.Serialization.SerializationUtility.DeserializeValue<IndexedDictionary>(stream, DataFormat.Binary, new List<UnityEngine.Object>(), context);
						if (cache == null)
						{
							cache = new IndexedDictionary();
						}
					}
					if (EntryCount > 0)
					{
						approximateSizePerEntry = (int)(file.Length / EntryCount);
					}
				}
				else
				{
					cache.Clear();
				}
			}
			catch (Exception exception)
			{
				cache = new IndexedDictionary();
				Debug.LogError("Exception happened when loading Persistent Context from file.");
				Debug.LogException(exception);
			}
		}

		[EnableIf("EnableCaching")]
		[Button(ButtonSizes.Medium)]
		[ButtonGroup("_DefaultGroup", 0f)]
		private void SaveCache()
		{
			if (!WriteToFile || !EnableCaching)
			{
				return;
			}
			try
			{
				approximateSizePerEntry = 50;
				string file = Path.Combine(SirenixAssetPaths.OdinTempPath, tempCacheFilename).Replace("\\", "/");
				FileInfo info = new FileInfo(file);
				if (cache.Count == 0)
				{
					if (info.Exists)
					{
						DeleteCache();
					}
					return;
				}
				if (!Directory.Exists(SirenixAssetPaths.OdinTempPath))
				{
					Directory.CreateDirectory(SirenixAssetPaths.OdinTempPath);
				}
				using (FileStream stream = info.OpenWrite())
				{
					Sirenix.Serialization.SerializationUtility.SerializeValue(cache, stream, DataFormat.Binary, out var unityReferences);
					if (unityReferences != null && unityReferences.Count > 0)
					{
						Debug.LogError("Cannot reference UnityEngine Objects with PersistentContext.");
					}
				}
				approximateSizePerEntry = (int)(info.Length / EntryCount);
			}
			catch (Exception exception)
			{
				Debug.LogError("Exception happened when saving Persistent Context to file.");
				Debug.LogException(exception);
			}
		}

		/// <summary>
		/// Delete the persistent cache file.
		/// </summary>
		[EnableIf("EnableCaching")]
		[ButtonGroup("_DefaultGroup", 0f)]
		[Button(ButtonSizes.Medium)]
		public void DeleteCache()
		{
			approximateSizePerEntry = 50;
			cache.Clear();
			string path = Path.Combine(SirenixAssetPaths.OdinTempPath, tempCacheFilename).Replace("\\", "/");
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}

		private GlobalPersistentContext<TValue> TryGetContext<TValue>(ContextKey key, out bool isNew)
		{
			EnsureIsInitialized();
			if (EnableCaching && cache.TryGetValue(key, out var context) && context is GlobalPersistentContext<TValue>)
			{
				isNew = false;
				return (GlobalPersistentContext<TValue>)context;
			}
			isNew = true;
			GlobalPersistentContext<TValue> c = GlobalPersistentContext<TValue>.Create();
			cache[key] = c;
			return c;
		}
	}
}

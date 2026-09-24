using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sirenix.OdinInspector.Editor.Internal
{
	[InitializeOnLoad]
	internal static class OVDFFileWatcher
	{
		public sealed class DesignerFileRecord
		{
			public DesignerFile File;

			public Type Type;

			public bool HasIssues;

			public bool IsActive;

			public bool IsReadOnly;

			public string OverridingPath;

			public int RootIndex;

			public string RootPath;
		}

		private enum JobKind : byte
		{
			Parse,
			Delete
		}

		private enum PendingKind : byte
		{
			Parse,
			Delete
		}

		private struct Job
		{
			public JobKind Kind;

			public string Path;
		}

		private struct PendingResult
		{
			public PendingKind Kind;

			public string Path;

			public OVDFParser.OVDFFile OVDFFile;
		}

		private const double MainThreadBudgetMs = 1.0;

		private const string SearchPatternSuffix = ".ovdf";

		private static readonly Dictionary<Type, DesignerFile> designerFiles;

		private static readonly Dictionary<string, Type> pathToTypeMap;

		private static readonly List<DesignerFile> filesWithIssues;

		private static readonly Dictionary<string, OVDFParser.OVDFFile> parsedFiles;

		private static readonly Dictionary<string, DesignerFileRecord> fileRecords;

		private static readonly List<string> searchRoots;

		private static readonly object JobsLock;

		private static readonly Queue<Job> jobs;

		private static readonly AutoResetEvent workAvailable;

		private static readonly object PendingLock;

		private static readonly Queue<PendingResult> pending;

		private static readonly object filesToUpdateLock;

		private static readonly List<string> filesToAdd;

		private static readonly List<string> filesToRemove;

		private static readonly List<FileSystemWatcher> watchers;

		private static Thread parseThread;

		private static volatile bool stopRequested;

		public static IReadOnlyDictionary<Type, DesignerFile> DesignerFiles => designerFiles;

		public static IReadOnlyDictionary<string, Type> PathToTypeMap => pathToTypeMap;

		public static IReadOnlyList<DesignerFile> FilesWithIssues => filesWithIssues;

		public static IReadOnlyDictionary<string, OVDFParser.OVDFFile> ParsedFiles => parsedFiles;

		public static IReadOnlyDictionary<string, DesignerFileRecord> FileRecords => fileRecords;

		public static IReadOnlyList<string> SearchRoots => searchRoots;

		public static event Action<Type, DesignerFile, bool> FileParsed;

		public static event Action<string> FileDeleted;

		public static event Action<DesignerFile, bool> FileIssuesChanged;

		public static event Action<string> RootFolderPathChanged;

		public static event Action<string> InitialScanCompleted;

		static OVDFFileWatcher()
		{
			designerFiles = new Dictionary<Type, DesignerFile>();
			pathToTypeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
			filesWithIssues = new List<DesignerFile>();
			parsedFiles = new Dictionary<string, OVDFParser.OVDFFile>(StringComparer.OrdinalIgnoreCase);
			fileRecords = new Dictionary<string, DesignerFileRecord>(StringComparer.OrdinalIgnoreCase);
			searchRoots = new List<string>();
			JobsLock = new object();
			jobs = new Queue<Job>();
			workAvailable = new AutoResetEvent(initialState: false);
			PendingLock = new object();
			pending = new Queue<PendingResult>(4096);
			filesToUpdateLock = new object();
			filesToAdd = new List<string>(256);
			filesToRemove = new List<string>(256);
			watchers = new List<FileSystemWatcher>();
			EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, new EditorApplication.CallbackFunction(Update));
			EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, new EditorApplication.CallbackFunction(UpdateFiles));
			Initialize();
		}

		public static DesignerFile GetOrCreate(Type targetType)
		{
			if (designerFiles.TryGetValue(targetType, out var file))
			{
				return file;
			}
			return GetDefaultFile(targetType);
		}

		public static DesignerFile GetDefaultFile(Type targetType)
		{
			return new DesignerFile
			{
				Path = DesignerUtils.CreateFilePathFromType(targetType)
			};
		}

		public static bool TryGetFileRecord(string path, out DesignerFileRecord record)
		{
			path = PathUtils.GetCrossPlatformPath(path);
			return fileRecords.TryGetValue(path, out record);
		}

		public static bool TryGetActiveFileRecord(Type type, out DesignerFileRecord record)
		{
			record = null;
			if (!designerFiles.TryGetValue(type, out var file))
			{
				return false;
			}
			return TryGetFileRecord(file.Path, out record);
		}

		public static bool IsReadOnly(string path)
		{
			return !CanWriteToPath(path);
		}

		public static void RegisterSavedFile(Type type, string path)
		{
			path = PathUtils.GetCrossPlatformPath(Path.GetFullPath(path));
			DesignerLocalCache.UpdateEntry(type, path);
			TryParseFile(path, out var ovdfFile);
			ApplyParse(path, ovdfFile);
		}

		public static void Remove(DesignerFile file)
		{
			ApplyDelete(file.Path);
			DesignerLocalCache.RemoveEntry(file.Path);
		}

		public static bool TryDeleteFile(string path, out string errorMessage)
		{
			errorMessage = null;
			if (string.IsNullOrEmpty(path))
			{
				errorMessage = "No file path was provided.";
				return false;
			}
			try
			{
				path = NormalizePath(path);
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return false;
			}
			if (Directory.Exists(path))
			{
				errorMessage = "The selected path is a directory, not an OVDF file.";
				return false;
			}
			if (!File.Exists(path))
			{
				ApplyDelete(path);
				return true;
			}
			if (!string.Equals(Path.GetExtension(path), ".ovdf", StringComparison.OrdinalIgnoreCase))
			{
				errorMessage = "The selected file is not an OVDF file.";
				return false;
			}
			if (!TryGetRootInfo(path, out var _, out var _))
			{
				errorMessage = "The selected file is not inside any configured OVDF search root.";
				return false;
			}
			if (!fileRecords.ContainsKey(path))
			{
				errorMessage = "The selected file is not known to the Visual Designer.";
				return false;
			}
			try
			{
				if (TryGetProjectAssetPath(path, out var assetPath))
				{
					AssetDatabase.MoveAssetToTrash(assetPath);
					AssetDatabase.Refresh();
				}
				else
				{
					File.Delete(path);
				}
			}
			catch (Exception ex2)
			{
				errorMessage = ex2.Message;
				return false;
			}
			if (File.Exists(path))
			{
				errorMessage = "Unity or the file system refused to delete the file. It may be read-only, locked, or part of an immutable package.";
				return false;
			}
			ApplyDelete(path);
			AssetDatabase.Refresh();
			return true;
		}

		public static void UpdateRootFolders()
		{
			Initialize();
		}

		public static void UpdateRootFolder(string newRootFolderPath)
		{
			UpdateRootFolders();
		}

		private static void Initialize()
		{
			if (!Application.isBatchMode && SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null)
			{
				StopParseThread();
				DisposeWatchers();
				ClearState();
				SyncSearchRoots();
				StartParseThread();
				List<string> diskFiles = SyncWithLocalCache();
				InitialScanFiles(diskFiles);
				SetupWatchers();
				OVDFFileWatcher.RootFolderPathChanged?.Invoke(null);
				OVDFFileWatcher.InitialScanCompleted?.Invoke(null);
			}
		}

		private static void ClearState()
		{
			designerFiles.Clear();
			pathToTypeMap.Clear();
			filesWithIssues.Clear();
			parsedFiles.Clear();
			fileRecords.Clear();
			lock (JobsLock)
			{
				jobs.Clear();
			}
			lock (PendingLock)
			{
				pending.Clear();
			}
		}

		private static void SyncSearchRoots()
		{
			searchRoots.Clear();
			searchRoots.AddRange(GlobalConfig<OdinVisualDesignerConfig>.Instance.GetEffectiveSearchRoots());
		}

		private static void StartParseThread()
		{
			stopRequested = false;
			parseThread = new Thread(ParseThreadWork);
			parseThread.IsBackground = true;
			parseThread.Name = "OVDF Parse Thread";
			parseThread.Start();
		}

		private static void StopParseThread()
		{
			if (parseThread != null)
			{
				stopRequested = true;
				workAvailable.Set();
				if (!parseThread.Join(250))
				{
					parseThread.Abort();
				}
				parseThread = null;
				stopRequested = false;
			}
		}

		internal static List<string> SyncWithLocalCache()
		{
			List<string> diskFiles = DesignerLocalCache.State.SyncEntriesWithDiskFiles();
			foreach (KeyValuePair<string, DesignerLocalCacheEntry> entry2 in DesignerLocalCache.State.Entries)
			{
				DesignerLocalCacheEntry entry = entry2.Value;
				if (!(entry.Type == null) && !string.IsNullOrEmpty(entry.Path) && File.Exists(entry.Path) && TryGetRootInfo(entry.Path, out var _, out var _))
				{
					UpsertRecord(entry.Path, entry.Type, hasIssues: false);
				}
			}
			RebuildActiveFiles(null);
			return diskFiles;
		}

		private static void InitialScanFiles(List<string> diskFiles)
		{
			if (diskFiles != null)
			{
				for (int i = 0; i < diskFiles.Count; i++)
				{
					EnqueueJob(diskFiles[i], JobKind.Parse);
				}
			}
		}

		private static void SetupWatchers()
		{
			string savePath = NormalizePath(GlobalConfig<OdinVisualDesignerConfig>.Instance.SavePath);
			List<string> watchedRoots = new List<string>(searchRoots.Count);
			for (int i = 0; i < searchRoots.Count; i++)
			{
				string root = searchRoots[i];
				if (!Directory.Exists(root))
				{
					if (!string.Equals(root, savePath, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					Directory.CreateDirectory(root);
				}
				if (!IsRootAlreadyWatched(root, watchedRoots))
				{
					FileSystemWatcher watcher = new FileSystemWatcher(root, "*.ovdf");
					watcher.IncludeSubdirectories = true;
					watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime;
					watcher.Created += OnFileCreated;
					watcher.Changed += OnFileChanged;
					watcher.Deleted += OnFileDeleted;
					watcher.Renamed += OnFileRenamed;
					watcher.EnableRaisingEvents = true;
					watchers.Add(watcher);
					watchedRoots.Add(root);
				}
			}
		}

		private static bool IsRootAlreadyWatched(string root, List<string> watchedRoots)
		{
			for (int i = 0; i < watchedRoots.Count; i++)
			{
				if (IsPathInRoot(root, watchedRoots[i]))
				{
					return true;
				}
			}
			return false;
		}

		private static void DisposeWatchers()
		{
			for (int i = 0; i < watchers.Count; i++)
			{
				FileSystemWatcher watcher = watchers[i];
				watcher.EnableRaisingEvents = false;
				watcher.Created -= OnFileCreated;
				watcher.Changed -= OnFileChanged;
				watcher.Deleted -= OnFileDeleted;
				watcher.Renamed -= OnFileRenamed;
				watcher.Dispose();
			}
			watchers.Clear();
		}

		private static void EnqueueJob(string path, JobKind kind)
		{
			path = PathUtils.GetCrossPlatformPath(path);
			if (!string.IsNullOrEmpty(path))
			{
				lock (JobsLock)
				{
					jobs.Enqueue(new Job
					{
						Kind = kind,
						Path = path
					});
				}
				workAvailable.Set();
			}
		}

		private static void ParseThreadWork()
		{
			while (!stopRequested)
			{
				bool hasJob = false;
				Job job;
				lock (JobsLock)
				{
					if (jobs.Count > 0)
					{
						job = jobs.Dequeue();
						hasJob = true;
					}
					else
					{
						job = default(Job);
					}
				}
				if (!hasJob)
				{
					workAvailable.WaitOne();
					continue;
				}
				if (job.Kind == JobKind.Delete)
				{
					lock (PendingLock)
					{
						pending.Enqueue(new PendingResult
						{
							Kind = PendingKind.Delete,
							Path = job.Path,
							OVDFFile = null
						});
					}
					continue;
				}
				TryParseFile(job.Path, out var ovdfFile);
				lock (PendingLock)
				{
					pending.Enqueue(new PendingResult
					{
						Kind = PendingKind.Parse,
						Path = job.Path,
						OVDFFile = ovdfFile
					});
				}
			}
		}

		private static void Update()
		{
			double startMs = EditorApplication.timeSinceStartup * 1000.0;
			while (EditorApplication.timeSinceStartup * 1000.0 - startMs < 1.0)
			{
				PendingResult result;
				lock (PendingLock)
				{
					if (pending.Count == 0)
					{
						break;
					}
					result = pending.Dequeue();
				}
				if (result.Kind == PendingKind.Delete)
				{
					ApplyDelete(result.Path);
				}
				else
				{
					ApplyParse(result.Path, result.OVDFFile);
				}
			}
		}

		private static void UpdateFiles()
		{
			if (filesToAdd.Count == 0 && filesToRemove.Count == 0)
			{
				return;
			}
			lock (filesToUpdateLock)
			{
				if (filesToAdd.Count != 0 || filesToRemove.Count != 0)
				{
					for (int i = 0; i < filesToRemove.Count; i++)
					{
						UpdateFile(filesToRemove[i]);
					}
					filesToRemove.Clear();
					for (int j = 0; j < filesToAdd.Count; j++)
					{
						UpdateFile(filesToAdd[j]);
					}
					filesToAdd.Clear();
					FinalizedInspectorInfoCache.ClearAll();
					DesignerUtils.RefreshInspectorAndEditors();
				}
			}
		}

		private static void UpdateFile(string path)
		{
			path = PathUtils.GetCrossPlatformPath(Path.GetFullPath(path));
			if (!TryGetRootInfo(path, out var _, out var _))
			{
				return;
			}
			if (File.Exists(path))
			{
				if (DesignerUtils.TryReadMetadataFromFile(path, out var type, out var guid) && type != null)
				{
					DesignerLocalCache.UpdateEntry(type, path, guid);
					UpsertRecord(path, type, hasIssues: false);
					RebuildActiveFiles(type);
				}
				EnqueueJob(path, JobKind.Parse);
			}
			else
			{
				DesignerLocalCache.RemoveEntry(path);
				ApplyDelete(path);
			}
		}

		public static OVDFParser.OVDFFile GetOVDFFile(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}
			path = PathUtils.GetCrossPlatformPath(path);
			if (parsedFiles.TryGetValue(path, out var existing))
			{
				return existing;
			}
			if (!File.Exists(path))
			{
				ApplyDelete(path);
				return null;
			}
			TryParseFile(path, out var ovdfFile);
			ApplyParse(path, ovdfFile);
			return ovdfFile;
		}

		private static void ApplyParse(string path, OVDFParser.OVDFFile ovdfFile)
		{
			path = PathUtils.GetCrossPlatformPath(path);
			if (string.IsNullOrEmpty(path))
			{
				return;
			}
			if (ovdfFile != null)
			{
				parsedFiles[path] = ovdfFile;
			}
			else
			{
				parsedFiles.Remove(path);
			}
			if (!File.Exists(path))
			{
				parsedFiles.Remove(path);
				ApplyDelete(path);
				return;
			}
			Type type = DesignerUtils.GetTypeFromFile(path);
			if (type == null && ovdfFile != null && !ovdfFile.Header.Guid.IsNullOrWhitespace())
			{
				type = GetTypeFromScriptGuid(ovdfFile.Header.Guid);
			}
			bool hasDiagnostics = ovdfFile != null && ovdfFile.Diagnostics != null && ovdfFile.Diagnostics.Count > 0;
			bool hasIssues = ovdfFile == null || hasDiagnostics || type == null;
			UpsertRecord(path, type, hasIssues);
			RebuildActiveFiles(type);
			if (type != null)
			{
				DesignerUtils.TryReadMetadataFromFile(path, out var _, out var guid);
				DesignerLocalCache.UpdateEntry(type, path, guid);
			}
			DesignerFile file = new DesignerFile(path);
			OVDFFileWatcher.FileParsed?.Invoke(type, file, hasIssues);
			OVDFFileWatcher.FileIssuesChanged?.Invoke(file, hasIssues);
		}

		private static void UpsertRecord(string path, Type type, bool hasIssues)
		{
			path = PathUtils.GetCrossPlatformPath(path);
			if (!fileRecords.TryGetValue(path, out var record))
			{
				record = new DesignerFileRecord();
				record.File = new DesignerFile(path);
				fileRecords[path] = record;
			}
			record.Type = type;
			record.HasIssues = hasIssues;
			record.IsReadOnly = IsReadOnly(path);
			if (TryGetRootInfo(path, out var rootIndex, out var rootPath))
			{
				record.RootIndex = rootIndex;
				record.RootPath = rootPath;
			}
			else
			{
				record.RootIndex = -1;
				record.RootPath = null;
			}
		}

		private static void RebuildActiveFiles(Type preferredRefreshType)
		{
			Dictionary<Type, string> previousActiveFiles = new Dictionary<Type, string>(designerFiles.Count);
			foreach (KeyValuePair<Type, DesignerFile> kvp in designerFiles)
			{
				previousActiveFiles[kvp.Key] = kvp.Value.Path;
			}
			designerFiles.Clear();
			pathToTypeMap.Clear();
			filesWithIssues.Clear();
			foreach (DesignerFileRecord record in fileRecords.Values)
			{
				record.IsActive = false;
				record.OverridingPath = null;
				record.IsReadOnly = IsReadOnly(record.File.Path);
				if (record.HasIssues)
				{
					filesWithIssues.Add(record.File);
				}
				if (!(record.Type == null))
				{
					pathToTypeMap[record.File.Path] = record.Type;
					if (!designerFiles.TryGetValue(record.Type, out var currentWinner) || ComparePrecedence(record.File.Path, currentWinner.Path) > 0)
					{
						designerFiles[record.Type] = record.File;
					}
					if (record.Type.IsGenericType)
					{
						DesignerRegistry.RegisterGenericVariant(record.Type, validateTypeArgs: false);
					}
				}
			}
			foreach (DesignerFileRecord record2 in fileRecords.Values)
			{
				if (!(record2.Type == null) && designerFiles.TryGetValue(record2.Type, out var activeFile))
				{
					record2.IsActive = string.Equals(activeFile.Path, record2.File.Path, StringComparison.OrdinalIgnoreCase);
					record2.OverridingPath = (record2.IsActive ? null : activeFile.Path);
				}
			}
			HashSet<Type> affectedTypes = new HashSet<Type>();
			if (preferredRefreshType != null)
			{
				affectedTypes.Add(preferredRefreshType);
			}
			foreach (KeyValuePair<Type, string> kvp2 in previousActiveFiles)
			{
				DesignerFile currentFile;
				string currentPath = (designerFiles.TryGetValue(kvp2.Key, out currentFile) ? currentFile.Path : null);
				if (!string.Equals(kvp2.Value, currentPath, StringComparison.OrdinalIgnoreCase))
				{
					affectedTypes.Add(kvp2.Key);
				}
			}
			foreach (KeyValuePair<Type, DesignerFile> kvp3 in designerFiles)
			{
				if (!previousActiveFiles.TryGetValue(kvp3.Key, out var previousPath) || !string.Equals(previousPath, kvp3.Value.Path, StringComparison.OrdinalIgnoreCase))
				{
					affectedTypes.Add(kvp3.Key);
				}
			}
			foreach (Type type in affectedTypes)
			{
				RefreshCachedPatch(type);
			}
		}

		private static int ComparePrecedence(string leftPath, string rightPath)
		{
			TryGetRootInfo(leftPath, out var leftRoot, out var rootPath);
			TryGetRootInfo(rightPath, out var rightRoot, out rootPath);
			int rootCompare = leftRoot.CompareTo(rightRoot);
			if (rootCompare == 0)
			{
				return string.Compare(leftPath, rightPath, StringComparison.OrdinalIgnoreCase);
			}
			return rootCompare;
		}

		private static void RefreshCachedPatch(Type type)
		{
			if (type == null || !TypePatchCache.TryGet(type, out var patch) || patch == null)
			{
				return;
			}
			if (designerFiles.TryGetValue(type, out var file) && File.Exists(file.Path))
			{
				DesignerSerializer.Deserialize(file.Path, patch);
				if (patch.HasEditorVariant)
				{
					patch.EditorVariant.IsOutOfSyncWithFileOnDisk = true;
				}
			}
			else
			{
				TypePatchCache.TryClear(type);
			}
		}

		private static void ApplyDelete(string path)
		{
			path = PathUtils.GetCrossPlatformPath(path);
			if (!string.IsNullOrEmpty(path))
			{
				Type affectedType = null;
				if (fileRecords.TryGetValue(path, out var record))
				{
					affectedType = record.Type;
				}
				fileRecords.Remove(path);
				parsedFiles.Remove(path);
				pathToTypeMap.Remove(path);
				DesignerLocalCache.RemoveEntry(path);
				RebuildActiveFiles(affectedType);
				OVDFFileWatcher.FileDeleted?.Invoke(path);
				OVDFFileWatcher.FileIssuesChanged?.Invoke(new DesignerFile(path), arg2: false);
			}
		}

		private static bool TryParseFile(string path, out OVDFParser.OVDFFile ovdfFile)
		{
			ovdfFile = null;
			try
			{
				string source = File.ReadAllText(path);
				ovdfFile = OVDFParser.Parse(source);
				return true;
			}
			catch
			{
				ovdfFile = null;
				return false;
			}
		}

		private static Type GetTypeFromScriptGuid(string guid)
		{
			if (string.IsNullOrEmpty(guid))
			{
				return null;
			}
			string assetPath = AssetDatabase.GUIDToAssetPath(guid);
			if (string.IsNullOrEmpty(assetPath))
			{
				return null;
			}
			MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
			if (!(monoScript == null))
			{
				return monoScript.GetClass();
			}
			return null;
		}

		private static bool TryGetRootInfo(string path, out int rootIndex, out string rootPath)
		{
			path = NormalizePath(path);
			rootIndex = -1;
			rootPath = null;
			for (int i = 0; i < searchRoots.Count; i++)
			{
				string root = searchRoots[i];
				if (IsPathInRoot(path, root))
				{
					rootIndex = i;
					rootPath = root;
				}
			}
			return rootIndex >= 0;
		}

		private static bool IsPathInRoot(string path, string root)
		{
			path = NormalizePath(path).TrimEnd(new char[1] { '/' });
			root = NormalizePath(root).TrimEnd(new char[1] { '/' });
			if (!string.Equals(path, root, StringComparison.OrdinalIgnoreCase))
			{
				return path.StartsWith(root + "/", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}

		private static string NormalizePath(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return string.Empty;
			}
			return PathUtils.GetCrossPlatformPath(Path.GetFullPath(path));
		}

		private static bool TryGetProjectAssetPath(string filePath, out string assetPath)
		{
			assetPath = FileUtil.GetProjectRelativePath(filePath).Replace('\\', '/');
			if (string.IsNullOrEmpty(assetPath) || assetPath.StartsWith("../") || assetPath.StartsWith("..\\"))
			{
				return false;
			}
			return AssetDatabase.LoadMainAssetAtPath(assetPath) != null;
		}

		private static bool CanWriteToPath(string path)
		{
			path = NormalizePath(path);
			try
			{
				if (File.Exists(path))
				{
					FileAttributes attributes = File.GetAttributes(path);
					if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
					{
						return false;
					}
					using (new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
					{
					}
					return true;
				}
				string directory = (Directory.Exists(path) ? path : Path.GetDirectoryName(path));
				while (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
				{
					directory = Path.GetDirectoryName(directory);
				}
				if (string.IsNullOrEmpty(directory))
				{
					return false;
				}
				string testPath = Path.Combine(directory, ".ovdf_write_test_" + Guid.NewGuid().ToString("N") + ".tmp");
				using (FileStream stream = new FileStream(testPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
				{
					stream.WriteByte(0);
				}
				File.Delete(testPath);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static void OnFileCreated(object sender, FileSystemEventArgs e)
		{
			lock (filesToUpdateLock)
			{
				filesToAdd.Add(e.FullPath);
			}
		}

		private static void OnFileChanged(object sender, FileSystemEventArgs e)
		{
			lock (filesToUpdateLock)
			{
				filesToAdd.Add(e.FullPath);
			}
		}

		private static void OnFileDeleted(object sender, FileSystemEventArgs e)
		{
			lock (filesToUpdateLock)
			{
				filesToRemove.Add(e.FullPath);
			}
		}

		private static void OnFileRenamed(object sender, RenamedEventArgs e)
		{
			lock (filesToUpdateLock)
			{
				filesToRemove.Add(e.OldFullPath);
				filesToAdd.Add(e.FullPath);
			}
		}
	}
}

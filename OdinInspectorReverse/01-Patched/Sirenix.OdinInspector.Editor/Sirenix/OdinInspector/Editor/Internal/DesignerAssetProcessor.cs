using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.Utilities;
using UnityEditor;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class DesignerAssetProcessor : AssetPostprocessor
	{
		internal struct RelevantData
		{
			public DesignerEditor Editor;

			public TypePatch TypePatch;
		}

		public const string EXTENSION_WITH_DOT = ".ovdf";

		public static readonly HashSet<string> HandledPaths = new HashSet<string>();

		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			HandledPaths.Clear();
			bool needsRefresh = ProcessImportedAssets(importedAssets);
			if (ProcessDeletedAssets(deletedAssets) || needsRefresh)
			{
				DesignerPatcher.Reset(null);
				DesignerUtils.RefreshInspectorAndEditors();
			}
		}

		private static bool IsFileRelevant(string path)
		{
			return path.FastEndsWith(".ovdf");
		}

		private static string GetUniversalFullPath(string path)
		{
			return Path.GetFullPath(path).Replace('\\', '/');
		}

		private static bool ProcessImportedAssets(string[] importedAssets)
		{
			bool needsRefresh = false;
			for (int i = 0; i < importedAssets.Length; i++)
			{
				string path = importedAssets[i];
				if (!IsFileRelevant(path))
				{
					continue;
				}
				path = GetUniversalFullPath(path);
				if (!HandledPaths.Add(path))
				{
					continue;
				}
				Type type = DesignerUtils.GetTypeFromFile(path);
				if (type == null || !TypePatchCache.TryGet(type, out var patch))
				{
					continue;
				}
				needsRefresh = true;
				DesignerSerializer.Deserialize(path, patch);
				if (EditorTypePatches.TryGet(type, out var editorPatch))
				{
					bool wantsReload = true;
					if (editorPatch.IsDirty)
					{
						string msg = $"The designer file for the type '{type}' has been modified outside the editor.";
						wantsReload = EditorUtility.DisplayDialog("Odin Visual Designer - File Changed Externally", msg, "Discard Local Changes", "Keep Local Changes");
					}
					if (wantsReload)
					{
						editorPatch.Initialize(patch);
					}
					DesignerEditors.ForceSyncInheritors(editorPatch);
				}
			}
			return needsRefresh;
		}

		private static bool ProcessDeletedAssets(string[] deletedAssets)
		{
			bool needsRefresh = false;
			for (int i = 0; i < deletedAssets.Length; i++)
			{
				string path = deletedAssets[i];
				if (!IsFileRelevant(path))
				{
					continue;
				}
				path = GetUniversalFullPath(path);
				if (!HandledPaths.Add(path) || !OVDFFileWatcher.PathToTypeMap.TryGetValue(path, out var type))
				{
					continue;
				}
				DesignerFile file = new DesignerFile(path);
				OVDFFileWatcher.Remove(file);
				if (!TypePatchCache.TryGet(type, out var patch))
				{
					continue;
				}
				needsRefresh = true;
				if (EditorTypePatches.TryGet(type, out var editorPatch))
				{
					patch.SelfPatches?.Clear();
					patch.GroupPatches?.Clear();
					patch.PropertyPatches?.Clear();
					bool discardChanges = true;
					if (editorPatch.IsDirty)
					{
						string msg = $"The designer file for the type '{type}' has been removed outside the editor.";
						discardChanges = EditorUtility.DisplayDialog("Odin Visual Designer - File Removed Externally", msg, "Discard Local Changes", "Keep Local Changes");
					}
					if (discardChanges)
					{
						editorPatch.DiscardChanges();
					}
					patch.EditorVariant = editorPatch;
					EditorTypePatches.State.Patches[type] = editorPatch;
					DesignerEditors.ForceSyncInheritors(editorPatch);
				}
				else
				{
					TypePatchCache.Remove(type);
				}
			}
			return needsRefresh;
		}
	}
}

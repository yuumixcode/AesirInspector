using System;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerEditors
	{
		public static Dictionary<EditorKey, DesignerEditor> Editors = new Dictionary<EditorKey, DesignerEditor>();

		internal static readonly List<DesignerEditor> SnapshotBuffer = new List<DesignerEditor>(32);

		public static DesignerEditor Get(Type type, object instance, string path)
		{
			if (!DesignerUtils.CanTypeBeDesigned(type))
			{
				return null;
			}
			instance = null;
			path = null;
			EditorKey key = new EditorKey(type, instance, path);
			if (Editors.TryGetValue(key, out var editor))
			{
				if (editor.TypePatch == null)
				{
					editor.TypePatch = EditorTypePatches.Get(type);
					editor.ForceSync = true;
				}
				if (editor.Context == null)
				{
					editor.Context = new DesignerEditorContext(editor);
				}
				return editor;
			}
			editor = new DesignerEditor(key)
			{
				SyncTag = 0uL,
				ForceSync = true,
				Instance = instance,
				InstancePath = path,
				TypePatch = EditorTypePatches.Get(type)
			};
			editor.Context = new DesignerEditorContext(editor);
			return Editors[key] = editor;
		}

		public static DesignerEditor Get(InspectorProperty property)
		{
			if (property == null)
			{
				return null;
			}
			Type type = ((property.ValueEntry != null) ? property.ValueEntry.TypeOfValue : property.Info?.TypeOfValue);
			if (type == null)
			{
				return null;
			}
			object rootValue = property.Tree?.RootProperty?.ValueEntry?.WeakSmartValue;
			if (rootValue != null)
			{
				string path = (property.IsTreeRoot ? null : property.Path);
				return Get(type, rootValue, path);
			}
			return Get(type, null, null);
		}

		public static DesignerEditor GetForOwner(InspectorProperty property)
		{
			if (property == null)
			{
				return null;
			}
			InspectorProperty owner = (property.IsTreeRoot ? property : property.ParentValueProperty);
			return Get(owner);
		}

		public static void Remove(DesignerEditor editor)
		{
			if (editor != null)
			{
				if (editor.Context != null)
				{
					editor.Context.SelectionTree?.Dispose();
					editor.Context.SelectionTree = null;
				}
				Editors.Remove(editor.Key);
			}
		}

		public static List<DesignerEditor> SnapshotCurrentEditorsTmp()
		{
			SnapshotBuffer.Clear();
			foreach (KeyValuePair<EditorKey, DesignerEditor> keyValuePair in Editors)
			{
				SnapshotBuffer.Add(keyValuePair.Value);
			}
			return SnapshotBuffer;
		}

		public static void ForceSyncInheritors(EditorTypePatch editorTypePatch)
		{
			foreach (KeyValuePair<EditorKey, DesignerEditor> editor2 in Editors)
			{
				DesignerEditor editor = editor2.Value;
				if (editor != null && (editor.TypePatch == editorTypePatch || editor.TypePatch.InheritsFrom(editorTypePatch)))
				{
					editor.ForceSync = true;
				}
			}
		}

		public static void ForceSyncAll()
		{
			foreach (KeyValuePair<EditorKey, DesignerEditor> editor2 in Editors)
			{
				DesignerEditor editor = editor2.Value;
				if (editor != null)
				{
					editor.ForceSync = true;
				}
			}
		}
	}
}

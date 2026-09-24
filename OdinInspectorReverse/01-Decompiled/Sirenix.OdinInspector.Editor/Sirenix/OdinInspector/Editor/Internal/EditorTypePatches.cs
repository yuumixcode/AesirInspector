using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class EditorTypePatches
	{
		public const string STATE_KEY = "ODIN-VISUAL-DESIGNER$EditorTypePatchesState.InstanceId";

		private static EditorTypePatchesState stateBackingField;

		public static readonly Dictionary<EditorTypePatch, HashSet<EditorWindow>> EditorTypePatchRetainedInEditorWindows = new Dictionary<EditorTypePatch, HashSet<EditorWindow>>(32);

		public static EditorTypePatchesState State
		{
			get
			{
				if (stateBackingField != null)
				{
					return stateBackingField;
				}
				stateBackingField = PersistentObject.GetOrCreate<EditorTypePatchesState>("ODIN-VISUAL-DESIGNER$EditorTypePatchesState.InstanceId");
				stateBackingField.Deserialize();
				if (stateBackingField.Patches == null)
				{
					stateBackingField.Patches = new Dictionary<Type, EditorTypePatch>(8);
				}
				return stateBackingField;
			}
		}

		public static EditorTypePatch Get(Type type)
		{
			if (type == null)
			{
				return null;
			}
			if (TryGet(type, out var result))
			{
				return result;
			}
			TypePatch typePatch = TypePatchCache.Get(type);
			if (typePatch == null)
			{
				return null;
			}
			result = EditorTypePatch.CreateFromTypePatch(typePatch);
			EditorUtility.ClearDirty(result);
			State.Patches[type] = result;
			return result;
		}

		public static bool TryGet(Type type, out EditorTypePatch patch)
		{
			return State.Patches.TryGetValue(type, out patch);
		}

		public static bool Exists(Type type)
		{
			return State.Patches.ContainsKey(type);
		}

		public static void Remove(Type type)
		{
			if (TryGet(type, out var patch))
			{
				State.Patches.Remove(type);
				UnityEngine.Object.DestroyImmediate(patch);
			}
		}

		public static void Remove(EditorTypePatch patch)
		{
			if (!(patch == null))
			{
				State.Patches.Remove(patch.TargetType);
				UnityEngine.Object.DestroyImmediate(patch);
			}
		}

		public static HashSet<EditorWindow> GetEditorWindowsRetaining(EditorTypePatch patch)
		{
			if (patch == null)
			{
				return null;
			}
			if (!EditorTypePatchRetainedInEditorWindows.TryGetValue(patch, out var set))
			{
				set = (EditorTypePatchRetainedInEditorWindows[patch] = new HashSet<EditorWindow>());
			}
			return set;
		}
	}
}

using System;
using System.Collections.Generic;
using Sirenix.Reflection.Editor;
using Sirenix.Serialization;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	[Serializable]
	internal class EditorTypePatchesState : ScriptableObject, ISerializationCallbackReceiver
	{
		[Serializable]
		private struct EditorTypePatchSerialized
		{
			public string TypeBinding;

			public string TypeGuid;

			public OdinEntityId PatchEntityId;

			public EditorTypePatch ToEditorTypePatch()
			{
				return PatchEntityId.ToObject() as EditorTypePatch;
			}
		}

		[NonSerialized]
		public Dictionary<Type, EditorTypePatch> Patches = new Dictionary<Type, EditorTypePatch>();

		[SerializeField]
		private List<EditorTypePatchSerialized> patchesSerialized;

		public void OnBeforeSerialize()
		{
			if (patchesSerialized == null)
			{
				patchesSerialized = new List<EditorTypePatchSerialized>(Patches.Count + 8);
			}
			else
			{
				patchesSerialized.Clear();
				if (patchesSerialized.Capacity < Patches.Count)
				{
					patchesSerialized.Capacity = Patches.Count + 8;
				}
			}
			foreach (KeyValuePair<Type, EditorTypePatch> kvp in Patches)
			{
				patchesSerialized.Add(new EditorTypePatchSerialized
				{
					TypeBinding = TwoWaySerializationBinder.Default.BindToName(kvp.Key),
					TypeGuid = kvp.Value.TargetGuid,
					PatchEntityId = OdinEntityId.FromObject(kvp.Value)
				});
			}
		}

		public void OnAfterDeserialize()
		{
		}

		public void Deserialize()
		{
			if (patchesSerialized == null)
			{
				return;
			}
			Patches.Clear();
			for (int i = 0; i < patchesSerialized.Count; i++)
			{
				EditorTypePatchSerialized serializedPatch = patchesSerialized[i];
				Type type = TwoWaySerializationBinder.Default.BindToType(serializedPatch.TypeBinding);
				if (type == null)
				{
					type = DesignerUtils.GetTypeFromScriptGuid(serializedPatch.TypeGuid);
				}
				EditorTypePatch patch = serializedPatch.ToEditorTypePatch();
				if (type == null)
				{
					if (patch != null)
					{
						UnityEngine.Object.DestroyImmediate(patch);
					}
				}
				else
				{
					patch.TargetType = type;
					patch.TargetGuid = serializedPatch.TypeGuid;
					TypePatch originalPatch = TypePatchCache.Get(type);
					originalPatch.EditorVariant = patch;
					Patches[type] = patch;
				}
			}
		}
	}
}

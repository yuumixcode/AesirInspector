using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class EditorTypePatch : SerializedScriptableObject, IOverridesSerializationFormat
	{
		public ulong SyncTag;

		public bool HasBasePatch;

		public bool IsOutOfSyncWithFileOnDisk;

		[NonSerialized]
		public Type TargetType;

		[NonSerialized]
		public string TargetGuid;

		[OdinSerialize]
		public RefList<AttributePatch> SelfPatches;

		[NonSerialized]
		[OdinSerialize]
		public List<GroupPatch> GroupPatches;

		[NonSerialized]
		[OdinSerialize]
		public List<PropertyPatch> PropertyPatches;

		public bool IsDirty => EditorUtility.IsDirty(this);

		public DataFormat GetFormatToSerializeAs(bool isPlayer)
		{
			return DataFormat.Binary;
		}

		public GroupPatch CreateGroupPatch(string id)
		{
			if (GroupPatches == null)
			{
				GroupPatches = new List<GroupPatch>();
			}
			GroupPatch patch = new GroupPatch
			{
				ParentId = null,
				Id = id,
				DesiredIndex = -1,
				GroupAttributePatch = new AttributePatch
				{
					MemberDeltas = RefList<MemberDelta>.Empty
				}
			};
			GroupPatches.Add(patch);
			return patch;
		}

		public PropertyPatch CreatePropertyPatch(string name)
		{
			if (PropertyPatches == null)
			{
				PropertyPatches = new List<PropertyPatch>();
			}
			PropertyPatch patch = new PropertyPatch
			{
				ParentId = null,
				Name = name,
				DesiredIndex = -1,
				AttributePatches = new RefList<AttributePatch>()
			};
			PropertyPatches.Add(patch);
			return patch;
		}

		public static EditorTypePatch CreateFromTypePatch(TypePatch patch)
		{
			EditorTypePatch result = ScriptableObject.CreateInstance<EditorTypePatch>();
			result.hideFlags = HideFlags.HideAndDontSave;
			UnityEngine.Object.DontDestroyOnLoad(result);
			result.Initialize(patch);
			return result;
		}

		public void Initialize(TypePatch patch)
		{
			IsOutOfSyncWithFileOnDisk = false;
			SyncTag = 0uL;
			HasBasePatch = patch.BasePatch != null;
			TargetType = patch.TargetType;
			TargetGuid = DesignerUtils.TryGetScriptGuid(patch.TargetType);
			SelfPatches = new RefList<AttributePatch>(patch.SelfPatches.Length);
			GroupPatches = ((patch.GroupPatches == null) ? new List<GroupPatch>(8) : new List<GroupPatch>(DesignerUtils.Round8(patch.GroupPatches.Count)));
			PropertyPatches = ((patch.PropertyPatches == null) ? new List<PropertyPatch>(8) : new List<PropertyPatch>(DesignerUtils.Round8(patch.PropertyPatches.Count)));
			for (int i = 0; i < patch.SelfPatches.Length; i++)
			{
				AttributePatch copy = patch.SelfPatches[i].DeepCopy();
				SelfPatches.Add(ref copy);
			}
			if (patch.GroupPatches != null)
			{
				for (int j = 0; j < patch.GroupPatches.Count; j++)
				{
					GroupPatch copy2 = patch.GroupPatches[j].DeepCopy();
					GroupPatches.Add(copy2);
				}
			}
			if (patch.PropertyPatches != null)
			{
				for (int k = 0; k < patch.PropertyPatches.Count; k++)
				{
					PropertyPatch copy3 = patch.PropertyPatches[k].DeepCopy();
					PropertyPatches.Add(copy3);
				}
			}
			patch.EditorVariant = this;
		}

		public void HandleDiskSynchronization()
		{
			if (!IsOutOfSyncWithFileOnDisk || TargetType == null)
			{
				return;
			}
			if (!TypePatchCache.TryGet(TargetType, out var patch))
			{
				IsOutOfSyncWithFileOnDisk = false;
				return;
			}
			if (!IsDirty || EditorUtility.DisplayDialog("Odin Visual Designer", "The type '" + TargetType.GetNiceName() + "' was modified outside of the Visual Designer.\n\nDo you want to reload the updated version from disk? This will replace your current in-editor changes.", "Reload from Disk", "Keep Current Version"))
			{
				Initialize(patch);
			}
			IsOutOfSyncWithFileOnDisk = false;
		}

		public void MarkDirty()
		{
			SyncTag++;
			EditorUtility.SetDirty(this);
			FinalizedInspectorInfoCache.Clear();
		}

		public void ClearDirty()
		{
			EditorUtility.ClearDirty(this);
		}

		public void MoveDataToTypePatch(TypePatch patch)
		{
			patch.TargetType = TargetType;
			patch.EditorVariant = null;
			if (SelfPatches.Length > 0)
			{
				patch.SelfPatches = SelfPatches.CopyTrim();
			}
			else
			{
				patch.SelfPatches = RefList<AttributePatch>.Empty;
			}
			patch.GroupPatches?.Clear();
			if (GroupPatches != null)
			{
				if (patch.GroupPatches == null)
				{
					patch.GroupPatches = new List<GroupPatch>(GroupPatches.Count);
				}
				for (int i = 0; i < GroupPatches.Count; i++)
				{
					GroupPatch groupPatch = GroupPatches[i];
					if (groupPatch.HasChanges())
					{
						patch.GroupPatches.Add(groupPatch.DeepCopy());
					}
				}
			}
			else
			{
				patch.GroupPatches = null;
			}
			patch.PropertyPatches?.Clear();
			if (PropertyPatches != null)
			{
				if (patch.PropertyPatches == null)
				{
					patch.PropertyPatches = new List<PropertyPatch>(PropertyPatches.Count);
				}
				for (int j = 0; j < PropertyPatches.Count; j++)
				{
					PropertyPatch propertyPatch = PropertyPatches[j];
					if (propertyPatch.HasChanges())
					{
						patch.PropertyPatches.Add(propertyPatch.DeepCopy());
					}
				}
			}
			else
			{
				patch.PropertyPatches = null;
			}
		}

		public bool HasChanges()
		{
			if (SelfPatches.Length <= 0 && GroupPatches.Count <= 0)
			{
				return PropertyPatches.Count > 0;
			}
			return true;
		}

		public bool InheritsFrom(EditorTypePatch patch)
		{
			if (patch == null)
			{
				return false;
			}
			Type current = TargetType;
			Type target = patch.TargetType;
			if (current == target)
			{
				return false;
			}
			while (current != null)
			{
				if (current == target)
				{
					return true;
				}
				current = DesignerUtils.GetBaseType(current);
			}
			return false;
		}

		public void BeginUndo()
		{
			Undo.RecordObject(this, "Designer Change");
		}

		public void EndUndo()
		{
			MarkDirty();
		}

		public void RetainIn(EditorWindow window)
		{
			if (!(this == null))
			{
				HashSet<EditorWindow> set = EditorTypePatches.GetEditorWindowsRetaining(this);
				set.Add(window);
			}
		}

		public void ReleaseFrom(EditorWindow window)
		{
			if (this == null)
			{
				return;
			}
			HashSet<EditorWindow> set = EditorTypePatches.GetEditorWindowsRetaining(this);
			set.Remove(window);
			if (set.Count > 0)
			{
				return;
			}
			if (!IsDirty)
			{
				EditorTypePatches.Remove(this);
				DesignerPatcher.Reset(null);
				DesignerUtils.RefreshInspectorAndEditors();
				return;
			}
			bool isPopup = window is DesignerAttributePopup;
			bool isAutoSaveOn = GlobalConfig<OdinVisualDesignerConfig>.Instance.AutoSave;
			if (isPopup || isAutoSaveOn)
			{
				SaveChanges(notify: false, null);
				EditorTypePatches.Remove(this);
				DesignerPatcher.Reset(null);
				DesignerUtils.RefreshInspectorAndEditors();
				return;
			}
			if (EditorUtility.DisplayDialog("Odin Visual Designer - Unsaved Changes", $"Do you want to save the changes you have made for: {TargetType}?\n\nYour changes will be lost if you don't save them.", "Save", "Discard"))
			{
				SaveChanges(notify: false, null);
			}
			EditorTypePatches.Remove(this);
			DesignerPatcher.Reset(null);
			DesignerUtils.RefreshInspectorAndEditors();
		}

		public void SaveChanges(bool notify, OdinEditorWindow toastWindow)
		{
			if (!IsDirty)
			{
				return;
			}
			DesignerFile file = OVDFFileWatcher.GetOrCreate(TargetType);
			if (File.Exists(file.Path) && OVDFFileWatcher.IsReadOnly(file.Path))
			{
				if (notify)
				{
					return;
				}
				DesignerFile overrideFile = OVDFFileWatcher.GetDefaultFile(TargetType);
				if (!EditorUtility.DisplayDialog("Odin Visual Designer - Read Only File", "The active designer file for '" + TargetType.GetNiceName() + "' is read-only and cannot be saved in place.\n\nDo you want to create a writable override in the default save path?\n\n" + overrideFile.Path, "Create Override", "Cancel"))
				{
					return;
				}
				if (OVDFFileWatcher.IsReadOnly(overrideFile.Path))
				{
					EditorUtility.DisplayDialog("Odin Visual Designer - Cannot Save Override", "The default save path is not writable:\n\n" + overrideFile.Path, "OK");
					return;
				}
				file = overrideFile;
			}
			else if (!File.Exists(file.Path) && OVDFFileWatcher.IsReadOnly(file.Path))
			{
				if (!notify)
				{
					EditorUtility.DisplayDialog("Odin Visual Designer - Cannot Save", "The default save path is not writable:\n\n" + file.Path, "OK");
				}
				return;
			}
			TypePatch patch = TypePatchCache.Get(TargetType);
			MoveDataToTypePatch(patch);
			DesignerSerializer.Serialize(file, patch);
			DesignerLocalCache.UpdateEntry(TargetType, file.Path);
			OVDFFileWatcher.RegisterSavedFile(TargetType, file.Path);
			patch.EditorVariant = this;
			if (!notify)
			{
				if (toastWindow != null)
				{
					Action openDirAction = delegate
					{
						if (File.Exists(file.Path))
						{
							EditorUtility.RevealInFinder(file.Path);
						}
					};
					Color toastColor = new Color(0.42f, 0.65f, 0.55f);
					if (!File.Exists(file.Path))
					{
						openDirAction = null;
					}
					toastWindow.ShowToast(ToastPosition.BottomLeft, SdfIconType.DiscFill, "<b>Changes Saved</b>", toastColor, 4f, "Open Directory", openDirAction);
				}
				else
				{
					DesignerLogger.Log("Changes saved for '" + TargetType.GetNiceName() + "' at '" + file.Path.Replace('\\', '/') + "'.");
				}
			}
			ClearDirty();
			AssetDatabase.Refresh();
		}

		public void DiscardChanges()
		{
			BeginUndo();
			SelfPatches?.Clear();
			GroupPatches?.Clear();
			PropertyPatches?.Clear();
			EndUndo();
		}
	}
}

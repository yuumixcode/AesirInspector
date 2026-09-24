using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class MergedTypePatch
	{
		public Type TargetType;

		public RefList<MergedGroupPatch> GroupPatches = new RefList<MergedGroupPatch>(16);

		public List<MergedPropertyPatch> PropertyPatches = new List<MergedPropertyPatch>(32);

		public Dictionary<string, MergedPropertyPatch> PropertyPatchMap = new Dictionary<string, MergedPropertyPatch>(32);

		public static readonly Dictionary<string, RefList<MergedGroupPatch>.IndexRef> GroupPatchHandles = new Dictionary<string, RefList<MergedGroupPatch>.IndexRef>(16);

		public static readonly Dictionary<string, MutablePropertyInfo> Groups = new Dictionary<string, MutablePropertyInfo>(16);

		public static readonly Dictionary<string, MutablePropertyInfo> Properties = new Dictionary<string, MutablePropertyInfo>(32);

		public static readonly MutablePropertyInfo Root = new MutablePropertyInfo
		{
			Children = new List<MutablePropertyInfo>(32)
		};

		[Obsolete]
		public void ApplyPatches(InspectorPropertyInfo info)
		{
			throw new NotImplementedException();
		}

		public void Merge(TypePatch typePatch, int depth)
		{
			TargetType = typePatch.TargetType;
			typePatch.GetActivePatches(out var groupPatches, out var propertyPatches);
			GroupPatchHandles.Clear();
			for (int i = 0; i < GroupPatches.Length; i++)
			{
				ref MergedGroupPatch patch = ref GroupPatches[i];
				GroupPatchHandles[patch.Id] = GroupPatches.GetIndexRef(i);
			}
			if (groupPatches != null)
			{
				for (int j = 0; j < groupPatches.Count; j++)
				{
					GroupPatch patch2 = groupPatches[j];
					if (!GroupPatchHandles.TryGetValue(patch2.Id, out var handle))
					{
						MergedGroupPatch newMergedPatch = new MergedGroupPatch(ref patch2, depth);
						GroupPatches.Add(ref newMergedPatch);
					}
					else
					{
						handle.Ref.Merge(ref patch2, depth);
					}
				}
			}
			if (propertyPatches == null)
			{
				return;
			}
			for (int k = 0; k < propertyPatches.Count; k++)
			{
				PropertyPatch patch3 = propertyPatches[k];
				if (!PropertyPatchMap.TryGetValue(patch3.Name, out var mergedPatch))
				{
					mergedPatch = PropertyInfoPatchesPool.Rent();
					PropertyPatches.Add(mergedPatch);
					PropertyPatchMap[patch3.Name] = mergedPatch;
				}
				mergedPatch.Merge(ref patch3, depth);
			}
		}

		public void ApplyAttributePatches(List<InspectorPropertyInfo> rootMemberProperties)
		{
			for (int i = 0; i < rootMemberProperties.Count; i++)
			{
				InspectorPropertyInfo info = rootMemberProperties[i];
				if (!PropertyPatchMap.TryGetValue(info.PropertyName, out var patch))
				{
					InspectorPropertyInfoUtility.CheckForShowHideAttributes(info.attributes, out var hasShowAttribute, out var hasHideAttribute);
					if (!hasShowAttribute && hasHideAttribute)
					{
						info.IsShownInInspector = false;
					}
					else if (hasShowAttribute)
					{
						info.IsShownInInspector = true;
					}
				}
				else
				{
					patch.Apply(info);
				}
			}
		}

		public void BuildGroupsAndHandlePositionChanges(InspectorProperty parentProperty, List<InspectorPropertyInfo> rootMemberProperties)
		{
			Groups.Clear();
			Properties.Clear();
			for (int i = 0; i < rootMemberProperties.Count; i++)
			{
				AddMutableInfo(rootMemberProperties[i], Root);
			}
			for (int j = 0; j < GroupPatches.Length; j++)
			{
				ref MergedGroupPatch patch = ref GroupPatches[j];
				if (!Groups.TryGetValue(patch.Id, out var group))
				{
					if (!patch.IsAddedByDesigner)
					{
						continue;
					}
					group = AddMissingGroup(patch.Id, TargetType);
					group.IsAddedByDesigner = true;
				}
				try
				{
					group.ApplyPatch(ref patch);
				}
				catch (Exception)
				{
					Debug.Log($"Failed to apply patch {patch.GroupAttributePatch.PatchType} {patch.GroupAttributePatch.AttributeType} {patch.Id} for {group}");
				}
				if (!patch.HasBeenMoved)
				{
					continue;
				}
				group.Info.DesignerDesiredIndex = patch.DesiredIndex;
				group.Info.DesignerHierarchyDepth = patch.DesiredIndexDepth;
				MutablePropertyInfo lastParent = group.Parent;
				if (patch.ParentId != string.Empty)
				{
					if (!Groups.TryGetValue(patch.ParentId, out var parentGroup))
					{
						parentGroup = AddMissingGroup(patch.ParentId, TargetType);
					}
					group.Parent = parentGroup;
				}
				else
				{
					group.Parent = Root;
				}
				if (lastParent != group.Parent)
				{
					lastParent?.Children.Remove(group);
					group.Parent.Children.Add(group);
				}
			}
			foreach (MergedPropertyPatch patch2 in PropertyPatches)
			{
				if (patch2.HasBeenMoved && Properties.TryGetValue(patch2.Name, out var property))
				{
					property.Info.DesignerDesiredIndex = patch2.DesiredIndex;
					property.Info.DesignerHierarchyDepth = patch2.DesiredIndexDepth;
					MutablePropertyInfo parent;
					if (patch2.ParentId == string.Empty)
					{
						parent = Root;
					}
					else if (!Groups.TryGetValue(patch2.ParentId, out parent))
					{
						parent = AddMissingGroup(patch2.ParentId, TargetType);
					}
					if (property.Parent != parent)
					{
						property.Parent.Children.Remove(property);
						property.Parent = parent;
						parent.Children.Add(property);
					}
				}
			}
			bool isEditor = parentProperty.Tree.IsMadeForDesignerEditor;
			Root.SortByDesiredIndex();
			if (!isEditor)
			{
				Root.RemoveHiddenChildren();
				Root.SortExcludedByOrder();
			}
			for (int k = 0; k < Root.Children.Count; k++)
			{
				Root.Children[k].BakeToInfo(isEditor);
			}
			rootMemberProperties.Clear();
			for (int l = 0; l < Root.Children.Count; l++)
			{
				rootMemberProperties.Add(Root.Children[l].Info);
			}
			for (int m = 0; m < Root.Children.Count; m++)
			{
				MutablePropertyInfoPool.Return(Root.Children[m]);
			}
			Root.Children.Clear();
		}

		public static void AddMutableInfo(InspectorPropertyInfo info, MutablePropertyInfo parent)
		{
			MutablePropertyInfo mutableInfo = MutablePropertyInfoPool.Rent(info);
			mutableInfo.Parent = parent;
			parent.Children.Add(mutableInfo);
			mutableInfo.IsAddedByCode = true;
			if (info.PropertyType != PropertyType.Group)
			{
				Properties[info.PropertyName] = mutableInfo;
				return;
			}
			string id = ((PropertyGroupAttribute)info.attributes[0]).GroupID;
			Groups[id] = mutableInfo;
			for (int i = 0; i < info.groupInfos.Length; i++)
			{
				AddMutableInfo(info.groupInfos[i], mutableInfo);
			}
		}

		public static MutablePropertyInfo AddMissingGroup(string id, Type typeOfOwner)
		{
			InspectorPropertyInfo info = new InspectorPropertyInfo(0f, typeOfOwner, null, PropertyType.Group, SerializationBackend.None, isEditable: false);
			MutablePropertyInfo mutableInfo = MutablePropertyInfoPool.Rent(info);
			Groups[id] = mutableInfo;
			return mutableInfo;
		}
	}
}

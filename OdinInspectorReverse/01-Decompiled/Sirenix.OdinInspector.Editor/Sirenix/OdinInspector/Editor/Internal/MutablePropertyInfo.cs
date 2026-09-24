using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector.Internal;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class MutablePropertyInfo
	{
		public MutablePropertyInfo Parent;

		public InspectorPropertyInfo Info;

		public List<MutablePropertyInfo> Children;

		public bool IsAddedByDesigner;

		public bool IsAddedByCode;

		public bool IsRemoved;

		public float InspectorOrder;

		public int DesiredIndex;

		public bool IsVisibilityAltered;

		public int RowColumnCount;

		public HashSet<string> UsedGroupNames = new HashSet<string>();

		public static readonly List<MutablePropertyInfo> ToInsert = new List<MutablePropertyInfo>(16);

		private static List<MutablePropertyInfo> SortResult = new List<MutablePropertyInfo>(32);

		private static List<MutablePropertyInfo> ToSort = new List<MutablePropertyInfo>(32);

		private static MemberInfo groupNameMember = typeof(PropertyGroupAttribute).GetField("GroupName");

		public static readonly Dictionary<Type, int> AttributeMap = new Dictionary<Type, int>(8);

		public bool IsGroup
		{
			get
			{
				if (Info != null)
				{
					return Info.PropertyType == PropertyType.Group;
				}
				return false;
			}
		}

		public bool IsValidGroup
		{
			get
			{
				if (!IsAddedByDesigner)
				{
					return IsAddedByCode;
				}
				return true;
			}
		}

		public bool IsEmptyGroup => Children.Count == 0;

		public void Initialize(InspectorPropertyInfo info)
		{
			Parent = null;
			Info = info;
			IsAddedByDesigner = false;
			IsAddedByCode = true;
			IsRemoved = false;
			DesiredIndex = -1;
			IsVisibilityAltered = false;
		}

		public void BakeToInfo(bool isForEditor)
		{
			if (!IsGroup)
			{
				return;
			}
			if (Info.PropertyName == null)
			{
				PropertyGroupAttribute groupAttribute = ((Info.attributes.Count > 0) ? ((PropertyGroupAttribute)Info.attributes[0]) : null);
				if (groupAttribute != null)
				{
					string groupName = null;
					if (groupAttribute is ColumnGroupAttribute)
					{
						groupName = $"#Row{Parent.RowColumnCount}";
						Parent.RowColumnCount++;
					}
					else if (groupAttribute is ColumnGroupAttribute.ColumnSubGroupAttribute)
					{
						groupName = $"#Col{Parent.RowColumnCount}";
						Parent.RowColumnCount++;
					}
					else
					{
						groupName = groupAttribute.GroupName;
					}
					Info.PropertyName = GetValidGroupName(groupName, Parent.UsedGroupNames);
				}
				else
				{
					Info.PropertyName = "#" + Info.DesignerId;
				}
			}
			else if (Parent != null && !Parent.UsedGroupNames.Add(Info.PropertyName))
			{
				Info.PropertyName = GetValidGroupName(Info.PropertyName, Parent.UsedGroupNames);
			}
			SortByDesiredIndex();
			if (!isForEditor)
			{
				RemoveHiddenChildren();
				SortExcludedByOrder();
			}
			if (Children.Count == 0)
			{
				Info.groupInfos = Array.Empty<InspectorPropertyInfo>();
				Info.memberInfos = Array.Empty<MemberInfo>();
				return;
			}
			RowColumnCount = 0;
			UsedGroupNames.Clear();
			for (int i = 0; i < Children.Count; i++)
			{
				MutablePropertyInfo current = Children[i];
				current.BakeToInfo(isForEditor);
			}
			if (Info.groupInfos == null || Info.groupInfos.Length != Children.Count)
			{
				Info.groupInfos = new InspectorPropertyInfo[Children.Count];
			}
			for (int j = 0; j < Info.groupInfos.Length; j++)
			{
				Info.groupInfos[j] = Children[j].Info;
			}
			Info.UpdateMemberInfosForGroup();
		}

		public static string GetValidGroupName(string desiredName, HashSet<string> existingNames)
		{
			if (!string.IsNullOrEmpty(desiredName) && desiredName[0] != '#')
			{
				desiredName = "#" + desiredName;
			}
			if (existingNames.Add(desiredName))
			{
				return desiredName;
			}
			int value = 1;
			string current = desiredName + $" ({value})";
			while (!existingNames.Add(current))
			{
				current = desiredName + $" ({++value})";
			}
			return current;
		}

		public void ApplyInfoModificationsFromAttributes()
		{
			if (IsGroup)
			{
				for (int i = 0; i < Children.Count; i++)
				{
					Children[i].ApplyInfoModificationsFromAttributes();
				}
				return;
			}
			bool hasShowAttribute = false;
			bool hasHideAttribute = false;
			for (int j = 0; j < Info.attributes.Count; j++)
			{
				Attribute current = Info.attributes[j];
				if (current is ReadOnlyAttribute)
				{
					Info.IsEditable = false;
				}
				else if (current is ShowInInspectorAttribute)
				{
					hasShowAttribute = true;
				}
				else if (current is HideInInspector)
				{
					hasHideAttribute = true;
				}
			}
			if (!IsVisibilityAltered)
			{
				if (hasHideAttribute && !hasShowAttribute)
				{
					Info.IsShownInInspector = false;
				}
				else if (hasShowAttribute)
				{
					Info.IsShownInInspector = true;
				}
			}
		}

		public bool IsExcluded()
		{
			InspectorPropertyInfo info = Info;
			if (DesignerRegistry.IsTypeOfOwnerExcluded(info))
			{
				return true;
			}
			for (int i = 0; i < info.attributes.Count; i++)
			{
				Attribute attribute = info.attributes[i];
				if (attribute is ExcludeInOdinDesignerAttribute)
				{
					return true;
				}
			}
			return false;
		}

		public void RemoveExcludedChildren()
		{
			if (Children == null)
			{
				return;
			}
			for (int i = Children.Count - 1; i >= 0; i--)
			{
				MutablePropertyInfo current = Children[i];
				if (current.IsExcluded())
				{
					Children.RemoveAt(i);
				}
			}
		}

		public void RemoveHiddenChildren()
		{
			for (int i = Children.Count - 1; i >= 0; i--)
			{
				MutablePropertyInfo current = Children[i];
				if (current.IsGroup)
				{
					if (current.IsEmptyGroup)
					{
						Children.RemoveAt(i);
					}
				}
				else if (!current.Info.IsShownInInspector)
				{
					Children.RemoveAt(i);
				}
			}
		}

		public bool HandleMissingAndRemovedGroups(bool isEditor, List<MutablePropertyInfo> newChildrenBuffer)
		{
			if (Children == null)
			{
				return false;
			}
			if (IsRemoved || Info.attributes.Count == 0)
			{
				Parent.Children.Remove(this);
				for (int i = Children.Count - 1; i >= 0; i--)
				{
					MutablePropertyInfo current = Children[i];
					if (!current.HandleMissingAndRemovedGroups(isEditor, newChildrenBuffer))
					{
						newChildrenBuffer.Add(current);
					}
				}
				return true;
			}
			for (int i2 = Children.Count - 1; i2 >= 0; i2--)
			{
				MutablePropertyInfo child = Children[i2];
				child.HandleMissingAndRemovedGroups(isEditor, newChildrenBuffer);
			}
			return false;
		}

		public static MutablePropertyInfo GetValidParentForChildren(MutablePropertyInfo parent)
		{
			while (parent != null)
			{
				if (parent.Info.PropertyName == "$ROOT")
				{
					return parent;
				}
				if (parent.Info.attributes.Count > 0)
				{
					List<Attribute> attribute = parent.Info.attributes;
					if (!DesignerRegistry.ExcludedGroups.Contains(attribute.GetType()))
					{
						return parent;
					}
				}
				parent = parent.Parent;
			}
			return null;
		}

		public void ConsumeDesiredIndicesRecursively()
		{
			if (Children == null)
			{
				return;
			}
			ToInsert.Clear();
			for (int i = Children.Count - 1; i >= 0; i--)
			{
				MutablePropertyInfo current = Children[i];
				if (current.DesiredIndex >= 0)
				{
					ToInsert.Add(current);
					Children.RemoveAt(i);
				}
			}
			ToInsert.Sort((MutablePropertyInfo lhs, MutablePropertyInfo rhs) => lhs.DesiredIndex.CompareTo(rhs.DesiredIndex));
			for (int i2 = 0; i2 < ToInsert.Count; i2++)
			{
				MutablePropertyInfo current2 = ToInsert[i2];
				if (current2.DesiredIndex >= Children.Count)
				{
					Children.Add(current2);
				}
				else
				{
					Children.Insert(current2.DesiredIndex, current2);
				}
				current2.DesiredIndex = -1;
			}
			for (int i3 = 0; i3 < Children.Count; i3++)
			{
				MutablePropertyInfo current3 = Children[i3];
				current3.ConsumeDesiredIndicesRecursively();
			}
		}

		public void BakeToInfo(Dictionary<string, float> propertyInfoOrders, ref Dictionary<InspectorPropertyInfo, float> groupMemberOrders, bool isForEditor)
		{
			if (!IsGroup)
			{
				InspectorOrder = propertyInfoOrders[Info.PropertyName];
				return;
			}
			if (Info.PropertyName == null)
			{
				Info.PropertyName = "#" + ((PropertyGroupAttribute)Info.attributes[0]).GroupName;
			}
			if (!isForEditor)
			{
				for (int i = Children.Count - 1; i >= 0; i--)
				{
					MutablePropertyInfo current = Children[i];
					if (current.IsGroup)
					{
						if (current.IsEmptyGroup)
						{
							Children.RemoveAt(i);
						}
					}
					else if (!current.Info.IsShownInInspector)
					{
						Children.RemoveAt(i);
					}
				}
			}
			if (Children.Count == 0)
			{
				Info.groupInfos = Array.Empty<InspectorPropertyInfo>();
				Info.memberInfos = Array.Empty<MemberInfo>();
				groupMemberOrders[Info] = groupMemberOrders.Count;
				return;
			}
			for (int j = 0; j < Children.Count; j++)
			{
				Children[j].BakeToInfo(propertyInfoOrders, ref groupMemberOrders, isForEditor);
			}
			Children.Sort((MutablePropertyInfo a, MutablePropertyInfo b) => a.InspectorOrder.CompareTo(b.InspectorOrder));
			InspectorOrder = Children[0].InspectorOrder;
			groupMemberOrders[Info] = InspectorOrder;
			PropertyGroupAttribute group = (PropertyGroupAttribute)Info.attributes[0];
			if (group.Order != 0f)
			{
				Info.Order = group.Order;
			}
			else
			{
				Info.Order = Children[0].Info.Order;
			}
			if (group is ISubGroupProviderAttribute)
			{
				for (int i2 = 0; i2 < Children.Count; i2++)
				{
					Children[i2].Info.Order = i2;
				}
			}
			if (Info.groupInfos == null || Info.groupInfos.Length != Children.Count)
			{
				Info.groupInfos = new InspectorPropertyInfo[Children.Count];
			}
			else
			{
				bool isDifferent = false;
				for (int i3 = 0; i3 < Info.groupInfos.Length; i3++)
				{
					if (Children[i3].Info != Info.groupInfos[i3])
					{
						isDifferent = true;
						break;
					}
				}
				if (!isDifferent)
				{
					return;
				}
			}
			for (int i4 = 0; i4 < Info.groupInfos.Length; i4++)
			{
				Info.groupInfos[i4] = Children[i4].Info;
			}
			Info.UpdateMemberInfosForGroup();
		}

		public void SortByDesiredIndex()
		{
			if (Children == null || Children.Count == 0)
			{
				return;
			}
			SortResult.Clear();
			ToSort.Clear();
			for (int i = 0; i < Children.Count; i++)
			{
				MutablePropertyInfo current = Children[i];
				if (current.Info.DesignerDesiredIndex >= 0)
				{
					ToSort.Add(current);
				}
				SortResult.Add(current);
			}
			ToSort.Sort(delegate(MutablePropertyInfo lhs, MutablePropertyInfo rhs)
			{
				InspectorPropertyInfo info = lhs.Info;
				InspectorPropertyInfo info2 = rhs.Info;
				int num = info.DesignerHierarchyDepth.CompareTo(info2.DesignerHierarchyDepth);
				return (num == 0) ? info2.DesignerDesiredIndex.CompareTo(info.DesignerDesiredIndex) : num;
			});
			for (int i2 = 0; i2 < ToSort.Count; i2++)
			{
				MutablePropertyInfo current2 = ToSort[i2];
				SortResult.Remove(current2);
				bool isAdded = false;
				for (int j = 0; j < SortResult.Count; j++)
				{
					InspectorPropertyInfo currentInfo = current2.Info;
					if (currentInfo.DesignerDesiredIndex == j)
					{
						SortResult.Insert(j, current2);
						isAdded = true;
						break;
					}
				}
				if (!isAdded)
				{
					SortResult.Add(current2);
				}
			}
			Children.Clear();
			Children.AddRange(SortResult);
		}

		public void SortExcludedByOrder()
		{
			if (Children == null || Children.Count == 0)
			{
				return;
			}
			ToSort.Clear();
			SortResult.Clear();
			for (int i = 0; i < Children.Count; i++)
			{
				MutablePropertyInfo child = Children[i];
				if (child.IsExcluded())
				{
					ToSort.Add(child);
				}
				else
				{
					SortResult.Add(child);
				}
			}
			if (ToSort.Count == 0)
			{
				return;
			}
			for (int i2 = ToSort.Count - 1; i2 >= 0; i2--)
			{
				MutablePropertyInfo current = ToSort[i2];
				bool isAdded = false;
				for (int j = 0; j < SortResult.Count; j++)
				{
					MutablePropertyInfo other = SortResult[j];
					if (other.Info.Order >= current.Info.Order)
					{
						SortResult.Insert(j, current);
						isAdded = true;
						break;
					}
				}
				if (!isAdded)
				{
					SortResult.Add(current);
				}
			}
			Children.Clear();
			Children.AddRange(SortResult);
		}

		public void ApplyPatch(ref MergedGroupPatch patch)
		{
			ref BakedAttributePatch attributePatch = ref patch.GroupAttributePatch;
			Attribute instance = TryGetGroupAttribute();
			switch (attributePatch.PatchType)
			{
			case AttributePatchType.Add:
				IsRemoved = false;
				if (instance == null)
				{
					instance = DesignerAttributeCreator.Create(attributePatch.AttributeType);
					((PropertyGroupAttribute)instance).GroupID = patch.Id;
					Info.DesignerId = patch.Id;
					if (Info.attributes == null)
					{
						Info.attributes = new List<Attribute>(1);
					}
					Info.attributes.Add(instance);
				}
				break;
			case AttributePatchType.Remove:
				IsRemoved = true;
				if (instance == null)
				{
					return;
				}
				break;
			case AttributePatchType.Modify:
			case AttributePatchType.ModifyUnused:
				IsRemoved = false;
				if (instance == null)
				{
					attributePatch.PatchType = AttributePatchType.ModifyUnused;
					return;
				}
				attributePatch.PatchType = AttributePatchType.Modify;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			object instanceObj = instance;
			for (int i = 0; i < attributePatch.BakedMemberDeltas.Length; i++)
			{
				ref BakedMemberDelta delta = ref attributePatch.BakedMemberDeltas[i];
				delta.Setter(ref instanceObj, delta.Value);
			}
		}

		public void ApplyPatch(GroupPatch patch)
		{
			ref AttributePatch attributePatch = ref patch.GroupAttributePatch;
			if (attributePatch.AttributeType == null)
			{
				return;
			}
			Attribute instance = TryGetGroupAttribute();
			switch (attributePatch.PatchType)
			{
			default:
				return;
			case AttributePatchType.Add:
				IsRemoved = false;
				if (instance == null)
				{
					instance = DesignerAttributeCreator.Create(attributePatch.AttributeType);
					((PropertyGroupAttribute)instance).GroupID = patch.Id;
					Info.DesignerId = patch.Id;
					if (Info.attributes == null)
					{
						Info.attributes = new List<Attribute>(1);
					}
					Info.attributes.Add(instance);
				}
				break;
			case AttributePatchType.Remove:
				IsRemoved = true;
				if (instance == null)
				{
					return;
				}
				break;
			case AttributePatchType.Modify:
			case AttributePatchType.ModifyUnused:
				IsRemoved = instance == null;
				if (instance == null)
				{
					attributePatch.PatchType = AttributePatchType.ModifyUnused;
					return;
				}
				attributePatch.PatchType = AttributePatchType.Modify;
				break;
			}
			object instanceObj = instance;
			for (int i = 0; i < attributePatch.MemberDeltas.Length; i++)
			{
				ref MemberDelta delta = ref attributePatch.MemberDeltas[i];
				WeakValueSetter setter = DesignerAttributeSetters.GetSetter(delta.Member);
				object value = delta.Value;
				if (delta.Member == groupNameMember && value == null)
				{
					value = string.Empty;
				}
				setter(ref instanceObj, value);
			}
		}

		public PropertyGroupAttribute TryGetGroupAttribute()
		{
			if (Info.attributes == null || Info.attributes.Count == 0)
			{
				return null;
			}
			return (PropertyGroupAttribute)Info.attributes[0];
		}

		public void ApplyPatch(PropertyPatch patch)
		{
			List<Attribute> attributes = Info.attributes;
			AttributeMap.Clear();
			for (int i = 0; i < attributes.Count; i++)
			{
				Attribute attribute = attributes[i];
				AttributeMap[attribute.GetType()] = i;
			}
			for (int j = 0; j < patch.AttributePatches.Length; j++)
			{
				ref AttributePatch attributePatch = ref patch.AttributePatches[j];
				if (attributePatch.AttributeType == null)
				{
					continue;
				}
				Attribute instance = null;
				if (AttributeMap.TryGetValue(attributePatch.AttributeType, out var instanceIndex))
				{
					instance = attributes[instanceIndex];
				}
				switch (attributePatch.PatchType)
				{
				case AttributePatchType.Add:
					if (instance == null)
					{
						instance = DesignerAttributeCreator.Create(attributePatch.AttributeType);
						attributes.Add(instance);
					}
					else
					{
						instance = (attributes[instanceIndex] = FastDeepCopier.DeepCopy(instance));
					}
					break;
				case AttributePatchType.Remove:
					if (instance != null)
					{
						attributes.Remove(instance);
					}
					continue;
				case AttributePatchType.Modify:
				case AttributePatchType.ModifyUnused:
					if (instance == null)
					{
						attributePatch.PatchType = AttributePatchType.ModifyUnused;
						continue;
					}
					attributePatch.PatchType = AttributePatchType.Modify;
					instance = (attributes[instanceIndex] = FastDeepCopier.DeepCopy(instance));
					break;
				}
				object instanceObj = instance;
				for (int k = 0; k < attributePatch.MemberDeltas.Length; k++)
				{
					ref MemberDelta delta = ref attributePatch.MemberDeltas[k];
					WeakValueSetter setter = DesignerAttributeSetters.GetSetter(delta.Member);
					setter(ref instanceObj, delta.Value);
				}
			}
			switch (patch.Visibility)
			{
			case PropertyVisibilityState.Shown:
				Info.IsShownInInspector = true;
				IsVisibilityAltered = true;
				break;
			case PropertyVisibilityState.Hidden:
				Info.IsShownInInspector = false;
				IsVisibilityAltered = true;
				break;
			}
		}
	}
}

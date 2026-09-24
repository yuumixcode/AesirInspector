using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class MergedPropertyPatch
	{
		public string Name;

		public bool IsMoved;

		public string ParentId;

		public int DesiredIndex;

		public int DesiredIndexDepth;

		public PropertyVisibilityState Visibility;

		public RefList<BakedAttributePatch> AttributePatches;

		private static readonly Dictionary<Type, RefList<BakedAttributePatch>.IndexRef> AttributePatchHandles = new Dictionary<Type, RefList<BakedAttributePatch>.IndexRef>(16);

		public bool HasBeenMoved => ParentId != null;

		public void Merge(ref PropertyPatch patch, int depth)
		{
			Name = patch.Name;
			if (patch.Visibility != PropertyVisibilityState.Default)
			{
				Visibility = patch.Visibility;
			}
			if (patch.HasBeenMoved)
			{
				ParentId = patch.ParentId;
				DesiredIndex = patch.DesiredIndex;
				DesiredIndexDepth = depth;
			}
			else if (patch.HasDesiredIndex)
			{
				DesiredIndex = patch.DesiredIndex;
				DesiredIndexDepth = depth;
			}
			AttributePatchHandles.Clear();
			for (int i = 0; i < AttributePatches.Length; i++)
			{
				ref BakedAttributePatch attributePatch = ref AttributePatches[i];
				AttributePatchHandles[attributePatch.AttributeType] = AttributePatches.GetIndexRef(i);
			}
			if (AttributePatches == RefList<BakedAttributePatch>.Empty && patch.AttributePatches.Length > 0)
			{
				AttributePatches = new RefList<BakedAttributePatch>();
			}
			for (int j = 0; j < patch.AttributePatches.Length; j++)
			{
				ref AttributePatch attributePatch2 = ref patch.AttributePatches[j];
				if (!AttributePatchHandles.TryGetValue(attributePatch2.AttributeType, out var handle))
				{
					BakedAttributePatch newItem = new BakedAttributePatch(ref attributePatch2);
					AttributePatches.Add(ref newItem);
				}
				else
				{
					handle.Ref.Merge(ref attributePatch2);
				}
			}
		}

		public void Apply(InspectorPropertyInfo info)
		{
			for (int i = 0; i < AttributePatches.Length; i++)
			{
				ref BakedAttributePatch patch = ref AttributePatches[i];
				int attributeIndex = GetAttributeIndex(info.attributes, patch.AttributeType);
				Attribute instance;
				switch (patch.PatchType)
				{
				case AttributePatchType.Add:
					if (attributeIndex != -1)
					{
						instance = FastDeepCopier.DeepCopy(info.attributes[attributeIndex]);
						info.attributes[attributeIndex] = instance;
					}
					else
					{
						instance = DesignerAttributeCreator.Create(patch.AttributeType);
						info.attributes.Add(instance);
					}
					break;
				case AttributePatchType.Remove:
					if (attributeIndex >= 0)
					{
						info.attributes.RemoveAt(attributeIndex);
					}
					continue;
				case AttributePatchType.Modify:
					if (attributeIndex == -1)
					{
						continue;
					}
					instance = FastDeepCopier.DeepCopy(info.attributes[attributeIndex]);
					info.attributes[attributeIndex] = instance;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				object instanceObj = instance;
				for (int j = 0; j < patch.BakedMemberDeltas.Length; j++)
				{
					ref BakedMemberDelta delta = ref patch.BakedMemberDeltas[j];
					delta.Setter(ref instanceObj, delta.Value);
				}
			}
			bool hasShowAttribute = false;
			bool hasHideAttribute = false;
			for (int k = 0; k < info.attributes.Count; k++)
			{
				Attribute current = info.attributes[k];
				if (current is PropertyOrderAttribute orderAttr)
				{
					info.Order = orderAttr.Order;
				}
				else if (current is ReadOnlyAttribute)
				{
					info.IsEditable = false;
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
			switch (Visibility)
			{
			case PropertyVisibilityState.Default:
				if (hasHideAttribute && !hasShowAttribute)
				{
					info.IsShownInInspector = false;
				}
				else if (hasShowAttribute)
				{
					info.IsShownInInspector = true;
				}
				break;
			case PropertyVisibilityState.Shown:
				info.IsShownInInspector = true;
				break;
			case PropertyVisibilityState.Hidden:
				info.IsShownInInspector = false;
				break;
			}
		}

		private static int GetAttributeIndex(List<Attribute> attributes, Type attributeType)
		{
			for (int i = 0; i < attributes.Count; i++)
			{
				Attribute current = attributes[i];
				if (current.GetType() == attributeType)
				{
					return i;
				}
			}
			return -1;
		}
	}
}

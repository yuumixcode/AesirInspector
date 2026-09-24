using System;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.Internal
{
	[Serializable]
	internal class GroupPatch
	{
		public string ParentId;

		public string Id;

		public int DesiredIndex;

		public bool IsAddedByDesigner;

		public AttributePatch GroupAttributePatch;

		public bool HasBeenMoved => ParentId != null;

		public GroupPatch DeepCopy()
		{
			return new GroupPatch
			{
				ParentId = ParentId,
				Id = Id,
				IsAddedByDesigner = IsAddedByDesigner,
				DesiredIndex = DesiredIndex,
				GroupAttributePatch = GroupAttributePatch.DeepCopy()
			};
		}

		public PropertyGroupAttribute Apply(InspectorPropertyInfo groupInfo)
		{
			ref AttributePatch patch = ref GroupAttributePatch;
			PropertyGroupAttribute result = null;
			if (groupInfo != null)
			{
				result = (PropertyGroupAttribute)groupInfo.attributes[0];
			}
			else if (patch.PatchType == AttributePatchType.Add)
			{
				result = (PropertyGroupAttribute)DesignerAttributeCreator.Create(patch.AttributeType);
			}
			if (result == null)
			{
				return null;
			}
			object resultObj = result;
			for (int i = 0; i < patch.MemberDeltas.Length; i++)
			{
				ref MemberDelta delta = ref patch.MemberDeltas[i];
				WeakValueSetter setter = DesignerAttributeSetters.GetSetter(delta.Member);
				setter(ref resultObj, delta.Value);
			}
			return result;
		}

		public void AddAttributeDeltaChange(InspectorProperty property)
		{
			InspectorProperty attribute = property.ParentValueProperty;
			Type attributeType = attribute.ValueEntry.TypeOfValue;
			AttributePatchType patchType = GroupAttributePatch.PatchType;
			if (patchType != AttributePatchType.None)
			{
				if (patchType != AttributePatchType.Remove)
				{
					goto IL_0040;
				}
			}
			else
			{
				GroupAttributePatch.AttributeType = attributeType;
			}
			GroupAttributePatch.PatchType = AttributePatchType.Modify;
			goto IL_0040;
			IL_0040:
			GroupAttributePatch.AddDeltaChange(property, attribute.ValueEntry.WeakSmartValue);
		}

		public void AddAttributeDeltaChange(Type attributeType, FieldInfo field, object value)
		{
			AttributePatchType patchType = GroupAttributePatch.PatchType;
			if (patchType != AttributePatchType.None)
			{
				if (patchType != AttributePatchType.Remove)
				{
					goto IL_002d;
				}
			}
			else
			{
				GroupAttributePatch.AttributeType = attributeType;
			}
			GroupAttributePatch.PatchType = AttributePatchType.Modify;
			goto IL_002d;
			IL_002d:
			GroupAttributePatch.AddDeltaChange(field, value);
		}

		public bool HasChanges()
		{
			if (DesiredIndex != -1)
			{
				return true;
			}
			if (ParentId != null)
			{
				return true;
			}
			switch (GroupAttributePatch.PatchType)
			{
			case AttributePatchType.Add:
			case AttributePatchType.Remove:
				return true;
			case AttributePatchType.Modify:
				return GroupAttributePatch.MemberDeltas.Length > 0;
			default:
				return false;
			}
		}
	}
}

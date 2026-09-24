using System;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerPatchUtils
	{
		public static void AddAttributeDeltaChange(InspectorProperty memberProperty, ref RefList<AttributePatch> attributePatches)
		{
			InspectorProperty attributeProperty = memberProperty.ParentValueProperty;
			Type attributeType = attributeProperty.ValueEntry.TypeOfValue;
			if (attributePatches.Capacity == 0)
			{
				attributePatches = new RefList<AttributePatch>();
			}
			int attributeIndex = -1;
			for (int i = 0; i < attributePatches.Length; i++)
			{
				if (attributePatches[i].AttributeType == attributeType)
				{
					attributeIndex = i;
					break;
				}
			}
			if (attributeIndex == -1)
			{
				AttributePatch newPatch = new AttributePatch
				{
					AttributeType = attributeType,
					PatchType = AttributePatchType.Modify,
					MemberDeltas = new RefList<MemberDelta>()
				};
				newPatch.AddDeltaChange(memberProperty, attributeProperty.ValueEntry.WeakSmartValue);
				attributePatches.Add(ref newPatch);
				return;
			}
			ref AttributePatch patch = ref attributePatches[attributeIndex];
			AttributePatchType patchType = patch.PatchType;
			if (patchType != AttributePatchType.None)
			{
				if (patchType != AttributePatchType.Remove)
				{
					goto IL_00cd;
				}
			}
			else
			{
				patch.AttributeType = attributeType;
			}
			patch.PatchType = AttributePatchType.Modify;
			goto IL_00cd;
			IL_00cd:
			patch.AddDeltaChange(memberProperty, attributeProperty.ValueEntry.WeakSmartValue);
		}

		public static void AddAttribute(Type attributeType, ref RefList<AttributePatch> attributePatches)
		{
			for (int i = 0; i < attributePatches.Length; i++)
			{
				ref AttributePatch patch = ref attributePatches[i];
				if (!(patch.AttributeType != attributeType))
				{
					if (patch.PatchType == AttributePatchType.Remove)
					{
						patch.PatchType = AttributePatchType.Add;
					}
					return;
				}
			}
			if (attributePatches.Capacity == 0)
			{
				attributePatches = new RefList<AttributePatch>();
			}
			AttributePatch newPatch = new AttributePatch
			{
				AttributeType = attributeType,
				PatchType = AttributePatchType.Add,
				MemberDeltas = RefList<MemberDelta>.Empty
			};
			attributePatches.Add(ref newPatch);
		}

		public static void RemoveAttribute(Type attributeType, ref RefList<AttributePatch> attributePatches)
		{
			for (int i = 0; i < attributePatches.Length; i++)
			{
				ref AttributePatch patch = ref attributePatches[i];
				if (patch.AttributeType != attributeType)
				{
					continue;
				}
				if (patch.PatchType != AttributePatchType.Remove)
				{
					if (patch.PatchType == AttributePatchType.Add)
					{
						attributePatches.RemoveAt(i);
					}
					else
					{
						patch.PatchType = AttributePatchType.Remove;
					}
				}
				return;
			}
			if (attributePatches.Capacity == 0)
			{
				attributePatches = new RefList<AttributePatch>();
			}
			AttributePatch newPatch = new AttributePatch
			{
				AttributeType = attributeType,
				PatchType = AttributePatchType.Remove,
				MemberDeltas = RefList<MemberDelta>.Empty
			};
			attributePatches.Add(ref newPatch);
		}

		public static void TransferAttributeDeltas(Attribute attribute, ref RefList<AttributePatch> attributePatches)
		{
			Type attributeType = attribute.GetType();
			for (int i = 0; i < attributePatches.Length; i++)
			{
				ref AttributePatch patch = ref attributePatches[i];
				if (patch.AttributeType != attributeType)
				{
					continue;
				}
				patch.MemberDeltas = RefList<MemberDelta>.Empty;
				Attribute baseline = DesignerAttributeCreator.Create(attributeType);
				using PropertyTree tree = PropertyTree.Create(attribute, SerializationBackend.None);
				using PropertyTree baselineTree = PropertyTree.Create(baseline, SerializationBackend.None);
				InspectorProperty attributeProperty = tree.RootProperty;
				InspectorProperty attributeBaselineProperty = baselineTree.RootProperty;
				for (int j = 0; j < attributeProperty.Children.Count; j++)
				{
					InspectorProperty property = attributeProperty.Children[j];
					if (DesignerUtils.CanAttributePropertyBeModified(property))
					{
						InspectorProperty propertyBaseline = attributeBaselineProperty.Children[property.Name];
						if (!object.Equals(property.ValueEntry.WeakSmartValue, propertyBaseline.ValueEntry.WeakSmartValue))
						{
							patch.AddDeltaChange(property, attribute);
						}
					}
				}
				break;
			}
		}
	}
}

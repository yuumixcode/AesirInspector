using System;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class DesignerSelection
	{
		public DesignerEditor owningEditor;

		public bool IsGroup;

		public string Id;

		[HideLabel]
		[InlineProperty]
		[HideReferenceObjectPicker]
		[ShowInInspector]
		public List<Attribute> Attributes;

		public HashSet<Type> AttributeTypes;

		public HashSet<Type> AttributesFromCode;

		public HashSet<AttributeMemberKey> ChangedMembers = new HashSet<AttributeMemberKey>();

		public bool IsValid => !string.IsNullOrEmpty(Id);

		public DesignerSelection(DesignerEditor editor, DesignerEditorNode node)
		{
			owningEditor = editor;
			IsGroup = false;
			Id = node?.Path;
			Attributes = new List<Attribute>(8);
			AttributesFromCode = node?.CodeAttributes;
			AttributeTypes = new HashSet<Type>();
		}

		public DesignerEditorNode Find(DesignerEditorNode current)
		{
			if (string.IsNullOrEmpty(Id))
			{
				return null;
			}
			for (int i = 0; i < current.Children.Count; i++)
			{
				if (IsGroup)
				{
					if (current.Children[i].GetDesignerId() == Id)
					{
						return current.Children[i];
					}
				}
				else if (current.Children[i].Path == Id)
				{
					return current.Children[i];
				}
				if (current.Children[i].NodeType != DesignerEditorNodeType.Member)
				{
					DesignerEditorNode childResult = Find(current.Children[i]);
					if (childResult != null)
					{
						return childResult;
					}
				}
			}
			return null;
		}

		public void Update(DesignerEditorContext ctx, DesignerEditorNode node)
		{
			Attributes.Clear();
			AttributeTypes.Clear();
			ChangedMembers.Clear();
			if (node == null)
			{
				Id = null;
				AttributesFromCode = null;
				return;
			}
			AttributesFromCode = node.CodeAttributes;
			switch (node.NodeType)
			{
			case DesignerEditorNodeType.Root:
				Id = "$self";
				IsGroup = false;
				break;
			case DesignerEditorNodeType.Group:
				Id = node.GetDesignerId();
				IsGroup = true;
				break;
			case DesignerEditorNodeType.Member:
				Id = node.Path;
				IsGroup = false;
				break;
			default:
				Id = null;
				IsGroup = false;
				return;
			}
			if (node.NodeType == DesignerEditorNodeType.Member || node.NodeType == DesignerEditorNodeType.Root)
			{
				for (int i = 0; i < node.Attributes.Count; i++)
				{
					Attribute current = node.Attributes[i];
					if (DesignerRegistry.IsValidAttribute(current))
					{
						Attributes.Add(FastDeepCopier.DeepCopy(current));
						AttributeTypes.Add(current.GetType());
						UpdatePatchChangesHashset(ctx, node);
					}
				}
			}
			else
			{
				Attributes.Add(FastDeepCopier.DeepCopy(node.GroupAttribute));
				AttributeTypes.Add(node.GroupAttribute.GetType());
				UpdatePatchChangesHashset(ctx, node);
			}
		}

		public void UpdatePatchChangesHashset(DesignerEditorContext ctx, DesignerEditorNode node)
		{
			ChangedMembers.Clear();
			if (node == null)
			{
				return;
			}
			if (node.NodeType == DesignerEditorNodeType.Root)
			{
				RefList<AttributePatch> selfPatches = ctx.EditorPatch.SelfPatches;
				for (int i = 0; i < selfPatches.Length; i++)
				{
					AddAttributePatchChangesToHashset(ref selfPatches[i]);
				}
				return;
			}
			if (node.PropertyPatch != null)
			{
				PropertyPatch patch = node.PropertyPatch;
				for (int j = 0; j < patch.AttributePatches.Length; j++)
				{
					AddAttributePatchChangesToHashset(ref patch.AttributePatches[j]);
				}
			}
			if (node.GroupPatch != null)
			{
				GroupPatch patch2 = node.GroupPatch;
				AddAttributePatchChangesToHashset(ref patch2.GroupAttributePatch);
			}
		}

		private void AddAttributePatchChangesToHashset(ref AttributePatch patch)
		{
			for (int i = 0; i < Attributes.Count; i++)
			{
				Attribute current = Attributes[i];
				if (!(current.GetType() != patch.AttributeType))
				{
					for (int j = 0; j < patch.MemberDeltas.Length; j++)
					{
						ref MemberDelta delta = ref patch.MemberDeltas[j];
						ChangedMembers.Add(new AttributeMemberKey(patch.AttributeType, delta.Member.Name));
					}
				}
			}
		}

		public bool IsPropertyChanged(InspectorProperty property)
		{
			InspectorProperty attribute = property.Parent;
			while (attribute != null && attribute.ValueEntry == null)
			{
				attribute = attribute.Parent;
			}
			if (attribute == null)
			{
				return false;
			}
			OdinDesignerBindingAttribute binding = property.GetAttribute<OdinDesignerBindingAttribute>();
			if (binding == null)
			{
				return ChangedMembers.Contains(new AttributeMemberKey(attribute.ValueEntry.TypeOfValue, property.Name));
			}
			bool isAnyNotChanged = false;
			for (int i = 0; i < binding.MemberNames.Length; i++)
			{
				AttributeMemberKey key = new AttributeMemberKey(attribute.ValueEntry.TypeOfValue, binding.MemberNames[i]);
				if (!ChangedMembers.Contains(key))
				{
					isAnyNotChanged = true;
					break;
				}
			}
			return !isAnyNotChanged;
		}
	}
}

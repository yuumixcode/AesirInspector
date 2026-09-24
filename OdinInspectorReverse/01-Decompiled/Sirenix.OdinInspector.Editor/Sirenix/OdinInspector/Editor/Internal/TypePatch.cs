using System;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class TypePatch
	{
		public Type TargetType;

		public TypePatch BasePatch;

		public EditorTypePatch EditorVariant;

		public RefList<AttributePatch> SelfPatches;

		public List<GroupPatch> GroupPatches;

		public List<PropertyPatch> PropertyPatches;

		private static readonly Stack<TypePatch> TraversalStack = new Stack<TypePatch>(16);

		public bool HasEditorVariant => EditorVariant != null;

		public IEnumerable<TypePatch> TraverseFromRoot()
		{
			TraversalStack.Clear();
			for (TypePatch current = this; current != null; current = current.BasePatch)
			{
				TraversalStack.Push(current);
			}
			while (TraversalStack.Count > 0)
			{
				yield return TraversalStack.Pop();
			}
		}

		public void GetActivePatches(out List<GroupPatch> groupPatches, out List<PropertyPatch> propertyPatches)
		{
			if (HasEditorVariant)
			{
				groupPatches = EditorVariant.GroupPatches;
				propertyPatches = EditorVariant.PropertyPatches;
			}
			else
			{
				groupPatches = GroupPatches;
				propertyPatches = PropertyPatches;
			}
		}

		public bool HasChanges()
		{
			if (SelfPatches != null && SelfPatches.Length > 0)
			{
				return true;
			}
			if (GroupPatches != null && GroupPatches.Count > 0)
			{
				return true;
			}
			if (PropertyPatches != null && PropertyPatches.Count > 0)
			{
				return true;
			}
			return false;
		}
	}
}

using System;

namespace Sirenix.OdinInspector.Editor.Internal
{
	[Serializable]
	internal class PropertyPatch
	{
		public string ParentId;

		public string Name;

		public PropertyVisibilityState Visibility;

		public int DesiredIndex;

		public RefList<AttributePatch> AttributePatches;

		public bool HasBeenMoved => ParentId != null;

		public bool HasDesiredIndex => DesiredIndex >= 0;

		public PropertyPatch DeepCopy()
		{
			PropertyPatch result = new PropertyPatch
			{
				ParentId = ParentId,
				Name = Name,
				Visibility = Visibility,
				DesiredIndex = DesiredIndex
			};
			if (AttributePatches.Length == 0)
			{
				result.AttributePatches = RefList<AttributePatch>.Empty;
			}
			else
			{
				result.AttributePatches = new RefList<AttributePatch>(AttributePatches.Length);
				for (int i = 0; i < AttributePatches.Length; i++)
				{
					AttributePatch copy = AttributePatches[i].DeepCopy();
					result.AttributePatches.Add(ref copy);
				}
			}
			return result;
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
			if (Visibility != PropertyVisibilityState.Default)
			{
				return true;
			}
			if (AttributePatches != null && AttributePatches.Length > 0)
			{
				for (int i = 0; i < AttributePatches.Length; i++)
				{
					ref AttributePatch patch = ref AttributePatches[i];
					switch (patch.PatchType)
					{
					case AttributePatchType.Add:
					case AttributePatchType.Remove:
						return true;
					case AttributePatchType.Modify:
						return patch.MemberDeltas.Length > 0;
					}
				}
			}
			return false;
		}

		public bool IsComplexMethod()
		{
			int paramStart = -1;
			for (int i = 0; i < Name.Length; i++)
			{
				switch (Name[i])
				{
				case '<':
				case '>':
					if (paramStart == -1)
					{
						return true;
					}
					break;
				case '(':
					paramStart = i;
					break;
				case ')':
				{
					int paramLength = i - paramStart - 1;
					if (paramStart != -1)
					{
						return paramLength > 0;
					}
					return false;
				}
				}
			}
			return false;
		}
	}
}

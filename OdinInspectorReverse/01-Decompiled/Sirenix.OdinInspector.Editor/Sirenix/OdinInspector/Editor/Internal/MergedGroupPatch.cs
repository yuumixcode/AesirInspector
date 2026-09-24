namespace Sirenix.OdinInspector.Editor.Internal
{
	internal struct MergedGroupPatch
	{
		public string ParentId;

		public string Id;

		public int DesiredIndex;

		public int DesiredIndexDepth;

		public bool IsAddedByDesigner;

		public BakedAttributePatch GroupAttributePatch;

		public bool HasBeenMoved => ParentId != null;

		public MergedGroupPatch(ref GroupPatch patch, int depth)
		{
			ParentId = patch.ParentId;
			Id = patch.Id;
			DesiredIndex = patch.DesiredIndex;
			IsAddedByDesigner = patch.IsAddedByDesigner;
			GroupAttributePatch = new BakedAttributePatch(ref patch.GroupAttributePatch);
			DesiredIndexDepth = ((DesiredIndex != -1) ? depth : 0);
		}

		public void Merge(ref GroupPatch patch, int depth)
		{
			if (patch.HasBeenMoved)
			{
				ParentId = patch.ParentId;
				DesiredIndex = patch.DesiredIndex;
				DesiredIndexDepth = depth;
			}
			GroupAttributePatch.Merge(ref patch.GroupAttributePatch);
		}
	}
}

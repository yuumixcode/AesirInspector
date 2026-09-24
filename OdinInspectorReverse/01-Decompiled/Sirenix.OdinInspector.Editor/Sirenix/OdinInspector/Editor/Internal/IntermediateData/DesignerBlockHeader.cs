namespace Sirenix.OdinInspector.Editor.Internal.IntermediateData
{
	internal ref struct DesignerBlockHeader
	{
		public string Id;

		public bool IsReference;

		public bool IsGroupFromCode;

		public string ParentId;

		public int DesiredIndex;

		public PropertyVisibilityState Visibility;

		public static DesignerBlockHeader Invalid => new DesignerBlockHeader
		{
			Id = null,
			IsReference = true
		};

		public static DesignerBlockHeader Self => new DesignerBlockHeader
		{
			Id = "self",
			IsReference = true
		};

		public bool IsValid
		{
			get
			{
				if (Id == null)
				{
					return !IsReference;
				}
				return true;
			}
		}

		public DesignerBlockHeader(ref PropertyPatch patch)
		{
			Id = patch.Name;
			IsReference = false;
			IsGroupFromCode = false;
			ParentId = patch.ParentId;
			DesiredIndex = patch.DesiredIndex;
			Visibility = patch.Visibility;
		}

		public DesignerBlockHeader(ref GroupPatch patch)
		{
			Id = patch.Id;
			IsReference = patch.IsAddedByDesigner;
			IsGroupFromCode = !patch.IsAddedByDesigner;
			ParentId = patch.ParentId;
			DesiredIndex = patch.DesiredIndex;
			Visibility = PropertyVisibilityState.Default;
		}
	}
}

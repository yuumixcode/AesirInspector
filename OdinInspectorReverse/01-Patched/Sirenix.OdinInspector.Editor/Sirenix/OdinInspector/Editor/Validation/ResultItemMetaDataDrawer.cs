namespace Sirenix.OdinInspector.Editor.Validation
{
	internal class ResultItemMetaDataDrawer
	{
		public ResultItemMetaData[] MetaData;

		public bool ExcludeFirstButton;

		public ResultItemMetaDataDrawer(ResultItemMetaData[] metaData, bool excludeFirstButton)
		{
			MetaData = metaData;
			ExcludeFirstButton = excludeFirstButton;
		}
	}
}

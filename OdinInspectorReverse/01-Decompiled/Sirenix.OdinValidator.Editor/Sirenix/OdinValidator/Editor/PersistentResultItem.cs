using Sirenix.OdinInspector.Editor.Validation;

namespace Sirenix.OdinValidator.Editor
{
	public struct PersistentResultItem
	{
		public string Message;

		public ValidationResultType ResultType;

		public PersistenceData Data;

		public PersistentResultItem(string message, ValidationResultType type)
		{
			this = default(PersistentResultItem);
			Message = message;
			ResultType = type;
		}

		public PersistentResultItem(in ResultItem result)
		{
			this = default(PersistentResultItem);
			Data.RichText = result.RichText;
			Data.Fix = result.Fix;
			Data.MetaData = result.MetaData;
			Data.OnContextClick = result.OnContextClick;
			Message = result.Message;
			ResultType = result.ResultType;
			if ((bool)result.SelectionObject && ObjectAddress.TryCreateObjectAddress(result.SelectionObject, out var address, out var _))
			{
				Data.SelectionObjectAddress = DynamicObjectAddress.GetOrCreate(result.SelectionObject, address);
			}
		}
	}
}

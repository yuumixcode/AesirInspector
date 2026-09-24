namespace Sirenix.OdinInspector.Editor
{
	[ShowOdinSerializedPropertiesInInspector]
	public class TempKeyValuePair<TKey, TValue>
	{
		[ShowInInspector]
		public TKey Key;

		[ShowInInspector]
		public TValue Value;
	}
}

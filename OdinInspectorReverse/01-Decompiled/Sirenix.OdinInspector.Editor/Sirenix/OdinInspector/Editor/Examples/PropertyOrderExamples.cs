namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(PropertyOrderAttribute))]
	internal class PropertyOrderExamples
	{
		[PropertyOrder(1f)]
		public int Second;

		[PropertyOrder(-1f)]
		[InfoBox("PropertyOrder is used to change the order of properties in the inspector.", InfoMessageType.Info, null)]
		public int First;
	}
}

namespace Sirenix.OdinInspector.Editor
{
	public abstract class PropertyComponent
	{
		public readonly InspectorProperty Property;

		public PropertyComponent(InspectorProperty property)
		{
			Property = property;
		}

		public virtual void Reset()
		{
		}
	}
}

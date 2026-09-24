namespace Sirenix.OdinInspector.Editor
{
	public abstract class ComponentProvider
	{
		public abstract PropertyComponent CreateComponent(InspectorProperty property);
	}
}

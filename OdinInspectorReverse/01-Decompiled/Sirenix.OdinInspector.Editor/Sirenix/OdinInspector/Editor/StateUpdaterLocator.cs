namespace Sirenix.OdinInspector.Editor
{
	public abstract class StateUpdaterLocator
	{
		public abstract StateUpdater[] GetStateUpdaters(InspectorProperty property);
	}
}

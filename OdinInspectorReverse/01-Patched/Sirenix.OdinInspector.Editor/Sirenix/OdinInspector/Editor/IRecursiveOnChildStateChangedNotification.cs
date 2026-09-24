namespace Sirenix.OdinInspector.Editor
{
	public interface IRecursiveOnChildStateChangedNotification
	{
		void OnChildStateChanged(InspectorProperty child, string state);
	}
}

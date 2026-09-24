namespace Sirenix.OdinInspector.Editor
{
	public interface IPathRedirector
	{
		bool TryGetRedirectedProperty(string childName, out InspectorProperty property);
	}
}

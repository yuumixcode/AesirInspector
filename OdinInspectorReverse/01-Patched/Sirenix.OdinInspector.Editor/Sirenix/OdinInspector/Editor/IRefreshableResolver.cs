namespace Sirenix.OdinInspector.Editor
{
	public interface IRefreshableResolver
	{
		bool ChildPropertyRequiresRefresh(int index, InspectorPropertyInfo info);
	}
}

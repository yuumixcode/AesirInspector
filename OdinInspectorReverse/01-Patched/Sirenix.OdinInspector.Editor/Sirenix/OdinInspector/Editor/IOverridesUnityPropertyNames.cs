namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// A property resolver can implement this interface to override the Unity property names for all child indices.
	/// This can be necessary when the Odin path and the Unity property path are different by some necessity or other.
	/// </summary>
	public interface IOverridesUnityPropertyNames
	{
		string GetUnityPropertyName(int index);
	}
}

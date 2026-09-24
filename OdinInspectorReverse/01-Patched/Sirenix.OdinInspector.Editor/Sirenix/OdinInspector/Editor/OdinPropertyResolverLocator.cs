namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Base class for locator of <see cref="T:Sirenix.OdinInspector.Editor.OdinPropertyResolver" />. Use <see cref="T:Sirenix.OdinInspector.Editor.DefaultOdinPropertyResolverLocator" /> for default implementation.
	/// </summary>
	public abstract class OdinPropertyResolverLocator
	{
		/// <summary>
		/// Gets an <see cref="T:Sirenix.OdinInspector.Editor.OdinPropertyResolver" /> instance for the specified property.
		/// </summary>
		/// <param name="property">The property to get an <see cref="T:Sirenix.OdinInspector.Editor.OdinPropertyResolver" /> instance for.</param>
		/// <returns>An instance of <see cref="T:Sirenix.OdinInspector.Editor.OdinPropertyResolver" /> to resolver the specified property.</returns>
		public abstract OdinPropertyResolver GetResolver(InspectorProperty property);
	}
}

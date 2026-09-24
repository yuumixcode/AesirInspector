namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Base drawer to inherit from to draw methods.
	/// </summary>
	public abstract class MethodDrawer : OdinDrawer
	{
		public sealed override bool CanDrawProperty(InspectorProperty property)
		{
			if (property.Info.PropertyType == PropertyType.Method)
			{
				return CanDrawMethodProperty(property);
			}
			return false;
		}

		protected virtual bool CanDrawMethodProperty(InspectorProperty property)
		{
			return true;
		}
	}
}

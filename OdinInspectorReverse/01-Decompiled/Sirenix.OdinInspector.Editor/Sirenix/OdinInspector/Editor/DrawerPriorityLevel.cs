namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>
	/// DrawerPriorityLevel is used in conjunction with <see cref="T:Sirenix.OdinInspector.Editor.DrawerPriority" />.
	/// </para>
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.DrawerPriority" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.DrawerPriorityAttribute" />
	public enum DrawerPriorityLevel
	{
		/// <summary>
		/// Auto priority is defined by setting all of the components to zero.
		/// If no <see cref="T:Sirenix.OdinInspector.Editor.DrawerPriorityAttribute" /> is defined on a drawer, it will default to AutoPriority.
		/// </summary>
		AutoPriority,
		/// <summary>
		/// The value priority. Mostly used by <see cref="T:Sirenix.OdinInspector.Editor.OdinValueDrawer`1" />s.
		/// </summary>
		ValuePriority,
		/// <summary>
		/// The attribute priority. Mostly used by <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeDrawer`2" />s.
		/// </summary>
		AttributePriority,
		/// <summary>
		/// The wrapper priority. Mostly used by drawers used to decorate properties.
		/// </summary>
		WrapperPriority,
		/// <summary>
		/// The super priority. Mostly used by drawers that wants to wrap the entire property but don't draw the actual property.
		/// These drawers typically don't draw the property itself, and calls CallNextDrawer.
		/// </summary>
		SuperPriority
	}
}

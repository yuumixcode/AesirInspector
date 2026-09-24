using System;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>DrawerPriority is used on inspector drawers and indicates the priority of the drawer.</para>
	/// <para>Use this to make your custom drawer to come before or after other drawers, and potentially hide other drawers.</para>
	/// </summary>
	/// <example>
	/// <para>The following example shows how DrawerPriority could be apply to a value drawer.</para>
	/// <code>
	/// [DrawerPriority(DrawerPriorityLevel.ValuePriority)]
	///
	///             	public sealed class MyIntDrawer : InspectorValuePropertyDrawer&lt;int&gt;
	///             	{
	///             		// ...
	///             	}
	/// </code>
	/// </example>
	/// <example>
	/// <para>The following example shows how DrawerPriority is used to mark a custom int drawer as a high priority drawer.</para>
	/// <code>
	/// [DrawerPriority(1, 0, 0)]
	///
	///             	public sealed class MySpecialIntDrawer : InspectorValuePropertyDrawer&lt;int&gt;
	///             	{
	///             		// ...
	///             	}
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.DrawerPriority" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.DrawerPriorityLevel" />
	[AttributeUsage(AttributeTargets.Class)]
	public class DrawerPriorityAttribute : Attribute
	{
		/// <summary>
		/// The priority of the drawer.
		/// </summary>
		public DrawerPriority Priority { get; private set; }

		/// <summary>
		/// Indicates the priority of an inspector drawer.
		/// </summary>
		/// <param name="priority">Option for priority for the inspector drawer.</param>
		public DrawerPriorityAttribute(DrawerPriorityLevel priority)
		{
			Priority = new DrawerPriority(priority);
		}

		/// <summary>
		/// Indicates the priority of an inspector drawer.
		/// </summary>
		/// <param name="super">
		/// The super priority. Mostly used by drawers that wants to wrap the entire property but don't draw the actual property.
		/// These drawers typically don't draw the property itself, and calls CallNextDrawer.</param>
		/// <param name="wrapper">The wrapper priority. Mostly used by drawers used to decorate properties.</param>
		/// <param name="value">The value priority. Mostly used by <see cref="T:Sirenix.OdinInspector.Editor.OdinValueDrawer`1" />s and <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeDrawer`2" />s.</param>
		public DrawerPriorityAttribute(double super = 0.0, double wrapper = 0.0, double value = 0.0)
		{
			Priority = new DrawerPriority(super, wrapper, value);
		}
	}
}

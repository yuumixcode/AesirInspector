using System;
using System.ComponentModel;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>When creating custom property drawers with <see cref="T:Sirenix.OdinInspector.Editor.OdinValueDrawer`1" /> or <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeDrawer`2" /> etc,
	/// an OdinDrawerAttribute must be defined on the custom drawer class itself in order to specify that the drawer is meant to be included in the inspector.</para>
	/// <para>If no OdinDrawerAttribute is defined, the <see cref="!:DrawerLocator" /> will ignore your drawer.</para>
	/// </summary>
	/// <remarks>
	/// Checkout the manual for more information.
	/// </remarks>
	/// <example>
	/// <code>
	///  // Specify that this drawer must be included in the inspector; without this, it will not be drawn
	/// public class MyCustomTypeDrawer&lt;T&gt; : OdinValueDrawer&lt;T&gt; where T : MyCustomBaseType
	/// {
	///     protected override void DrawPropertyLayout(IPropertyValueEntry&lt;T&gt; entry, GUIContent label)
	///     {
	///         T value = entry.SmartValue;
	///         // Draw property here.
	///
	///         // Optionally, call the next drawer in line.
	///         this.CallNextDrawer(entry, label);
	///     }
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="!:OdinDrawerBehaviour" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinAttributeDrawer`2" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinValueDrawer`1" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinGroupDrawer`1" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinDrawer" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.DrawerPriorityAttribute" />
	[Obsolete("Drawers are now registered by default without applying this attribute - use [DontRegisterDrawer] instead on drawers that you don't want to be globally registered.", false)]
	[AttributeUsage(AttributeTargets.Class)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class OdinDrawerAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.OdinDrawerAttribute" /> class.
		/// </summary>
		public OdinDrawerAttribute()
		{
		}
	}
}

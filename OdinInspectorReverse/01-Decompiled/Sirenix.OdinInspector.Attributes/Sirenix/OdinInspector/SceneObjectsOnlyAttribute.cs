using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>SceneObjectsOnly is used on object properties, and restricts the property to scene objects, and not project assets.</para>
	/// <para>Use this when you want to ensure an object is a scene object, and not from a project asset.</para>
	/// </summary>
	/// <example>
	/// <para>The following example shows a component with a game object property, that must be from a scene, and not a prefab asset.</para>
	/// <code>
	/// public MyComponent : MonoBehaviour
	/// {
	/// 	[SceneObjectsOnly]
	/// 	public GameObject MyPrefab;
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.AssetsOnlyAttribute" />
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public sealed class SceneObjectsOnlyAttribute : Attribute
	{
	}
}

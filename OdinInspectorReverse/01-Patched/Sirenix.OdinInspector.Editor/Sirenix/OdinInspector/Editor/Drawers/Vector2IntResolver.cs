using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public sealed class Vector2IntResolver : BaseMemberPropertyResolver<Vector2Int>
	{
		protected override InspectorPropertyInfo[] GetPropertyInfos()
		{
			return new InspectorPropertyInfo[2]
			{
				InspectorPropertyInfo.CreateValue("x", 0f, base.Property.ValueEntry.SerializationBackend, new GetterSetter<Vector2Int, int>(delegate(ref Vector2Int vec)
				{
					return vec.x;
				}, delegate(ref Vector2Int vec, int value)
				{
					vec.x = value;
				})),
				InspectorPropertyInfo.CreateValue("y", 0f, base.Property.ValueEntry.SerializationBackend, new GetterSetter<Vector2Int, int>(delegate(ref Vector2Int vec)
				{
					return vec.y;
				}, delegate(ref Vector2Int vec, int value)
				{
					vec.y = value;
				}))
			};
		}
	}
}

using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public sealed class Vector3IntResolver : BaseMemberPropertyResolver<Vector3Int>
	{
		protected override InspectorPropertyInfo[] GetPropertyInfos()
		{
			return new InspectorPropertyInfo[3]
			{
				InspectorPropertyInfo.CreateValue("x", 0f, base.Property.ValueEntry.SerializationBackend, new GetterSetter<Vector3Int, int>(delegate(ref Vector3Int vec)
				{
					return vec.x;
				}, delegate(ref Vector3Int vec, int value)
				{
					vec.x = value;
				})),
				InspectorPropertyInfo.CreateValue("y", 0f, base.Property.ValueEntry.SerializationBackend, new GetterSetter<Vector3Int, int>(delegate(ref Vector3Int vec)
				{
					return vec.y;
				}, delegate(ref Vector3Int vec, int value)
				{
					vec.y = value;
				})),
				InspectorPropertyInfo.CreateValue("z", 0f, base.Property.ValueEntry.SerializationBackend, new GetterSetter<Vector3Int, int>(delegate(ref Vector3Int vec)
				{
					return vec.z;
				}, delegate(ref Vector3Int vec, int value)
				{
					vec.z = value;
				}))
			};
		}
	}
}

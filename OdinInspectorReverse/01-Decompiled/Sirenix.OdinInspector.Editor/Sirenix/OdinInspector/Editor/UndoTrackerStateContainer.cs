using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Serialization;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	[ShowOdinSerializedPropertiesInInspector]
	internal class UndoTrackerStateContainer : SessionSingletonSO<UndoTrackerStateContainer>, ISerializationCallbackReceiver
	{
		public int LatestIndex;

		public Dictionary<int, List<Object>> UndoGroups = new Dictionary<int, List<Object>>();

		[HideInInspector]
		private SerializationData serializationData;

		public void OnAfterDeserialize()
		{
			UnitySerializationUtility.DeserializeUnityObject(this, ref serializationData);
		}

		public void OnBeforeSerialize()
		{
			UnitySerializationUtility.SerializeUnityObject(this, ref serializationData);
		}
	}
}

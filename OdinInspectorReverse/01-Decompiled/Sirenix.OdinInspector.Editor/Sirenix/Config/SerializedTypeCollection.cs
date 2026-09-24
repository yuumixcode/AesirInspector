using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;

namespace Sirenix.Config
{
	[Serializable]
	public abstract class SerializedTypeCollection<T> : StringSerializedCollection<T, Type>, ISerializationCallbackReceiver where T : ICollection<Type>, new()
	{
		public void OnBeforeSerialize()
		{
			serializedCollection.Clear();
			foreach (Type type in Collection)
			{
				serializedCollection.Add(TwoWaySerializationBinder.Default.BindToName(type));
			}
		}

		public void OnAfterDeserialize()
		{
			if (Collection.Count > 0)
			{
				return;
			}
			foreach (string s in serializedCollection)
			{
				Collection.Add(TwoWaySerializationBinder.Default.BindToType(s));
			}
		}
	}
}

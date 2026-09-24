using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.Config
{
	[Serializable]
	public class SerializedStringHashSet : StringSerializedCollection<HashSet<string>, string>, ISerializationCallbackReceiver
	{
		public void OnBeforeSerialize()
		{
			serializedCollection.Clear();
			foreach (string s in Collection)
			{
				serializedCollection.Add(s);
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
				Collection.Add(s);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Sirenix.Config
{
	[Serializable]
	[HideReferenceObjectPicker]
	[HideLabel]
	[InlineProperty]
	public class SerializedTypePriorityDictionary : ISerializationCallbackReceiver
	{
		[Serializable]
		public class SerializedData
		{
			public string typeName;

			public int priority;
		}

		[NonSerialized]
		[LabelText("@$property.Parent.NiceName")]
		[ShowInInspector]
		public Dictionary<Type, int> Dictionary = new Dictionary<Type, int>();

		[SerializeField]
		[HideInInspector]
		public List<SerializedData> serializedDictionary = new List<SerializedData>();

		public void OnBeforeSerialize()
		{
			serializedDictionary.Clear();
			foreach (KeyValuePair<Type, int> kvp in Dictionary)
			{
				SerializedData serializedData = new SerializedData
				{
					typeName = TwoWaySerializationBinder.Default.BindToName(kvp.Key),
					priority = kvp.Value
				};
				serializedDictionary.Add(serializedData);
			}
		}

		public void OnAfterDeserialize()
		{
			if (Dictionary.Count > 0)
			{
				return;
			}
			foreach (SerializedData serializedData in serializedDictionary)
			{
				Type type = TwoWaySerializationBinder.Default.BindToType(serializedData.typeName);
				int priority = serializedData.priority;
				Dictionary[type] = priority;
			}
		}
	}
}

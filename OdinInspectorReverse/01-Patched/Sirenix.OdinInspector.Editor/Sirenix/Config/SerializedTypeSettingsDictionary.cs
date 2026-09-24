using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Sirenix.Config
{
	[Serializable]
	[HideReferenceObjectPicker]
	[InlineProperty]
	[HideLabel]
	public class SerializedTypeSettingsDictionary : ISerializationCallbackReceiver
	{
		[Serializable]
		public class SerializedData
		{
			public string typeBound;

			public string displayName;

			public string category;

			public SdfIconType sdfIconType;

			public Color lightColor;

			public Color darkColor;
		}

		[NonSerialized]
		[ShowInInspector]
		[LabelText("@$property.Parent.NiceName")]
		public Dictionary<Type, TypeSettings> Dictionary = new Dictionary<Type, TypeSettings>();

		[SerializeField]
		[HideInInspector]
		public List<SerializedData> serializedDictionary = new List<SerializedData>();

		public void OnBeforeSerialize()
		{
			serializedDictionary.Clear();
			foreach (KeyValuePair<Type, TypeSettings> kvp in Dictionary)
			{
				TypeSettings data = kvp.Value;
				SerializedData serializedData = new SerializedData
				{
					typeBound = TwoWaySerializationBinder.Default.BindToName(kvp.Key),
					displayName = data.Name,
					category = data.Category,
					sdfIconType = data.Icon,
					lightColor = (data.LightIconColor ?? Color.clear),
					darkColor = (data.DarkIconColor ?? Color.clear)
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
				Type type = TwoWaySerializationBinder.Default.BindToType(serializedData.typeBound);
				TypeSettings data = new TypeSettings
				{
					Name = serializedData.displayName,
					Category = serializedData.category,
					Icon = serializedData.sdfIconType,
					LightIconColor = ((serializedData.lightColor != Color.clear) ? new Color?(serializedData.lightColor) : ((Color?)null)),
					DarkIconColor = ((serializedData.darkColor != Color.clear) ? new Color?(serializedData.darkColor) : ((Color?)null))
				};
				Dictionary[type] = data;
			}
		}
	}
}

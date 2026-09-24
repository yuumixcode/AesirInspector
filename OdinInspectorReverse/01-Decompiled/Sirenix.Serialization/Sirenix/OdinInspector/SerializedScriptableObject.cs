using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// A Unity ScriptableObject which is serialized by the Sirenix serialization system.
	/// </summary>
	[ShowOdinSerializedPropertiesInInspector]
	public abstract class SerializedScriptableObject : ScriptableObject, ISerializationCallbackReceiver
	{
		[SerializeField]
		[HideInInspector]
		private SerializationData serializationData;

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (!this.SafeIsUnityNull())
			{
				UnitySerializationUtility.DeserializeUnityObject(this, ref serializationData);
				OnAfterDeserialize();
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			if (!this.SafeIsUnityNull())
			{
				OnBeforeSerialize();
				UnitySerializationUtility.SerializeUnityObject(this, ref serializationData);
			}
		}

		/// <summary>
		/// Invoked after deserialization has taken place.
		/// </summary>
		protected virtual void OnAfterDeserialize()
		{
		}

		/// <summary>
		/// Invoked before serialization has taken place.
		/// </summary>
		protected virtual void OnBeforeSerialize()
		{
		}

		[HideInTables]
		[OnInspectorGUI]
		[PropertyOrder(-2.1474836E+09f)]
		private void InternalOnInspectorGUI()
		{
			EditorOnlyModeConfigUtility.InternalOnInspectorGUI(this);
		}
	}
}

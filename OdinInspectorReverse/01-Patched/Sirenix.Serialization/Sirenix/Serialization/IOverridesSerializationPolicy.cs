namespace Sirenix.Serialization
{
	/// <summary>
	/// Indicates that an Odin-serialized Unity object provides its own serialization policy rather than using the default policy.
	/// <para />
	/// Note that THE VALUES RETURNED BY THIS INTERFACE WILL OVERRIDE THE PARAMETERS PASSED TO <see cref="M:Sirenix.Serialization.UnitySerializationUtility.SerializeUnityObject(UnityEngine.Object,Sirenix.Serialization.SerializationData@,System.Boolean,Sirenix.Serialization.SerializationContext)" /> and <see cref="M:Sirenix.Serialization.UnitySerializationUtility.DeserializeUnityObject(UnityEngine.Object,Sirenix.Serialization.SerializationData@,Sirenix.Serialization.DeserializationContext)" />.
	/// </summary>
	public interface IOverridesSerializationPolicy
	{
		ISerializationPolicy SerializationPolicy { get; }

		bool OdinSerializesUnityFields { get; }
	}
}

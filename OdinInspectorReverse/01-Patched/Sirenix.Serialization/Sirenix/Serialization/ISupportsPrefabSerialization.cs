namespace Sirenix.Serialization
{
	/// <summary>
	/// Indicates that an Odin-serialized Unity object supports prefab serialization.
	/// </summary>
	public interface ISupportsPrefabSerialization
	{
		/// <summary>
		/// Gets or sets the serialization data of the object.
		/// </summary>
		SerializationData SerializationData { get; set; }
	}
}

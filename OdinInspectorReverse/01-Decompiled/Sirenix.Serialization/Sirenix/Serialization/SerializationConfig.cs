namespace Sirenix.Serialization
{
	/// <summary>
	/// Defines the configuration during serialization and deserialization. This class is thread-safe.
	/// </summary>
	public class SerializationConfig
	{
		private readonly object LOCK = new object();

		private volatile ISerializationPolicy serializationPolicy;

		private volatile DebugContext debugContext;

		/// <summary>
		/// <para>
		/// Setting this member to true indicates that in the case where, when expecting to deserialize an instance of a certain type, 
		/// but encountering an incompatible, uncastable type in the data being read, the serializer should attempt to deserialize an 
		/// instance of the expected type using the stored, possibly invalid data.
		/// </para>
		/// <para>
		/// This is equivalent to applying the <see cref="F:Sirenix.Serialization.SerializationConfig.AllowDeserializeInvalidData" /> attribute, except global 
		/// instead of specific to a single type. Note that if this member is set to false, individual types may still be deserialized
		/// with invalid data if they are decorated with the <see cref="F:Sirenix.Serialization.SerializationConfig.AllowDeserializeInvalidData" /> attribute.
		/// </para>
		/// </summary>
		public bool AllowDeserializeInvalidData;

		/// <summary>
		/// Gets or sets the serialization policy. This value is never null; if set to null, it will default to <see cref="P:Sirenix.Serialization.SerializationPolicies.Unity" />.
		/// </summary>
		/// <value>
		/// The serialization policy.
		/// </value>
		public ISerializationPolicy SerializationPolicy
		{
			get
			{
				if (serializationPolicy == null)
				{
					lock (LOCK)
					{
						if (serializationPolicy == null)
						{
							serializationPolicy = SerializationPolicies.Unity;
						}
					}
				}
				return serializationPolicy;
			}
			set
			{
				lock (LOCK)
				{
					serializationPolicy = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the debug context. This value is never null; if set to null, a new default instance of <see cref="P:Sirenix.Serialization.SerializationConfig.DebugContext" /> will be created upon the next get.
		/// </summary>
		/// <value>
		/// The debug context.
		/// </value>
		public DebugContext DebugContext
		{
			get
			{
				if (debugContext == null)
				{
					lock (LOCK)
					{
						if (debugContext == null)
						{
							debugContext = new DebugContext();
						}
					}
				}
				return debugContext;
			}
			set
			{
				lock (LOCK)
				{
					debugContext = value;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Serialization.SerializationConfig" /> class.
		/// </summary>
		public SerializationConfig()
		{
			ResetToDefault();
		}

		/// <summary>
		/// Resets the configuration to a default configuration, as if the constructor had just been called.
		/// </summary>
		public void ResetToDefault()
		{
			lock (LOCK)
			{
				AllowDeserializeInvalidData = false;
				serializationPolicy = null;
				if (debugContext != null)
				{
					debugContext.ResetToDefault();
				}
			}
		}
	}
}

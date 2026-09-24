using System;
using System.Collections.Generic;
using System.ComponentModel;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Unity serialized data struct that contains all data needed by Odin serialization.
	/// </summary>
	[Serializable]
	public struct SerializationData
	{
		/// <summary>
		/// The name of the <see cref="F:Sirenix.Serialization.SerializationData.PrefabModificationsReferencedUnityObjects" /> field.
		/// </summary>
		public const string PrefabModificationsReferencedUnityObjectsFieldName = "PrefabModificationsReferencedUnityObjects";

		/// <summary>
		/// The name of the <see cref="F:Sirenix.Serialization.SerializationData.PrefabModifications" /> field.
		/// </summary>
		public const string PrefabModificationsFieldName = "PrefabModifications";

		/// <summary>
		/// The name of the <see cref="F:Sirenix.Serialization.SerializationData.Prefab" /> field.
		/// </summary>
		public const string PrefabFieldName = "Prefab";

		/// <summary>
		/// The data format used by the serializer. This field will be automatically set to the format specified in the global serialization config
		/// when the Unity object gets serialized, unless the Unity object implements the <see cref="T:Sirenix.Serialization.IOverridesSerializationFormat" /> interface.
		/// </summary>
		[SerializeField]
		public DataFormat SerializedFormat;

		/// <summary>
		/// The serialized data when serializing with the Binray format.
		/// </summary>
		[SerializeField]
		public byte[] SerializedBytes;

		/// <summary>
		/// All serialized Unity references.
		/// </summary>
		[SerializeField]
		public List<UnityEngine.Object> ReferencedUnityObjects;

		/// <summary>
		/// The serialized data when serializing with the JSON format.
		/// </summary>
		[SerializeField]
		public string SerializedBytesString;

		/// <summary>
		/// The reference to the prefab this is only populated in prefab scene instances.
		/// </summary>
		[SerializeField]
		public UnityEngine.Object Prefab;

		/// <summary>
		/// All serialized Unity references.
		/// </summary>
		[SerializeField]
		public List<UnityEngine.Object> PrefabModificationsReferencedUnityObjects;

		/// <summary>
		/// All Odin serialized prefab modifications.
		/// </summary>
		[SerializeField]
		public List<string> PrefabModifications;

		/// <summary>
		/// The serialized data when serializing with the Nodes format.
		/// </summary>
		[SerializeField]
		public List<SerializationNode> SerializationNodes;

		/// <summary>
		/// Whether the object contains any serialized data.
		/// </summary>
		[Obsolete("Use ContainsData instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool HasEditorData
		{
			get
			{
				switch (SerializedFormat)
				{
				case DataFormat.Binary:
				case DataFormat.JSON:
					if (SerializedBytesString.IsNullOrWhitespace())
					{
						if (SerializedBytes != null)
						{
							return SerializedBytes.Length != 0;
						}
						return false;
					}
					return true;
				case DataFormat.Nodes:
					if (SerializationNodes != null)
					{
						return SerializationNodes.Count != 0;
					}
					return false;
				default:
					throw new NotImplementedException(SerializedFormat.ToString());
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the struct contains any data.
		/// If this is false, then it could mean that Unity has not yet deserialized the struct.
		/// </summary>
		public bool ContainsData
		{
			get
			{
				if (SerializedBytes != null && SerializationNodes != null && PrefabModifications != null)
				{
					return ReferencedUnityObjects != null;
				}
				return false;
			}
		}

		/// <summary>
		/// Resets all data.
		/// </summary>
		public void Reset()
		{
			SerializedFormat = DataFormat.Binary;
			if (SerializedBytes != null && SerializedBytes.Length != 0)
			{
				SerializedBytes = new byte[0];
			}
			if (ReferencedUnityObjects != null && ReferencedUnityObjects.Count > 0)
			{
				ReferencedUnityObjects.Clear();
			}
			Prefab = null;
			if (SerializationNodes != null && SerializationNodes.Count > 0)
			{
				SerializationNodes.Clear();
			}
			if (SerializedBytesString != null && SerializedBytesString.Length > 0)
			{
				SerializedBytesString = string.Empty;
			}
			if (PrefabModificationsReferencedUnityObjects != null && PrefabModificationsReferencedUnityObjects.Count > 0)
			{
				PrefabModificationsReferencedUnityObjects.Clear();
			}
			if (PrefabModifications != null && PrefabModifications.Count > 0)
			{
				PrefabModifications.Clear();
			}
		}
	}
}

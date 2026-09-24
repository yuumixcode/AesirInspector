#define UNITY_EDITOR
using System;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Serializer for the <see cref="T:System.Guid" /> type.
	/// </summary>
	/// <seealso cref="!:Serializer&lt;System.Guid&gt;" />
	public sealed class GuidSerializer : Serializer<Guid>
	{
		/// <summary>
		/// Reads a value of type <see cref="T:System.Guid" />.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <returns>
		/// The value which has been read.
		/// </returns>
		public override Guid ReadValue(IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.Guid)
			{
				if (!reader.ReadGuid(out var value))
				{
					reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry);
				}
				return value;
			}
			reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.Guid.ToString() + ", but got entry '" + name + "' of type " + entry);
			reader.SkipEntry();
			return default(Guid);
		}

		/// <summary>
		/// Writes a value of type <see cref="T:System.Guid" />.
		/// </summary>
		/// <param name="name">The name of the value to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="writer">The writer to use.</param>
		public override void WriteValue(string name, Guid value, IDataWriter writer)
		{
			Serializer<Guid>.FireOnSerializedType();
			writer.WriteGuid(name, value);
		}
	}
}

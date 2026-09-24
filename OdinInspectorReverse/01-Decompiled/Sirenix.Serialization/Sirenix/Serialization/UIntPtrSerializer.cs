#define UNITY_EDITOR
using System;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Serializer for the <see cref="T:System.UIntPtr" /> type.
	/// </summary>
	/// <seealso cref="!:Serializer&lt;System.UIntPtr&gt;" />
	public sealed class UIntPtrSerializer : Serializer<UIntPtr>
	{
		/// <summary>
		/// Reads a value of type <see cref="T:System.UIntPtr" />.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <returns>
		/// The value which has been read.
		/// </returns>
		public override UIntPtr ReadValue(IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.Integer)
			{
				if (!reader.ReadUInt64(out var value))
				{
					reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry);
				}
				return new UIntPtr(value);
			}
			reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.Integer.ToString() + ", but got entry '" + name + "' of type " + entry);
			reader.SkipEntry();
			return (UIntPtr)0u;
		}

		/// <summary>
		/// Writes a value of type <see cref="T:System.UIntPtr" />.
		/// </summary>
		/// <param name="name">The name of the value to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="writer">The writer to use.</param>
		public override void WriteValue(string name, UIntPtr value, IDataWriter writer)
		{
			Serializer<UIntPtr>.FireOnSerializedType();
			writer.WriteUInt64(name, (ulong)value);
		}
	}
}

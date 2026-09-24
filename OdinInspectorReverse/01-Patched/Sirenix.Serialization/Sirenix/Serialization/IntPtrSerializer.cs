#define UNITY_EDITOR
using System;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Serializer for the <see cref="T:System.IntPtr" /> type.
	/// </summary>
	/// <seealso cref="!:Serializer&lt;System.IntPtr&gt;" />
	public sealed class IntPtrSerializer : Serializer<IntPtr>
	{
		/// <summary>
		/// Reads a value of type <see cref="T:System.IntPtr" />.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <returns>
		/// The value which has been read.
		/// </returns>
		public override IntPtr ReadValue(IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.Integer)
			{
				if (!reader.ReadInt64(out var value))
				{
					reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry);
				}
				return new IntPtr(value);
			}
			reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.Integer.ToString() + ", but got entry '" + name + "' of type " + entry);
			reader.SkipEntry();
			return (IntPtr)0;
		}

		/// <summary>
		/// Writes a value of type <see cref="T:System.IntPtr" />.
		/// </summary>
		/// <param name="name">The name of the value to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="writer">The writer to use.</param>
		public override void WriteValue(string name, IntPtr value, IDataWriter writer)
		{
			Serializer<IntPtr>.FireOnSerializedType();
			writer.WriteInt64(name, (long)value);
		}
	}
}

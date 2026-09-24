#define UNITY_EDITOR
using System;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Serializer for all enums.
	/// </summary>
	/// <typeparam name="T">The type of the enum to serialize and deserialize.</typeparam>
	/// <seealso cref="T:Sirenix.Serialization.Serializer`1" />
	public sealed class EnumSerializer<T> : Serializer<T>
	{
		static EnumSerializer()
		{
			if (!typeof(T).IsEnum)
			{
				throw new Exception("Type " + typeof(T).Name + " is not an enum.");
			}
		}

		/// <summary>
		/// Reads an enum value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <returns>
		/// The value which has been read.
		/// </returns>
		public override T ReadValue(IDataReader reader)
		{
			string name;
			EntryType entry = reader.PeekEntry(out name);
			if (entry == EntryType.Integer)
			{
				if (!reader.ReadUInt64(out var value))
				{
					reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry);
				}
				return (T)Enum.ToObject(typeof(T), value);
			}
			reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.Integer.ToString() + ", but got entry '" + name + "' of type " + entry);
			reader.SkipEntry();
			return default(T);
		}

		/// <summary>
		/// Writes an enum value of type <see cref="!:T" />.
		/// </summary>
		/// <param name="name">The name of the value to write.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="writer">The writer to use.</param>
		public override void WriteValue(string name, T value, IDataWriter writer)
		{
			Serializer<T>.FireOnSerializedType();
			ulong ul;
			try
			{
				ul = Convert.ToUInt64(value as Enum);
			}
			catch (OverflowException)
			{
				ul = (ulong)Convert.ToInt64(value as Enum);
			}
			writer.WriteUInt64(name, ul);
		}
	}
}

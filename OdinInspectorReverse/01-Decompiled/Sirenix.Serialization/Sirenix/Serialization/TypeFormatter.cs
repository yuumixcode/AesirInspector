using System;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Formatter for the <see cref="T:System.Type" /> type which uses the reader/writer's <see cref="T:Sirenix.Serialization.TwoWaySerializationBinder" /> to bind types.
	/// </summary>
	/// <seealso cref="T:Sirenix.Serialization.MinimalBaseFormatter`1" />
	public sealed class TypeFormatter : MinimalBaseFormatter<Type>
	{
		/// <summary>
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected override void Read(ref Type value, IDataReader reader)
		{
			if (reader.PeekEntry(out var name) == EntryType.String)
			{
				reader.ReadString(out name);
				value = reader.Context.Binder.BindToType(name, reader.Context.Config.DebugContext);
				if (value != null)
				{
					RegisterReferenceID(value, reader);
				}
			}
		}

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected override void Write(ref Type value, IDataWriter writer)
		{
			writer.WriteString(null, writer.Context.Binder.BindToName(value, writer.Context.Config.DebugContext));
		}

		/// <summary>
		/// Returns null.
		/// </summary>
		/// <returns>null.</returns>
		protected override Type GetUninitializedObject()
		{
			return null;
		}
	}
}

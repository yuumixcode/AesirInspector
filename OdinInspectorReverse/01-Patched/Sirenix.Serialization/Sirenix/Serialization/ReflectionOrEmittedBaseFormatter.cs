namespace Sirenix.Serialization
{
	public abstract class ReflectionOrEmittedBaseFormatter<T> : ReflectionFormatter<T>
	{
		protected override void DeserializeImplementation(ref T value, IDataReader reader)
		{
			if (!(FormatterEmitter.GetEmittedFormatter(typeof(T), reader.Context.Config.SerializationPolicy) is FormatterEmitter.RuntimeEmittedFormatter<T> formatter))
			{
				return;
			}
			int count = 0;
			EntryType entry;
			string name;
			while ((entry = reader.PeekEntry(out name)) != EntryType.EndOfNode && entry != EntryType.EndOfArray && entry != EntryType.EndOfStream)
			{
				formatter.Read(ref value, name, entry, reader);
				count++;
				if (count > 1000)
				{
					reader.Context.Config.DebugContext.LogError("Breaking out of infinite reading loop!");
					break;
				}
			}
		}

		protected override void SerializeImplementation(ref T value, IDataWriter writer)
		{
			if (FormatterEmitter.GetEmittedFormatter(typeof(T), writer.Context.Config.SerializationPolicy) is FormatterEmitter.RuntimeEmittedFormatter<T> formatter)
			{
				formatter.Write(ref value, writer);
			}
		}
	}
}

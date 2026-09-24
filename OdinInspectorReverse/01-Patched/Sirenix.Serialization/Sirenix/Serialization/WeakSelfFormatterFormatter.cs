using System;

namespace Sirenix.Serialization
{
	public sealed class WeakSelfFormatterFormatter : WeakBaseFormatter
	{
		public WeakSelfFormatterFormatter(Type serializedType)
			: base(serializedType)
		{
		}

		/// <summary>
		/// Calls <see cref="M:Sirenix.Serialization.ISelfFormatter.Deserialize(Sirenix.Serialization.IDataReader)" />  on the value to deserialize.
		/// </summary>
		protected override void DeserializeImplementation(ref object value, IDataReader reader)
		{
			((ISelfFormatter)value).Deserialize(reader);
		}

		/// <summary>
		/// Calls <see cref="M:Sirenix.Serialization.ISelfFormatter.Serialize(Sirenix.Serialization.IDataWriter)" />  on the value to deserialize.
		/// </summary>
		protected override void SerializeImplementation(ref object value, IDataWriter writer)
		{
			((ISelfFormatter)value).Serialize(writer);
		}
	}
}

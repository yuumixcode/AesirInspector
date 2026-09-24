using System;
using Sirenix.Serialization;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Context that persists across reloading and restarting Unity.
	/// </summary>
	[AlwaysFormatsSelf]
	public abstract class GlobalPersistentContext : ISelfFormatter
	{
		/// <summary>
		/// Time stamp for when the persistent context value was last used.
		/// Used for purging unused context.
		/// </summary>
		[OdinSerialize]
		public long TimeStamp { get; protected set; }

		public abstract Type ValueType { get; }

		/// <summary>
		/// Instatiates a persistent context.
		/// </summary>
		protected GlobalPersistentContext()
		{
		}

		/// <summary>
		/// Updates the time stamp to now.
		/// </summary>
		protected void UpdateTimeStamp()
		{
			TimeStamp = DateTime.Now.Ticks;
		}

		public abstract void Serialize(IDataWriter writer);

		public abstract void Deserialize(IDataReader reader);
	}
	/// <summary>
	/// Context that persists across reloading and restarting Unity.
	/// </summary>
	/// <typeparam name="T">The type of the context value.</typeparam>
	[AlwaysFormatsSelf]
	public sealed class GlobalPersistentContext<T> : GlobalPersistentContext
	{
		private static readonly Serializer<T> ValueSerializer = Serializer.Get<T>();

		private T value;

		/// <summary>
		/// The value of the context.
		/// </summary>
		public T Value
		{
			get
			{
				UpdateTimeStamp();
				return value;
			}
			set
			{
				this.value = value;
				UpdateTimeStamp();
			}
		}

		public override Type ValueType => typeof(T);

		/// <summary>
		/// Creates a new persistent context object.
		/// </summary>
		public static GlobalPersistentContext<T> Create()
		{
			GlobalPersistentContext<T> c = new GlobalPersistentContext<T>();
			c.UpdateTimeStamp();
			return c;
		}

		public override void Deserialize(IDataReader reader)
		{
			reader.ReadInt64(out var time);
			base.TimeStamp = time;
			value = ValueSerializer.ReadValue(reader);
		}

		public override void Serialize(IDataWriter writer)
		{
			writer.WriteInt64(null, base.TimeStamp);
			ValueSerializer.WriteValue(value, writer);
		}

		/// <summary>
		/// Formats a string with the time stamp, and the value.
		/// </summary>
		public override string ToString()
		{
			return new DateTime(base.TimeStamp).ToString("dd/MM/yy HH:mm:ss") + " <" + typeof(T).GetNiceName() + "> " + ((value != null) ? value.ToString() : "(null)");
		}
	}
}

using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom formatter for the <see cref="T:UnityEngine.Keyframe" /> type.
	/// </summary>
	/// <seealso cref="!:MinimalBaseFormatter&lt;UnityEngine.Keyframe&gt;" />
	public class KeyframeFormatter : MinimalBaseFormatter<Keyframe>
	{
		private static readonly Serializer<float> FloatSerializer;

		private static readonly Serializer<int> IntSerializer;

		private static readonly bool Is_In_2018_1_Or_Above;

		private static IFormatter<Keyframe> Formatter;

		static KeyframeFormatter()
		{
			FloatSerializer = Serializer.Get<float>();
			IntSerializer = Serializer.Get<int>();
			Is_In_2018_1_Or_Above = typeof(Keyframe).GetProperty("weightedMode") != null;
			if (Is_In_2018_1_Or_Above)
			{
				if (EmitUtilities.CanEmit)
				{
					Formatter = (IFormatter<Keyframe>)FormatterEmitter.GetEmittedFormatter(typeof(Keyframe), SerializationPolicies.Everything);
				}
				else
				{
					Formatter = new ReflectionFormatter<Keyframe>(SerializationPolicies.Everything);
				}
			}
		}

		/// <summary>
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected override void Read(ref Keyframe value, IDataReader reader)
		{
			string name;
			EntryType first = reader.PeekEntry(out name);
			if (first == EntryType.Integer && name == "ver")
			{
				if (Formatter == null)
				{
					Formatter = new ReflectionFormatter<Keyframe>(SerializationPolicies.Everything);
				}
				reader.ReadInt32(out var _);
				value = Formatter.Deserialize(reader);
			}
			else
			{
				value.inTangent = FloatSerializer.ReadValue(reader);
				value.outTangent = FloatSerializer.ReadValue(reader);
				value.time = FloatSerializer.ReadValue(reader);
				value.value = FloatSerializer.ReadValue(reader);
				value.tangentMode = IntSerializer.ReadValue(reader);
			}
		}

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected override void Write(ref Keyframe value, IDataWriter writer)
		{
			if (Is_In_2018_1_Or_Above)
			{
				writer.WriteInt32("ver", 1);
				Formatter.Serialize(value, writer);
				return;
			}
			FloatSerializer.WriteValue(value.inTangent, writer);
			FloatSerializer.WriteValue(value.outTangent, writer);
			FloatSerializer.WriteValue(value.time, writer);
			FloatSerializer.WriteValue(value.value, writer);
			IntSerializer.WriteValue(value.tangentMode, writer);
		}
	}
}

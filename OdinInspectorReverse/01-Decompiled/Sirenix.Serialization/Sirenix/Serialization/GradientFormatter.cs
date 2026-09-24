using System;
using System.Reflection;
using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom formatter for the <see cref="T:UnityEngine.Gradient" /> type.
	/// </summary>
	/// <seealso cref="!:MinimalBaseFormatter&lt;UnityEngine.Gradient&gt;" />
	public class GradientFormatter : MinimalBaseFormatter<Gradient>
	{
		private static readonly Serializer<GradientAlphaKey[]> AlphaKeysSerializer = Serializer.Get<GradientAlphaKey[]>();

		private static readonly Serializer<GradientColorKey[]> ColorKeysSerializer = Serializer.Get<GradientColorKey[]>();

		private static readonly PropertyInfo ModeProperty = typeof(Gradient).GetProperty("mode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

		private static readonly Serializer<object> EnumSerializer = ((ModeProperty != null) ? Serializer.Get<object>() : null);

		protected override Gradient GetUninitializedObject()
		{
			return new Gradient();
		}

		/// <summary>
		/// Reads into the specified value using the specified reader.
		/// </summary>
		/// <param name="value">The value to read into.</param>
		/// <param name="reader">The reader to use.</param>
		protected override void Read(ref Gradient value, IDataReader reader)
		{
			value.alphaKeys = AlphaKeysSerializer.ReadValue(reader);
			value.colorKeys = ColorKeysSerializer.ReadValue(reader);
			reader.PeekEntry(out var name);
			if (!(name == "mode"))
			{
				return;
			}
			try
			{
				if (ModeProperty != null)
				{
					ModeProperty.SetValue(value, EnumSerializer.ReadValue(reader), null);
				}
				else
				{
					reader.SkipEntry();
				}
			}
			catch (Exception)
			{
				reader.Context.Config.DebugContext.LogWarning("Failed to read Gradient.mode, due to Unity's API disallowing setting of this member on other threads than the main thread. Gradient.mode value will have been lost.");
			}
		}

		/// <summary>
		/// Writes from the specified value using the specified writer.
		/// </summary>
		/// <param name="value">The value to write from.</param>
		/// <param name="writer">The writer to use.</param>
		protected override void Write(ref Gradient value, IDataWriter writer)
		{
			AlphaKeysSerializer.WriteValue(value.alphaKeys, writer);
			ColorKeysSerializer.WriteValue(value.colorKeys, writer);
			if (ModeProperty != null)
			{
				try
				{
					EnumSerializer.WriteValue("mode", ModeProperty.GetValue(value, null), writer);
				}
				catch (Exception)
				{
					writer.Context.Config.DebugContext.LogWarning("Failed to write Gradient.mode, due to Unity's API disallowing setting of this member on other threads than the main thread. Gradient.mode will have been lost upon deserialization.");
				}
			}
		}
	}
}

using System;

namespace Sirenix.Reflection.Editor
{
	public struct LayoutSize
	{
		public float Value;

		public SizeMode Type;

		public static LayoutSize Auto = new LayoutSize(0f, SizeMode.Auto);

		internal float PixelsOrZero
		{
			get
			{
				if (Type != SizeMode.Pixels)
				{
					return 0f;
				}
				return Value;
			}
		}

		internal float PercentageOrZero
		{
			get
			{
				if (Type != SizeMode.Percentage)
				{
					return 0f;
				}
				return Value;
			}
		}

		public LayoutSize(float value, SizeMode sizeType)
		{
			this = default(LayoutSize);
			Value = value;
			Type = sizeType;
		}

		public static LayoutSize Percentage(float percentage)
		{
			return new LayoutSize(percentage, SizeMode.Percentage);
		}

		public static LayoutSize Pixels(float pixels)
		{
			return new LayoutSize(pixels, SizeMode.Pixels);
		}

		public static implicit operator LayoutSize(float val)
		{
			return new LayoutSize(val, SizeMode.Pixels);
		}

		public static implicit operator LayoutSize(SizeMode type)
		{
			return new LayoutSize(0f, type);
		}

		public override string ToString()
		{
			return $"Type: {Type}, Value: {Value}";
		}

		public float GetSizeInPixels(float width)
		{
			if (Type == SizeMode.Percentage)
			{
				return Value * width;
			}
			if (Type == SizeMode.Pixels)
			{
				return Value;
			}
			throw new NotImplementedException();
		}
	}
}

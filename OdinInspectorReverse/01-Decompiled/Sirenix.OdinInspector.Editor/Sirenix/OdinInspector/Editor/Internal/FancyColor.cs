using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	public struct FancyColor : IFormattable, IEquatable<FancyColor>
	{
		public enum BlendMode
		{
			Normal,
			Multiply,
			Screen,
			Overlay
		}

		public struct BlendLayer
		{
			public FancyColor Top;

			public BlendMode BlendMode;
		}

		public static readonly Stack<BlendLayer> BlendStack = new Stack<BlendLayer>();

		public const float EQUATABLE_THRESHOLD = 0.0019607844f;

		private const float MAX_BYTE_F = 255f;

		public float R;

		public float G;

		public float B;

		public float A;

		public static FancyColor White => new FancyColor(1f);

		public static FancyColor Gray => new FancyColor(0.5f);

		public static FancyColor Black => new FancyColor(0f);

		public static FancyColor Red => new FancyColor(1f, 0f, 0f);

		public static FancyColor Blue => new FancyColor(0f, 0f, 1f);

		public static FancyColor Green => new FancyColor(0f, 1f, 0f);

		public static FancyColor Yellow => new FancyColor(1f, 1f, 0f);

		public static FancyColor Cyan => new FancyColor(0f, 1f, 1f);

		public static FancyColor Magenta => new FancyColor(1f, 0f, 1f);

		public static FancyColor Clear => new FancyColor(0f, 0f);

		public FancyColor Inverse => new FancyColor(InverseR, InverseG, InverseB, InverseA);

		public float InverseR => 1f - R;

		public float InverseG => 1f - G;

		public float InverseB => 1f - B;

		public float InverseA => 1f - A;

		public byte ByteR => (byte)Mathf.Round(Mathf.Clamp01(R) * 255f);

		public byte ByteG => (byte)Mathf.Round(Mathf.Clamp01(G) * 255f);

		public byte ByteB => (byte)Mathf.Round(Mathf.Clamp01(B) * 255f);

		public byte ByteA => (byte)Mathf.Round(Mathf.Clamp01(A) * 255f);

		public float this[int index]
		{
			get
			{
				return index switch
				{
					0 => R, 
					1 => G, 
					2 => B, 
					3 => A, 
					_ => throw new ArgumentOutOfRangeException($"Expected index within the range of 0..3, the actual index was: {index}"), 
				};
			}
			set
			{
				switch (index)
				{
				case 0:
					R = value;
					break;
				case 1:
					G = value;
					break;
				case 2:
					B = value;
					break;
				case 3:
					A = value;
					break;
				default:
					throw new ArgumentOutOfRangeException($"Expected index within the range of 0..3, the actual index was: {index}");
				}
			}
		}

		public static void PushBlend(FancyColor color, BlendMode blendMode)
		{
			BlendStack.Push(new BlendLayer
			{
				Top = color,
				BlendMode = blendMode
			});
		}

		public static void PopBlend()
		{
			if (BlendStack.Count > 0)
			{
				BlendStack.Pop();
			}
		}

		public FancyColor(float r, float g, float b, float a = 1f)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public FancyColor(float value, float a = 1f)
		{
			R = value;
			G = value;
			B = value;
			A = a;
		}

		public static FancyColor Create32(byte r, byte g, byte b, byte a = byte.MaxValue)
		{
			return new FancyColor
			{
				R = (float)(int)r / 255f,
				G = (float)(int)g / 255f,
				B = (float)(int)b / 255f,
				A = (float)(int)a / 255f
			};
		}

		public static FancyColor Create32(byte value, byte a = byte.MaxValue)
		{
			float valueFloat = (float)(int)value / 255f;
			return new FancyColor
			{
				R = valueFloat,
				G = valueFloat,
				B = valueFloat,
				A = (float)(int)a / 255f
			};
		}

		public static FancyColor CreateHex(int rgb, float a = 1f)
		{
			return new FancyColor
			{
				R = (float)((rgb >> 16) & 0xFF) / 255f,
				G = (float)((rgb >> 8) & 0xFF) / 255f,
				B = (float)(rgb & 0xFF) / 255f,
				A = a
			};
		}

		public static FancyColor CreateHtmlString(string htmlHex)
		{
			if (ColorUtility.TryParseHtmlString(htmlHex, out var color))
			{
				return new FancyColor
				{
					R = color.r,
					G = color.g,
					B = color.b,
					A = color.a
				};
			}
			Debug.LogError("Failed to parse html-hex-string: " + htmlHex);
			return Magenta;
		}

		public static implicit operator FancyColor(Color color)
		{
			return new FancyColor(color.r, color.g, color.b, color.a);
		}

		public static implicit operator Color(FancyColor fancyColor)
		{
			float r;
			float g;
			float b;
			float a;
			if (BlendStack.Count > 0)
			{
				BlendLayer currentBlendItem = BlendStack.Peek();
				FancyColor blendResult = fancyColor.Blend(currentBlendItem.Top, currentBlendItem.BlendMode);
				r = blendResult.R;
				g = blendResult.G;
				b = blendResult.B;
				a = blendResult.A;
			}
			else
			{
				r = fancyColor.R;
				g = fancyColor.G;
				b = fancyColor.B;
				a = fancyColor.A;
			}
			return new Color(r, g, b, a);
		}

		public static implicit operator FancyColor(Color32 color32)
		{
			return new FancyColor((int)color32.r, (int)color32.g, (int)color32.b, (int)color32.a);
		}

		public static implicit operator Color32(FancyColor fancyColor)
		{
			return new Color32(fancyColor.ByteR, fancyColor.ByteG, fancyColor.ByteB, fancyColor.ByteA);
		}

		public void Deconstruct(out float r, out float g, out float b)
		{
			r = R;
			g = G;
			b = B;
		}

		public void Deconstruct(out float r, out float g, out float b, out float a)
		{
			r = R;
			g = G;
			b = B;
			a = A;
		}

		public static bool operator ==(FancyColor self, FancyColor other)
		{
			return self.Equals(other);
		}

		public static bool operator !=(FancyColor self, FancyColor other)
		{
			return !self.Equals(other);
		}

		public static FancyColor operator +(FancyColor self, FancyColor other)
		{
			return new FancyColor(self.R + other.R, self.G + other.G, self.B + other.B, self.A + other.A);
		}

		public static FancyColor operator -(FancyColor self, FancyColor other)
		{
			return new FancyColor(self.R - other.R, self.G - other.G, self.B - other.B, self.A - other.A);
		}

		public static FancyColor operator *(FancyColor self, FancyColor other)
		{
			float r = ((self.R > 1f) ? (1f * other.R + self.R - 1f) : (self.R * other.R));
			float g = ((self.G > 1f) ? (1f * other.G + self.G - 1f) : (self.G * other.G));
			float b = ((self.B > 1f) ? (1f * other.B + self.B - 1f) : (self.B * other.B));
			float a = ((self.A > 1f) ? (1f * other.A + self.A - 1f) : (self.A * other.A));
			return new FancyColor(r, g, b, a);
		}

		public static FancyColor operator *(FancyColor self, float value)
		{
			float r = ((self.R > 1f) ? (1f * value + self.R - 1f) : (self.R * value));
			float g = ((self.G > 1f) ? (1f * value + self.G - 1f) : (self.G * value));
			float b = ((self.B > 1f) ? (1f * value + self.B - 1f) : (self.B * value));
			float a = ((self.A > 1f) ? (1f * value + self.A - 1f) : (self.A * value));
			return new FancyColor(r, g, b, a);
		}

		public static FancyColor operator *(float value, FancyColor self)
		{
			float r = ((self.R > 1f) ? (value * 1f + self.R - 1f) : (value * self.R));
			float g = ((self.G > 1f) ? (value * 1f + self.G - 1f) : (value * self.G));
			float b = ((self.B > 1f) ? (value * 1f + self.B - 1f) : (value * self.B));
			float a = ((self.A > 1f) ? (value * 1f + self.A - 1f) : (value * self.A));
			return new FancyColor(r, g, b, a);
		}

		public static bool operator <(FancyColor self, float value)
		{
			if (!(self.R < value) && !(self.G < value))
			{
				return self.B < value;
			}
			return true;
		}

		public static bool operator >(FancyColor self, float value)
		{
			if (!(self.R > value) && !(self.G > value))
			{
				return self.B > value;
			}
			return true;
		}

		public void Clamp()
		{
			R = Mathf.Clamp01(R);
			G = Mathf.Clamp01(G);
			B = Mathf.Clamp01(B);
			A = Mathf.Clamp01(A);
		}

		public FancyColor BakeBlends()
		{
			BlendLayer currentBlendItem = BlendStack.Peek();
			return Blend(currentBlendItem.Top, currentBlendItem.BlendMode);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			return $"(R: {R}, G: {G}, B: {B}, A: {A}) [{ToHexCode()}]";
		}

		public string ToHexCode()
		{
			return $"#{ByteR:X2}{ByteG:X2}{ByteB:X2}{ByteA:X2}";
		}

		public bool Equals(float value, int index)
		{
			return Math.Abs(this[index] - value) < 0.0019607844f;
		}

		public bool EqualsR(float value)
		{
			return Math.Abs(R - value) < 0.0019607844f;
		}

		public bool EqualsG(float value)
		{
			return Math.Abs(G - value) < 0.0019607844f;
		}

		public bool EqualsB(float value)
		{
			return Math.Abs(B - value) < 0.0019607844f;
		}

		public bool EqualsA(float value)
		{
			return Math.Abs(A - value) < 0.0019607844f;
		}

		public bool Equals(FancyColor other)
		{
			if (Math.Abs(R - other.R) < 0.0019607844f && Math.Abs(G - other.G) < 0.0019607844f && Math.Abs(B - other.B) < 0.0019607844f)
			{
				return Math.Abs(A - other.A) < 0.0019607844f;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is FancyColor other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int hashCode = R.GetHashCode();
			hashCode = (hashCode * 397) ^ G.GetHashCode();
			hashCode = (hashCode * 397) ^ B.GetHashCode();
			return (hashCode * 397) ^ A.GetHashCode();
		}

		public FancyColor Lerp(FancyColor target, float t)
		{
			FancyColor result = this;
			result.R = Mathf.Lerp(result.R, target.R, t);
			result.G = Mathf.Lerp(result.G, target.G, t);
			result.B = Mathf.Lerp(result.B, target.B, t);
			result.A = Mathf.Lerp(result.A, target.A, t);
			return result;
		}

		public float Luminosity(bool includeAlpha = true)
		{
			if (includeAlpha && A <= 0f)
			{
				return 0f;
			}
			float luminosity = 0.3f * R + 0.59f * G + 0.11f * B;
			if (includeAlpha && A < 1f)
			{
				luminosity *= A;
			}
			return luminosity;
		}

		public FancyColor InvertLuminosity()
		{
			float luminosity = Luminosity();
			float invertedLuminosity = 1f - luminosity;
			float luminosityMultiplier = invertedLuminosity / luminosity;
			return new FancyColor(R * luminosityMultiplier, G * luminosityMultiplier, B * luminosityMultiplier, A);
		}

		public FancyColor Blend(FancyColor top, BlendMode blendMode)
		{
			FancyColor result = blendMode switch
			{
				BlendMode.Normal => this, 
				BlendMode.Multiply => this * top, 
				BlendMode.Screen => (Inverse * top.Inverse).Inverse, 
				BlendMode.Overlay => (!(this < 0.5f)) ? (2f * Inverse * top.Inverse).Inverse : (2f * this * top), 
				_ => throw new ArgumentOutOfRangeException("blendMode", blendMode, null), 
			};
			result.Clamp();
			if (A < 1f || top.A < 1f)
			{
				result.R = Mathf.Lerp(result.R, R, top.InverseA);
				result.G = Mathf.Lerp(result.G, G, top.InverseA);
				result.B = Mathf.Lerp(result.B, B, top.InverseA);
				result.A = A;
			}
			return result;
		}
	}
}

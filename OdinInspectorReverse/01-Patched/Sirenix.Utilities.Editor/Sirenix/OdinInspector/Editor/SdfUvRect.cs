using System.Runtime.InteropServices;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	[StructLayout(LayoutKind.Explicit)]
	public struct SdfUvRect
	{
		public static readonly SdfUvRect Identity = new SdfUvRect(0f, 0f, 1f, 1f);

		[FieldOffset(0)]
		public Vector4 V4;

		[FieldOffset(0)]
		public Vector2 TL;

		[FieldOffset(0)]
		public float Left;

		[FieldOffset(0)]
		public float X;

		[FieldOffset(4)]
		public float Top;

		[FieldOffset(4)]
		public float Y;

		[FieldOffset(8)]
		public float Right;

		[FieldOffset(8)]
		public float Z;

		[FieldOffset(12)]
		public float Bottom;

		[FieldOffset(12)]
		public float W;

		public Vector2 Center => new Vector2((Left + Right) * 0.5f, (Top + Bottom) * 0.5f);

		public float Width
		{
			get
			{
				return Right - Left;
			}
			set
			{
				Right = Left + value;
			}
		}

		public float Height
		{
			get
			{
				return Bottom - Top;
			}
			set
			{
				Bottom = Top + value;
			}
		}

		public SdfUvRect(Vector4 v4)
		{
			this = default(SdfUvRect);
			V4 = v4;
		}

		public SdfUvRect(float left, float top, float right, float bottom)
		{
			this = default(SdfUvRect);
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

		public static Vector2 operator *(SdfUvRect region, Vector2 uv)
		{
			return new Vector2(region.X + uv.x * region.Width, region.Y + uv.y * region.Height);
		}

		public SdfUvRect Mul(SdfUvRect uv)
		{
			float x = Left;
			float y = Top;
			x += Width * uv.X;
			y += Height * uv.Y;
			return new SdfUvRect(x, y, x + Width * uv.Width, y + Height * uv.Height);
		}
	}
}

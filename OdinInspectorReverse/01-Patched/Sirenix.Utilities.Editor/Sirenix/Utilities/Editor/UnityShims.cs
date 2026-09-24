using System;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// <para>
	/// This is an internal class that is used solely in pre-built assembly versions of Odin, not in source distributions.
	/// Some Unity API's differ in different versions of the engine, like the Color, Color32, Rect and Vector2/3/4 structs.
	/// This class contains replacements or reimplementations of these Unity APIs. This class should not be used directly.
	/// </para>
	/// <para>
	/// At build time, Odin's assemblies are IL post-processed to redirect all relevant calls with shims into the implementations
	/// here instead of using the Unity versions of these APIs, such that Odin's pre-built assemblies work across a wide 
	/// range of Unity versions.
	/// </para>
	/// </summary>
	internal static class UnityShims
	{
		public static class Color
		{
			public static bool op_Equality(UnityEngine.Color a, UnityEngine.Color b)
			{
				if (a.r == b.r && a.g == b.g && a.b == b.b)
				{
					return a.a == b.a;
				}
				return false;
			}

			public static bool op_Inequality(UnityEngine.Color a, UnityEngine.Color b)
			{
				return !(a == b);
			}

			public static UnityEngine.Color op_Addition(UnityEngine.Color a, UnityEngine.Color b)
			{
				return new UnityEngine.Color(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a);
			}

			public static UnityEngine.Color op_Subtraction(UnityEngine.Color a, UnityEngine.Color b)
			{
				return new UnityEngine.Color(a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a);
			}

			public static UnityEngine.Color op_Multiply(UnityEngine.Color a, UnityEngine.Color b)
			{
				return new UnityEngine.Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
			}

			public static UnityEngine.Color op_Multiply(UnityEngine.Color color, float f)
			{
				return new UnityEngine.Color(color.r * f, color.g * f, color.b * f, color.a * f);
			}
		}

		public static class Color32
		{
			public static UnityEngine.Color op_Implicit(UnityEngine.Color32 c)
			{
				return new UnityEngine.Color((float)(int)c.r / 255f, (float)(int)c.g / 255f, (float)(int)c.b / 255f, (float)(int)c.a / 255f);
			}

			public static UnityEngine.Color32 op_Implicit(UnityEngine.Color c)
			{
				return new UnityEngine.Color32((byte)Mathf.Round(Mathf.Clamp01(c.r) * 255f), (byte)Mathf.Round(Mathf.Clamp01(c.g) * 255f), (byte)Mathf.Round(Mathf.Clamp01(c.b) * 255f), (byte)Mathf.Round(Mathf.Clamp01(c.a) * 255f));
			}
		}

		public static class Rect
		{
			public static bool op_Equality(UnityEngine.Rect a, UnityEngine.Rect b)
			{
				if (a.x == b.x && a.y == b.y && a.width == b.width)
				{
					return a.height == b.height;
				}
				return false;
			}

			public static bool op_Inequality(UnityEngine.Rect a, UnityEngine.Rect b)
			{
				return !(a == b);
			}

			[ShimName(".ctor")]
			public static UnityEngine.Rect Ctor(UnityEngine.Rect rect)
			{
				return rect;
			}

			[ShimName(".ctor")]
			public static void Ctor(out UnityEngine.Rect result, UnityEngine.Rect rect)
			{
				result = rect;
			}

			[ShimName(".ctor")]
			public static UnityEngine.Rect Ctor(UnityEngine.Vector2 position, UnityEngine.Vector2 size)
			{
				return new UnityEngine.Rect(position.x, position.y, size.x, size.y);
			}

			[ShimName(".ctor")]
			public static void Ctor(out UnityEngine.Rect rect, UnityEngine.Vector2 position, UnityEngine.Vector2 size)
			{
				rect = new UnityEngine.Rect(position.x, position.y, size.x, size.y);
			}
		}

		public static class Vector2
		{
			public static bool op_Equality(UnityEngine.Vector2 a, UnityEngine.Vector2 b)
			{
				if (a.x == b.x)
				{
					return a.y == b.y;
				}
				return false;
			}

			public static bool op_Inequality(UnityEngine.Vector2 a, UnityEngine.Vector2 b)
			{
				return !(a == b);
			}

			public static UnityEngine.Vector3 op_Implicit(UnityEngine.Vector2 vec)
			{
				return new UnityEngine.Vector3(vec.x, vec.y);
			}

			public static UnityEngine.Vector2 op_Multiply(UnityEngine.Vector2 vec, float f)
			{
				return new UnityEngine.Vector2(vec.x * f, vec.y * f);
			}

			public static UnityEngine.Vector2 op_Addition(UnityEngine.Vector2 a, UnityEngine.Vector2 b)
			{
				return new UnityEngine.Vector2(a.x + b.x, a.y + b.y);
			}

			public static UnityEngine.Vector2 op_Subtraction(UnityEngine.Vector2 a, UnityEngine.Vector2 b)
			{
				return new UnityEngine.Vector2(a.x - b.x, a.y - b.y);
			}

			public static UnityEngine.Vector2 op_UnaryNegation(UnityEngine.Vector2 vec)
			{
				return new UnityEngine.Vector2(0f - vec.x, 0f - vec.y);
			}
		}

		public static class Vector2Int
		{
			public static bool op_Equality(UnityEngine.Vector2Int a, UnityEngine.Vector2Int b)
			{
				if (a.x == b.x)
				{
					return a.y == b.y;
				}
				return false;
			}

			public static bool op_Inequality(UnityEngine.Vector2Int a, UnityEngine.Vector2Int b)
			{
				return !(a == b);
			}

			public static UnityEngine.Vector2 op_Implicit(UnityEngine.Vector2Int vec)
			{
				return new UnityEngine.Vector2(vec.x, vec.y);
			}
		}

		public static class Vector3
		{
			public static bool op_Equality(UnityEngine.Vector3 a, UnityEngine.Vector3 b)
			{
				if (a.x == b.x && a.y == b.y)
				{
					return a.z == b.z;
				}
				return false;
			}

			public static bool op_Inequality(UnityEngine.Vector3 a, UnityEngine.Vector3 b)
			{
				return !(a == b);
			}

			public static UnityEngine.Vector3 op_Addition(UnityEngine.Vector3 a, UnityEngine.Vector3 b)
			{
				return new UnityEngine.Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
			}
		}

		public static class Vector3Int
		{
			public static bool op_Equality(UnityEngine.Vector3Int a, UnityEngine.Vector3Int b)
			{
				if (a.x == b.x && a.y == b.y)
				{
					return a.z == b.z;
				}
				return false;
			}

			public static bool op_Inequality(UnityEngine.Vector3Int a, UnityEngine.Vector3Int b)
			{
				return !(a == b);
			}

			public static UnityEngine.Vector3 op_Implicit(UnityEngine.Vector3Int vec)
			{
				return new UnityEngine.Vector3(vec.x, vec.y, vec.z);
			}
		}

		public static class Vector4
		{
			public static bool op_Equality(UnityEngine.Vector4 a, UnityEngine.Vector4 b)
			{
				if (a.x == b.x && a.y == b.y && a.z == b.z)
				{
					return a.w == b.w;
				}
				return false;
			}

			public static bool op_Inequality(UnityEngine.Vector4 a, UnityEngine.Vector4 b)
			{
				return !(a == b);
			}

			public static UnityEngine.Vector4 op_Implicit(UnityEngine.Vector2 vec)
			{
				return new UnityEngine.Vector4(vec.x, vec.y);
			}

			public static UnityEngine.Vector4 op_Multiply(UnityEngine.Vector4 vec, float f)
			{
				return new UnityEngine.Vector4(vec.x * f, vec.y * f, vec.z * f, vec.w * f);
			}

			public static UnityEngine.Vector4 op_Implicit(UnityEngine.Vector3 vec)
			{
				return new UnityEngine.Vector4(vec.x, vec.y, vec.z);
			}

			[ShimName("op_Implicit")]
			public static UnityEngine.Vector2 op_ImplicitVec2(UnityEngine.Vector4 vec)
			{
				return new UnityEngine.Vector2(vec.x, vec.y);
			}

			[ShimName("op_Implicit")]
			public static UnityEngine.Vector3 op_ImplicitVec3(UnityEngine.Vector4 vec)
			{
				return new UnityEngine.Vector3(vec.x, vec.y, vec.z);
			}
		}

		public static class Quaternion
		{
			public static bool op_Equality(UnityEngine.Quaternion a, UnityEngine.Quaternion b)
			{
				if (a.x == b.x && a.y == b.y && a.z == b.z)
				{
					return a.w == b.w;
				}
				return false;
			}

			public static bool op_Inequality(UnityEngine.Quaternion a, UnityEngine.Quaternion b)
			{
				return !(a == b);
			}
		}

		public static class Matrix4x4
		{
			public static UnityEngine.Matrix4x4 op_Multiply(UnityEngine.Matrix4x4 a, UnityEngine.Matrix4x4 b)
			{
				UnityEngine.Matrix4x4 result = default(UnityEngine.Matrix4x4);
				result.m00 = a.m00 * b.m00 + a.m01 * b.m10 + a.m02 * b.m20 + a.m03 * b.m30;
				result.m01 = a.m00 * b.m01 + a.m01 * b.m11 + a.m02 * b.m21 + a.m03 * b.m31;
				result.m02 = a.m00 * b.m02 + a.m01 * b.m12 + a.m02 * b.m22 + a.m03 * b.m32;
				result.m03 = a.m00 * b.m03 + a.m01 * b.m13 + a.m02 * b.m23 + a.m03 * b.m33;
				result.m10 = a.m10 * b.m00 + a.m11 * b.m10 + a.m12 * b.m20 + a.m13 * b.m30;
				result.m11 = a.m10 * b.m01 + a.m11 * b.m11 + a.m12 * b.m21 + a.m13 * b.m31;
				result.m12 = a.m10 * b.m02 + a.m11 * b.m12 + a.m12 * b.m22 + a.m13 * b.m32;
				result.m13 = a.m10 * b.m03 + a.m11 * b.m13 + a.m12 * b.m23 + a.m13 * b.m33;
				result.m20 = a.m20 * b.m00 + a.m21 * b.m10 + a.m22 * b.m20 + a.m23 * b.m30;
				result.m21 = a.m20 * b.m01 + a.m21 * b.m11 + a.m22 * b.m21 + a.m23 * b.m31;
				result.m22 = a.m20 * b.m02 + a.m21 * b.m12 + a.m22 * b.m22 + a.m23 * b.m32;
				result.m23 = a.m20 * b.m03 + a.m21 * b.m13 + a.m22 * b.m23 + a.m23 * b.m33;
				result.m30 = a.m30 * b.m00 + a.m31 * b.m10 + a.m32 * b.m20 + a.m33 * b.m30;
				result.m31 = a.m30 * b.m01 + a.m31 * b.m11 + a.m32 * b.m21 + a.m33 * b.m31;
				result.m32 = a.m30 * b.m02 + a.m31 * b.m12 + a.m32 * b.m22 + a.m33 * b.m32;
				result.m33 = a.m30 * b.m03 + a.m31 * b.m13 + a.m32 * b.m23 + a.m33 * b.m33;
				return result;
			}
		}

		[Flags]
		public enum EventModifiers
		{
			None = 0,
			Shift = 1,
			Control = 2,
			Alt = 4,
			Command = 8,
			Numeric = 0x10,
			CapsLock = 0x20,
			FunctionKey = 0x40
		}

		public static class Misc
		{
			public static Func<Event, int> Event_ModifiersGetter;

			static Misc()
			{
				try
				{
					PropertyInfo modifiersProp = typeof(Event).GetProperty("modifiers");
					if (modifiersProp != null)
					{
						DynamicMethod builder = new DynamicMethod("UnityEngine_Event_ModifiersGetter", typeof(int), new Type[1] { typeof(Event) });
						ILGenerator il = builder.GetILGenerator();
						il.Emit(OpCodes.Ldarg_0);
						il.Emit(OpCodes.Call, modifiersProp.GetGetMethod());
						il.Emit(OpCodes.Ret);
						Event_ModifiersGetter = (Func<Event, int>)builder.CreateDelegate(typeof(Func<Event, int>));
					}
				}
				catch
				{
				}
				if (Event_ModifiersGetter == null)
				{
					Debug.LogError("The UnityEngine.Event.modifiers property could not be found in this version of Unity. Until this is fixed, Odin will be unable to react to modifier keys in the inspector, such as Ctrl and Shift.");
				}
			}

			public static int GetEventModifiers(Event @event)
			{
				if (Event_ModifiersGetter == null)
				{
					return 0;
				}
				return Event_ModifiersGetter(@event);
			}
		}
	}
}

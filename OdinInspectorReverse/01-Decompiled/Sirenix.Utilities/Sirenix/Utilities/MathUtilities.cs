using System;
using UnityEngine;

namespace Sirenix.Utilities
{
	/// <summary>
	/// Collection of math function.
	/// </summary>
	public static class MathUtilities
	{
		private const float ZERO_TOLERANCE = 1E-06f;

		/// <summary>
		/// Distance from a point to a line.
		/// </summary>
		public static float PointDistanceToLine(Vector3 point, Vector3 a, Vector3 b)
		{
			return Mathf.Abs((b.x - a.x) * (a.y - point.y) - (a.x - point.x) * (b.y - a.y)) / Mathf.Sqrt(Mathf.Pow(b.x - a.x, 2f) + Mathf.Pow(b.y - a.y, 2f));
		}

		/// <summary>
		/// Returns a smooth value between start and end based on t.
		/// </summary>
		/// <param name="start">First point.</param>
		/// <param name="end">Second point.</param>
		/// <param name="t">Position between 0 and 1.</param>
		public static float Hermite(float start, float end, float t)
		{
			return Mathf.Lerp(start, end, t * t * (3f - 2f * t));
		}

		/// <summary>
		/// Returns a smooth value between start and end based on t.
		/// </summary>
		/// <param name="start">First point.</param>
		/// <param name="end">Second point.</param>
		/// <param name="t">Position between 0 and 1.</param>
		/// <param name="count">Number of interpolations to make.</param>
		public static float StackHermite(float start, float end, float t, int count)
		{
			for (int i = 0; i < count; i++)
			{
				t = Hermite(start, end, t);
			}
			return t;
		}

		/// <summary>
		/// Returns the fractional of the value.
		/// </summary>
		/// <param name="value">The value to get the fractional of.</param>
		public static float Fract(float value)
		{
			return value - (float)Math.Truncate(value);
		}

		/// <summary>
		/// Returns the fractional of the value.
		/// </summary>
		/// <param name="value">The value to get the fractional of.</param>
		public static Vector2 Fract(Vector2 value)
		{
			return new Vector2(Fract(value.x), Fract(value.y));
		}

		/// <summary>
		/// Returns the fractional of the value.
		/// </summary>
		/// <param name="value">The value to get the fractional of.</param>
		public static Vector3 Fract(Vector3 value)
		{
			return new Vector3(Fract(value.x), Fract(value.y), Fract(value.z));
		}

		/// <summary>
		/// Returns a value based on t, that bounces faster and faster.
		/// </summary>
		/// <param name="t">The value to bounce.</param>
		public static float BounceEaseInFastOut(float t)
		{
			return Mathf.Cos(t * t * (float)Math.PI * 2f) * -0.5f + 0.5f;
		}

		/// <summary>
		/// Returns a smooth value between 0 and 1 based on t.
		/// </summary>
		/// <param name="t">Position between 0 and 1.</param>
		public static float Hermite01(float t)
		{
			return Mathf.Lerp(0f, 1f, t * t * (3f - 2f * t));
		}

		/// <summary>
		/// Returns a smooth value between 0 and 1 based on t.
		/// </summary>
		/// <param name="t">Position between 0 and 1.</param>
		/// <param name="count">Number of interpolations to make.</param>
		public static float StackHermite01(float t, int count)
		{
			for (int i = 0; i < count; i++)
			{
				t = Hermite01(t);
			}
			return t;
		}

		/// <summary>
		/// Returns an unclamped linear interpolation of two vectors.
		/// </summary>
		/// <param name="from">The first vector.</param>
		/// <param name="to">The second vector.</param>
		/// <param name="amount">The interpolation factor.</param>
		public static Vector3 LerpUnclamped(Vector3 from, Vector3 to, float amount)
		{
			return new Vector3(from.x + (to.x - from.x) * amount, from.y + (to.y - from.y) * amount, from.z + (to.z - from.z) * amount);
		}

		/// <summary>
		/// Returns an unclamped linear interpolation of two vectors.
		/// </summary>
		/// <param name="from">The first vector.</param>
		/// <param name="to">The second vector.</param>
		/// <param name="amount">The interpolation factor.</param>
		public static Vector2 LerpUnclamped(Vector2 from, Vector2 to, float amount)
		{
			return new Vector2(from.x + (to.x - from.x) * amount, from.y + (to.y - from.y) * amount);
		}

		/// <summary>
		/// Returns a value that bounces between 0 and 1 based on value.
		/// </summary>
		/// <param name="value">The value to bounce.</param>
		public static float Bounce(float value)
		{
			return Mathf.Abs(Mathf.Sin(value % 1f * (float)Math.PI));
		}

		/// <summary>
		/// Returns a value that eases in elasticly.
		/// </summary>
		/// <param name="value">The value to ease in elasticly.</param>
		/// <param name="amplitude">The amplitude.</param>
		/// <param name="length">The length.</param>
		public static float EaseInElastic(float value, float amplitude = 0.25f, float length = 0.6f)
		{
			value = Mathf.Clamp01(value);
			float slope = Mathf.Clamp01(value * 7.5f);
			float b = 1f - slope * slope * (3f - 2f * slope);
			float a = Mathf.Pow(1f - Mathf.Sin(Mathf.Min(value * (1f - length), 0.5f) * (float)Math.PI), 2f);
			float c = Mathf.Cos((float)Math.PI + value * 23f) * amplitude + b * (0f - (1f - amplitude));
			return 1f + c * a;
		}

		/// <summary>
		/// Pows each element of the vector.
		/// </summary>
		/// <param name="v">The vector.</param>
		/// <param name="p">The power.</param>
		public static Vector3 Pow(this Vector3 v, float p)
		{
			v.x = Mathf.Pow(v.x, p);
			v.y = Mathf.Pow(v.y, p);
			v.z = Mathf.Pow(v.z, p);
			return v;
		}

		/// <summary>
		/// Returns a Vector2 with each element set to their respective sign.
		/// </summary>
		/// <param name="v">The vector to sign.</param>
		public static Vector3 Abs(this Vector3 v)
		{
			v.x = Mathf.Abs(v.x);
			v.y = Mathf.Abs(v.y);
			v.z = Mathf.Abs(v.z);
			return v;
		}

		/// <summary>
		/// Returns a Vector3 with each element set to their respective sign.
		/// </summary>
		/// <param name="v">The vector to sign.</param>
		public static Vector3 Sign(this Vector3 v)
		{
			return new Vector3(Mathf.Sign(v.x), Mathf.Sign(v.y), Mathf.Sign(v.z));
		}

		/// <summary>
		/// Returns a value that eases out elasticly.
		/// </summary>
		/// <param name="value">The value to ease out elasticly.</param>
		/// <param name="amplitude">The amplitude.</param>
		/// <param name="length">The length.</param>
		public static float EaseOutElastic(float value, float amplitude = 0.25f, float length = 0.6f)
		{
			return 1f - EaseInElastic(1f - value, amplitude, length);
		}

		/// <summary>
		/// Returns a smooth value betweeen that peaks at t=0.5 and then comes back down again.
		/// </summary>
		/// <param name="t">A value between 0 and 1.</param>
		public static float EaseInOut(float t)
		{
			t = 1f - Mathf.Abs(Mathf.Clamp01(t) * 2f - 1f);
			t = t * t * (3f - 2f * t);
			return t;
		}

		/// <summary>
		/// Clamps the value of a Vector3.
		/// </summary>
		/// <param name="value">The vector to clamp.</param>
		/// <param name="min">The min value.</param>
		/// <param name="max">The max value.</param>
		public static Vector3 Clamp(this Vector3 value, Vector3 min, Vector3 max)
		{
			return new Vector3(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y), Mathf.Clamp(value.z, min.z, max.z));
		}

		/// <summary>
		/// Clamps the value of a Vector2.
		/// </summary>
		/// <param name="value">The vector to clamp.</param>
		/// <param name="min">The min value.</param>
		/// <param name="max">The max value.</param>
		public static Vector2 Clamp(this Vector2 value, Vector2 min, Vector2 max)
		{
			return new Vector2(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y));
		}

		/// <summary>
		/// Computes a hash for a byte array.
		/// </summary>
		/// <param name="data">The byte array.</param>
		public static int ComputeByteArrayHash(byte[] data)
		{
			int hash = -2128831035;
			for (int i = 0; i < data.Length; i++)
			{
				hash = (hash ^ data[i]) * 16777619;
			}
			hash += hash << 13;
			hash ^= hash >> 7;
			hash += hash << 3;
			hash ^= hash >> 17;
			return hash + (hash << 5);
		}

		/// <summary>
		/// Gives a smooth path between a collection of points.
		/// </summary>
		/// <param name="path">The collection of point.</param>
		/// <param name="t">The current position in the path. 0 is at the start of the path, 1 is at the end of the path.</param>
		public static Vector3 InterpolatePoints(Vector3[] path, float t)
		{
			t = Mathf.Clamp01(t * (1f - 1f / (float)path.Length));
			int lastI = path.Length - 1;
			int i = Mathf.FloorToInt(t * (float)path.Length);
			float f = t * (float)path.Length - (float)i;
			Vector3 a = path[Mathf.Max(0, --i)];
			Vector3 b = path[Mathf.Min(i + 1, lastI)];
			Vector3 c = path[Mathf.Min(i + 2, lastI)];
			Vector3 d = path[Mathf.Min(i + 3, lastI)];
			float f2 = f * f;
			float f3 = f2 * f;
			return new Vector3(0.5f * ((0f - a.x + 3f * b.x - 3f * c.x + d.x) * f3 + (2f * a.x - 5f * b.x + 4f * c.x - d.x) * f2 + (0f - a.x + c.x) * f + 2f * b.x), 0.5f * ((0f - a.y + 3f * b.y - 3f * c.y + d.y) * f3 + (2f * a.y - 5f * b.y + 4f * c.y - d.y) * f2 + (0f - a.y + c.y) * f + 2f * b.y), 0.5f * ((0f - a.z + 3f * b.z - 3f * c.z + d.z) * f3 + (2f * a.z - 5f * b.z + 4f * c.z - d.z) * f2 + (0f - a.z + c.z) * f + 2f * b.z));
		}

		/// <summary>
		/// Checks if two given lines intersect with one another and returns the intersection point (if
		/// any) in an out parameter.
		/// Source: http://stackoverflow.com/questions/3746274/line-intersection-with-aabb-rectangle.
		/// Edited to implement Cohen-Sutherland type pruning for efficiency.
		/// </summary>
		/// <param name="a1">Starting point of line a.</param>
		/// <param name="a2">Ending point of line a.</param>
		/// <param name="b1">Starting point of line b.</param>
		/// <param name="b2">Ending point of line b.</param>
		/// <param name="intersection">
		/// The out parameter which contains the intersection point if there was any.
		/// </param>
		/// <returns>True if the two lines intersect, otherwise false.</returns>
		public static bool LineIntersectsLine(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection)
		{
			intersection = Vector2.zero;
			Vector2 boundsLowerLeft = new Vector2((b1.x < b2.x) ? b1.x : b2.x, (b1.y > b2.y) ? b1.y : b2.y);
			Vector2 boundsUpperRight = new Vector2((b1.x > b2.x) ? b1.x : b2.x, (b1.y < b2.y) ? b1.y : b2.y);
			if ((a1.x < boundsLowerLeft.x && a2.x < boundsLowerLeft.x) || (a1.y > boundsLowerLeft.y && a2.y > boundsLowerLeft.y) || (a1.x > boundsUpperRight.x && a2.x > boundsUpperRight.x) || (a1.y < boundsUpperRight.y && a2.y < boundsUpperRight.y))
			{
				return false;
			}
			Vector2 b3 = new Vector2(a2.x - a1.x, a2.y - a1.y);
			Vector2 d = new Vector2(b2.x - b1.x, b2.y - b1.y);
			float dot = b3.x * d.y - b3.y * d.x;
			if (dot == 0f)
			{
				return false;
			}
			Vector2 c = new Vector2(b1.x - a1.x, b1.y - a1.y);
			float t = (c.x * d.y - c.y * d.x) / dot;
			if (t < 0f || t > 1f)
			{
				return false;
			}
			float u = (c.x * b3.y - c.y * b3.x) / dot;
			if (u < 0f || u > 1f)
			{
				return false;
			}
			intersection = new Vector2(a1.x + t * b3.x, a1.y + t * b3.y);
			return true;
		}

		/// <summary>
		/// Returns the collision point between two infinite lines.
		/// </summary>
		public static Vector2 InfiniteLineIntersect(Vector2 ps1, Vector2 pe1, Vector2 ps2, Vector2 pe2)
		{
			float a1 = pe1.y - ps1.y;
			float b1 = ps1.x - pe1.x;
			float c1 = a1 * ps1.x + b1 * ps1.y;
			float a2 = pe2.y - ps2.y;
			float b2 = ps2.x - pe2.x;
			float c2 = a2 * ps2.x + b2 * ps2.y;
			float delta = a1 * b2 - a2 * b1;
			if (delta == 0f)
			{
				throw new Exception("Lines are parallel");
			}
			return new Vector2((b2 * c1 - b1 * c2) / delta, (a1 * c2 - a2 * c1) / delta);
		}

		/// <summary>
		/// Distance from line to plane.
		/// </summary>
		/// <param name="planeOrigin">Position of the plane.</param>
		/// <param name="planeNormal">Surface normal of the plane.</param>
		/// <param name="lineOrigin">Origin of the line.</param>
		/// <param name="lineDirectionNormalized">Line direction normal.</param>
		public static float LineDistToPlane(Vector3 planeOrigin, Vector3 planeNormal, Vector3 lineOrigin, Vector3 lineDirectionNormalized)
		{
			return Vector3.Dot(lineDirectionNormalized, planeNormal) * Vector3.Distance(planeOrigin, lineOrigin);
		}

		/// <summary>
		/// Distance from ray to plane.
		/// </summary>
		/// <param name="ray">The ray.</param>
		/// <param name="plane">The plane.</param>
		public static float RayDistToPlane(Ray ray, Plane plane)
		{
			float direction = Vector3.Dot(plane.normal, ray.direction);
			if (Mathf.Abs(direction) < 1E-06f)
			{
				return 0f;
			}
			float position = Vector3.Dot(plane.normal, ray.origin);
			return (0f - plane.distance - position) / direction;
		}

		/// <summary>
		/// Rotates a Vector2 by an angle.
		/// </summary>
		/// <param name="point">The point to rotate.</param>
		/// <param name="degrees">The angle to rotate.</param>
		public static Vector2 RotatePoint(Vector2 point, float degrees)
		{
			float angleInRadians = degrees * ((float)Math.PI / 180f);
			float cosTheta = Mathf.Cos(angleInRadians);
			float sinTheta = Mathf.Sin(angleInRadians);
			return new Vector2(cosTheta * point.x - sinTheta * point.y, sinTheta * point.x + cosTheta * point.y);
		}

		/// <summary>
		/// Rotates a Vector2 around a point by an angle..
		/// </summary>
		/// <param name="point">The point to rotate.</param>
		/// <param name="around">The point to rotate around.</param>
		/// <param name="degrees">The angle to rotate.</param>
		public static Vector2 RotatePoint(Vector2 point, Vector2 around, float degrees)
		{
			float angleInRadians = degrees * ((float)Math.PI / 180f);
			float cosTheta = Mathf.Cos(angleInRadians);
			float sinTheta = Mathf.Sin(angleInRadians);
			return new Vector2(cosTheta * (point.x - around.x) - sinTheta * (point.y - around.y) + around.x, sinTheta * (point.x - around.x) + cosTheta * (point.y - around.y) + around.y);
		}

		/// <summary>
		/// Interpolates t between a and b to a value between 0 and 1 using a Hermite polynomial.
		/// </summary>
		/// <param name="a">The first value.</param>
		/// <param name="b">The second value.</param>
		/// <param name="t">The position value.</param>
		/// <returns>A smoothed value between 0 and 1.</returns>
		public static float SmoothStep(float a, float b, float t)
		{
			t = Mathf.Clamp01((t - a) / (b - a));
			return t * t * (3f - 2f * t);
		}

		/// <summary>
		/// Interpolates t between a and b to a value between 0 and 1.
		/// </summary>
		/// <param name="a">The first value.</param>
		/// <param name="b">The second value.</param>
		/// <param name="t">The position value.</param>
		/// <returns>Linear value between 0 and 1.</returns>
		public static float LinearStep(float a, float b, float t)
		{
			return Mathf.Clamp01((t - a) / (b - a));
		}

		/// <summary>
		/// Wraps a value between min and max.
		/// </summary>
		/// <param name="value">The value to wrap.</param>
		/// <param name="min">The minimum value.</param>
		/// <param name="max">The maximum value.</param>
		public static double Wrap(double value, double min, double max)
		{
			double range = max - min;
			range = ((range < 0.0) ? (0.0 - range) : range);
			if (value < min)
			{
				return value + range * Math.Ceiling(Math.Abs(value / range));
			}
			if (value >= max)
			{
				return value - range * Math.Floor(Math.Abs(value / range));
			}
			return value;
		}

		/// <summary>
		/// Wraps a value between min and max.
		/// </summary>
		/// <param name="value">The value to wrap.</param>
		/// <param name="min">The minimum value.</param>
		/// <param name="max">The maximum value.</param>
		public static float Wrap(float value, float min, float max)
		{
			float range = max - min;
			range = (((double)range < 0.0) ? (0f - range) : range);
			if (value < min)
			{
				return value + range * (float)Math.Ceiling(Math.Abs(value / range));
			}
			if (value >= max)
			{
				return value - range * (float)Math.Floor(Math.Abs(value / range));
			}
			return value;
		}

		/// <summary>
		/// Wraps a value between min and max.
		/// </summary>
		/// <param name="value">The value to wrap.</param>
		/// <param name="min">The minimum value.</param>
		/// <param name="max">The maximum value.</param>
		public static int Wrap(int value, int min, int max)
		{
			int range = max - min;
			range = ((range < 0) ? (-range) : range);
			if (value < min)
			{
				return value + range * (Math.Abs(value / range) + 1);
			}
			if (value >= max)
			{
				return value - range * Math.Abs(value / range);
			}
			return value;
		}

		/// <summary>
		/// Rounds a number based on a mininum difference.
		/// </summary>
		/// <param name="valueToRound">The value to round.</param>
		/// <param name="minDifference">The min difference.</param>
		/// <returns>The rounded value.</returns>
		public static double RoundBasedOnMinimumDifference(double valueToRound, double minDifference)
		{
			if (minDifference == 0.0)
			{
				return DiscardLeastSignificantDecimal(valueToRound);
			}
			return (float)Math.Round(valueToRound, GetNumberOfDecimalsForMinimumDifference(minDifference), MidpointRounding.AwayFromZero);
		}

		/// <summary>
		/// Discards the least significant demicals.
		/// </summary>
		/// <param name="v">The value of insignificant decimals.</param>
		/// <returns>Value with significant decimals.</returns>
		public static double DiscardLeastSignificantDecimal(double v)
		{
			int digits = Math.Max(0, (int)(5.0 - Math.Log10(Math.Abs(v))));
			try
			{
				return Math.Round(v, digits);
			}
			catch (ArgumentOutOfRangeException)
			{
				return 0.0;
			}
		}

		/// <summary>
		/// Clamps and wraps an angle between two values.
		/// </summary>
		public static float ClampWrapAngle(float angle, float min, float max)
		{
			float oneRevolution = 360f;
			float sMin = min;
			float sMax = max;
			float sAngle = angle;
			if (sMin < 0f)
			{
				sMin = sMin % oneRevolution + oneRevolution;
			}
			if (sMax < 0f)
			{
				sMax = sMax % oneRevolution + oneRevolution;
			}
			if (sAngle < 0f)
			{
				sAngle = sAngle % oneRevolution + oneRevolution;
			}
			float offset = (float)(int)(Math.Abs(min - max) / oneRevolution) * oneRevolution;
			sMax += offset;
			sAngle += offset;
			if (min > max)
			{
				sMax += oneRevolution;
			}
			if (sAngle < sMin)
			{
				sAngle = sMin;
			}
			if (sAngle > sMax)
			{
				sAngle = sMax;
			}
			return sAngle;
		}

		private static int GetNumberOfDecimalsForMinimumDifference(double minDifference)
		{
			return Mathf.Clamp(-Mathf.FloorToInt(Mathf.Log10(Mathf.Abs((float)minDifference))), 0, 15);
		}
	}
}

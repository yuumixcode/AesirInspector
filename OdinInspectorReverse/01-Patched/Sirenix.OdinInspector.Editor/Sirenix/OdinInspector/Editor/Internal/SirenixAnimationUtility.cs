using System;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	public static class SirenixAnimationUtility
	{
		public struct InterpolatedFloat
		{
			public float Start;

			public float Destination;

			public float Time;

			public Easing Easing;

			public bool IsDone => Time >= 1f;

			public bool HasBegun => Time > 0f;

			public float GetValue()
			{
				if (Time <= 0f)
				{
					return Start;
				}
				if (IsDone)
				{
					return Destination;
				}
				return Interpolate(Start, Destination, Time, Easing);
			}

			public void Move(float speed, Easing easing = Easing.Linear)
			{
				if (!IsDone)
				{
					Easing = easing;
					Time += GUITimeHelper.LayoutDeltaTime * speed;
					Time = Mathf.Clamp01(Time);
					GUIHelper.RequestRepaint();
				}
			}

			public void Reverse(float speed, Easing easing = Easing.Linear)
			{
				Easing = easing;
				Time -= GUITimeHelper.LayoutDeltaTime * speed;
				Time = Mathf.Clamp01(Time);
				GUIHelper.RequestRepaint();
			}

			public void ChangeDestination(float destination)
			{
				if (IsDone)
				{
					Start = Destination;
				}
				else if (HasBegun)
				{
					Start = GetValue();
				}
				Time = 0f;
				Destination = destination;
			}

			public void Reset()
			{
				Reset(Start);
			}

			public void Reset(float start)
			{
				Start = start;
				Time = 0f;
			}

			public static implicit operator InterpolatedFloat(float value)
			{
				return new InterpolatedFloat
				{
					Start = value,
					Time = 0f
				};
			}

			public static implicit operator float(InterpolatedFloat interpolatedFloat)
			{
				return interpolatedFloat.GetValue();
			}
		}

		public struct InterpolatedVector2
		{
			public Vector2 Start;

			public Vector2 Destination;

			public float Time;

			public Easing Easing;

			public bool IsDone => Time >= 1f;

			public bool HasBegun => Time > 0f;

			public Vector2 GetValue()
			{
				return new Vector2(Interpolate(Start.x, Destination.x, Time, Easing), Interpolate(Start.y, Destination.y, Time, Easing));
			}

			public void Move(float speed, Easing easing = Easing.Linear)
			{
				if (!IsDone)
				{
					Easing = easing;
					Time += GUITimeHelper.LayoutDeltaTime * speed;
					Time = Mathf.Clamp01(Time);
					GUIHelper.RequestRepaint();
				}
			}

			public void ChangeDestination(Vector2 destination)
			{
				if (IsDone)
				{
					Start = Destination;
				}
				else if (HasBegun)
				{
					Start = GetValue();
				}
				Time = 0f;
				Destination = destination;
			}

			public void Reset()
			{
				Reset(Start);
			}

			public void Reset(Vector2 start)
			{
				Start = start;
				Time = 0f;
			}

			public static implicit operator InterpolatedVector2(Vector2 value)
			{
				return new InterpolatedVector2
				{
					Start = value,
					Time = 0f
				};
			}

			public static implicit operator Vector2(InterpolatedVector2 interpolatedVector2)
			{
				return interpolatedVector2.GetValue();
			}
		}

		private static readonly object AnimationTempRefKey = new object();

		public static float Interpolate(float start, float end, float time, Easing easing = Easing.Linear)
		{
			return Mathf.Lerp(start, end, ApplyEasingFunction(time, easing));
		}

		public static Vector2 Interpolate(Vector2 start, Vector2 end, float time, Easing easing = Easing.Linear)
		{
			float easeT = ApplyEasingFunction(time, easing);
			return new Vector2(Mathf.Lerp(start.x, end.x, easeT), Mathf.Lerp(start.y, end.y, easeT));
		}

		public static ref InterpolatedFloat GetTemporaryFloat(object key, float defaultValue)
		{
			GUIContext<InterpolatedFloat> result = GUIHelper.GetTemporaryContext(AnimationTempRefKey, key, (InterpolatedFloat)defaultValue);
			return ref result.Value;
		}

		private static float EaseOutBounce(float value)
		{
			if (value < 0.36363637f)
			{
				return 7.5625f * value * value;
			}
			if (value < 0.72727275f)
			{
				return 7.5625f * (value -= 0.54545456f) * value + 0.75f;
			}
			if (value < 0.90909094f)
			{
				return 7.5625f * (value -= 0.8181818f) * value + 0.9375f;
			}
			return 7.5625f * (value -= 21f / 22f) * value + 63f / 64f;
		}

		public static float ApplyEasingFunction(float t, Easing function)
		{
			switch (function)
			{
			case Easing.None:
				return 0f;
			case Easing.Linear:
				return t;
			case Easing.InSine:
				return 1f - Mathf.Cos(t * (float)Math.PI * 0.5f);
			case Easing.OutSine:
				return Mathf.Sin(t * (float)Math.PI * 0.5f);
			case Easing.InOutSine:
				return (0f - (Mathf.Cos((float)Math.PI * t) - 1f)) * 0.5f;
			case Easing.InQuad:
				return t * t;
			case Easing.OutQuad:
				return 1f - (1f - t) * (1f - t);
			case Easing.InOutQuad:
				if (!(t < 0.5f))
				{
					return 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;
				}
				return 2f * t * t;
			case Easing.InCubic:
				return t * t * t;
			case Easing.OutCubic:
				return 1f - Mathf.Pow(1f - t, 3f);
			case Easing.InOutCubic:
				if (!(t < 0.5f))
				{
					return 1f - Mathf.Pow(-2f * t + 2f, 3f) * 0.5f;
				}
				return 4f * t * t * t;
			case Easing.InQuart:
				return t * t * t * t;
			case Easing.OutQuart:
				return 1f - Mathf.Pow(1f - t, 4f);
			case Easing.InOutQuart:
				if (!(t < 0.5f))
				{
					return 1f - Mathf.Pow(-2f * t + 2f, 4f) * 0.5f;
				}
				return 8f * t * t * t * t;
			case Easing.InQuint:
				return t * t * t * t * t;
			case Easing.OutQuint:
				return 1f - Mathf.Pow(1f - t, 5f);
			case Easing.InOutQuint:
				if (!(t < 0.5f))
				{
					return 1f - Mathf.Pow(-2f * t + 2f, 5f) * 0.5f;
				}
				return 16f * t * t * t * t * t;
			case Easing.InExpo:
				if (!(t <= 0f))
				{
					return Mathf.Pow(2f, 10f * t - 10f);
				}
				return 0f;
			case Easing.OutExpo:
				if (!(t >= 1f))
				{
					return 1f - Mathf.Pow(2f, -10f * t);
				}
				return 1f;
			case Easing.InOutExpo:
				if (t <= 0f)
				{
					return 0f;
				}
				if (t >= 1f)
				{
					return 1f;
				}
				if (!(t < 0.5f))
				{
					return (2f - Mathf.Pow(2f, -20f * t + 10f)) * 0.5f;
				}
				return Mathf.Pow(2f, 20f * t - 10f) * 0.5f;
			case Easing.InCirc:
				return 1f - Mathf.Sqrt(1f - Mathf.Pow(t, 2f));
			case Easing.OutCirc:
				return Mathf.Sqrt(1f - Mathf.Pow(t - 1f, 2f));
			case Easing.InOutCirc:
				if (!(t < 0.5f))
				{
					return (Mathf.Sqrt(1f - Mathf.Pow(-2f * t + 2f, 2f)) + 1f) * 0.5f;
				}
				return (1f - Mathf.Sqrt(1f - Mathf.Pow(2f * t, 2f))) * 0.5f;
			case Easing.InBack:
				return 2.70158f * t * t * t - 1.70158f * t * t;
			case Easing.OutBack:
				return 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);
			case Easing.InOutBack:
				if (t < 0.5f)
				{
					return Mathf.Pow(2f * t, 2f) * (7.189819f * t - 2.5949094f) * 0.5f;
				}
				return (Mathf.Pow(2f * t - 2f, 2f) * (3.5949094f * (t * 2f - 2f) + 2.5949094f) + 2f) * 0.5f;
			case Easing.InElastic:
				if (t <= 0f)
				{
					return 0f;
				}
				if (t >= 1f)
				{
					return 1f;
				}
				return (0f - Mathf.Pow(2f, 10f * t - 10f)) * Mathf.Sin((t * 10f - 10.75f) * ((float)Math.PI * 2f / 3f));
			case Easing.OutElastic:
				if (t <= 0f)
				{
					return 0f;
				}
				if (t >= 1f)
				{
					return 1f;
				}
				return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * ((float)Math.PI * 2f / 3f)) + 1f;
			case Easing.InOutElastic:
				if (t <= 0f)
				{
					return 0f;
				}
				if (t >= 1f)
				{
					return 1f;
				}
				if (t < 0.5f)
				{
					return (0f - Mathf.Pow(2f, 20f * t - 10f) * Mathf.Sin((20f * t - 11.125f) * ((float)Math.PI * 4f / 9f))) * 0.5f;
				}
				return Mathf.Pow(2f, -20f * t + 10f) * Mathf.Sin((20f * t - 11.125f) * ((float)Math.PI * 4f / 9f)) * 0.5f + 1f;
			case Easing.InBounce:
				return 1f - EaseOutBounce(1f - t);
			case Easing.OutBounce:
				return EaseOutBounce(t);
			case Easing.InOutBounce:
				if (!(t < 0.5f))
				{
					return (1f + EaseOutBounce(2f * t - 1f)) * 0.5f;
				}
				return (1f - EaseOutBounce(1f - 2f * t)) * 0.5f;
			default:
				throw new ArgumentOutOfRangeException("function", function, null);
			}
		}
	}
}

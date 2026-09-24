using System.Diagnostics;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	internal class OdinAssert
	{
		[Conditional("SIRENIX_INTERNAL")]
		public static void Log(object message)
		{
			UnityEngine.Debug.Log(message);
		}

		[Conditional("SIRENIX_INTERNAL")]
		public static void LogError(object message)
		{
			UnityEngine.Debug.LogError(message);
		}

		[Conditional("SIRENIX_INTERNAL")]
		public static void Assert(bool condition)
		{
			if (!condition)
			{
				UnityEngine.Debug.DebugBreak();
			}
		}

		[Conditional("SIRENIX_INTERNAL")]
		public static void Assert(bool condition, Object context)
		{
			if (!condition)
			{
				UnityEngine.Debug.DebugBreak();
			}
		}

		[Conditional("SIRENIX_INTERNAL")]
		public static void Assert(bool condition, object message)
		{
			if (!condition)
			{
				UnityEngine.Debug.DebugBreak();
			}
		}

		[Conditional("SIRENIX_INTERNAL")]
		public static void Assert(bool condition, string message)
		{
			if (!condition)
			{
				UnityEngine.Debug.DebugBreak();
			}
		}

		[Conditional("SIRENIX_INTERNAL")]
		public static void Assert(bool condition, object message, Object context)
		{
			if (!condition)
			{
				UnityEngine.Debug.DebugBreak();
			}
		}

		[Conditional("SIRENIX_INTERNAL")]
		public static void Assert(bool condition, string message, Object context)
		{
			if (!condition)
			{
				UnityEngine.Debug.DebugBreak();
			}
		}
	}
}

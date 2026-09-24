using System.Diagnostics;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerLogger
	{
		private const string TAG = "Odin Visual Designer";

		public static void Log(string message)
		{
			UnityEngine.Debug.unityLogger.Log(LogType.Log, "Odin Visual Designer", message);
		}

		public static void LogWarning(string message)
		{
			UnityEngine.Debug.unityLogger.Log(LogType.Warning, "Odin Visual Designer", message);
		}

		public static void LogError(string message)
		{
			UnityEngine.Debug.unityLogger.Log(LogType.Error, "Odin Visual Designer", message);
		}

		[Conditional("SIRENIX_INTERNAL")]
		public static void LogDebug(string message)
		{
			UnityEngine.Debug.unityLogger.Log(LogType.Log, "Odin Internal [Designer]", message);
		}
	}
}

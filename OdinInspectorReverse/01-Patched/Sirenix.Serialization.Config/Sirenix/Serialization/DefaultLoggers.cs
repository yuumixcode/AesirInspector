using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Defines default loggers for serialization and deserialization. This class and all of its loggers are thread safe.
	/// </summary>
	public static class DefaultLoggers
	{
		private static readonly object LOCK = new object();

		private static volatile ILogger unityLogger;

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static ILogger DefaultLogger => UnityLogger;

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static ILogger UnityLogger
		{
			get
			{
				if (unityLogger == null)
				{
					lock (LOCK)
					{
						if (unityLogger == null)
						{
							unityLogger = new CustomLogger(Debug.LogWarning, Debug.LogError, Debug.LogException);
						}
					}
				}
				return unityLogger;
			}
		}
	}
}

using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Utility class which initializes the Sirenix serialization system to be compatible with Unity.
	/// </summary>
	public static class UnitySerializationInitializer
	{
		private static readonly object LOCK = new object();

		private static bool initialized = false;

		public static bool Initialized => initialized;

		public static RuntimePlatform CurrentPlatform { get; private set; }

		/// <summary>
		/// Initializes the Sirenix serialization system to be compatible with Unity.
		/// </summary>
		public static void Initialize()
		{
			if (initialized)
			{
				return;
			}
			lock (LOCK)
			{
				if (initialized)
				{
					return;
				}
				try
				{
					GlobalConfig<GlobalSerializationConfig>.LoadInstanceIfAssetExists();
					CurrentPlatform = Application.platform;
					if (!Application.isEditor)
					{
						ArchitectureInfo.SetRuntimePlatform(CurrentPlatform);
					}
				}
				finally
				{
					initialized = true;
				}
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void InitializeRuntime()
		{
			Initialize();
		}

		[InitializeOnLoadMethod]
		private static void InitializeEditor()
		{
			Initialize();
		}
	}
}

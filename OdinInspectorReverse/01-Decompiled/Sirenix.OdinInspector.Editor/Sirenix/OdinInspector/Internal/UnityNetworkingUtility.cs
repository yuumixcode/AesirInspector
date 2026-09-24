using System;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Internal
{
	/// <summary>
	/// Contains references to UnityEngine.Networking types. These types have been removed in Unity 2019+, and thus may be null.
	/// </summary>
	internal static class UnityNetworkingUtility
	{
		public static readonly Type NetworkBehaviourType;

		public static readonly Type SyncListType;

		private static readonly Func<object, int> getNetworkChannelMethod;

		private static readonly Func<object, float> getNetworkIntervalMethod;

		public static bool IsUnityEngineNetworkingAvailable => NetworkBehaviourType != null;

		static UnityNetworkingUtility()
		{
			NetworkBehaviourType = AssemblyUtilities.GetTypeByCachedFullName("UnityEngine.Networking.NetworkBehaviour");
			SyncListType = AssemblyUtilities.GetTypeByCachedFullName("UnityEngine.Networking.SyncList`1");
			if (NetworkBehaviourType != null)
			{
				getNetworkChannelMethod = DeepReflection.CreateWeakInstanceValueGetter<int>(NetworkBehaviourType, "GetNetworkChannel()");
				getNetworkIntervalMethod = DeepReflection.CreateWeakInstanceValueGetter<float>(NetworkBehaviourType, "GetNetworkSendInterval()");
			}
		}

		public static int GetNetworkChannel(MonoBehaviour networkBehaviour)
		{
			if (!IsUnityEngineNetworkingAvailable)
			{
				throw new InvalidOperationException("UnityEngine.Networking is not available!");
			}
			if (networkBehaviour == null)
			{
				throw new ArgumentNullException("networkBehaviour");
			}
			if (!NetworkBehaviourType.IsAssignableFrom(networkBehaviour.GetType()))
			{
				throw new InvalidCastException("networkBehaviour object does not inherit from UnityEngine.Networking.NetworkBehaviour!");
			}
			return getNetworkChannelMethod(networkBehaviour);
		}

		public static float GetNetworkingInterval(MonoBehaviour networkBehaviour)
		{
			if (!IsUnityEngineNetworkingAvailable)
			{
				throw new InvalidOperationException("UnityEngine.Networking is not available!");
			}
			if (networkBehaviour == null)
			{
				throw new ArgumentNullException("networkBehaviour");
			}
			if (!NetworkBehaviourType.IsAssignableFrom(networkBehaviour.GetType()))
			{
				throw new InvalidCastException("networkBehaviour object does not inherit from UnityEngine.Networking.NetworkBehaviour!");
			}
			return getNetworkIntervalMethod(networkBehaviour);
		}
	}
}

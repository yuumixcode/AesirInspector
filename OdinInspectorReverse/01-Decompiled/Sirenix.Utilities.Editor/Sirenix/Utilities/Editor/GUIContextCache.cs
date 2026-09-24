using System;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	internal static class GUIContextCache<TPrimaryKey, TSecondaryKey, TValue>
	{
		private static GUIFrameCounter guiFrameCounter = new GUIFrameCounter();

		[NonSerialized]
		private static DoubleLookupDictionary<TPrimaryKey, TSecondaryKey, IControlContext> configs = new DoubleLookupDictionary<TPrimaryKey, TSecondaryKey, IControlContext>();

		private const int NUMBER_OF_FRAMES_CACHED = 1000;

		private static bool hasRemovedInLayoutEvent;

		public static GUIContext<TValue> GetConfig(TPrimaryKey primaryKey, TSecondaryKey secondaryKey)
		{
			if (primaryKey == null)
			{
				throw new ArgumentNullException("primaryKey");
			}
			if (secondaryKey == null)
			{
				throw new ArgumentNullException("secondaryKey");
			}
			RemoveUnusedConfigs();
			configs.TryGetInnerValue(primaryKey, secondaryKey, out var iControlConfig);
			GUIContext<TValue> config = iControlConfig as GUIContext<TValue>;
			if (config == null)
			{
				config = new GUIContext<TValue>();
				configs[primaryKey][secondaryKey] = config;
				iControlConfig = config;
			}
			iControlConfig.LastRenderedFrameId = guiFrameCounter.Update().FrameCount;
			return config;
		}

		private static void RemoveUnusedConfigs()
		{
			guiFrameCounter.Update();
			if (Event.current == null)
			{
				return;
			}
			if (Event.current.type == EventType.Layout)
			{
				if (!hasRemovedInLayoutEvent)
				{
					configs.RemoveWhere((IControlContext x) => x.LastRenderedFrameId + 1000 < guiFrameCounter.FrameCount);
					hasRemovedInLayoutEvent = true;
				}
			}
			else
			{
				hasRemovedInLayoutEvent = false;
			}
		}
	}
}

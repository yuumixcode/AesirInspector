using System;
using System.Collections.Generic;
using System.Reflection;

namespace Sirenix.OdinInspector.Editor
{
	internal static class FinalizedInspectorInfoCache
	{
		internal readonly struct Key : IEquatable<Key>
		{
			public readonly Type Type;

			public readonly ulong Hash;

			public Key(Type type, ulong hash)
			{
				Type = type;
				Hash = hash;
			}

			public bool Equals(Key other)
			{
				bool typeEquality = (object)Type == other.Type;
				if (!typeEquality)
				{
					typeEquality = Type == other.Type;
				}
				if (typeEquality)
				{
					return Hash == other.Hash;
				}
				return false;
			}

			public override bool Equals(object obj)
			{
				if (obj is Key other)
				{
					return Equals(other);
				}
				return false;
			}

			public override int GetHashCode()
			{
				int num = ((Type != null) ? Type.GetHashCode() : 0) * 397;
				ulong hash = Hash;
				return num ^ hash.GetHashCode();
			}
		}

		internal static readonly Dictionary<Key, InspectorPropertyInfo[]> Cache = new Dictionary<Key, InspectorPropertyInfo[]>(64);

		internal static readonly Dictionary<Key, bool> CanBeCachedCache = new Dictionary<Key, bool>(256);

		public static void Add(InspectorProperty parentProperty, ulong hash, InspectorPropertyInfo[] finalizedInfos)
		{
			if (finalizedInfos.Length != 0)
			{
				Type type = ((parentProperty.ValueEntry != null) ? parentProperty.ValueEntry.TypeOfValue : parentProperty.Info.TypeOfValue);
				Key key = new Key(type, hash);
				Cache[key] = finalizedInfos;
			}
		}

		public static bool TryGet(InspectorProperty parentProperty, ulong hash, out InspectorPropertyInfo[] finalizedInfos)
		{
			if (parentProperty == null || parentProperty.Tree.IsMadeForDesignerEditor)
			{
				finalizedInfos = null;
				return false;
			}
			Type type = ((parentProperty.ValueEntry != null) ? parentProperty.ValueEntry.TypeOfValue : parentProperty.Info.TypeOfValue);
			Key key = new Key(type, hash);
			return Cache.TryGetValue(key, out finalizedInfos);
		}

		internal static void Remove(Type type, ulong hash)
		{
			Key key = new Key(type, hash);
			Cache.Remove(key);
			CanBeCachedCache.Remove(key);
		}

		internal static void Clear()
		{
			Cache.Clear();
		}

		internal static void ClearAll()
		{
			Cache.Clear();
			CanBeCachedCache.Clear();
		}

		public static bool CanBeCached(InspectorProperty parentProperty, ulong hash, List<InspectorPropertyInfo> originalInfos)
		{
			Type type = ((parentProperty.ValueEntry != null) ? parentProperty.ValueEntry.TypeOfValue : parentProperty.Info.TypeOfValue);
			Key key = new Key(type, hash);
			if (Cache.ContainsKey(key))
			{
				return false;
			}
			if (CanBeCachedCache.TryGetValue(key, out var result))
			{
				return result;
			}
			return CanBeCachedCache[key] = DetermineIfResultCanBeCached(parentProperty, originalInfos);
		}

		internal static bool DetermineIfResultCanBeCached(InspectorProperty parentProperty, List<InspectorPropertyInfo> originalInfos)
		{
			List<OdinPropertyProcessor> propertyProcessors = OdinPropertyProcessorLocator.GetMemberProcessors(parentProperty);
			for (int i = 0; i < propertyProcessors.Count; i++)
			{
				OdinPropertyProcessor processor = propertyProcessors[i];
				if (processor.CanProcessForProperty(parentProperty) && !processor.GetType().IsDefined(typeof(OdinCacheableProcessorAttribute), inherit: false))
				{
					return false;
				}
			}
			OdinAttributeProcessorLocator processorLocator = parentProperty.Tree.AttributeProcessorLocator;
			List<OdinAttributeProcessor> selfProcessors = processorLocator.GetSelfProcessors(parentProperty);
			for (int j = 0; j < selfProcessors.Count; j++)
			{
				OdinAttributeProcessor processor2 = selfProcessors[j];
				if (processor2.CanProcessSelfAttributes(parentProperty) && !processor2.GetType().IsDefined(typeof(OdinCacheableProcessorAttribute), inherit: false))
				{
					return false;
				}
			}
			for (int k = 0; k < originalInfos.Count; k++)
			{
				InspectorPropertyInfo info = originalInfos[k];
				MemberInfo member = info.GetMemberInfo();
				if (member == null)
				{
					continue;
				}
				List<OdinAttributeProcessor> processors = processorLocator.GetChildProcessors(parentProperty, member);
				for (int l = 0; l < processors.Count; l++)
				{
					OdinAttributeProcessor processor3 = processors[l];
					if (processor3.CanProcessChildMemberAttributes(parentProperty, member) && !processor3.GetType().IsDefined(typeof(OdinCacheableProcessorAttribute), inherit: false))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}

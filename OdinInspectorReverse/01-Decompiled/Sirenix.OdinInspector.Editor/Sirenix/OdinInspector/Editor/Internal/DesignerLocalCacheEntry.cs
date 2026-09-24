using System;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal struct DesignerLocalCacheEntry
	{
		public Type Type;

		public string Path;

		public string Guid;

		public DateTime WriteTimeUtc;

		public DesignerLocalCacheEntry(Type type, string path, string guid, DateTime writeTimeUtc)
		{
			Type = type;
			Path = path;
			Guid = guid;
			WriteTimeUtc = writeTimeUtc;
		}
	}
}

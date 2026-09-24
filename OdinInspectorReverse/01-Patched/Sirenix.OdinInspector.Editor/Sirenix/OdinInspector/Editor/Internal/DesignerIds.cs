using System;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerIds
	{
		public static string Generate()
		{
			Guid guid = Guid.NewGuid();
			return DesignerBase58GuidEncoder.Encode(guid);
		}
	}
}

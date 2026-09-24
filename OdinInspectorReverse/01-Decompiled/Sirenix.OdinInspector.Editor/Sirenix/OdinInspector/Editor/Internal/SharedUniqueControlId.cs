using Sirenix.Reflection.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	public static class SharedUniqueControlId
	{
		private static int? uniqueIdBackingField;

		public static bool IsActive => GUIUtility.hotControl == UniqueId;

		public static int UniqueId
		{
			get
			{
				if (!uniqueIdBackingField.HasValue)
				{
					uniqueIdBackingField = GUIUtility_Internals.GetPermanentControlID();
				}
				return uniqueIdBackingField.Value;
			}
		}

		public static void SetActive()
		{
			GUIUtility.hotControl = UniqueId;
		}

		public static void SetInactive()
		{
			if (GUIUtility.hotControl == UniqueId)
			{
				GUIUtility.hotControl = 0;
			}
		}
	}
}

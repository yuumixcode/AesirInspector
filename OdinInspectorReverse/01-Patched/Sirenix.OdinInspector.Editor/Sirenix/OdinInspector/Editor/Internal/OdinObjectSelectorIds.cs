namespace Sirenix.OdinInspector.Editor.Internal
{
	/// <summary>
	/// Contains a set of Unique IDs used for various parts of Odin that don't rely on ControlIds as the ID identifier for OdinObjectSelector.
	/// </summary>
	public static class OdinObjectSelectorIds
	{
		private const int BASE = -2147475456;

		public const int POLYMORPHIC_FIELD = -2147475455;

		public const int OBJECT_FIELD = -2147475454;

		public const int PREVIEW_OBJECT_FIELD = -2147475453;

		public const int DROP_ZONE_SELECTOR = -2147475452;

		public const int LOCALIZATION_EDITOR = -2147475451;

		public const int ODIN_DRAWER_FIELD = -2147475450;
	}
}

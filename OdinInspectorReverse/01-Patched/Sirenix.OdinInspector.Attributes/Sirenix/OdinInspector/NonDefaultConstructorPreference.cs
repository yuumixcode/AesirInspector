namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Specifies how non-default constructors are handled.
	/// </summary>
	public enum NonDefaultConstructorPreference
	{
		/// <summary>
		/// Excludes types with non default constructors from the Selector.
		/// </summary>
		Exclude,
		/// <summary>
		/// Attempts to find the most straightforward constructor to call, prioritizing default values.
		/// </summary>
		ConstructIdeal,
		/// <summary>
		/// Uses <see cref="M:System.Runtime.Serialization.FormatterServices.GetUninitializedObject(System.Type)" /> if no default constructor is found.
		/// </summary>
		PreferUninitialized,
		/// <summary>
		/// Logs a warning instead of constructing the object, indicating that an attempt was made to construct an object without a default constructor.
		/// </summary>
		LogWarning
	}
}

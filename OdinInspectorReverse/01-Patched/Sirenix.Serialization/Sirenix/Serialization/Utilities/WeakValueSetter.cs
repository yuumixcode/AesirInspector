namespace Sirenix.Serialization.Utilities
{
	/// <summary>
	/// Not yet documented.
	/// </summary>
	internal delegate void WeakValueSetter(ref object instance, object value);
	/// <summary>
	/// Not yet documented.
	/// </summary>
	internal delegate void WeakValueSetter<FieldType>(ref object instance, FieldType value);
}

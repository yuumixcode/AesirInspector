namespace Sirenix.OdinInspector
{
	/// <summary>
	/// The scale mode used by <see cref="T:Sirenix.OdinInspector.ImageAttribute" />.
	/// </summary>
	public enum ImageScaleMode
	{
		/// <summary>
		/// Stretch the image to fill the whole image rect.
		/// </summary>
		StretchToFill,
		/// <summary>
		/// Scale the image proportionally so it fits inside the image rect.
		/// </summary>
		ScaleToFit,
		/// <summary>
		/// Scale the image proportionally so it covers the whole image rect, cropping as needed.
		/// </summary>
		ScaleAndCrop
	}
}

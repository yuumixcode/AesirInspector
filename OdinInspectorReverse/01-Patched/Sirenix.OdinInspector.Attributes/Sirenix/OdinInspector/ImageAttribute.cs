using System;
using System.Diagnostics;
using UnityEngine;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Draws an image directly in the inspector.
	/// </summary>
	/// <example>
	/// <para>The following example shows how Image is applied to draw images directly in the inspector.</para>
	/// <code>
	/// [Image("Banner", 96)]
	/// public class MyComponent : MonoBehaviour
	/// {
	///     [Image(128, DrawProperty = false)]
	///     public Texture2D Banner;
	///
	///     [Image("Icon", 64)]
	///     public string Text;
	///
	///     public Sprite Icon;
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.PreviewFieldAttribute" />
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	[DontApplyToListElements]
	[Conditional("UNITY_EDITOR")]
	public class ImageAttribute : Attribute
	{
		private string imageSource;

		/// <summary>
		/// The width of the image. Set to 0 to use the image's natural width.
		/// </summary>
		[TitleGroup("Layout", "Controls the image size and how much inspector space it uses.", TitleAlignments.Left, true, true, false, 0f)]
		[PropertyTooltip("The width of the image in pixels. Leave this at 0 to use the image's natural width, unless Height or FitToAvailableWidth decides the final size.")]
		public float Width;

		/// <summary>
		/// The height of the image. Set to 0 to use the image's natural height.
		/// </summary>
		[TitleGroup("Layout", null, TitleAlignments.Left, true, true, false, 0f)]
		[PropertyTooltip("The height of the image in pixels. Leave this at 0 to use the image's natural height, unless Width or FitToAvailableWidth decides the final size.")]
		public float Height;

		/// <summary>
		/// Whether the image should fit to the normal inspector layout width when Width and Height are both 0.
		/// </summary>
		[TitleGroup("Layout", null, TitleAlignments.Left, true, true, false, 0f)]
		[PropertyTooltip("When Width and Height are both 0, this makes the image fit to the normal inspector layout width while preserving aspect ratio. When disabled, the image uses its natural pixel size.")]
		public bool FitToAvailableWidth;

		/// <summary>
		/// Horizontal alignment of the image inside the available preview area. 0 is left, 0.5 is center, and 1 is right.
		/// </summary>
		[TitleGroup("Layout", null, TitleAlignments.Left, true, true, false, 0f)]
		[PropertyRange(0.0, 1.0)]
		[PropertyTooltip("Horizontal alignment of the image inside the available preview area. 0 is left, 0.5 is center, and 1 is right.")]
		public float Alignment = 0.5f;

		/// <summary>
		/// The scale mode used when drawing the image.
		/// </summary>
		[TitleGroup("Rendering", "Controls how the selected image is drawn.", TitleAlignments.Left, true, true, false, 0f)]
		[PropertyTooltip("Controls how the image fits inside the final draw area.")]
		public ImageScaleMode ScaleMode = ImageScaleMode.ScaleToFit;

		/// <summary>
		/// The filter mode to use while drawing the image.
		/// </summary>
		[TitleGroup("Rendering", null, TitleAlignments.Left, true, true, false, 0f)]
		[PropertyTooltip("The texture filtering mode used while drawing the image. Bilinear is usually best for banners; Point is useful for pixel art.")]
		public FilterMode FilterMode = FilterMode.Bilinear;

		/// <summary>
		/// Whether the image should be drawn with alpha blending.
		/// </summary>
		[TitleGroup("Rendering", null, TitleAlignments.Left, true, true, false, 0f)]
		[PropertyTooltip("Draw the image with alpha blending enabled. Turn this off only when the image should be treated as fully opaque.")]
		public bool AlphaBlend = true;

		/// <summary>
		/// Whether the property itself should be drawn after the image.
		/// </summary>
		[TitleGroup("Behavior", "Controls what happens around the drawn image.", TitleAlignments.Left, true, true, false, 0f)]
		[PropertyTooltip("When enabled, Odin draws the original property after the image. Turn this off when the property is only used as the image source.")]
		public bool DrawProperty = true;

		/// <summary>
		/// Whether the image should be drawn before or after the original property.
		/// </summary>
		[TitleGroup("Behavior", null, TitleAlignments.Left, true, true, false, 0f)]
		[PropertyTooltip("Controls whether the image is drawn before or after the original property. This only matters when DrawProperty is enabled.")]
		public ImageDrawPosition DrawPosition;

		/// <summary>
		/// Whether the image should ignore the current layout padding and indentation.
		/// </summary>
		[TitleGroup("Layout", null, TitleAlignments.Left, true, true, false, 0f)]
		[PropertyTooltip("Makes the image ignore the current layout padding and indentation. This is useful for banners and section headers that should reach the inspector edges.")]
		public bool IgnorePadding;

		/// <summary>
		/// A resolved value that should resolve to the image object to draw.
		/// </summary>
		[ShowInInspector]
		[TitleGroup("Image Source", "Member name, expression, or asset path.", TitleAlignments.Left, true, true, false, 0f)]
		[PropertyTooltip("The image source to draw. This can be a member name, an attribute expression, or an asset path that resolves to a texture, sprite, or another Unity object with a preview. Leave this empty to use the decorated property value.")]
		[ImageResolver]
		[OdinDesignerBinding(new string[] { "imageSource", "ImageSourceHasValue" })]
		public string ImageSource
		{
			get
			{
				return imageSource;
			}
			set
			{
				imageSource = (string.IsNullOrWhiteSpace(value) ? null : value);
				ImageSourceHasValue = imageSource != null;
			}
		}

		/// <summary>
		/// Whether an image source value is specified.
		/// </summary>
		public bool ImageSourceHasValue { get; private set; }

		/// <summary>
		/// Draws the property's value as an image.
		/// </summary>
		public ImageAttribute()
		{
		}

		/// <summary>
		/// Draws the property's value as an image.
		/// </summary>
		/// <param name="height">The height of the image.</param>
		public ImageAttribute(float height)
		{
			Height = height;
		}

		/// <summary>
		/// Draws the property's value as an image.
		/// </summary>
		/// <param name="height">The height of the image.</param>
		/// <param name="scaleMode">The scale mode used when drawing the image.</param>
		public ImageAttribute(float height, ImageScaleMode scaleMode)
		{
			Height = height;
			ScaleMode = scaleMode;
		}

		/// <summary>
		/// Draws the property's value as an image.
		/// </summary>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		public ImageAttribute(float width, float height)
		{
			Width = width;
			Height = height;
		}

		/// <summary>
		/// Draws the property's value as an image.
		/// </summary>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="scaleMode">The scale mode used when drawing the image.</param>
		public ImageAttribute(float width, float height, ImageScaleMode scaleMode)
		{
			Width = width;
			Height = height;
			ScaleMode = scaleMode;
		}

		/// <summary>
		/// Draws an image resolved by the specified source.
		/// </summary>
		/// <param name="imageSource">A resolved value that should resolve to the image object to draw.</param>
		public ImageAttribute(string imageSource)
		{
			ImageSource = imageSource;
		}

		/// <summary>
		/// Draws an image resolved by the specified source.
		/// </summary>
		/// <param name="imageSource">A resolved value that should resolve to the image object to draw.</param>
		/// <param name="height">The height of the image.</param>
		public ImageAttribute(string imageSource, float height)
		{
			ImageSource = imageSource;
			Height = height;
		}

		/// <summary>
		/// Draws an image resolved by the specified source.
		/// </summary>
		/// <param name="imageSource">A resolved value that should resolve to the image object to draw.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="scaleMode">The scale mode used when drawing the image.</param>
		public ImageAttribute(string imageSource, float height, ImageScaleMode scaleMode)
		{
			ImageSource = imageSource;
			Height = height;
			ScaleMode = scaleMode;
		}

		/// <summary>
		/// Draws an image resolved by the specified source.
		/// </summary>
		/// <param name="imageSource">A resolved value that should resolve to the image object to draw.</param>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		public ImageAttribute(string imageSource, float width, float height)
		{
			ImageSource = imageSource;
			Width = width;
			Height = height;
		}

		/// <summary>
		/// Draws an image resolved by the specified source.
		/// </summary>
		/// <param name="imageSource">A resolved value that should resolve to the image object to draw.</param>
		/// <param name="width">The width of the image.</param>
		/// <param name="height">The height of the image.</param>
		/// <param name="scaleMode">The scale mode used when drawing the image.</param>
		public ImageAttribute(string imageSource, float width, float height, ImageScaleMode scaleMode)
		{
			ImageSource = imageSource;
			Width = width;
			Height = height;
			ScaleMode = scaleMode;
		}
	}
}

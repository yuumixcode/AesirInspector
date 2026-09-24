using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.ImageAttribute" /> as an image directly in the inspector.
	/// </summary>
	[DrawerPriority(0.4, 0.0, 0.0)]
	public sealed class ImageAttributeDrawer : OdinAttributeDrawer<ImageAttribute>
	{
		private readonly struct ImageInfo
		{
			public readonly Texture Texture;

			public readonly Rect TexCoords;

			public readonly bool HasTexCoords;

			public readonly float Aspect;

			public readonly float Width;

			public readonly float Height;

			private ImageInfo(Texture texture, Rect texCoords, bool hasTexCoords, float width, float height)
			{
				Texture = texture;
				TexCoords = texCoords;
				HasTexCoords = hasTexCoords;
				Width = width;
				Height = height;
				Aspect = ((width > 0f && height > 0f) ? (width / height) : 1f);
			}

			public static bool TryCreate(Object source, out ImageInfo image)
			{
				image = default(ImageInfo);
				if (source == null)
				{
					return false;
				}
				if (source is Sprite sprite && sprite.texture != null)
				{
					Texture2D texture = sprite.texture;
					Rect textureRect = sprite.textureRect;
					Rect texCoords = new Rect(textureRect.x / (float)texture.width, textureRect.y / (float)texture.height, textureRect.width / (float)texture.width, textureRect.height / (float)texture.height);
					image = new ImageInfo(texture, texCoords, hasTexCoords: true, sprite.rect.width, sprite.rect.height);
					return true;
				}
				if (source is Texture sourceTexture)
				{
					image = new ImageInfo(sourceTexture, default(Rect), hasTexCoords: false, sourceTexture.width, sourceTexture.height);
					return true;
				}
				Texture previewTexture = GUIHelper.GetPreviewTexture(source);
				if (previewTexture == null)
				{
					return false;
				}
				image = new ImageInfo(previewTexture, default(Rect), hasTexCoords: false, previewTexture.width, previewTexture.height);
				return true;
			}
		}

		private ValueResolver<Object> imageResolver;

		private Object resolvedImageSourceAsset;

		private bool hasResolvedImageSourceAsset;

		private float lastIgnorePadding;

		protected override void Initialize()
		{
			if (base.Attribute.ImageSourceHasValue && !string.IsNullOrWhiteSpace(base.Attribute.ImageSource))
			{
				if (TryResolveGlobalObjectId(base.Attribute.ImageSource, out resolvedImageSourceAsset))
				{
					hasResolvedImageSourceAsset = true;
				}
				else
				{
					imageResolver = ValueResolver.Get<Object>(base.Property, base.Attribute.ImageSource);
				}
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (base.Attribute.DrawProperty && base.Attribute.DrawPosition == ImageDrawPosition.AfterProperty)
			{
				CallNextDrawer(label);
				DrawImageOrError();
				return;
			}
			DrawImageOrError();
			if (base.Attribute.DrawProperty)
			{
				CallNextDrawer(label);
			}
		}

		private void DrawImageOrError()
		{
			if (imageResolver != null && imageResolver.HasError)
			{
				imageResolver.DrawError();
			}
			else
			{
				DrawImage();
			}
		}

		private void DrawImage()
		{
			Object imageObject = GetImageObject();
			if (!ImageInfo.TryCreate(imageObject, out var image))
			{
				ReserveImageRectWithoutImage();
				return;
			}
			Rect rect = GetImageRect(image);
			if (rect.width <= 0f || rect.height <= 0f)
			{
				return;
			}
			FilterMode lastFilterMode = image.Texture.filterMode;
			image.Texture.filterMode = base.Attribute.FilterMode;
			try
			{
				DrawTexture(rect, image, base.Attribute.ScaleMode, base.Attribute.AlphaBlend);
			}
			finally
			{
				image.Texture.filterMode = lastFilterMode;
			}
		}

		private void ReserveImageRectWithoutImage()
		{
			if (base.Attribute.Height > 0f)
			{
				GetAvailableRect(base.Attribute.Height);
			}
		}

		private Object GetImageObject()
		{
			if (imageResolver != null)
			{
				return imageResolver.GetValue();
			}
			if (hasResolvedImageSourceAsset)
			{
				return resolvedImageSourceAsset;
			}
			return base.Property.ValueEntry?.WeakSmartValue as Object;
		}

		private Rect GetImageRect(ImageInfo image)
		{
			float width = base.Attribute.Width;
			float height = base.Attribute.Height;
			Rect rect;
			if (width > 0f && height > 0f)
			{
				rect = GetAvailableRect(height);
				return AlignFixedWidthRect(rect, width, base.Attribute.Alignment);
			}
			if (width > 0f)
			{
				rect = GetAvailableRect(width / image.Aspect);
				return AlignFixedWidthRect(rect, width, base.Attribute.Alignment);
			}
			if (height > 0f)
			{
				return GetAvailableRect(height);
			}
			if (base.Attribute.FitToAvailableWidth)
			{
				if (base.Attribute.IgnorePadding)
				{
					rect = GUILayoutUtility.GetRect(0f, GetIgnorePaddingWidth() / image.Aspect, GUILayout.ExpandWidth(expand: true));
					return ExpandAvailableRectToIgnorePadding(rect);
				}
				return GUILayoutUtility.GetAspectRect(image.Aspect, GUILayout.ExpandWidth(expand: true));
			}
			rect = GetAvailableRect(image.Height);
			return AlignFixedWidthRect(rect, image.Width, base.Attribute.Alignment);
		}

		private Rect GetAvailableRect(float height)
		{
			Rect rect = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(expand: true));
			if (!base.Attribute.IgnorePadding)
			{
				return rect;
			}
			return ExpandAvailableRectToIgnorePadding(rect);
		}

		private static Rect AlignFixedWidthRect(Rect rect, float width, float alignment)
		{
			float availableWidth = rect.width;
			rect.width = Mathf.Min(width, availableWidth);
			rect.x += (availableWidth - rect.width) * Mathf.Clamp01(alignment);
			return rect;
		}

		private Rect ExpandAvailableRectToIgnorePadding(Rect rect)
		{
			if (Event.current.type != EventType.Layout)
			{
				Rect visibleRect = GUIClipInfo.VisibleRect;
				if (visibleRect.width > 0f)
				{
					rect.xMin = Mathf.Floor(visibleRect.xMin) - 1f;
					rect.xMax = Mathf.Ceil(visibleRect.xMax) + 1f;
					lastIgnorePadding = rect.width;
					return rect;
				}
			}
			rect.x = 0f;
			rect.width = Mathf.Ceil(GetIgnorePaddingWidth());
			return rect;
		}

		private float GetIgnorePaddingWidth()
		{
			if (!(lastIgnorePadding > 0f))
			{
				return EditorGUIUtility.currentViewWidth;
			}
			return lastIgnorePadding;
		}

		private void DrawTexture(Rect rect, ImageInfo image, ImageScaleMode scaleMode, bool alphaBlend)
		{
			Rect texCoords = (image.HasTexCoords ? image.TexCoords : new Rect(0f, 0f, 1f, 1f));
			switch (scaleMode)
			{
			case ImageScaleMode.ScaleToFit:
				rect = ScaleRectToFit(rect, image.Aspect, base.Attribute.Alignment);
				break;
			case ImageScaleMode.ScaleAndCrop:
				texCoords = CropTexCoords(texCoords, image.Aspect, rect.width / rect.height, base.Attribute.Alignment);
				break;
			}
			GUI.DrawTextureWithTexCoords(rect, image.Texture, texCoords, alphaBlend);
		}

		private static Rect ScaleRectToFit(Rect rect, float imageAspect, float alignment)
		{
			if (rect.width <= 0f || rect.height <= 0f || imageAspect <= 0f)
			{
				return rect;
			}
			float rectAspect = rect.width / rect.height;
			if (rectAspect > imageAspect)
			{
				float width = rect.height * imageAspect;
				rect.x += (rect.width - width) * Mathf.Clamp01(alignment);
				rect.width = width;
			}
			else
			{
				float height = rect.width / imageAspect;
				rect.y += (rect.height - height) * 0.5f;
				rect.height = height;
			}
			return rect;
		}

		private static Rect CropTexCoords(Rect texCoords, float imageAspect, float rectAspect, float alignment)
		{
			if (imageAspect <= 0f || rectAspect <= 0f)
			{
				return texCoords;
			}
			if (imageAspect > rectAspect)
			{
				float width = texCoords.width * (rectAspect / imageAspect);
				texCoords.x += (texCoords.width - width) * Mathf.Clamp01(alignment);
				texCoords.width = width;
			}
			else
			{
				float height = texCoords.height * (imageAspect / rectAspect);
				texCoords.y += (texCoords.height - height) * 0.5f;
				texCoords.height = height;
			}
			return texCoords;
		}

		private static bool TryResolveGlobalObjectId(string source, out Object obj)
		{
			obj = null;
			if (string.IsNullOrEmpty(source))
			{
				return false;
			}
			if (!GlobalObjectId.TryParse(source, out var globalObjectId))
			{
				return false;
			}
			obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId);
			return obj != null;
		}
	}
}

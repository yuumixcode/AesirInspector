using System;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerTextures
	{
		private const float FADE_STRENGTH = 0.4f;

		private static Texture2D fadeMaskTop;

		private static Texture2D fadeMaskBottom;

		private static Texture2D fadeMaskLeft;

		private static Texture2D leftToRightFade;

		private static Texture2D righToLeftFade;

		private static Texture2D topToBottomFade;

		private static Texture2D bottomToTopFade;

		private static Texture2D roundBlur6;

		private static Texture2D gradientButton;

		private static Texture2D gradientHover;

		private static Texture2D roundBlur6Inverted;

		public static Texture2D FadeMaskTop
		{
			get
			{
				if (fadeMaskTop == null)
				{
					fadeMaskTop = MakeTopFadeMask(64, 0f, 2.2f);
				}
				return fadeMaskTop;
			}
		}

		public static Texture2D FadeMaskBottom
		{
			get
			{
				if (fadeMaskBottom == null)
				{
					fadeMaskBottom = MakeBottomFadeMask(64, 0f, 2.2f);
				}
				return fadeMaskBottom;
			}
		}

		public static Texture2D FadeMaskLeft
		{
			get
			{
				if (fadeMaskLeft == null)
				{
					fadeMaskLeft = MakeLeftFadeMask(64, 0f, 2.2f);
				}
				return fadeMaskLeft;
			}
		}

		public static Texture2D LeftToRightFade
		{
			get
			{
				if (leftToRightFade != null)
				{
					return leftToRightFade;
				}
				leftToRightFade = new Texture2D(128, 128)
				{
					hideFlags = HideFlags.HideAndDontSave
				};
				Color[] pixels = new Color[16384];
				int index = 0;
				for (int x = 0; x < 128; x++)
				{
					for (int y = 0; y < 128; y++)
					{
						float t = Mathf.Pow((float)y / 127f, 0.65f);
						pixels[index++] = Color.Lerp(Color.white, Color.clear, t);
					}
				}
				leftToRightFade.SetPixels(pixels);
				leftToRightFade.Apply();
				CleanupUtility.DestroyObjectOnAssemblyReload(leftToRightFade);
				return leftToRightFade;
			}
		}

		public static Texture2D RightToLeftFade
		{
			get
			{
				if (righToLeftFade != null)
				{
					return righToLeftFade;
				}
				righToLeftFade = new Texture2D(128, 128)
				{
					hideFlags = HideFlags.HideAndDontSave
				};
				Color[] pixels = new Color[16384];
				int index = 0;
				for (int x = 0; x < 128; x++)
				{
					for (int y = 0; y < 128; y++)
					{
						float t = (float)y / 127f;
						pixels[index++] = Color.Lerp(Color.clear, Color.white, t);
					}
				}
				righToLeftFade.SetPixels(pixels);
				righToLeftFade.Apply();
				CleanupUtility.DestroyObjectOnAssemblyReload(righToLeftFade);
				return righToLeftFade;
			}
		}

		public static Texture2D TopToBottomFade
		{
			get
			{
				if (topToBottomFade != null)
				{
					return topToBottomFade;
				}
				topToBottomFade = new Texture2D(32, 32)
				{
					hideFlags = HideFlags.HideAndDontSave
				};
				Color[] pixels = new Color[1024];
				int index = 0;
				for (int x = 0; x < 32; x++)
				{
					for (int y = 0; y < 32; y++)
					{
						pixels[index++] = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), Mathf.Pow(1f - (float)x / 31f, 0.4f));
					}
				}
				topToBottomFade.SetPixels(pixels);
				topToBottomFade.Apply();
				CleanupUtility.DestroyObjectOnAssemblyReload(topToBottomFade);
				return topToBottomFade;
			}
		}

		public static Texture2D BottomToTopFade
		{
			get
			{
				if (bottomToTopFade != null)
				{
					return bottomToTopFade;
				}
				bottomToTopFade = new Texture2D(32, 32)
				{
					hideFlags = HideFlags.HideAndDontSave
				};
				Color[] pixels = new Color[1024];
				int index = 0;
				for (int x = 0; x < 32; x++)
				{
					for (int y = 0; y < 32; y++)
					{
						pixels[index++] = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), Mathf.Pow((float)x / 31f, 0.4f));
					}
				}
				bottomToTopFade.SetPixels(pixels);
				bottomToTopFade.Apply();
				CleanupUtility.DestroyObjectOnAssemblyReload(bottomToTopFade);
				return bottomToTopFade;
			}
		}

		public static Texture2D RoundBlur6
		{
			get
			{
				if (roundBlur6 == null)
				{
					byte[] bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAGYAAABmCAYAAAA53+RiAAAFDElEQVR4Ae2dzZIURRSFaQFBDGBBsHGB76Ju0BfxkXwRZaXvIgs3BhEiIT/DYHu+pG+R3XN6mDa6uZuTEacr61ZWVcb3kVXFaq5dSwuBELg6gVUNXa/X1a1tHWO7G8bU8RqfrSdQYNnuhjPq+Dh7tXqP9cbY2/4p4J+pTK5PqRrbGqdu2iUEAP/vlHfqV6rO6VuCdsUAm5SQm+p/vgl9wjklSN3RIqlIvN/OkAv+uQ693eRMW8I+khhDW87bFcNBoFO/pdxW7ihfbML+LCdCBOSSVqulpLzW2FebvNSW/TcKxxG0tFnMvFKQgpC7yn3l3qZPreTkcSYYH2klhpWBBGS8UJ4r/AOHIY1xNZbtWBlsqzGQE4CPlAfKQ+Vb5XvlkcKjLe1wAjy6nio/K78pcKbxGKvHWT3SPrzA9VWGFKCzKlglSPlK+VF5rKQdj8ATXeon5Q/lmcIKYjWd6atsyKmlpNqQxBcYcninIOcbJVIE4cgNpjyFYAxrmMN+eWc7MSwxBvMo+0FJOw0BXg0whjXMLxWDqHrH8Eh7pKSdhgBsYWw/pnZXDPt8qZUclljaaQjAtqTAHPb2UcbtOTjLoZZ2OgIsgJIC96XNO9iq/8uUoGVgOichUJzZFv9xo1nMfOetQfOB9I9KYC/nfWKOevdc7HACTgwW0z4tgQvMnZhPO6XczRKIGIulvxgx/Q7sDCLGYukvRky/AzuDiLFY+osR0+/AziBiLJb+YsT0O7AziBiLpb8YMf0O7AwixmLpL0ZMvwM7g4ixWPqLEdPvwM4gYiyW/mLE9DuwM4gYi6W/GDH9DuwMIsZi6S9GTL8DO4OIsVj6ixHT78DOIGIslv5ixPQ7sDOIGIulvxgx/Q7sDCLGYukvRky/AzuDiLFY+osR0+/AziBiLJb+YsT0O7AziBiLpb8YMf0O7AwixmLpL0ZMvwM7g4ixWPqLEdPvwM4gYiyW/mLE9DuwM4gYi6W/GDH9DuwMIsZi6S9GTL8DO4OIsVj6ixHT78DOIGIslv5ixPQ7sDOIGIulvxgx/Q7sDCLGYukvRky/AzuDiLFY+osR0+/AziBiLJb+YsT0O7AziBiLpb8YMf0O7AwixmLpL0ZMvwM7g4ixWPqLEdPvwM4gYiyW/mLE9DuwM4gYi6W/GDH9DuwMnJjxZ8vt6BRPReACcyfmVDfPdQ8gsE8MBi9YPOC6GXo1Ans5z2JqEH9PvnK1y2fU/yVQnNkW/3GtWQyFGniu/tsxIj+nJABjWBf35V6zGIwxoKS8Vv9sGZnOsQnAFsazHByM5sQwkBNeKk/HqPycggBsYVxy6nE27rUr5p2qiHmlvFB+UdJOQwC2MIY1zGG/d8VwkCXG4L+VX5UnStpxCcAUtjCGNcy3xCx/fHm9XtO/odxSvlTuKw+Uh8p3ymPla+WmknY4AVbF7worBSl/Ks+U58o/yhvlfLVajVWzK4ZHG3JuK3eUuwqC7m361DiGHMYu56ufdpEAkHl3zO9tHl+sFITQr/fM+DorMUioVhdhSWGPVhfF6F9KSeG8iBGEj7RiOn/p8ugiJQTWMN96+c9idGw0BnAhLsoJ9THAKiElZf5wyMoRmKnBrho8iyksCe8UQr+kqPuhLUD1jqlq1QBPrk+pGtsap27aJQRq1ZQgRFSqxulDgB5l41IL3EnMOKCfOsZ2N4yp4/TT9hOof/Fsd8NZdXxcocSMnfyEQAhckcB/Z2rwteNGzXsAAAAASUVORK5CYII=");
					roundBlur6 = TextureUtilities.LoadImage(102, 102, bytes);
					roundBlur6.hideFlags = HideFlags.HideAndDontSave;
					CleanupUtility.DestroyObjectOnAssemblyReload(roundBlur6);
				}
				return roundBlur6;
			}
		}

		public static Texture2D GradientButton
		{
			get
			{
				if (gradientButton != null)
				{
					return gradientButton;
				}
				byte[] bytes = Convert.FromBase64String("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");
				gradientButton = TextureUtilities.LoadImage(200, 40, bytes);
				gradientButton.hideFlags = HideFlags.HideAndDontSave;
				CleanupUtility.DestroyObjectOnAssemblyReload(gradientButton);
				return gradientButton;
			}
		}

		public static Texture2D GradientHover
		{
			get
			{
				if (gradientHover != null)
				{
					return gradientHover;
				}
				gradientHover = TMP(new Color(1f, 1f, 1f, 0.25f), new Color(1f, 1f, 1f, 0f), 256, 0.1);
				gradientHover.hideFlags = HideFlags.HideAndDontSave;
				CleanupUtility.DestroyObjectOnAssemblyReload(gradientHover);
				return gradientHover;
			}
		}

		public static Texture2D RoundBlur6Inverted
		{
			get
			{
				if (roundBlur6Inverted != null)
				{
					return roundBlur6Inverted;
				}
				Color[] pixels = RoundBlur6.GetPixels();
				for (int i = 0; i < pixels.Length; i++)
				{
					pixels[i].r = (pixels[i].g = (pixels[i].b = 1f));
					pixels[i].a = 1f - pixels[i].a;
				}
				Texture2D image = new Texture2D(RoundBlur6.width, RoundBlur6.height, RoundBlur6.format, mipChain: false)
				{
					hideFlags = HideFlags.HideAndDontSave
				};
				image.SetPixels(pixels);
				image.Apply();
				CleanupUtility.DestroyObjectOnAssemblyReload(image);
				roundBlur6Inverted = image;
				return roundBlur6Inverted;
			}
		}

		private static Texture2D MakeTopFadeMask(int height, float fadeStart, float exponent)
		{
			height = Mathf.Max(1, height);
			fadeStart = Mathf.Clamp01(fadeStart);
			exponent = Mathf.Max(0.0001f, exponent);
			Texture2D texture = new Texture2D(1, height, TextureFormat.RGBA32, mipChain: false)
			{
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp,
				hideFlags = HideFlags.HideAndDontSave
			};
			Color[] pixels = new Color[height];
			for (int y = 0; y < height; y++)
			{
				float t = 1f - ((height <= 1) ? 0f : ((float)y / (float)(height - 1)));
				float f = ((t <= fadeStart) ? 0f : ((t - fadeStart) / (1f - fadeStart)));
				f = Mathf.Clamp01(f);
				float a = Mathf.Pow(1f - f, exponent);
				pixels[y] = new Color(1f, 1f, 1f, a);
			}
			texture.SetPixels(pixels);
			texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
			CleanupUtility.DestroyObjectOnAssemblyReload(texture);
			return texture;
		}

		private static Texture2D MakeBottomFadeMask(int height, float fadeStart, float exponent)
		{
			height = Mathf.Max(1, height);
			fadeStart = Mathf.Clamp01(fadeStart);
			exponent = Mathf.Max(0.0001f, exponent);
			Texture2D texture = new Texture2D(1, height, TextureFormat.RGBA32, mipChain: false)
			{
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp,
				hideFlags = HideFlags.HideAndDontSave
			};
			Color[] pixels = new Color[height];
			for (int y = 0; y < height; y++)
			{
				float t = ((height <= 1) ? 0f : ((float)y / (float)(height - 1)));
				float f = ((t <= fadeStart) ? 0f : ((t - fadeStart) / (1f - fadeStart)));
				f = Mathf.Clamp01(f);
				float a = Mathf.Pow(1f - f, exponent);
				pixels[y] = new Color(1f, 1f, 1f, a);
			}
			texture.SetPixels(pixels);
			texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
			CleanupUtility.DestroyObjectOnAssemblyReload(texture);
			return texture;
		}

		private static Texture2D MakeLeftFadeMask(int width, float fadeStart, float exponent)
		{
			width = Mathf.Max(1, width);
			fadeStart = Mathf.Clamp01(fadeStart);
			exponent = Mathf.Max(0.0001f, exponent);
			Texture2D texture = new Texture2D(width, 1, TextureFormat.RGBA32, mipChain: false)
			{
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp,
				hideFlags = HideFlags.HideAndDontSave
			};
			Color[] pixels = new Color[width];
			for (int x = 0; x < width; x++)
			{
				float t = ((width <= 1) ? 0f : ((float)x / (float)(width - 1)));
				float f = ((t <= fadeStart) ? 0f : ((t - fadeStart) / (1f - fadeStart)));
				f = Mathf.Clamp01(f);
				float a = Mathf.Pow(1f - f, exponent);
				pixels[x] = new Color(1f, 1f, 1f, a);
			}
			texture.SetPixels(pixels);
			texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
			CleanupUtility.DestroyObjectOnAssemblyReload(texture);
			return texture;
		}

		public static Texture2D TMP(Color centerColor, Color edgeColor, int size, double radiusInner = 0.2, double radiusOuter = 1.25, double noiseIntensity = 0.005)
		{
			Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false);
			texture.filterMode = FilterMode.Bilinear;
			texture.wrapMode = TextureWrapMode.Clamp;
			Vector2 center = new Vector2((float)size / 2f, (float)size / 2f);
			double maxDistance = (double)size / 2.0;
			System.Random rand = new System.Random();
			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					double dx = (float)x - center.x;
					double dy = (float)y - center.y;
					double dist = Math.Sqrt(dx * dx + dy * dy) / maxDistance;
					dist = Math.Min(1.0, Math.Max(0.0, dist));
					double t;
					if (dist <= radiusInner)
					{
						t = 0.0;
					}
					else if (dist >= radiusOuter)
					{
						t = 1.0;
					}
					else
					{
						double normDist = (dist - radiusInner) / (radiusOuter - radiusInner);
						t = ((normDist < 0.5) ? (4.0 * normDist * normDist * normDist) : (1.0 - Math.Pow(-2.0 * normDist + 2.0, 3.0) / 2.0));
					}
					double r = (double)centerColor.r + t * (double)(edgeColor.r - centerColor.r);
					double g = (double)centerColor.g + t * (double)(edgeColor.g - centerColor.g);
					double b = (double)centerColor.b + t * (double)(edgeColor.b - centerColor.b);
					double a = (double)centerColor.a + t * (double)(edgeColor.a - centerColor.a);
					int hash = (x * 73856093) ^ (y * 19349663);
					rand = new System.Random(hash);
					double noise = (rand.NextDouble() * 2.0 - 1.0) * noiseIntensity;
					r = Math.Min(1.0, Math.Max(0.0, r + noise));
					g = Math.Min(1.0, Math.Max(0.0, g + noise));
					b = Math.Min(1.0, Math.Max(0.0, b + noise));
					a = Math.Min(1.0, Math.Max(0.0, a + noise * 0.5));
					Color finalColor = new Color((float)r, (float)g, (float)b, (float)a);
					texture.SetPixel(x, y, finalColor);
				}
			}
			texture.Apply();
			return texture;
		}
	}
}

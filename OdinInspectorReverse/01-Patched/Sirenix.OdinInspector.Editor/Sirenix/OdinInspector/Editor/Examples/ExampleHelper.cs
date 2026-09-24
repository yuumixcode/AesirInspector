using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	public static class ExampleHelper
	{
		private enum ExampleTextureKind
		{
			Art,
			Checker,
			Transparent
		}

		private static readonly System.Random random;

		private static readonly string[] shaderNames;

		private static readonly string[] strings;

		private static readonly string[] meshNames;

		private static Material[] materials;

		private static Mesh[] meshes;

		private static readonly Dictionary<string, Texture2D> generatedTextures;

		private static bool initialized;

		static ExampleHelper()
		{
			random = new System.Random();
			shaderNames = new string[3] { "Standard", "Specular", "Skybox/Cubemap" };
			strings = new string[7] { "Hello World", "Sirenix", "Unity", "Lorem Ipsum", "Game Object", "Scriptable Objects", "Ramblings of a mad man" };
			meshNames = new string[4] { "Cube", "Sphere", "Cylinder", "Capsule" };
			generatedTextures = new Dictionary<string, Texture2D>();
			AssemblyReloadEvents.beforeAssemblyReload += DestroyGeneratedTextures;
			EditorApplication.quitting += DestroyGeneratedTextures;
		}

		private static void InitializeExampleDataSafely()
		{
			if (initialized)
			{
				return;
			}
			initialized = true;
			materials = shaderNames.Select((string s) => new Material(Shader.Find(s))).ToArray();
			meshes = meshNames.Select((string s) => Resources.FindObjectsOfTypeAll<Mesh>().FirstOrDefault((Mesh x) => x.name == s)).ToArray();
		}

		public static T GetScriptableObject<T>(string name) where T : ScriptableObject
		{
			T so = ScriptableObject.CreateInstance<T>();
			so.name = name ?? typeof(T).GetNiceName();
			return so;
		}

		public static Material GetMaterial()
		{
			InitializeExampleDataSafely();
			return PickRandom(materials);
		}

		public static Texture2D GetTexture()
		{
			return GetTexture(0);
		}

		public static Texture2D GetTexture(int variant)
		{
			return GetGeneratedTexture(96, 96, GetTheme(variant), ExampleTextureKind.Art, "Default", variant);
		}

		public static Texture2D GetTexture(int width, int height)
		{
			return GetTexture(width, height, ExampleTextureTheme.Blue, 0);
		}

		public static Texture2D GetTexture(int width, int height, ExampleTextureTheme theme)
		{
			return GetTexture(width, height, theme, 0);
		}

		public static Texture2D GetTexture(int width, int height, int variant)
		{
			return GetTexture(width, height, GetTheme(variant), variant);
		}

		public static Texture2D GetTexture(int width, int height, ExampleTextureTheme theme, int variant)
		{
			return GetGeneratedTexture(width, height, theme, ExampleTextureKind.Art, "Explicit", variant);
		}

		public static Texture2D GetCheckerTexture(int width, int height, int variant = 0)
		{
			return GetGeneratedTexture(width, height, ExampleTextureTheme.Pixel, ExampleTextureKind.Checker, "Checker", variant);
		}

		public static Texture2D GetTransparentTexture(int width, int height, ExampleTextureTheme theme = ExampleTextureTheme.Blue, int variant = 0)
		{
			return GetGeneratedTexture(width, height, theme, ExampleTextureKind.Transparent, "Transparent", variant);
		}

		public static Mesh GetMesh()
		{
			InitializeExampleDataSafely();
			return PickRandom(meshes);
		}

		public static string GetString()
		{
			return PickRandom(strings);
		}

		public static float RandomInt(int min, int max)
		{
			return random.Next(min, max);
		}

		public static float RandomFloat(float min, float max)
		{
			return (float)(random.NextDouble() * (double)(max - min) + (double)min);
		}

		private static T PickRandom<T>(IList<T> collection)
		{
			return collection[random.Next(collection.Count)];
		}

		private static Texture2D GetGeneratedTexture(int width, int height, ExampleTextureTheme theme, ExampleTextureKind kind, string family, int variant)
		{
			width = Mathf.Max(1, width);
			height = Mathf.Max(1, height);
			string key = width + "x" + height + ":" + theme.ToString() + ":" + kind.ToString() + ":" + family + ":" + variant;
			if (generatedTextures.TryGetValue(key, out var texture) && texture != null)
			{
				return texture;
			}
			int seed = StableHash(key);
			texture = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false)
			{
				name = "Odin Example Texture " + seed.ToString("X8"),
				hideFlags = HideFlags.HideAndDontSave,
				filterMode = ((kind != ExampleTextureKind.Checker) ? FilterMode.Bilinear : FilterMode.Point),
				wrapMode = TextureWrapMode.Clamp
			};
			Color32[] pixels = new Color32[width * height];
			if (kind == ExampleTextureKind.Checker)
			{
				FillChecker(pixels, width, height, seed);
			}
			else
			{
				FillArtTexture(pixels, width, height, theme, seed, kind == ExampleTextureKind.Transparent);
			}
			texture.SetPixels32(pixels);
			texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
			generatedTextures[key] = texture;
			return texture;
		}

		private static void FillArtTexture(Color32[] pixels, int width, int height, ExampleTextureTheme theme, int seed, bool transparent)
		{
			ExampleTextureColors colors = GetColors(theme);
			float phaseA = (float)((seed >> 8) & 0xFF) / 255f;
			float phaseB = (float)((seed >> 16) & 0xFF) / 255f;
			float centerX = Mathf.Lerp(0.28f, 0.72f, phaseA);
			float centerY = Mathf.Lerp(0.2f, 0.8f, phaseB);
			float glowStrength = Mathf.Lerp(0.12f, 0.24f, (float)((seed >> 24) & 0xFF) / 255f);
			for (int y = 0; y < height; y++)
			{
				float vertical = ((height == 1) ? 0f : ((float)y / (float)(height - 1)));
				for (int x = 0; x < width; x++)
				{
					float horizontal = ((width == 1) ? 0f : ((float)x / (float)(width - 1)));
					float gradient = Mathf.Clamp01(horizontal * 0.65f + vertical * 0.35f);
					Color color = Color.Lerp(colors.ColorA, colors.ColorB, gradient);
					float dx = horizontal - centerX;
					float dy = vertical - centerY;
					float glow = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy) * 2.2f);
					color = Color.Lerp(color, colors.Accent, glow * glowStrength);
					float vignetteX = Mathf.Abs(horizontal - 0.5f) * 2f;
					float vignetteY = Mathf.Abs(vertical - 0.5f) * 2f;
					float vignette = Mathf.Clamp01((vignetteX * vignetteX + vignetteY * vignetteY) * 0.25f);
					color = Color.Lerp(color, colors.Shadow, vignette * 0.45f);
					float highlight = Mathf.Sin(horizontal * 10f + vertical * 4f + phaseA * 6.28f) * 0.5f + 0.5f;
					color = Color.Lerp(color, Color.white, highlight * 0.035f);
					if (transparent)
					{
						float alpha = Mathf.Clamp01(1f - Mathf.Abs(horizontal - 0.5f) * 1.65f);
						color.a = Mathf.Lerp(0.12f, 0.92f, alpha);
					}
					pixels[y * width + x] = UnityShims.Color32.op_Implicit(color);
				}
			}
		}

		private static void FillChecker(Color32[] pixels, int width, int height, int seed)
		{
			ExampleTextureColors colors = GetColors(ExampleTextureTheme.Pixel);
			int cellSize = Mathf.Max(2, Mathf.Min(width, height) / 8);
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					Color color = (((x / cellSize + y / cellSize) % 2 == 0) ? colors.ColorA : colors.ColorB);
					if ((x + y + seed) % (cellSize * 3) < cellSize)
					{
						color = Color.Lerp(color, colors.Accent, 0.18f);
					}
					pixels[y * width + x] = UnityShims.Color32.op_Implicit(color);
				}
			}
		}

		private static ExampleTextureTheme GetTheme(int variant)
		{
			return (ExampleTextureTheme)((variant & 0x7FFFFFFF) % 4);
		}

		private static ExampleTextureColors GetColors(ExampleTextureTheme theme)
		{
			return theme switch
			{
				ExampleTextureTheme.Green => new ExampleTextureColors(new Color(0.16f, 0.35f, 0.3f), new Color(0.4f, 0.58f, 0.34f), new Color(0.84f, 0.76f, 0.4f), new Color(0.08f, 0.16f, 0.15f)), 
				ExampleTextureTheme.Purple => new ExampleTextureColors(new Color(0.33f, 0.28f, 0.5f), new Color(0.58f, 0.4f, 0.52f), new Color(0.94f, 0.66f, 0.36f), new Color(0.16f, 0.13f, 0.25f)), 
				ExampleTextureTheme.Warm => new ExampleTextureColors(new Color(0.37f, 0.24f, 0.35f), new Color(0.66f, 0.4f, 0.35f), new Color(0.92f, 0.68f, 0.4f), new Color(0.18f, 0.1f, 0.16f)), 
				ExampleTextureTheme.Pixel => new ExampleTextureColors(new Color(0.25f, 0.44f, 0.6f), new Color(0.88f, 0.76f, 0.42f), new Color(0.98f, 0.9f, 0.6f), new Color(0.12f, 0.21f, 0.29f)), 
				_ => new ExampleTextureColors(new Color(0.18f, 0.32f, 0.43f), new Color(0.2f, 0.5f, 0.56f), new Color(0.92f, 0.72f, 0.32f), new Color(0.08f, 0.14f, 0.2f)), 
			};
		}

		private static int StableHash(string value)
		{
			int hash = -2128831035;
			for (int i = 0; i < value.Length; i++)
			{
				hash ^= value[i];
				hash *= 16777619;
			}
			return hash;
		}

		private static void DestroyGeneratedTextures()
		{
			foreach (Texture2D texture in generatedTextures.Values)
			{
				if (texture != null)
				{
					UnityEngine.Object.DestroyImmediate(texture);
				}
			}
			generatedTextures.Clear();
		}
	}
}

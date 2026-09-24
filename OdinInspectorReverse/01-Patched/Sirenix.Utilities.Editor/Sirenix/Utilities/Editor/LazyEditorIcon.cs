using System;
using Sirenix.Reflection.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// Lazy loading Editor Icon.
	/// </summary>
	public class LazyEditorIcon : EditorIcon, IDisposable
	{
		private static Color inactiveColorPro = new Color(0.4f, 0.4f, 0.4f, 1f);

		private static Color activeColorPro = new Color(0.55f, 0.55f, 0.55f, 1f);

		private static Color highlightedColorPro = new Color(0.9f, 0.9f, 0.9f, 1f);

		private static Color inactiveColor = new Color(0.72f, 0.72f, 0.72f, 1f);

		private static Color activeColor = new Color(0.4f, 0.4f, 0.4f, 1f);

		private static Color highlightedColor = new Color(0.2f, 0.2f, 0.2f, 1f);

		private static Material iconMat;

		private Texture2D icon;

		private Texture inactive;

		private Texture active;

		private Texture highlighted;

		private string data;

		private int width;

		private int height;

		/// <summary>
		/// Gets the icon's highlight texture.
		/// </summary>
		public override Texture Highlighted
		{
			get
			{
				if (highlighted == null)
				{
					highlighted = RenderIcon(EditorGUIUtility.isProSkin ? highlightedColorPro : highlightedColor);
				}
				return highlighted;
			}
		}

		/// <summary>
		/// Gets the icon's active texture.
		/// </summary>
		public override Texture Active
		{
			get
			{
				if (active == null)
				{
					active = RenderIcon(EditorGUIUtility.isProSkin ? activeColorPro : activeColor);
				}
				return active;
			}
		}

		/// <summary>
		/// Gets the icon's inactive texture.
		/// </summary>
		public override Texture Inactive
		{
			get
			{
				if (inactive == null)
				{
					inactive = RenderIcon(EditorGUIUtility.isProSkin ? inactiveColorPro : inactiveColor);
				}
				return inactive;
			}
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override Texture2D Raw
		{
			get
			{
				if (icon == null)
				{
					byte[] bytes = Convert.FromBase64String(data);
					icon = TextureUtilities.LoadImage(width, height, bytes);
				}
				return icon;
			}
		}

		/// <summary>
		/// Loads an EditorIcon from the spritesheet.
		/// </summary>
		public LazyEditorIcon(int width, int height, string base64ImageDataPngOrJPG)
		{
			this.width = width;
			this.height = height;
			data = base64ImageDataPngOrJPG;
		}

		public void Dispose()
		{
			if (icon != null)
			{
				UnityEngine.Object.DestroyImmediate(icon);
			}
			if (inactive != null)
			{
				UnityEngine.Object.DestroyImmediate(inactive);
			}
			if (active != null)
			{
				UnityEngine.Object.DestroyImmediate(active);
			}
			if (highlighted != null)
			{
				UnityEngine.Object.DestroyImmediate(highlighted);
			}
		}

		private Texture RenderIcon(Color color)
		{
			if (iconMat == null || iconMat.shader == null)
			{
				OdinEntityId matInstanceId = OdinEntityId.GetSessionStateId("odin_inspector_lazy_icons", OdinEntityId.None);
				if (matInstanceId.IsValid)
				{
					iconMat = matInstanceId.ToObject() as Material;
				}
				if (iconMat == null)
				{
					Shader shader = Shader.Find("Hidden/Sirenix/Editor/GUIIcon");
					iconMat = new Material(shader);
					UnityEngine.Object.DontDestroyOnLoad(iconMat);
					iconMat.hideFlags = HideFlags.DontUnloadUnusedAsset;
					OdinEntityId.SetSessionStateId("odin_inspector_lazy_icons", OdinEntityId.FromObject(iconMat));
				}
			}
			iconMat.SetColor("_Color", color);
			bool prevSRGB = GL.sRGBWrite;
			GL.sRGBWrite = true;
			RenderTexture prev = RenderTexture.active;
			RenderTexture rt = (RenderTexture.active = RenderTexture.GetTemporary(width, height, 0));
			GL.Clear(clearDepth: false, clearColor: true, new Color(1f, 1f, 1f, 0f));
			Graphics.Blit(Raw, rt, iconMat);
			Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, mipChain: false, linear: true);
			texture.filterMode = FilterMode.Bilinear;
			texture.ReadPixels(new Rect(0f, 0f, rt.width, rt.height), 0, 0);
			texture.alphaIsTransparency = true;
			texture.Apply();
			RenderTexture.ReleaseTemporary(rt);
			RenderTexture.active = prev;
			GL.sRGBWrite = prevSRGB;
			return texture;
		}
	}
}

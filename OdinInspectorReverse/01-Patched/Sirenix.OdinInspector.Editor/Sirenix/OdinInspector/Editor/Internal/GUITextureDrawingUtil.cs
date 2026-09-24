using System;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	public static class GUITextureDrawingUtil
	{
		private static Material material;

		private static float _greyScale;

		private static Color _guiColor;

		private static Vector4 _uv;

		private static Color _hueColor;

		private const string SHADER_NAME = "Hidden/Sirenix/OdinGUIShader";

		public static Material Material
		{
			get
			{
				if (material == null)
				{
					OdinEntityId matEntityId = OdinEntityId.GetSessionStateId("odin_gui_material1", OdinEntityId.None);
					if (matEntityId.IsValid)
					{
						material = matEntityId.ToObject() as Material;
					}
					if (material == null)
					{
						Shader s = Shader.Find("Hidden/Sirenix/OdinGUIShader");
						material = new Material(s);
						UnityEngine.Object.DontDestroyOnLoad(material);
						material.hideFlags = HideFlags.DontUnloadUnusedAsset;
						OdinEntityId.SetSessionStateId("odin_gui_material1", OdinEntityId.FromObject(material));
					}
					Shader.SetGlobalColor("_SirenixOdin_GUIColor", new Color(1f, 1f, 1f, 1f));
					Shader.SetGlobalColor("_SirenixOdin_HueColor", new Color(1f, 1f, 1f, 0f));
					Shader.SetGlobalVector("_SirenixOdin_GUIUv", new Vector4(0f, 0f, 1f, 1f));
					Shader.SetGlobalFloat("_SirenixOdin_GreyScale", 0f);
					Shader.SetGlobalFloat("_SirenixOdin_GreyScale", 0f);
					_uv = new Vector4(0f, 0f, 1f, 1f);
					_hueColor = new Color(1f, 1f, 1f, 0f);
					_greyScale = 0f;
					_guiColor = new Color(1f, 1f, 1f, 1f);
				}
				return material;
			}
		}

		internal static Rect CalculateScaledTextureRects(Rect position, ScaleMode scaleMode, float imageAspect, out Rect uvRect)
		{
			float num = position.width / position.height;
			switch (scaleMode)
			{
			case ScaleMode.StretchToFill:
				uvRect = new Rect(0f, 0f, 1f, 1f);
				return position;
			case ScaleMode.ScaleAndCrop:
				if (num > imageAspect)
				{
					float num4 = imageAspect / num;
					uvRect = new Rect(0f, (1f - num4) * 0.5f, 1f, num4);
				}
				else
				{
					float num5 = num / imageAspect;
					uvRect = new Rect(0.5f - num5 * 0.5f, 0f, num5, 1f);
				}
				return position;
			case ScaleMode.ScaleToFit:
			{
				if (num > imageAspect)
				{
					float num2 = imageAspect / num;
					uvRect = new Rect(0f, 0f, 1f, 1f);
					return new Rect(position.xMin + position.width * (1f - num2) * 0.5f, position.yMin, num2 * position.width, position.height);
				}
				float num3 = num / imageAspect;
				uvRect = new Rect(0f, 0f, 1f, 1f);
				return new Rect(position.xMin, position.yMin + position.height * (1f - num3) * 0.5f, position.width, num3 * position.height);
			}
			default:
				throw new NotImplementedException();
			}
		}

		public static void DrawTexture(Rect rect, Texture2D texture, ScaleMode scaleMode, Color color, Color hueColor, float greyScale = 1f)
		{
			if (Event.current.type != EventType.Repaint || rect.width <= 0f || rect.height <= 0f)
			{
				return;
			}
			rect = CalculateScaledTextureRects(rect, scaleMode, (float)texture.width / (float)texture.height, out var uvRect);
			Rect unclipped = GUIClipInfo.Unclip(rect);
			Rect clipRect = GUIClipInfo.TopMostRect;
			if (unclipped.Overlaps(clipRect))
			{
				float uvXOrig = uvRect.x;
				float uvYOrig = uvRect.y;
				float uvWidthOrig = uvRect.width;
				float uvHeightOrig = uvRect.height;
				float uvX = uvXOrig;
				float uvY = uvYOrig;
				float uvWidth = uvWidthOrig;
				float uvHeight = uvHeightOrig;
				Rect clipped = rect;
				float clipTop = clipRect.y - unclipped.y;
				if (clipTop > 0f)
				{
					float amount = clipTop / rect.height;
					uvY += uvHeightOrig * amount;
					uvHeight -= uvHeightOrig * amount;
					clipped.y += clipTop;
					clipped.height -= clipTop;
				}
				float clipBottom = unclipped.yMax - clipRect.yMax;
				if (clipBottom > 0f)
				{
					float amount2 = clipBottom / rect.height;
					uvHeight -= uvHeight * amount2;
					clipped.height -= clipBottom;
				}
				float clipLeft = clipRect.x - unclipped.x;
				if (clipLeft > 0f)
				{
					float amount3 = clipLeft / rect.width;
					uvX += uvWidthOrig * amount3;
					uvWidth -= uvWidthOrig * amount3;
					clipped.x += clipLeft;
					clipped.width -= clipLeft;
				}
				float clipRight = unclipped.xMax - clipRect.xMax;
				if (clipRight > 0f)
				{
					float amount4 = clipRight / rect.width;
					uvWidth -= uvWidth * amount4;
					clipped.width -= clipRight;
				}
				color *= GUI.color;
				if (!GUI.enabled)
				{
					color.a *= 0.4f;
				}
				SetHueColor(hueColor);
				SetGUIColor(color);
				SetGreyScale(greyScale);
				SetUv(new Vector4(uvX, uvY, uvWidth, uvHeight));
				Graphics.DrawTexture(clipped, texture, Material);
			}
		}

		public static void SetProperties(Color guiColor, Color hueColor, Vector4 uv, float greyScale)
		{
			SetGUIColor(guiColor);
			SetHueColor(hueColor);
			SetUv(uv);
			SetGreyScale(greyScale);
		}

		private static void SetGUIColor(Color color)
		{
			if (_guiColor != color)
			{
				Shader.SetGlobalColor("_SirenixOdin_GUIColor", color);
				_guiColor = color;
			}
		}

		private static void SetHueColor(Color color)
		{
			if (_hueColor != color)
			{
				Shader.SetGlobalColor("_SirenixOdin_HueColor", color);
				_hueColor = color;
			}
		}

		private static void SetGreyScale(float factor)
		{
			if (_greyScale != factor)
			{
				Shader.SetGlobalFloat("_SirenixOdin_GreyScale", factor);
				_greyScale = factor;
			}
		}

		private static void SetUv(Vector4 uv)
		{
			if (_uv != uv)
			{
				Shader.SetGlobalVector("_SirenixOdin_GUIUv", uv);
				_uv = uv;
			}
		}
	}
}

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Sirenix.Reflection.Editor
{
	public static class GUI_Internals
	{
		private static int? _buttonHash;

		public static IEnumerable<ScrollViewState_Internal> ScrollViewStates
		{
			get
			{
				foreach (object item in GUI.scrollViewStates)
				{
					if (item is UnityEngine.ScrollViewState group)
					{
						yield return new ScrollViewState_Internal(group);
					}
				}
			}
		}

		public static Material BlendMaterial => GUI.blendMaterial;

		public static Material BlitMaterial => GUI.blitMaterial;

		public static Material RoundedRectMaterial => GUI.roundedRectMaterial;

		public static Material RoundedRectWithColorPerBorderMaterial => GUI.roundedRectWithColorPerBorderMaterial;

		public static int ButtonHash
		{
			get
			{
				if (!_buttonHash.HasValue)
				{
					FieldInfo fieldMisspelled = typeof(GUI).GetField("s_ButonHash", BindingFlags.Static | BindingFlags.NonPublic);
					if (fieldMisspelled != null)
					{
						_buttonHash = (int)fieldMisspelled.GetValue(null);
					}
					else
					{
						FieldInfo field = typeof(GUI).GetField("s_ButtonHash", BindingFlags.Static | BindingFlags.NonPublic);
						if (field != null)
						{
							_buttonHash = (int)field.GetValue(null);
						}
						else
						{
							_buttonHash = "Button".GetHashCode();
						}
					}
				}
				return _buttonHash.Value;
			}
		}

		public static void DrawTexture(Rect rect, Rect uvRect, Texture texture, Material material, Color color, float borderWidths, Vector4 borderRadius, bool drawSmoothCorners)
		{
			DrawTexture(rect, uvRect, texture, material, color, new Vector4(borderWidths, borderWidths, borderWidths, borderWidths), borderRadius, drawSmoothCorners);
		}

		public static void DrawTexture(Rect rect, Rect uvRect, Texture texture, Material material, Color color, float borderWidths, float borderRadius, bool drawSmoothCorners)
		{
			DrawTexture(rect, uvRect, texture, material, color, new Vector4(borderWidths, borderWidths, borderWidths, borderWidths), new Vector4(borderRadius, borderRadius, borderRadius, borderRadius), drawSmoothCorners);
		}

		public static void DrawTexture(Rect rect, Rect uvRect, Texture texture, Material material, Color color, Vector4 borderWidths, float borderRadius, bool drawSmoothCorners)
		{
			DrawTexture(rect, uvRect, texture, material, color, borderWidths, new Vector4(borderRadius, borderRadius, borderRadius, borderRadius), drawSmoothCorners);
		}

		public static void DrawTexture(Rect rect, Rect uvRect, Texture texture, Material material, Color color, Vector4 borderWidths, Vector4 borderRadius, bool drawSmoothCorners)
		{
			if ((object)texture == null)
			{
				Debug.LogWarning("GUI_Internals.DrawTexture received null texture.");
				return;
			}
			GUIUtility.CheckOnGUI();
			if (Event.current.type == EventType.Repaint)
			{
				UnityEngine.Internal_DrawTextureArguments args = new UnityEngine.Internal_DrawTextureArguments
				{
					screenRect = rect,
					sourceRect = uvRect,
					leftBorder = 0,
					rightBorder = 0,
					topBorder = 0,
					bottomBorder = 0,
					color = color,
					borderWidths = borderWidths,
					cornerRadiuses = borderRadius,
					texture = texture,
					smoothCorners = drawSmoothCorners,
					mat = material
				};
				Graphics.Internal_DrawTexture(ref args);
			}
		}

		public static void DrawTexture(Rect rect, Rect uvRect, Texture texture, Material material, Color leftColor, Color topColor, Color rightColor, Color bottomColor, float borderWidths, float borderRadius, bool drawSmoothCorners)
		{
			DrawTexture(rect, uvRect, texture, material, leftColor, topColor, rightColor, bottomColor, new Vector4(borderWidths, borderWidths, borderWidths, borderWidths), new Vector4(borderRadius, borderRadius, borderRadius, borderRadius), drawSmoothCorners);
		}

		public static void DrawTexture(Rect rect, Rect uvRect, Texture texture, Material material, Color leftColor, Color topColor, Color rightColor, Color bottomColor, float borderWidths, Vector4 borderRadius, bool drawSmoothCorners)
		{
			DrawTexture(rect, uvRect, texture, material, leftColor, topColor, rightColor, bottomColor, new Vector4(borderWidths, borderWidths, borderWidths, borderWidths), borderRadius, drawSmoothCorners);
		}

		public static void DrawTexture(Rect rect, Rect uvRect, Texture texture, Material material, Color leftColor, Color topColor, Color rightColor, Color bottomColor, Vector4 borderWidths, float borderRadius, bool drawSmoothCorners)
		{
			DrawTexture(rect, uvRect, texture, material, leftColor, topColor, rightColor, bottomColor, borderWidths, new Vector4(borderRadius, borderRadius, borderRadius, borderRadius), drawSmoothCorners);
		}

		public static void DrawTexture(Rect rect, Rect uvRect, Texture texture, Material material, Color leftColor, Color topColor, Color rightColor, Color bottomColor, Vector4 borderWidths, Vector4 borderRadius, bool drawSmoothCorners)
		{
			if ((object)texture == null)
			{
				Debug.LogWarning("GUI_Internals.DrawTexture received null texture.");
				return;
			}
			GUIUtility.CheckOnGUI();
			if (Event.current.type == EventType.Repaint)
			{
				UnityEngine.Internal_DrawTextureArguments args = new UnityEngine.Internal_DrawTextureArguments
				{
					screenRect = rect,
					sourceRect = uvRect,
					leftBorder = 0,
					rightBorder = 0,
					topBorder = 0,
					bottomBorder = 0,
					color = Color.white,
					leftBorderColor = leftColor,
					topBorderColor = topColor,
					rightBorderColor = rightColor,
					bottomBorderColor = bottomColor,
					borderWidths = borderWidths,
					cornerRadiuses = borderRadius,
					texture = texture,
					smoothCorners = drawSmoothCorners,
					mat = material
				};
				Graphics.Internal_DrawTexture(ref args);
			}
		}

		public static bool Button(Rect position, int id, GUIContent label, GUIStyle style)
		{
			return GUI.Button(position, id, label, style);
		}

		public static bool Button(Rect position, FocusType focusType, GUIContent label, GUIStyle style, out int id)
		{
			id = GUIUtility.GetControlID(ButtonHash, focusType, position);
			return GUI.Button(position, id, label, style);
		}
	}
}

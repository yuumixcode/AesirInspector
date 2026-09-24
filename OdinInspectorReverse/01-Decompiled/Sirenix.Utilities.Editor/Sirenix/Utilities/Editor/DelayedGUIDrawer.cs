using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	public class DelayedGUIDrawer
	{
		private Vector2 screenPos;

		private Material material;

		private RenderTexture prev;

		private RenderTexture target;

		public void Begin(float width, float height, bool drawGUI = false)
		{
			Begin(new Vector2(width, height), drawGUI);
		}

		public void Begin(Vector2 size, bool drawGUI = false)
		{
			UnityShims.Rect.Ctor(out var areaRect, screenPos, size);
			GUIHelper.BeginIgnoreInput();
			GUILayout.BeginArea(areaRect, SirenixGUIStyles.None);
			if (Event.current.type == EventType.Repaint)
			{
				prev = RenderTexture.active;
				if (target != null)
				{
					RenderTexture.ReleaseTemporary(target);
				}
				target = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
				RenderTexture.active = target;
				GL.Clear(clearDepth: false, clearColor: true, new Color(0f, 0f, 0f, 0f));
			}
		}

		public void End()
		{
			if (Event.current.type == EventType.Repaint)
			{
				RenderTexture.active = prev;
			}
			GUILayout.EndArea();
			GUIHelper.EndIgnoreInput();
		}

		public void Draw(Vector2 position)
		{
			if (Event.current.type != EventType.Layout)
			{
				screenPos = position;
			}
			if (Event.current.type == EventType.Repaint)
			{
				if (material == null)
				{
					material = new Material(Shader.Find("Unlit/Transparent"));
				}
				if (target != null)
				{
					Graphics.Blit(target, RenderTexture.active, material);
					RenderTexture.ReleaseTemporary(target);
					target = null;
				}
			}
		}
	}
}

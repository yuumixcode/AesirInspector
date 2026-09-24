using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	internal struct GUISpinner
	{
		public enum IconType
		{
			Valid,
			Error,
			Warning
		}

		private static float Accelleration = 4.5f;

		private float dt;

		private float prevT;

		private float spinDir;

		private float warning;

		private float error;

		private float valid;

		private float odin;

		private float spin;

		private bool mouseOver;

		private double mouseOverTime;

		private bool skipDeltaHack;

		public void DrawSceneWidgetSpinner(Rect rect, IconType type, bool spin, bool isEnabled, bool isMouseOver, float t)
		{
			if (Event.current.type != EventType.Layout)
			{
				if (isMouseOver != mouseOver)
				{
					mouseOver = isMouseOver;
					mouseOverTime = EditorApplication.timeSinceStartup;
					skipDeltaHack = true;
				}
				if (mouseOverTime + 1.0 > EditorApplication.timeSinceStartup)
				{
					GUIHelper.RequestRepaint();
				}
			}
			if (this.spin > 0f || spin)
			{
				GUIHelper.RequestRepaint();
			}
			if (Event.current.type == EventType.Repaint)
			{
				float newT = Time.realtimeSinceStartup;
				dt = newT - prevT;
				prevT = newT;
				GUISpinner prevSpiner = this;
				if ((double)this.spin < 0.5 && spin)
				{
					spinDir = -1f;
				}
				if ((double)this.spin > 0.5 && !spin)
				{
					spinDir = 1f;
				}
				float speed = dt * Accelleration;
				if (skipDeltaHack)
				{
					speed = 0f;
					skipDeltaHack = false;
				}
				warning = LerpSnap(warning, type == IconType.Warning, speed);
				error = LerpSnap(error, type == IconType.Error, speed);
				valid = LerpSnap(valid, type == IconType.Valid, speed);
				this.spin = LerpSnap(this.spin, spin, speed);
				odin = LerpSnap(odin, mouseOver, mouseOver ? (speed * 4f) : (speed * 3f));
				Material mat = ValidatorGui.SpinnerMat;
				Color color = ((!isEnabled) ? Color.gray : (ValidatorGui.DarkSkinRedErrorColor * error + ValidatorGui.GreenValidColor * valid + ValidatorGui.DarkSkinYellowWarningColor * warning));
				if ((double)this.spin > 0.01)
				{
					Color spinCol = Color.Lerp(Color.white, ValidatorGui.DarkSkinYellowWarningColor, warning);
					spinCol = Color.Lerp(spinCol, ValidatorGui.DarkSkinRedErrorColor, error);
					color = Color.Lerp(color, spinCol, this.spin);
				}
				Shader.SetGlobalVector("_SirenixOdinSpinner_Shape", new Vector4(odin, valid, warning, error));
				Shader.SetGlobalFloat("_SirenixOdinSpinner_Spin", this.spin);
				Shader.SetGlobalFloat("_SirenixOdinSpinner_SpinDir", spinDir);
				Shader.SetGlobalColor("_SirenixOdinSpinner_Color", color);
				Shader.SetGlobalFloat("_SirenixOdinSpinner_T", t);
				Shader.SetGlobalFloat("_SirenixOdinSpinner_SpinTime", (float)EditorApplication.timeSinceStartup);
				if (this.spin != prevSpiner.spin || spinDir != prevSpiner.spinDir || warning != prevSpiner.warning || error != prevSpiner.error || valid != prevSpiner.valid || odin != prevSpiner.odin)
				{
					GUIHelper.RequestRepaint();
				}
				Graphics.DrawTexture(rect, Texture2D.whiteTexture, mat);
			}
		}

		public void DrawOdinSpinner(Rect rect, bool spin, bool isMouseOver, bool enabled)
		{
			if (Event.current.type != EventType.Layout)
			{
				if (isMouseOver != mouseOver)
				{
					mouseOver = isMouseOver;
					mouseOverTime = EditorApplication.timeSinceStartup;
					skipDeltaHack = true;
				}
				if (mouseOverTime + 1.0 > EditorApplication.timeSinceStartup)
				{
					GUIHelper.RequestRepaint();
				}
			}
			if (this.spin > 0f || spin)
			{
				GUIHelper.RequestRepaint();
			}
			if (Event.current.type == EventType.Repaint)
			{
				float newT = Time.realtimeSinceStartup;
				dt = newT - prevT;
				prevT = newT;
				GUISpinner prevSpiner = this;
				float speed = dt * Accelleration;
				if (skipDeltaHack)
				{
					speed = 0f;
					skipDeltaHack = false;
				}
				warning = 0f;
				error = 0f;
				valid = 0f;
				this.spin = LerpSnap(this.spin, spin && !isMouseOver, speed);
				odin = 1f - this.spin;
				Material mat = ValidatorGui.SpinnerMat;
				Color color = ValidatorGui.BtnMouseOverContentColor;
				if (!enabled && !mouseOver)
				{
					color = ValidatorGui.BtnContentColor;
				}
				Shader.SetGlobalVector("_SirenixOdinSpinner_Shape", new Vector4(odin, valid, warning, error));
				Shader.SetGlobalFloat("_SirenixOdinSpinner_Spin", this.spin);
				Shader.SetGlobalFloat("_SirenixOdinSpinner_SpinDir", spinDir);
				Shader.SetGlobalColor("_SirenixOdinSpinner_Color", color);
				Shader.SetGlobalFloat("_SirenixOdinSpinner_SpinTime", (float)EditorApplication.timeSinceStartup);
				if (this.spin != prevSpiner.spin || spinDir != prevSpiner.spinDir || odin != prevSpiner.odin)
				{
					GUIHelper.RequestRepaint();
				}
				Graphics.DrawTexture(rect, Texture2D.whiteTexture, mat);
			}
		}

		private static float LerpSnap(float a, bool goTowardsOne, float t)
		{
			double threshold = 0.05;
			if ((double)a > 1.0 - threshold && goTowardsOne)
			{
				return 1f;
			}
			if ((double)a < threshold && !goTowardsOne)
			{
				return 0f;
			}
			return Mathf.Lerp(a, goTowardsOne ? 1 : 0, t);
		}
	}
}

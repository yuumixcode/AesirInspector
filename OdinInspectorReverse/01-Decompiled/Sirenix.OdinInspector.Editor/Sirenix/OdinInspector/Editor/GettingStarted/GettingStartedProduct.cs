using UnityEngine;

namespace Sirenix.OdinInspector.Editor.GettingStarted
{
	public struct GettingStartedProduct
	{
		public string Name;

		public string ReviewUrl;

		public bool Enabled;

		public Texture2D Logo;

		public SdfIconType StatusIcon;

		public Color StatusIconColor;

		public string Status;

		public Color HueColor;

		public float NotificationT;

		public float NotificationTargetT;

		public (GettingStartedPage page, string btnName)[] Pages;
	}
}

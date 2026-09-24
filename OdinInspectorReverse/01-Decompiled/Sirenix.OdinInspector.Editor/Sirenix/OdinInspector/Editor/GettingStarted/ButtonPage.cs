using System;

namespace Sirenix.OdinInspector.Editor.GettingStarted
{
	public class ButtonPage : GettingStartedPage
	{
		private Action action;

		public ButtonPage(Action action)
		{
			this.action = action;
		}

		public override void EnterPage()
		{
			action();
		}
	}
}

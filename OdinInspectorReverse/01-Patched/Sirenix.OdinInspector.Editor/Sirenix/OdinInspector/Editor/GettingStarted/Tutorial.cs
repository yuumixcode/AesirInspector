using System;
using System.Collections.Generic;

namespace Sirenix.OdinInspector.Editor.GettingStarted
{
	public struct Tutorial
	{
		public string Title;

		public string Description;

		public SdfIconType Icon;

		public Difficulty Difficulty;

		public Action OnClick;

		public GettingStartedPage ChildPage;

		public Func<bool> Enabled;

		public Func<bool> Visible;

		public List<(string btnName, Action action)> ActionButtons;
	}
}

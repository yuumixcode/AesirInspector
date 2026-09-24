using System;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.GettingStarted
{
	public class WizardPageStep
	{
		public string Name;

		public Action<Rect> OnGUI;

		public WizardPageStep(string name, Action<Rect> onGUI)
		{
			Name = name;
			OnGUI = onGUI;
		}
	}
}

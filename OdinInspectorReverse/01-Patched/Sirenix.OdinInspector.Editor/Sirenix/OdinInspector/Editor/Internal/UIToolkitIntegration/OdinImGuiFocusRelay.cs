using UnityEngine.UIElements;

namespace Sirenix.OdinInspector.Editor.Internal.UIToolkitIntegration
{
	public class OdinImGuiFocusRelay : VisualElement
	{
		public OdinImGuiElement parentOdinElement;

		public OdinImGuiFocusRelay(OdinImGuiElement parent, int direction)
		{
			OdinImGuiFocusRelay odinImGuiFocusRelay = this;
			parentOdinElement = parent;
			base.focusable = true;
			RegisterCallback(delegate(FocusInEvent ev)
			{
				if (ev.target == odinImGuiFocusRelay && odinImGuiFocusRelay.parentOdinElement.LastImGUIContainer != null)
				{
					odinImGuiFocusRelay.parentOdinElement.LastImGUIContainer.Focus();
					odinImGuiFocusRelay.parentOdinElement.SetImGuiFocusTo = direction;
				}
			});
		}
	}
}

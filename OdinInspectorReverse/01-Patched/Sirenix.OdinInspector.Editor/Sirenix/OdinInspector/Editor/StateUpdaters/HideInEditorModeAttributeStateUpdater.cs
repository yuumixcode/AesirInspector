using UnityEngine;

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class HideInEditorModeAttributeStateUpdater : AttributeStateUpdater<HideInEditorModeAttribute>
	{
		public override void OnStateUpdate()
		{
			base.Property.State.Visible = Application.isPlaying;
		}
	}
}

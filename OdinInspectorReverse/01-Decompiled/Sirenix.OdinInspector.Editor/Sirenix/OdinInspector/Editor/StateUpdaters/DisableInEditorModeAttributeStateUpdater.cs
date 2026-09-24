using UnityEngine;

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public class DisableInEditorModeAttributeStateUpdater : AttributeStateUpdater<DisableInEditorModeAttribute>
	{
		public override void OnStateUpdate()
		{
			if (base.Property.State.Enabled && !Application.isPlaying)
			{
				base.Property.State.Enabled = false;
			}
		}
	}
}

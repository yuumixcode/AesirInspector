using UnityEngine;

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public class DisableInPlayModeAttributeStateUpdater : AttributeStateUpdater<DisableInPlayModeAttribute>
	{
		public override void OnStateUpdate()
		{
			base.Property.State.Enabled = !Application.isPlaying;
		}
	}
}

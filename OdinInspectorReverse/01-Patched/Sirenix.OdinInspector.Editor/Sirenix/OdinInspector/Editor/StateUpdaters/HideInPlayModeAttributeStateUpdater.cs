using UnityEngine;

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class HideInPlayModeAttributeStateUpdater : AttributeStateUpdater<HideInPlayModeAttribute>
	{
		public override void OnStateUpdate()
		{
			base.Property.State.Visible = !Application.isPlaying;
		}
	}
}

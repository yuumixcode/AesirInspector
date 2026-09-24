using System;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal interface IHackyListDrawerInteractions
	{
		bool CanCreateValuesToAdd { get; }

		void CreateValuesToAdd(Action<object[]> onCreated, Rect potentialPopupPosition);
	}
}

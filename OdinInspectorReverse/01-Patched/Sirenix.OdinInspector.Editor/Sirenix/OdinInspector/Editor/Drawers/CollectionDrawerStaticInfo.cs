using System;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public static class CollectionDrawerStaticInfo
	{
		public static InspectorProperty CurrentDraggingPropertyInfo;

		public static InspectorProperty CurrentDroppingPropertyInfo;

		public static DelayedGUIDrawer DelayedGUIDrawer = new DelayedGUIDrawer();

		public static Action NextCustomAddFunction;
	}
}

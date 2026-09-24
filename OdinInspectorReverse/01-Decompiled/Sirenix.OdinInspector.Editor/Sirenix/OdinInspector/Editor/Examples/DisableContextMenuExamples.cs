namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(DisableContextMenuAttribute))]
	internal class DisableContextMenuExamples
	{
		[DisableContextMenu(true, false)]
		[InfoBox("DisableContextMenu disables all right-click context menus provided by Odin. It does not disable Unity's context menu.", InfoMessageType.Warning, null)]
		public int[] NoRightClickList = new int[3] { 2, 3, 5 };

		[DisableContextMenu(false, true)]
		public int[] NoRightClickListOnListElements = new int[2] { 7, 11 };

		[DisableContextMenu(true, true)]
		public int[] DisableRightClickCompletely = new int[2] { 13, 17 };

		[DisableContextMenu(true, false)]
		public int NoRightClickField = 19;
	}
}

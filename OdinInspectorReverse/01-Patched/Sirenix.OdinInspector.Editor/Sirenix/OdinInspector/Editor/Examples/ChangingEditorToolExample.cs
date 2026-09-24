using UnityEditor;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(OnValueChangedAttribute), Order = 10f)]
	[AttributeExampleDescription("Example of using EnumPaging together with OnValueChanged.")]
	[AttributeExample(typeof(EnumPagingAttribute), Order = 10f)]
	internal class ChangingEditorToolExample
	{
		[OnValueChanged("SetCurrentTool", false)]
		[InfoBox("Changing this property will change the current selected tool in the Unity editor.", InfoMessageType.Info, null)]
		[EnumPaging]
		public Tool sceneTool;

		private void SetCurrentTool()
		{
			Tools.current = sceneTool;
		}
	}
}

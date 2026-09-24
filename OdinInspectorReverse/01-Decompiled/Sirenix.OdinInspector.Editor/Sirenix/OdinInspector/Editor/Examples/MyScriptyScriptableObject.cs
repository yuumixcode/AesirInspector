using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[TypeInfoBox("The TypeInfoBox attribute can also be used to display a text at the top of, for example, MonoBehaviours or ScriptableObjects.")]
	public class MyScriptyScriptableObject : ScriptableObject
	{
		public string MyText = ExampleHelper.GetString();

		[TextArea(10, 15)]
		public string Box;
	}
}

using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ShowInInspectorAttribute), Name = "Inspect Properties")]
	internal class ShowPropertiesInTheInspectorExamples
	{
		[SerializeField]
		[HideInInspector]
		private int evenNumber;

		[ShowInInspector]
		public int EvenNumber
		{
			get
			{
				return evenNumber;
			}
			set
			{
				evenNumber = value - value % 2;
			}
		}
	}
}

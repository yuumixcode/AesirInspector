using System;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public struct ResultItemMetaData
	{
		public string Name;

		public object Value;

		public Attribute[] Attributes;

		public ResultItemMetaData(string name, object value, params Attribute[] attributes)
		{
			Name = name;
			Value = value;
			Attributes = attributes;
		}
	}
}

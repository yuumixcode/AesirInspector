using System;
using System.Text;

namespace Sirenix.OdinInspector.Editor
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class OdinVisualDesignerAttributeItem : Attribute
	{
		private static readonly StringBuilder Buffer = new StringBuilder();

		public string Label;

		public string Category;

		public Type AttributeType;

		public OdinVisualDesignerAttributeItem(string category, Type attributeType)
		{
			Category = category;
			AttributeType = attributeType;
			Buffer.Clear();
			string typeName = AttributeType.Name;
			int length = (typeName.EndsWith("Attribute") ? (typeName.Length - "Attribute".Length) : typeName.Length);
			Buffer.Append(typeName[0]);
			for (int i = 1; i < length; i++)
			{
				char current = typeName[i];
				if (!char.IsLower(current) && char.IsLower(typeName[i - 1]))
				{
					Buffer.Append(' ');
				}
				Buffer.Append(current);
			}
			Label = Buffer.ToString();
		}
	}
}

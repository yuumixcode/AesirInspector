using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace Sirenix.OdinValidator.Editor
{
	[Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	internal class PlaceholderLabelAttribute : Attribute
	{
		public SdfIconType Icon;

		public string LabelText;

		public PlaceholderLabelAttribute()
		{
		}

		public PlaceholderLabelAttribute(string labelText)
		{
			LabelText = labelText;
		}

		public PlaceholderLabelAttribute(string labelText, SdfIconType icon)
		{
			LabelText = labelText;
			Icon = icon;
		}
	}
}
